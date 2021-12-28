using System;
using System.Collections;
using System.IO;
using System.Linq;
using GlobalEnums;
using HutongGames.PlayMaker;
using Modding;
using On;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Satchel;
using System.Reflection;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RandomTeleport.TeleportTriggers;
using Satchel.BetterMenus;

namespace RandomTeleport
{
    public class RandomTeleport : Mod,IGlobalSettings<GlobalSettings>,ICustomMenuMod,ITogglableMod
    {
        public override string GetVersion() => AssemblyUtils.GetAssemblyVersionHash();
        public static RandomTeleport Instance;
        public static bool enabled = true;
        public static bool Initialized = false;

        public static GlobalSettings settings { get; set; } = new GlobalSettings();

        public bool ToggleButtonInsideMenu => true;

        public void OnLoadGlobal(GlobalSettings s) => settings = s;
        public GlobalSettings OnSaveGlobal() => settings;

        public GameObject RandomTeleporterGo;
        private TimeTeleport TimeTeleportComponent;

        public override void Initialize()
        {
            Instance ??= this;

            enabled = true;

            if (!Initialized)
            {
                RandomTeleporterGo = new GameObject("RandomTeleporter",
                    typeof(TimeTeleport),
                    typeof(DamageTeleport),
                    typeof(KeyPressTeleport));
                UnityEngine.Object.DontDestroyOnLoad(RandomTeleporterGo);

                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneTransitionFixer.ApplyTransitionFixes;
                On.GameManager.BeginSceneTransition += SceneTransitionFixer.ApplySaveDataChanges ;

                TimeTeleportComponent = RandomTeleporterGo.GetComponent<TimeTeleport>();

                DebugMod.AddActionToKeyBindList(Teleport, "Randomly Teleport", "Random Teleport");
                DebugMod.AddActionToKeyBindList(() => { TimeTeleportComponent.timer = 0f; }, "Reset Timer",
                    "Random Teleport");

                On.GameManager.SaveLevelState += SavePersistentBoolItems;
                Initialized = true;
            }
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateMenuScreen(modListMenu, toggleDelegates);
        
        public void Unload()
        {
            enabled = false;
        }

        public void Teleport()
        {
            TimeTeleportComponent.timer = 0f;
            GameManager.instance.StartCoroutine(Teleporter.TeleportCoro());
        }
        
        public static void SavePersistentBoolItemState(PersistentBoolData pbd)
        {
            GameManager.instance.sceneData.SaveMyState(pbd);
            QueuedPersistentBoolData.Add(pbd);
        }
        
        private static List<PersistentBoolData> QueuedPersistentBoolData = new List<PersistentBoolData>();

        // Save our PersistentBoolData after the game does, so we overwrite the game's data rather than the other way round
        public static void SavePersistentBoolItems(On.GameManager.orig_SaveLevelState orig, GameManager self)
        {
            orig(self);
            foreach (PersistentBoolData pbd in QueuedPersistentBoolData)
            {
                SceneData.instance.SaveMyState(pbd);
            }
            QueuedPersistentBoolData.Clear();
        }
    }
}
