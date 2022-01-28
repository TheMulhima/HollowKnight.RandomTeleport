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
        private static List<string> AllScenes;
        private static List<string> SceneNameExclusions = new List<string>
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
            "Finale",
            "Dream",//this one is messy to include because dying sends you to dream nail collection for whatever reason
            "Room_Tram",//this one breaks and i could fix it but then going to the respective room in area is same thing so not worth it
            "Room_temple",//this one is wierd idk
            "Grimm_Nightmare",
            "Room_Sly_Storeroom",//cuz rando does it too (going to sly room and then basement is only option now)
            "Room_Bretta_Basement"
        };

        private static string[] FinalBossScenes = new[]
            { "Room_Final_Boss_Atrium", "Room_Final_Boss_Core"};
        
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

            availableTeleportScenes = availableTeleportScenes.Where(scene => RandomTeleport.settings.AllowTHK ||  !FinalBossScenes.Contains(scene))
                .Where(isNotGodHomeSoftLockScene).ToList();
            

            if (availableTeleportScenes.Count == 0 || (availableTeleportScenes.Count == 1 && availableTeleportScenes[0] == GameManager.instance.GetSceneNameString()))
            {
                throw new Exception($"Cannot execute teleport because no scenes available");
            }
            
            return availableTeleportScenes;
        }

        private static  readonly string[] GGScenesExclusions = new string[]
        {
            "Waterways",
            "Atrium",
            "Lurker",
            "Pipeway",
            "Workshop",
            "Atrium_Roof",
            "Blue_Room",
            "Land_of_Storms",
            "Unlock_Wastes"
        };

        private static bool isNotGodHomeSoftLockScene(string sceneName)
        {
            if (RandomTeleport.settings.AllowGodHomeBosses) return true;
            string[] sceneNameParts = sceneName.Split(new[] { '_' }, 2);
            if (sceneNameParts.Length == 1)//for funny stuff like Town
            {
                return true;
            }
            string scenePrefix = sceneNameParts[0];
            string sceneSuffix = sceneNameParts[1];

            if (scenePrefix != "GG")
            {
                return true;
            }
            else
            {
                return GGScenesExclusions.Contains(sceneSuffix);
            }
        }

        public static bool IsAScene(this string scene) => AllScenes.Contains(scene);

        static SceneNameParser()
        {
            AllScenes = Enumerable.Range(0, UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings)
                .Select(sceneNumber => Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(sceneNumber))).ToList();
                
                TeleportScenes = AllScenes
                    .Where(sceneName => !SceneNameExclusions.Any(exclusion => sceneName.Contains(exclusion))).ToList();
        }
    }
}