using System.Text.RegularExpressions;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vasi;

namespace RandomTeleport1_4
{
    public static class SceneTransitionFixer
    {
        /*
         * Method for enforcing the proper sequence on certain npc quests, etc.
         * This method is currently only used for transition randomizer
         */

        public static void ApplySaveDataChanges(On.GameManager.orig_BeginSceneTransition orig, GameManager self, GameManager.SceneLoadInfo info)
        {
            ApplySaveDataChanges(info.SceneName, info.EntryGateName ?? string.Empty);
            orig(self, info);
        }

        public static void ApplyTransitionFixes(Scene From, Scene To)
        {
            ApplyTransitionFixes(To);
        }

        /*
         * Use this method for fixing bugs related to passing through transitions from an unintended direction.
         * Fixes should be appropriate in any randomizer mode.
         * Fixes specific to transition randomizer need bool checks
         * This method is called after the scene is loaded, but before the player enters
         */

        private static void ApplyTransitionFixes(Scene newScene)
        {
            if (!RandomTeleport1_4.enabled) return;
            switch (newScene.name)
            {
                case "Abyss_06_Core":

                    // Opens floor to void heart (bypasses subsequent checks for kingsoul to be equipped)
                    if (GameManager.instance.entryGateName.StartsWith("b"))
                    {
                        PlayerData.instance.SetBool("openedBlackEggPath", true);
                    }
                    if (PlayerData.instance.openedBlackEggPath)
                    {
                        Object.Destroy(newScene. GetGameObjectByName("floor_closed"));
                    }
                    break;
                case "Deepnest_41":
                    if (GameManager.instance.entryGateName.StartsWith("left1"))
                    {
                        foreach (Transform t in newScene.GetGameObjectByName("Collapser Small (2)").FindGameObjectInChildren("floor1").transform)
                        {
                            if (t.gameObject.name.StartsWith("msk")) Object.Destroy(t.gameObject);
                        }
                    }
                    break;
                case "Deepnest_East_02":
                    if (GameManager.instance.entryGateName.StartsWith("bot2"))
                    {
                        GameObject active = newScene.GetGameObjectByName("Quake Floor").FindGameObjectInChildren("Active");
                        Object.Destroy(active.FindGameObjectInChildren("msk_generic"));
                        Object.Destroy(active.FindGameObjectInChildren("msk_generic (1)"));
                        Object.Destroy(active.FindGameObjectInChildren("msk_generic (2)"));
                        Object.Destroy(active.FindGameObjectInChildren("msk_generic (3)"));
                    }
                    break;
                case "Deepnest_East_03":
                    // When entering from one of the other entrances, it's possible that the player will reach Cornifer before the big title popup
                    // appears; this can lead to a hard lock if the player interacts with Cornifer and then the popup appears during the interaction.
                    if (!GameManager.instance.entryGateName.StartsWith("left"))
                    {
                        PlayerData.instance.SetBool(nameof(PlayerData.visitedOutskirts), true);
                    }
                    break;
                case "Fungus2_15":
                    if (GameManager.instance.entryGateName.StartsWith("left"))
                    {
                        Object.Destroy(newScene.GetGameObjectByName("deepnest_mantis_gate").FindGameObjectInChildren("Collider"));
                        Object.Destroy(newScene.GetGameObjectByName("deepnest_mantis_gate"));
                    }
                    break;
                case "Fungus2_25":
                    if (GameManager.instance.entryGateName.StartsWith("right"))
                    {
                        Object.Destroy(newScene.GetGameObjectByName("mantis_big_door"));
                    }
                    break;
                case "Ruins1_09":
                    if (GameManager.instance.entryGateName.StartsWith("t"))
                    {
                        Object.Destroy(newScene.GetGameObjectByName("Battle Gate"));
                        Object.Destroy(newScene.GetGameObjectByName("Battle Scene"));
                    }
                    break;
                case "Waterways_04":
                    if (GameManager.instance.entryGateName.StartsWith("b"))
                    {
                        GameObject[] gs = Object.FindObjectsOfType<GameObject>();
                        foreach (GameObject g in gs)
                        {
                            if (g.name.StartsWith("Mask"))
                            {
                                g.SetActive(false);
                            }
                        }
                    }
                    break;
                case "White_Palace_03_hub":
                    {
                        GameObject[] gs = Object.FindObjectsOfType<GameObject>();
                        foreach (GameObject g in gs)
                        {
                            if (g.name.StartsWith("Progress"))
                            {
                                Object.Destroy(g);
                            }
                        }
                    }
                    break;
                case "White_Palace_06":
                    if (newScene.GetGameObjectByName("Path of Pain Blocker") != null)
                    {
                        Object.Destroy(newScene.GetGameObjectByName("Path of Pain Blocker"));
                    }
                    break;

                // traverse first room of PoP backwards. Doesn't really belong here, but w/e
                case "White_Palace_18":
                    const float SAW = 1.362954f;
                    GameObject saw = newScene.GetGameObjectByName("wp_saw (4)");

                    GameObject topSaw = Object.Instantiate(saw);
                    topSaw.transform.SetPositionX(165f);
                    topSaw.transform.SetPositionY(30.5f);
                    topSaw.transform.localScale = new Vector3(SAW / 1.5f, SAW / 2, SAW);

                    GameObject botSaw = Object.Instantiate(saw);
                    botSaw.transform.SetPositionX(161.4f);
                    botSaw.transform.SetPositionY(21.4f);
                    botSaw.transform.localScale = new Vector3(SAW / 1.5f, SAW / 2, SAW);

                    break;
            }
        }

