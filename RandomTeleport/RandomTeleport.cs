using System.Collections;
using Modding;
using UnityEngine;
using Satchel;
using System.Collections.Generic;
using RandomTeleport.TeleportTriggers;

namespace RandomTeleport
{
    public class RandomTeleport : Mod,IGlobalSettings<GlobalSettings>,ILocalSettings<SaveSettings>,ITogglableMod, ICustomMenuMod
    {
        public override string GetVersion() => AssemblyUtils.GetAssemblyVersionHash();

        public static RandomTeleport Instance;
        public static int currentSeed = new System.Random().Next(999999);
        
        public static GlobalSettings settings { get; set; } = new GlobalSettings();
        public void OnLoadGlobal(GlobalSettings s) => settings = s;
        public GlobalSettings OnSaveGlobal() => settings;
        public static SaveSettings saveSettings { get; set; } = new SaveSettings();
        public void OnLoadLocal(SaveSettings s) => saveSettings = s;
        public SaveSettings OnSaveLocal() => saveSettings;
        public bool ToggleButtonInsideMenu => true;
        

        public static GameObject RandomTeleporterGo = null;

        private static Dictionary<Triggers, TeleportTrigger> TriggerComponents = new Dictionary<Triggers, TeleportTrigger>();

        public override void Initialize()
        {
            Instance ??= this;
            if (RandomTeleporterGo == null)
            {
                RandomTeleporterGo = new GameObject("RandomTeleporter",
                    typeof(TimeTeleport),
                    typeof(DamageTeleport),
                    typeof(KeyPressTeleport));
                Object.DontDestroyOnLoad(RandomTeleporterGo);
                TriggerComponents.Add(Triggers.Time, RandomTeleporterGo.GetAddComponent<TimeTeleport>());
                TriggerComponents.Add(Triggers.Damage, RandomTeleporterGo.GetAddComponent<DamageTeleport>());
                TriggerComponents.Add(Triggers.KeyPress, RandomTeleporterGo.GetAddComponent<KeyPressTeleport>());
                AddToDebug();
            }

            UpdateEnabledComponents();

            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += SceneTransitionFixer.ApplyTransitionFixes;
            On.GameManager.BeginSceneTransition += SceneTransitionFixer.ApplySaveDataChanges;
            On.GameManager.SaveLevelState += SavePersistentBoolItems;

            //new game hook compatible with rando
            On.UIManager.StartNewGame += CreateRNG;

            //to make sure 2 new games created in same session dont result in same rng seed
            On.QuitToMenu.Start += ResetCurrentSeed;

        }

        public void UpdateEnabledComponents()
        {
            foreach (var (Triggers, component) in TriggerComponents)
            {
                if (settings.TriggersState[Triggers])
                {
                    component.Load();
                }
                else
                {
                    component.Unload();
                }
            }
        }

        private void AddToDebug()
        {
            DebugMod.AddActionToKeyBindList(Teleport, "Randomly Teleport", "Random Teleport");
            DebugMod.AddActionToKeyBindList(ResetTimer, "Reset Timer", "Random Teleport");
            DebugMod.AddActionToKeyBindList(TeleportToPrevious, "Go to previous teleport", "Random Teleport");
            DebugMod.AddActionToKeyBindList(() => DebugMod.LogToConsole($"The current seed is {saveSettings.seed}"), 
                "Log seed to console", "Random Teleport");
        }

        private IEnumerator ResetCurrentSeed(On.QuitToMenu.orig_Start orig, QuitToMenu self)
        {
            currentSeed = new System.Random().Next(999999);
            yield return orig(self);
        }

        private void CreateRNG(On.UIManager.orig_StartNewGame orig, UIManager self, bool permadeath, bool bossrush)
        {
            orig(self, permadeath, bossrush);
            saveSettings.RNG = new System.Random(currentSeed);
            saveSettings.seed = currentSeed;
        }

        public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates) => ModMenu.CreateMenuScreen(modListMenu, toggleDelegates.GetValueOrDefault());
        
        public void Unload()
        {
            foreach (var (_, component) in TriggerComponents)
            {
                component.Unload();
            }
            
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= SceneTransitionFixer.ApplyTransitionFixes;
            On.GameManager.BeginSceneTransition -= SceneTransitionFixer.ApplySaveDataChanges;
            On.GameManager.SaveLevelState -= SavePersistentBoolItems;
            On.UIManager.StartNewGame -= CreateRNG;
            On.QuitToMenu.Start -= ResetCurrentSeed;
        }

        public void Teleport()
        {
            ResetTimer();
            GameManager.instance.StartCoroutine(Teleporter.TeleportCoro());
        }
        public void TeleportToPrevious()
        {
            GameManager.instance.StartCoroutine(Teleporter.TeleportCoro(true));
        }

        public void ResetTimer()
        {
            ((TimeTeleport)TriggerComponents[Triggers.Time]).timer = 0f;
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
