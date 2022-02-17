using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using HutongGames.PlayMaker.Actions;
using MonoMod.Utils;
using Satchel;
using UnityEngine;
using ReflectionHelper = Modding.ReflectionHelper;

namespace RandomTeleport
{
    public static class Teleporter
    {
        public static string PreviousScene;
        public static Vector3 PreviousPos;
        
        private static FastReflectionDelegate HCResetMotion = typeof(HeroController)
            .GetMethod("ResetMotion", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .GetFastDelegate(); 
        
        private static FastReflectionDelegate HCFinishedEnteringScene = typeof(HeroController)
            .GetMethod("FinishedEnteringScene", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
            .GetFastDelegate();
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

                HeroController.instance.IgnoreInputWithoutReset();

                HeroController.instance.CancelSuperDash();
                HCResetMotion(HeroController.instance, null);
                ReflectionHelper.SetField(HeroController.instance, "airDashed", false);
                ReflectionHelper.SetField(HeroController.instance, "doubleJumped", false);
                HeroController.instance.AffectedByGravity(false);
                
                
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

                ReflectionHelper.SetField(GameManager.instance.cameraCtrl, "isGameplayScene", true);

                GameManager.instance.cameraCtrl.PositionToHero(false);

                yield return new WaitUntil(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == scene);

                GameManager.instance.cameraCtrl.FadeSceneIn();

                HeroController.instance.TakeMP(1);
                HeroController.instance.AddMPChargeSpa(1);
                HeroController.instance.TakeHealth(1);
                HeroController.instance.AddHealth(1);

                GameCameras.instance.hudCanvas.gameObject.SetActive(true);

                ReflectionHelper.SetField(GameManager.instance.cameraCtrl, "isGameplayScene", true);

                yield return null;

                Vector3? HeroPos = GoToprevious ? PreviousPos : GetPos(scene);

                if (!HeroPos.HasValue) continue;
                HeroController.instance.transform.position = HeroPos.Value;

                HeroController.instance.cState.inConveyorZone = false;
                HeroController.instance.cState.onConveyor = false;
                HeroController.instance.cState.onConveyorV = false;
                HCFinishedEnteringScene(HeroController.instance, true, false);
                yield return null;

                isTeleported = true;
                PreviousScene = currentScene;
                PreviousPos = HeroPos.Value;
                //TODO: copy full code for left and right transitions in HC.EnterScene

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
                List<GameObject> RespawnPoints = GameObject.FindGameObjectsWithTag("Respawn").Where(go => go != null).ToList();
                List<TransitionPoint> AllTransitionPoints = new List<TransitionPoint>();
                foreach (var respawn in RespawnPoints)
                {
                    var transitionPoint = respawn.GetComponentInParent(typeof(TransitionPoint));
                    if (transitionPoint != null)
                    {
                        AllTransitionPoints.Add(transitionPoint as TransitionPoint);
                    }
                }
                
                TransitionPoint randomTransitionPoint = AllTransitionPoints[RandomTeleport.saveSettings.RNG.Next(0, AllTransitionPoints.Count)];

                var position = randomTransitionPoint.transform.position;
                var offset = randomTransitionPoint.entryOffset;
                switch (randomTransitionPoint.GetGatePosition())
                {
                    case GatePosition.top:
                        
                        RandomTeleport.Instance.LogDebug("Top");
                        return new Vector2(position.x + offset.x,position.y + offset.y);
                    
                    case GatePosition.bottom:
                        
                        RandomTeleport.Instance.LogDebug("bottom");
                        if (randomTransitionPoint.alwaysEnterRight) HeroController.instance.FaceRight();
                        if (randomTransitionPoint.alwaysEnterLeft) HeroController.instance.FaceLeft();
                        ReflectionHelper.SetField(HeroController.instance, "transition_vel", !HeroController.instance.cState.facingRight ? new Vector2(-HeroController.instance.SPEED_TO_ENTER_SCENE_HOR, HeroController.instance.SPEED_TO_ENTER_SCENE_UP) : new Vector2(HeroController.instance.SPEED_TO_ENTER_SCENE_HOR, HeroController.instance.SPEED_TO_ENTER_SCENE_UP));
                        return new Vector2(position.x + offset.x, position.y + offset.y + 3.0f);
                    
                    case GatePosition.left:
                    
                        RandomTeleport.Instance.LogDebug("left");
                        HeroController.instance.FaceLeft();
                        ReflectionHelper.SetField(HeroController.instance, "transition_vel", new Vector2(HeroController.instance.RUN_SPEED, 0f));
                        return new Vector2(position.x + offset.x -2f, HeroController.instance.FindGroundPoint(new Vector2(position.x + offset.x + 2f, position.y)).y + 1f);
                    
                    case GatePosition.right:
                    
                        RandomTeleport.Instance.LogDebug("right");
                        HeroController.instance.FaceRight();
                        ReflectionHelper.SetField(HeroController.instance, "transition_vel", new Vector2(-HeroController.instance.RUN_SPEED, 0f));
                        return new Vector2(position.x + offset.x + 2f,
                        HeroController.instance.FindGroundPoint(new Vector2(position.x + offset.x - 2f, position.y)).y + 1f);
                    
                    case GatePosition.door:
                    
                        RandomTeleport.Instance.LogDebug("door");
                        return HeroController.instance.FindGroundPoint(position);
                }
                
            }
            List<GameObject> AllpossibleSpawnLocations = GameObject.FindGameObjectsWithTag("Respawn").Where(go => go != null).ToList();

            if (AllpossibleSpawnLocations.Count == 0) return null; //99% sure not gonna happen

            List<GameObject> HazardRespawnLoacations = AllpossibleSpawnLocations.Where(go => go.GetPath().Contains("Hazard Respawn Trigger")).ToList();
            if (HazardRespawnLoacations.Count > 0)
            {
                return HeroController.instance.FindGroundPoint(HazardRespawnLoacations[RandomTeleport.saveSettings.RNG.Next(0, HazardRespawnLoacations.Count)].transform.position, true);
            }

            List<GameObject> DoorRespawns = AllpossibleSpawnLocations.Where(go => go.transform.parent != null && go.transform.parent.name.Contains("door")).ToList();
            if (DoorRespawns.Count > 0)
            {
                return HeroController.instance.FindGroundPoint(DoorRespawns[RandomTeleport.saveSettings.RNG.Next(0, DoorRespawns.Count)].transform.position, true);
            }
            
            return HeroController.instance.FindGroundPoint(AllpossibleSpawnLocations[RandomTeleport.saveSettings.RNG.Next(0, AllpossibleSpawnLocations.Count)].transform.position, true);
        }
    }
}