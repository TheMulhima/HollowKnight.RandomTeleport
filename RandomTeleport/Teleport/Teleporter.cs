﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GlobalEnums;
using HKMirror.Reflection.SingletonClasses;
using HKMirror.Reflection;
using Satchel;
using UnityEngine;

namespace RandomTeleport
{
    public static class Teleporter
    {
        public static string PreviousScene;
        public static Vector3 PreviousPos;

        internal static IEnumerator TeleportCoro(bool GoToprevious = false)
        {
             bool isTeleported = false;
            List<string> availableTeleportScenes = new List<string>();

            try
            {
                availableTeleportScenes = SceneNameParser.GetAvailableTeleportScenes();
            }
            catch (Exception e)
            {
                RandomTeleport.Instance.LogError($"Cannot execute teleport because error occured: {e.Message}");
                //theres no scene to be teleported so no teleportation
                yield break;
            }

            //for when scene transition fails
            while (!isTeleported)
            {
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                string scene;
                if (GoToprevious)
                {
                    if (PreviousScene.IsAScene() && currentScene != PreviousScene)
                    {
                        scene = PreviousScene;
                    }
                    else
                    {
                        RandomTeleport.Instance.LogWarn("Cannot teleport to previous scene because you are in the same scene");
                        yield break;//no teleport 
                    }
                }
                else
                {
                    scene = availableTeleportScenes[RandomTeleport.saveSettings.RNG.Next(0, availableTeleportScenes.Count)];
                }
                
                //dont wanna load same scene. not fun. so load next scene in list
                if (scene == currentScene)
                {
                    scene = availableTeleportScenes[availableTeleportScenes.IndexOf(scene) + 1];
                }

                HeroControllerR.IgnoreInputWithoutReset();

                HeroControllerR.CancelSuperDash();
                HeroControllerR.ResetMotion();
                HeroControllerR.airDashed = false;
                HeroControllerR.doubleJumped = false;
                HeroControllerR.AffectedByGravity(false);
                
                
                //yes this is a savestate load
                GameManager.instance.entryGateName = "dreamGate";
                GameManager.instance.startedOnThisScene = true;

                GameManager.instance.BeginSceneTransition
                (
                    new GameManager.SceneLoadInfo
                    {
                        SceneName = scene,
                        HeroLeaveDirection = GatePosition.unknown,
                        EntryGateName = "dreamGate",
                        EntryDelay = 0f,
                        WaitForSceneTransitionCameraFade = false,
                        Visualization = 0,
                        AlwaysUnloadUnusedAssets = true
                    }
                );

                GameManager.instance.cameraCtrl.Reflect().isGameplayScene = true;

                GameManager.instance.cameraCtrl.PositionToHero(false);

                yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == scene);

                GameManager.instance.cameraCtrl.FadeSceneIn();

                HeroControllerR.TakeMP(1);
                HeroControllerR.AddMPChargeSpa(1);
                HeroControllerR.TakeHealth(1);
                HeroControllerR.AddHealth(1);

                GameCameras.instance.hudCanvas.gameObject.SetActive(true);

                GameManager.instance.cameraCtrl.Reflect().isGameplayScene = true;

                yield return null;

                Vector3? HeroPos = GoToprevious ? PreviousPos : GetPos(scene);

                if (!HeroPos.HasValue) continue;
                HeroControllerR.transform.position = HeroPos.Value;

                HeroControllerR.cState.inConveyorZone = false;
                HeroControllerR.cState.onConveyor = false;
                HeroControllerR.cState.onConveyorV = false;
                HeroControllerR.FinishedEnteringScene(true, false);
                yield return null;

                isTeleported = true;
                PreviousScene = currentScene;
                PreviousPos = HeroPos.Value;
                
                GameCameras.instance.StopCameraShake();
                RandomTeleport.Instance.Log($"Loading Scene:({availableTeleportScenes.IndexOf(scene)}) {scene} at {HeroPos.GetValueOrDefault()}");
                
            }
        }

