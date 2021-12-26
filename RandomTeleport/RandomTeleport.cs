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

                TimeTeleportComponent = RandomTeleporterGo.GetComponent<TimeTeleport>();
                
                DebugMod.AddActionToKeyBindList(Teleport, "Randomly Teleport", "Random Teleport");
                DebugMod.AddActionToKeyBindList(() => { TimeTeleportComponent.timer = 0f; }, "Reset Timer", "Random Teleport");


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
    }
}
