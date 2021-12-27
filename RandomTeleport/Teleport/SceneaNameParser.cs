using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace RandomTeleport
{
    public static class SceneNameParser
    {
        private static List<string> TeleportScenes;
        private static readonly List<string> SceneNameExclusions = new List<string>
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

        private static readonly Dictionary<string, Func<bool>> CompletionDependantScenes = new Dictionary<string, Func<bool>>()
        {
            {"Room_mapper", () => PlayerData.instance.openedMapperShop},
            {"Room_shop", () => PlayerData.instance.slyRescued},
            {"Room_Sly_Storeroom", () => PlayerData.instance.hasAllNailArts && ! PlayerData.instance.gotSlyCharm},
            {"Room_Bretta", () => PlayerData.instance.brettaRescued},
            {"Room_Bretta_Basement", () => PlayerData.instance.brettaRescued && PlayerData.instance.hasDoubleJump && PlayerData.instance.zoteDefeated},
            {"Room_Ouiji", () => PlayerData.instance.jijiDoorUnlocked && PlayerData.instance.permadeathMode == 0},
            {"Room_Jinn", () => PlayerData.instance.jijiDoorUnlocked && PlayerData.instance.permadeathMode == 1},
            {"Grimm_Divine", () => PlayerData.instance.divineInTown},
            {"Grimm_Main_Tent", () => PlayerData.instance.troupeInTown && !PlayerData.instance.defeatedNightmareGrimm},
            {"Grimm_Nightmare", () => !PlayerData.instance.defeatedNightmareGrimm && PlayerData.instance.killedGrimm},
            {"Dream_Mighty_Zote", () => PlayerData.instance.brettaRescued && PlayerData.instance.hasDoubleJump && PlayerData.instance.zoteDefeated},
            {"Room_Mender_House", () => PlayerData.instance.menderDoorOpened},
            {"Room_temple", () => false},//idk its not working. if you get to crossroads_02 its the same thing
            {"Room_Final_Boss_Atrium", () => PlayerData.instance.openedBlackEggDoor},
            {"Room_Final_Boss_Core", () => PlayerData.instance.openedBlackEggDoor},
            {"Dream_Final_Boss", () => PlayerData.instance.gotShadeCharm},
            {"Dream_01_False_Knight", () => false},//dont wanna bother with it
            {"Dream_Guardian_Monomon", () => false},
            {"Room_Tram_RG", () => false},
            {"Room_Tram", () => false},
            {"Dream_Backer_Shrine", () => false},
            {"Dream_Room_Believer_Shrine", () => false},
            {"Dream_02_Mage_Lord", () => false},
            {"Ruins_House_03", () => PlayerData.instance.city2_sewerDoor},
            {"Ruins_Bathhouses", () => PlayerData.instance.bathHouseOpened},
            {"Dream_Guardian_Lurien", () => false},
            {"Dream_03_Infected_Knight", () => false},
            {"Dream_Abyss", () => PlayerData.instance.abyssGateOpened},
            {"Dream_04_White_Defender", () => false},
        };
        private static readonly Dictionary<string, List<string>> RelatedScenes = new Dictionary<string, List<string>>()
        {
            {"Town", new List<string> {"Room_Town_Stag_Station","Room_mapper","Room_shop","Room_Sly_Storeroom","Room_Bretta","Room_Bretta_Basement","Room_Ouiji","Room_Jinn","Grimm_Divine","Grimm_Main_Tent","Grimm_Nightmare","Dream_Mighty_Zote"}},
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

        private static readonly List<string> NotAreaRoomPrefix = new List<string>  { "Room", "Dream", "Ruins", "Grimm" };
        
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

        internal static List<string> GetAvailableTeleportScenes()
        {
            
            List<string> availableTeleportScenes = TeleportScenes;

            if (RandomTeleport.settings.onlyVisitedScenes)
            {
                availableTeleportScenes = availableTeleportScenes.Where(scene => PlayerData.instance.scenesVisited.Contains(scene)).ToList();
            }

            if (RandomTeleport.settings.sameAreaTeleport)
            {
                availableTeleportScenes = getSameAreaScenes(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name, availableTeleportScenes);
                //if its 1 then its same scene
                if (availableTeleportScenes.Count <= 1)
                {
                    throw new Exception("The scene you are in has no other scenes in the same area");
                }
            }

            if (RandomTeleport.settings.OnlyAllowInMapAndAccessibleRooms)
            {
                List<string> DependantRooms = CompletionDependantScenes.Keys.ToList();
                availableTeleportScenes =
                    availableTeleportScenes.Where(scene => !DependantRooms.Contains(scene) 
                                                           || (DependantRooms.Contains(scene) && CompletionDependantScenes[scene].Invoke())
                                                           || isGodHomeBossScene(scene)).ToList();
            }
            
            if (availableTeleportScenes.Count == 0 || (availableTeleportScenes.Count == 1 && availableTeleportScenes[0] == GameManager.instance.GetSceneNameString()))
            {
                throw new Exception($"Cannot execute teleport because no scenes available");
            }

            return availableTeleportScenes;
        }

        internal static bool isGodHomeBossScene(string sceneName)
        {
            string[] GGScenesExclusions = new string[]
            {
                "Waterways",
                "Atrium",
                "Lurker",
                "Pipeway",
                "Spa",
                "Unlock",
                "Workshop",
                "End_Sequence",
                "Atrium_Roof",
                "Blue_Room",
                "Engine",
                "Engine_Prime",
                "Engine_Root",
                "Entrance_Cutscene",
                "Land_of_Storms",
                "Boss_Door_Entrance",
                "Wyrm",
                "Unn",
                "Door_5_Finale",
                "Unlock_Wastes"
            };

            string[] sceneNameParts = sceneName.Split(new[] { '_' }, 2);
            string scenePrefix = sceneNameParts[0];
            string sceneSuffix = sceneNameParts[1];

            return scenePrefix == "GG" && !GGScenesExclusions.Contains(sceneSuffix);
        }

        static SceneNameParser()
        {
            TeleportScenes = Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
                .Select(sceneNumber => Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneNumber)))
                .Where(sceneName => !SceneNameExclusions.Any(exclusion => sceneName.Contains(exclusion))).ToList();
        }
    }
}