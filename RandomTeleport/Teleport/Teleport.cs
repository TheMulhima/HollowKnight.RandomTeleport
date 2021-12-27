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
                
                RandomTeleport.Instance.Log($"Loading Scene: {scene})");

                //yes this is a savestate load
                GameManager.instance.entryGateName = "dreamGate";
                GameManager.instance.startedOnThisScene = true;
                
                //HeroController.instance.LeaveScene();

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

                GameObject[] possibleSpawnLocations = GameObject.FindGameObjectsWithTag("Respawn");

                //if no possibleSpawnLocations, then load another scene
                if (possibleSpawnLocations.Length == 0) continue;
                
                HazardRespawnMarker randomSpawnLocations = possibleSpawnLocations[UnityEngine.Random.Range(0, possibleSpawnLocations.Length)].GetComponent<HazardRespawnMarker>();
                
                yield return null;

                //gets all hazard respawn gos and transtion gate gos
                HeroController.instance.transform.position = HeroController.instance.FindGroundPoint(randomSpawnLocations.transform.position, true);

                HeroController.instance.cState.inConveyorZone = false;
                HeroController.instance.cState.onConveyor = false;
                HeroController.instance.cState.onConveyorV = false;

                typeof(HeroController).GetMethod("FinishedEnteringScene", BindingFlags.NonPublic | BindingFlags.Instance)?
                                      .Invoke(HeroController.instance, new object[] { true, false });

                isTeleported = true;
            }

        }
    }
}