        // WARNING - the next method is called before the next scene begins to load.

        public static void ApplySaveDataChanges(string sceneName, string entryGateName)
        {
            if (!RandomTeleport1_4.enabled) return;
            if (string.IsNullOrEmpty(sceneName)) return;
            entryGateName ??= string.Empty;

            switch (sceneName)
            {
                case "Tutorial_01":
                    RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                    {
                        sceneName = "Tutorial_01",
                        id = "Initial Fall Impact",
                        activated = true,
                        semiPersistent = false
                    });
                    if (entryGateName.StartsWith("right"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Door",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Collapser Tute 01",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Tute Door 1",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Tute Door 2",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Tute Door 3",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Tute Door 4",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Tute Door 5",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Tute Door 7",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Tutorial_01",
                            id = "Break Floor 1",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;

                case "Abyss_01":
                    if (entryGateName.StartsWith("left1"))
                    {
                        PlayerData.instance.SetBool("dungDefenderWallBroken", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Waterways_05",
                            id = "One Way Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Abyss_03_c":
                    if (entryGateName.StartsWith("r"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Abyss_03_c",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Abyss_03_c",
                            id = "Mask 1",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Abyss_03_c",
                            id = "Mask 1 (1)",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Abyss_05":
                    if (entryGateName == "right1")
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Abyss_05",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Cliffs_01":
                    if (entryGateName.StartsWith("right4"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Cliffs_01",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Cliffs_01",
                            id = "Breakable Wall grimm",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Crossroads_04":
                    PlayerData.instance.menderState = 2;
                    PlayerData.instance.menderDoorOpened = true;
                    PlayerData.instance.hasMenderKey = true;
                    PlayerData.instance.menderSignBroken = true;
                    if (entryGateName.StartsWith("d"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Crossroads_04",
                            id = "Secret Mask",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Crossroads_04",
                            id = "Secret Mask (1)",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Crossroads_06":
                    // Opens gate in room after False Knight
                    if (entryGateName.StartsWith("l"))
                    {
                        PlayerData.instance.SetBool("shamanPillar", true);
                    }
                    break;
                case "Crossroads_07":
                    if (entryGateName.StartsWith("left3"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Crossroads_07",
                            id = "Tute Door 1",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Crossroads_08":
                    if (entryGateName == "left2")
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Crossroads_08",
                            id = "Battle Scene",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Crossroads_09":
                    if (entryGateName.StartsWith("r"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Crossroads_09",
                            id = "Break Floor 1",
                            activated = true,
                            semiPersistent = false
                        });
                        PlayerData.instance.SetBool("crossroadsMawlekWall", true);
                    }
                    break;
                case "Crossroads_21":
                    // Makes room visible entering from gwomb entrance
                    if (entryGateName.StartsWith("t"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Crossroads_21",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Crossroads_21",
                            id = "Collapser Small",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Crossroads_21",
                            id = "Secret Mask (1)",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Crossroads_33":
                    if (entryGateName.StartsWith("left1"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Crossroads_09",
                            id = "Break Floor 1",
                            activated = true,
                            semiPersistent = false
                        });
                        PlayerData.instance.SetBool("crossroadsMawlekWall", true);
                    }
                    // Opens gate in room after False Knight
                    if (entryGateName.StartsWith("right1"))
                    {
                        PlayerData.instance.SetBool("shamanPillar", true);
                    }
                    break;
                case "Deepnest_01":
                    if (entryGateName.StartsWith("r"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_01",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Fungus2_20",
                            id = "Breakable Wall Waterways",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Deepnest_02":
                    if (entryGateName.StartsWith("r"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_02",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Deepnest_03":
                    if (entryGateName.StartsWith("left2"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_03",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Deepnest_26":
                    if (entryGateName.StartsWith("left2"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_26",
                            id = "Inverse Remasker",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_26",
                            id = "Secret Mask (1)",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Deepnest_31":
                    if (entryGateName.StartsWith("right2"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_31",
                            id = "Secret Mask",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_31",
                            id = "Secret Mask (1)",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_31",
                            id = "Secret Mask (2)",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_31",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_31",
                            id = "Breakable Wall (1)",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Deepnest_East_02":
                    if (entryGateName.StartsWith("r"))
                    {
                        PlayerData.instance.SetBool("outskirtsWall", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_East_02",
                            id = "One Way Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Deepnest_East_03":
                    if (entryGateName.StartsWith("left2"))
                    {
                        PlayerData.instance.SetBool("outskirtsWall", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_East_02",
                            id = "One Way Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Deepnest_East_16":
                    if (entryGateName.StartsWith("b"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_East_16",
                            id = "Quake Floor",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Fungus2_20":
                    if (entryGateName.StartsWith("l"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Deepnest_01",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Fungus2_20",
                            id = "Breakable Wall Waterways",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Fungus3_02":
                    if (entryGateName.StartsWith("right1"))
                    {
                        PlayerData.instance.SetBool("oneWayArchive", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Fungus3_47",
                            id = "One Way Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Fungus3_13":
                    if (entryGateName.StartsWith("left2"))
                    {
                        PlayerData.instance.SetBool("openedGardensStagStation", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Fungus3_40",
                            id = "Gate Switch",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Fungus3_40":
                    if (entryGateName.StartsWith("r"))
                    {
                        PlayerData.instance.SetBool("openedGardensStagStation", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Fungus3_40",
                            id = "Gate Switch",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Fungus3_44":
                    RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                    {
                        sceneName = "Fungus3_44",
                        id = "Secret Mask",
                        activated = true,
                        semiPersistent = false
                    });
                    break;
                case "Fungus3_47":
                    if (entryGateName.StartsWith("l"))
                    {
                        PlayerData.instance.SetBool("oneWayArchive", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Fungus3_47",
                            id = "One Way Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Mines_05":
                    // breakable wall leading to Deep Focus
                    if (entryGateName.StartsWith("left2"))
                    {
                        PlayerData.instance.SetBool("brokeMinersWall", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Mines_05",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "RestingGrounds_02":
                    if (entryGateName.StartsWith("b"))
                    {
                        PlayerData.instance.SetBool("openedRestingGrounds02", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_06",
                            id = "Resting Grounds Slide Floor",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_06",
                            id = "Gate Switch",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "RestingGrounds_05":
                    if (entryGateName.StartsWith("right1"))
                    {
                        PlayerData.instance.SetBool("gladeDoorOpened", true);
                        PlayerData.instance.SetBool("dreamReward2", true);
                    }
                    if (entryGateName.StartsWith("b"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_05",
                            id = "Quake Floor",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "RestingGrounds_06":
                    if (entryGateName.StartsWith("t"))
                    {
                        PlayerData.instance.SetBool("openedRestingGrounds02", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_06",
                            id = "Resting Grounds Slide Floor",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_06",
                            id = "Gate Switch",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "RestingGrounds_10":
                    if (entryGateName.StartsWith("l"))
                    {
                        PlayerData.instance.SetBool("restingGroundsCryptWall", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_10",
                            id = "One Way Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    if (entryGateName.StartsWith("top2"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_10",
                            id = "Breakable Wall (5)",
                            activated = true,
                            semiPersistent = false
                        });
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_10",
                            id = "Breakable Wall (7)",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Room_Town_Stag_Station":
                    if (entryGateName.StartsWith("left1"))
                    {
                        PlayerData.instance.SetBool("openedTownBuilding", true);
                        PlayerData.instance.SetBool("openedTown", true);
                    }
                    break;
                case "Ruins1_05b":
                    if (entryGateName.StartsWith("b"))
                    {
                        PlayerData.instance.SetBool("openedWaterwaysManhole", true);
                    }
                    break;
                case "Ruins1_23":
                    if (entryGateName.StartsWith("t"))
                    {
                        PlayerData.instance.SetBool("brokenMageWindow", true);
                        PlayerData.instance.SetBool("brokenMageWindowGlass", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Ruins1_30",
                            id = "Quake Floor Glass (2)",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Ruins1_24":
                    if (entryGateName.StartsWith("right2"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Ruins1_24",
                            id = "Secret Mask (1)",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Ruins1_30":
                    if (entryGateName.StartsWith("b"))
                    {
                        PlayerData.instance.SetBool("brokenMageWindow", true);
                        PlayerData.instance.SetBool("brokenMageWindowGlass", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Ruins1_30",
                            id = "Quake Floor Glass (2)",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Ruins1_31":
                    if (entryGateName.StartsWith("left3"))
                    {
                        PlayerData.instance.SetBool("openedMageDoor_v2", true);
                    }
                    if (entryGateName.StartsWith("left2"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Ruins1_31",
                            id = "Ruins Lever",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Ruins1_31b":
                    if (entryGateName.StartsWith("right1"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Ruins1_31",
                            id = "Ruins Lever",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Ruins2_01":
                    if (entryGateName.StartsWith("t"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Ruins2_01",
                            id = "Secret Mask",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Ruins2_04":
                    if (entryGateName.StartsWith("door_Ruin_House_03"))
                    {
                        PlayerData.instance.SetBool("city2_sewerDoor", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Ruins_House_03",
                            id = "Ruins Lever",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    else if (entryGateName.StartsWith("door_Ruin_Elevator"))
                    {
                        PlayerData.instance.SetBool("bathHouseOpened", true);
                    }
                    break;
                case "Ruins2_10":
                    if (entryGateName.StartsWith("r"))
                    {
                        PlayerData.instance.SetBool("restingGroundsCryptWall", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "RestingGrounds_10",
                            id = "One Way Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Ruins2_10b":
                    if (entryGateName.StartsWith("l"))
                    {
                        PlayerData.instance.SetBool("bathHouseWall", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Ruins_Bathhouse",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Ruins2_11_b":
                    if (entryGateName.StartsWith("l"))
                    {
                        PlayerData.instance.SetBool("openedLoveDoor", true);
                    }
                    break;
                case "Ruins_House_03":
                    if (entryGateName.StartsWith("left1"))
                    {
                        PlayerData.instance.SetBool("city2_sewerDoor", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Ruins_House_03",
                            id = "Ruins Lever",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Ruins_Bathhouse":
                    if (entryGateName.StartsWith("r"))
                    {
                        PlayerData.instance.SetBool("bathHouseWall", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Ruins_Bathhouse",
                            id = "Breakable Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Town":
                    switch (entryGateName)
                    {
                        case "door_sly":
                            PlayerData.instance.SetBool("slyRescued", true);
                            PlayerData.instance.SetBool("openedSlyShop", true);
                            break;
                        case "door_station":
                            PlayerData.instance.SetBool("openedTownBuilding", true);
                            PlayerData.instance.SetBool("openedTown", true);
                            break;
                        case "door_mapper":
                            PlayerData.instance.SetBool("openedMapperShop", true);
                            break;
                        case "door_bretta":
                            PlayerData.instance.SetBool("brettaRescued", true);
                            break;
                        case "door_jiji":
                            PlayerData.instance.SetBool("jijiDoorUnlocked", true);
                            break;
                        case "room_grimm":
                            PlayerData.instance.SetBool(nameof(PlayerData.troupeInTown), true);
                            break;
                        case "room_divine":
                            PlayerData.instance.SetBool(nameof(PlayerData.divineInTown), true);
                            break;
                    }
                    if (entryGateName != "left1")
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Town",
                            id = "Door Destroyer",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Waterways_01":
                    if (entryGateName.StartsWith("t"))
                    {
                        PlayerData.instance.SetBool("openedWaterwaysManhole", true);
                    }
                    if (entryGateName.StartsWith("r"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Waterways_01",
                            id = "Breakable Wall Waterways",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Waterways_02":
                    if (entryGateName.StartsWith("bot1"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Waterways_02",
                            id = "Quake Floor",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Waterways_04":
                    if (entryGateName.StartsWith("b"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Waterways_04",
                            id = "Quake Floor (1)",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Waterways_05":
                    if (entryGateName.StartsWith("r"))
                    {
                        PlayerData.instance.SetBool("dungDefenderWallBroken", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Waterways_05",
                            id = "One Way Wall",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    if (entryGateName.StartsWith("bot2"))
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Waterways_05",
                            id = "Quake Floor",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Waterways_07":
                    if (entryGateName.StartsWith("right1"))
                    {
                        PlayerData.instance.SetBool("waterwaysAcidDrained", true);
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Waterways_05",
                            id = "Waterways_Crank_Lever",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Waterways_08":
                    if (entryGateName == "left2")
                    {
                        RandomTeleport1_4.SavePersistentBoolItemState(new PersistentBoolData
                        {
                            sceneName = "Waterways_08",
                            id = "Breakable Wall Waterways",
                            activated = true,
                            semiPersistent = false
                        });
                    }
                    break;
                case "Waterways_09":
                    if (entryGateName.StartsWith("left"))
                    {
                        PlayerData.instance.SetBool("waterwaysGate", true);
                    }
                    break;
                case "White_Palace_13":
                    PlayerData.instance.SetBool(nameof(PlayerData.whitePalaceSecretRoomVisited), true);
                    break;
                case "GG_Atrium":
                    if (entryGateName == "door1_blueRoom")
                    {
                        PlayerData.instance.SetBool(nameof(PlayerData.blueRoomDoorUnlocked), true);
                        PlayerData.instance.SetBool(nameof(PlayerData.blueRoomActivated), true);
                    }
                    break;
            }
        }
    }
}