        //some scenes that have borked hazard respawns so they need to be done manually
        private static readonly Dictionary<string, Vector2> SpecialCases = new Dictionary<string, Vector2>()
        {
            {"Deepnest_17", new Vector2(17.0f, 4.8f)},
            {"Grimm_Main_Tent", new Vector2(14.5f, 7.0f)},
            {"Crossroads_43", new Vector2(4.2f, 7.5f)},
            {"Ruins1_01", new Vector2(6.72f, 17.5f)},
            {"Fungus2_26", new Vector2(36.5f, 5.5f)},
            {"Fungus3_35", new Vector2(16f, 5.5f)},
            {"Deepnest_26", new Vector2(163f, 22f)},
            {"Deepnest_41", new Vector2(78.5f, 72.5f)},
            {"Room_Wyrm", new Vector2(18.5f, 7.5f)},
            {"Mines_03", new Vector2(30f, 75.5f)},
            {"Mines_35", new Vector2(5f, 49f)},
        };

        //gets all hazard respawn gos and transtion gate gos
        private static Vector3? GetPos(string SceneName)
        {
            //special conditions
            if (SpecialCases.Keys.Contains(SceneName))
            {
                //do a call on RNG to make sure GetPos always calls Next once each scene load
                _ = RandomTeleport.saveSettings.RNG.Next();
                return SpecialCases[SceneName];
            }
            
            if (RandomTeleport.settings.OnlySpawnInTransitions)
            {
                //look for HazardRespawnMarker gos whose parent has a TransitionPoint component.
                //This is because we need the HazardRespawnMarker position to teleport to and not the transition point location
                // but we also only want transitions
            
                List<GameObject> RespawnPoints = GameObject.FindGameObjectsWithTag("Respawn").Where(go => go != null)
                    .Where(respawngo => respawngo.GetComponentInParent(typeof(TransitionPoint))).ToList();

                return HeroControllerR.FindGroundPoint(RespawnPoints[RandomTeleport.saveSettings.RNG.Next(0, RespawnPoints.Count)].transform.position, true);

            }
            
            //Find all gos with respawn tag. Finds all gos with component HazardRespawnMarker. Includes normal hazard respawns and transitions
            List<GameObject> AllpossibleSpawnLocations = GameObject.FindGameObjectsWithTag("Respawn").Where(go => go != null).ToList();

            if (AllpossibleSpawnLocations.Count == 0) return null; //99% sure not gonna happen

            //Get the gos that are actually only hazard respawns (not transitions). does this by seeing parent name
            //This is prioritized because it seems more interesting to be spawned middle of the room (also because some transitions break)
            List<GameObject> HazardRespawnLoacations = AllpossibleSpawnLocations.Where(go => go.GetPath().Contains("Hazard Respawn Trigger")).ToList();
            if (HazardRespawnLoacations.Count > 0)
            {
                return HeroControllerR.FindGroundPoint(HazardRespawnLoacations[RandomTeleport.saveSettings.RNG.Next(0, HazardRespawnLoacations.Count)].transform.position, true);
            }

            //door transitions also count as middle of the room so i prioritized this too
            List<GameObject> DoorRespawns = AllpossibleSpawnLocations.Where(go => go.transform.parent != null && go.transform.parent.name.Contains("door")).ToList();
            if (DoorRespawns.Count > 0)
            {
                return HeroControllerR.FindGroundPoint(DoorRespawns[RandomTeleport.saveSettings.RNG.Next(0, DoorRespawns.Count)].transform.position, true);
            }
            
            //if room neither has door or hazard respawns, just teleport to any hazardRespawnMarker. Most likely the transition
            return HeroControllerR.FindGroundPoint(AllpossibleSpawnLocations[RandomTeleport.saveSettings.RNG.Next(0, AllpossibleSpawnLocations.Count)].transform.position, true);
        }
    }
}