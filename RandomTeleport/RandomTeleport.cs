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
using System.Reflection;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RandomTeleport1_4.TeleportTriggers;
using Vasi;
using Random = System.Random;

namespace RandomTeleport1_4
{
    public class RandomTeleport1_4 : Mod,ITogglableMod
    {
        public override string GetVersion() => Vasi.VersionUtil.GetVersion<RandomTeleport1_4>();
        public string GetName() => "RandomTeleport1_4";

        public static RandomTeleport1_4 Instance;
        public static bool enabled = false;
        public static bool Initialized = false;
        
        public GlobalSettings settings = new GlobalSettings();
        public override ModSettings GlobalSettings
        {
            get => settings;
            set => settings = (GlobalSettings) value;
        }

        public bool ToggleButtonInsideMenu => true;
        

        public GameObject RandomTeleport1_4erGo;
        private TimeTeleport TimeTeleportComponent;

        public override void Initialize()
        {
            Instance ??= this;

            enabled = true;

            if (!Initialized)
            {
                RandomTeleport1_4erGo = new GameObject("RandomTeleport1_4er",
                    typeof(TimeTeleport),
                    typeof(DamageTeleport),
                    typeof(KeyPressTeleport));
                UnityEngine.Object.DontDestroyOnLoad(RandomTeleport1_4erGo);

                UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneTransitionFixer.ApplyTransitionFixes;
                On.GameManager.BeginSceneTransition += SceneTransitionFixer.ApplySaveDataChanges ;

                TimeTeleportComponent = RandomTeleport1_4erGo.GetComponent<TimeTeleport>();

                DebugMod.AddActionToKeyBindList(Teleport, "Randomly Teleport", "Random Teleport");
                DebugMod.AddActionToKeyBindList(() => { TimeTeleportComponent.timer = 0f; }, "Reset Timer",
                    "Random Teleport");

                On.GameManager.SaveLevelState += SavePersistentBoolItems;
                Initialized = true;
            }
        }
        
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
