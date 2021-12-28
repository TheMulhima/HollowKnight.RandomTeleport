using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using Modding;
using Satchel;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace RandomTeleport
{
    public static class Teleporter
    {
        internal static IEnumerator TeleportCoro()
        {
            //TODO: Exclude non map scenes
            bool isTeleported = false;
            List<string> availableTeleportScenes = new List<string>();

            try
            {
                availableTeleportScenes = SceneNameParser.GetAvailableTeleportScenes();
            }
            catch (Exception e)
            {
                RandomTeleport.Instance.Log($"Cannot execute teleport because error occured: {e.Message}");
                //theres no scene to be teleported so no teleportation
                yield break;
            }

            //for when scene transition fails
            while (!isTeleported)
            {
                string scene = availableTeleportScenes[UnityEngine.Random.Range(0, availableTeleportScenes.Count)];
                //dont wanna load same scene. not fun
                if (scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) continue;
                
                RandomTeleport.Instance.LogDebug($"Loading Scene: {scene}");
                
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

                FieldInfo cameraGameplayScene = typeof(CameraController).GetField("isGameplayScene", BindingFlags.Instance | BindingFlags.NonPublic);

                cameraGameplayScene.SetValue(GameManager.instance.cameraCtrl, true);
                
                //gets all hazard respawn gos and transtion gate gos
                               
                yield return null;

                Vector3? HeroPos = GetPos(scene);
                
                if (!HeroPos.HasValue) continue;
                HeroController.instance.transform.position =  HeroPos.Value;

                    HeroController.instance.cState.inConveyorZone = false;
                    HeroController.instance.cState.onConveyor = false;
                    HeroController.instance.cState.onConveyorV = false;
    
                    typeof(HeroController).GetMethod("FinishedEnteringScene", BindingFlags.NonPublic | BindingFlags.Instance)?
                                          .Invoke(HeroController.instance, new object[] { true, false });

                    yield return null;
                    
    
                    isTeleported = true;
            }
        }

        private static Dictionary<string, Vector2> SpecialCases = new Dictionary<string, Vector2>()
        {
            //Do not ask why this dict exists. some of them just dont want to work
            {"Deepnest_17", new Vector2(17.0f, 4.8f)},
            {"Grimm_Main_Tent", new Vector2(14.5f, 7.0f)},
            {"Crossroads_43", new Vector2(4.2f, 7.5f)},
            {"Ruins1_01", new Vector2(6.72f, 17.5f)},
            {"Fungus2_26", new Vector2(36.5f, 5.5f)},
            {"Fungus3_35", new Vector2(16f, 5.5f)},
            {"Deepnest_26", new Vector2(163f, 22f)},
            {"Deepnest_41", new Vector2(78.5f, 72.5f)},
            {"Room_Wyrm", new Vector2(18.5f, 7.5f)},
        };

        private static Vector3? GetPos(string SceneName)
        {
            if (SpecialCases.Keys.Contains(SceneName))
            {
                return SpecialCases[SceneName];
            }
            List<GameObject> AllpossibleSpawnLocations = GameObject.FindGameObjectsWithTag("Respawn").Where(go => go != null).ToList();

            if (AllpossibleSpawnLocations.Count == 0) return null;
            
            List<GameObject> HazardRespawnLoacations = AllpossibleSpawnLocations.Where(go => go.GetPath().Contains("Hazard Respawn Trigger")).ToList();
            if (HazardRespawnLoacations.Count > 0)
            {
                return HeroController.instance.FindGroundPoint(HazardRespawnLoacations[UnityEngine.Random.Range(0, HazardRespawnLoacations.Count)].transform.position, true);
            }

            List<GameObject> DoorRespawns = AllpossibleSpawnLocations.Where(go => go.transform.parent != null && go.transform.parent.name.Contains("door")).ToList();
            if (DoorRespawns.Count > 0)
            {
                return HeroController.instance.FindGroundPoint(DoorRespawns[UnityEngine.Random.Range(0, DoorRespawns.Count)].transform.position, true);
            }
            
            return HeroController.instance.FindGroundPoint(AllpossibleSpawnLocations[UnityEngine.Random.Range(0, AllpossibleSpawnLocations.Count)].transform.position, true);

        }
    }
}