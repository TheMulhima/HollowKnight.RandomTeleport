using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GlobalEnums;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace RandomTeleport
{
    public static class Teleporter
    {

        static Teleporter()
        {
            TeleportScenes = Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
                .Select(sceneNumber => Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneNumber)))
                .Where(sceneName => !exclusions.Any(exclusion => sceneName.Contains(exclusion))).ToList();
        }

        private static List<string> TeleportScenes;
        private static readonly List<string> exclusions = new List<string>
        {
            "Cutscene",
            "Credits",
            "End",
            "Cinematic",
            "PermaDeath",
            "Menu",
            "BetaEnd",
            "Knight_Pickup",
            "Sequence",
            "preload",
            "boss",
            "test",
            "Entrance",
            "Finale"
        };
        private static Dictionary<string, List<string>> RelatedScenes = new Dictionary<string, List<string>>()
        {
            {"Town", new List<string> {"Room_Town_Stag_Station","Room_mapper","Room_shop","Room_Sly_Storeroom","Room_Bretta","Room_Bretta_Basement","Room_Ouiji","Room_Jinn","Grimm_Divine","Grimm_Main_Tent","Grimm_Main_Tent_boss","Grimm_Nightmare","Dream_Mighty_Zote"}},
            {"Crossroads", new List<string> {"Room_Mender_House","Room_Charm_Shop","Room_temple","Room_ruinhouse","Dream_01_False_Knight","Dream_Final_Boss","Room_Final_Boss_Atrium","Room_Final_Boss_Core"}},
            {"Cliffs", new List<string>  {"Room_nailmaster"}},
            {"Fungus1" , new List<string>  {"Room_nailmaster_02", "Room_Slug_Shrine"} },
            {"Fungus3", new List<string>  {"Room_Fungus_Shaman","Room_Queen","Dream_Guardian_Monomon"} },
            {"Deepnest", new List<string>  {"Room_Mask_Maker","Room_spider_small","Dream_Guardian_Hegemol"} },
            {"Deepnest_East", new List<string>  {"Room_nailmaster_03","Room_Colosseum_01","Room_Colosseum_02","Room_Colosseum_Bronze","Room_Colosseum_Silver","Room_Colosseum_Gold","Room_Colosseum_Spectate","Room_Wyrm"}},
            {"RestingGrounds", new List<string>  {"Room_Mansion","Room_Tram_RG","Dream_Nailcollection","Dream_Backer_Shrine","Dream_Room_Believer_Shrine"}},
            {"Ruins1", new List<string>  {"Room_nailsmith","Dream_02_Mage_Lord"}},
            {"Ruins2", new List<string>  {"Ruins_House_01","Ruins_House_02","Ruins_House_03","Ruins_Elevator","Ruins_Bathhouse","Dream_Guardian_Lurien"}},
            {"Abyss", new List<string>  {"Room_Tram","Dream_03_Infected_Knight","Dream_Abyss"}},
            {"Waterways", new List<string>  {"Dream_04_White_Defender"}},
            {"Fungus2", new List<string>()},
            {"Mines", new List<string>()},
            {"Hive", new List<string>()},
            {"GG", new List<string>()},
            {"Tutorial", new List<string> {"Room_GG_Shortcut"}},
            {"White", new List<string>()},

        };

        private static List<string> NotAreaRoomPrefix = new List<string>  { "Room", "Dream", "Ruins", "Grimm" };

        private static List<string> getSameAreaScenes(string scene, List<string> availableTeleportScenes)
        {
            string[] sceneNameParts = scene.Split(new[] { '_' }, 2);
            string scenePrefix = sceneNameParts[0];

            if (scenePrefix == "Deepnest")
            {
                if ((sceneNameParts[1].Split(new[] { '_' }, 2))[0] == "East")
                {
                    scenePrefix += "_East";
                }
            }

            string currentArea = null;

            if (NotAreaRoomPrefix.Contains(scenePrefix))
            {
                foreach((string area, List<string>  scenes) in RelatedScenes)
                {
                    if (scenes.Contains(scene)) currentArea = area;
                }

                if (currentArea == null)
                {
                    throw new Exception("The scene you are in has no other scenes in the same area");
                }
            }
            else
            {
                currentArea = scenePrefix;
            }
            if (!RelatedScenes.ContainsKey(currentArea))
            {
                throw new Exception("The scene you are in has no other scenes in the same area");
            }

            List<string> currentAreaScenes = availableTeleportScenes.Where(sceneName => sceneName.StartsWith(currentArea) || RelatedScenes[currentArea].Contains(sceneName)).ToList();
            
            if (currentArea == "Deepnest")
            {
                currentAreaScenes = currentAreaScenes.Where(sceneName => !sceneName.StartsWith("Deepnest_East")).ToList();
            }

            return currentAreaScenes;
        }
        
        internal static IEnumerator TeleportCoro()
        {
            bool isTeleported = false;

            List<string> availableTeleportScenes = TeleportScenes;

            if (RandomTeleport.settings.onlyVisitedScenes)
            {
                availableTeleportScenes = availableTeleportScenes.Where(scene => PlayerData.instance.scenesVisited.Contains(scene)).ToList();
            }
            if (RandomTeleport.settings.sameAreaTeleport)
            {
                try
                {
                    availableTeleportScenes = getSameAreaScenes(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, availableTeleportScenes);
                    //if its 1 then its same scene
                    if (availableTeleportScenes.Count <= 1)
                    {
                        throw new Exception("The scene you are in has no other scenes in the same area");
                    }
                }
                catch(Exception e)
                {
                    RandomTeleport.Instance.Log($"Cannot execute teleport because error occured: {e.Message}");
                    //theres no scene to be teleported so no teleportation
                    yield break;
                }
            }

            //for when scene transition fails
            while (!isTeleported)
            {
                string scene = availableTeleportScenes[UnityEngine.Random.Range(0, availableTeleportScenes.Count)];

                //dont wanna load same scene. not fun
                if (scene == UnityEngine.SceneManagement.SceneManager.GetActiveScene().name) continue;

                RandomTeleport.Instance.Log($"Loading Scene: {scene}");

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

                yield return null;

                //gets all hazard respawn gos and transtion gate gos
                GameObject[] possibleSpawnLocations = GameObject.FindGameObjectsWithTag("Respawn");

                //if no possibleSpawnLocations, then load another scene
                if (possibleSpawnLocations.Length == 0) continue;

                HazardRespawnMarker randomSpawnLocations = possibleSpawnLocations[UnityEngine.Random.Range(0, possibleSpawnLocations.Length)].GetComponent< HazardRespawnMarker>();

                HeroController.instance.transform.position = HeroController.instance.FindGroundPoint(randomSpawnLocations.transform.position, true);

                //poor attempt at trying to reduce affects of storage
                //wind storage NOT fixed
                HeroController.instance.cState.onConveyor = false;
                HeroController.instance.cState.onConveyorV = false;
                HeroController.instance.cState.recoilFrozen = false; 
                HeroController.instance.cState.recoiling= false; 
                HeroController.instance.cState.recoilingLeft = false;
                HeroController.instance.cState.recoilingRight= false;
                HeroController.instance.cState.slidingLeft = false;
                HeroController.instance.cState.slidingRight = false;
                HeroController.instance.cState.wallSliding = false;
                Rigidbody2D rb2d = ReflectionHelper.GetField<HeroController, Rigidbody2D>(HeroController.instance, "rb2d");
                rb2d.velocity = new Vector2(0f, 0f);

                typeof(HeroController).GetMethod("FinishedEnteringScene", BindingFlags.NonPublic | BindingFlags.Instance)?
                                      .Invoke(HeroController.instance, new object[] { true, false });

                isTeleported = true;
            }

        }
    }
}