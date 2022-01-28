using System.Collections.Generic;
using System.Linq;
using Modding;
using Satchel.BetterMenus;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MenuButton = Satchel.BetterMenus.MenuButton;
using UMenuButton = UnityEngine.UI.MenuButton;
using Object = UnityEngine.Object;

namespace RandomTeleport
{
    public static class ModMenu
    {
        private static Menu MenuRef, ExtraMenuRef;
        private static MenuScreen MainMenu, ExtraMenu;
        
        private static readonly Dictionary<Triggers, List<string>> DependantOptions = new Dictionary<Triggers, List<string>>()
        {
            {Triggers.Time, new List<string> { "showTimer", "teleportTime", "damageTimeIncrease", "geoTimeIncrease" }},
            {Triggers.Damage, new List<string> { "damageTeleportChance" }},
            {Triggers.KeyPress, new List<string> { "changeSceneKey", "previousSceneKey" }},
        };

        private static readonly List<string> TriggerOptionIds = new List<string>()
        {
            "timeTrigger", "damageTrigger", "keyPressTrigger"
        };

        private static string GetTimeTriggerDesc => $"If Enabled you will be randomly teleported every {RandomTeleport.settings.teleportTime}";
        private static string GetDamageTriggerDesc => $"If Enabled you will be randomly teleported on hit with a {RandomTeleport.settings.chanceOfTeleportOnDamage}% chance";

        public static MenuScreen CreateMenuScreen(MenuScreen modListMenu, ModToggleDelegates toggleDelegates)
        {
            MenuRef = new Menu("Random Teleport", new Element[]
            {
                toggleDelegates.CreateToggle("Mod Toggle", "Use this to enable or disable the mod"),

                new MenuButton("Active Triggers",
                    "Below is a list of active triggers that will determine how you will get teleported",
                    _ =>
                    {
                        foreach (string id in TriggerOptionIds)
                        {
                            MenuRef.Find(id).isVisible = !MenuRef.Find(id).isVisible;
                        }
                        MenuRef.Update();
                    }),
                
                new HorizontalOption("Trigger: Time", GetTimeTriggerDesc, 
                    new []{"Enabled", "Disabled"},
                    s =>
                    {
                        RandomTeleport.settings.TriggersState[Triggers.Time] = s == 0;
                        UpdateTriggerSubOptions();
                        RandomTeleport.Instance.ResetTimer();
                        RandomTeleport.Instance.UpdateEnabledComponents();
                    },
                    () => RandomTeleport.settings.TriggersState[Triggers.Time] ? 0 : 1,
                    Id: "timeTrigger"),
                
                new HorizontalOption("Trigger: Damage", GetDamageTriggerDesc, 
                    new []{"Enabled", "Disabled"},
                    s =>
                    {
                        RandomTeleport.settings.TriggersState[Triggers.Damage] = s == 0;
                        UpdateTriggerSubOptions();
                        RandomTeleport.Instance.UpdateEnabledComponents();
                    },
                    () => RandomTeleport.settings.TriggersState[Triggers.Damage] ? 0 : 1,
                    Id: "damageTrigger"),
                
                new HorizontalOption("Trigger: KeyPress", "If Enabled you will be able to press a key to manually teleport", 
                    new []{"Enabled", "Disabled"},
                    s =>
                    {
                        RandomTeleport.settings.TriggersState[Triggers.KeyPress] = s == 0;
                        UpdateTriggerSubOptions();
                        RandomTeleport.Instance.UpdateEnabledComponents();
                    },
                    () => RandomTeleport.settings.TriggersState[Triggers.KeyPress] ? 0 : 1,
                    Id: "keyPressTrigger"),
                
                new TextPanel(""),//empty panel
                new TextPanel("Settings for the enabled triggers", width:1000f, fontSize:45),

               new HorizontalOption("Show Timer", "",
                    new [] {"Yes", "No"},
                    s => RandomTeleport.settings.showTimer = s == 0,
                    () => RandomTeleport.settings.showTimer ? 0:1, Id: "showTimer") 
                   {isVisible = RandomTeleport.settings.TriggersState[Triggers.Time]},

               new HorizontalOption("Random Teleport Time",
                    "Time between teleports in minutes",
                    //generate list from 20 to 900 with 20 step increments
                    Enumerable.Range(1, 45).Select(x => (x * 20).ToString()).ToArray(),
                    s =>
                    {
                        RandomTeleport.settings.teleportTime = (s + 1) * 20;
                        RandomTeleport.Instance.ResetTimer();
                        ((HorizontalOption)MenuRef.Find("timeTrigger")).Description = GetTimeTriggerDesc;
                        ((HorizontalOption)MenuRef.Find("timeTrigger")).Update();
                    },
                    () => (RandomTeleport.settings.teleportTime / 20) - 1, Id: "teleportTime")
                    {isVisible = RandomTeleport.settings.TriggersState[Triggers.Time]},

                new HorizontalOption("Time reduction from damage",
                    "How much time is removed from the timer when the player takes damage. Negative values will increase time.",
                    //generate list from -120 to 120 with 5 seconds increments
                    Enumerable.Range(-24, 49).Select(x => (x * 5).ToString()).ToArray(),
                    s => RandomTeleport.settings.timeLostFromHit = (s - 24) * 5,
                    () => RandomTeleport.settings.timeLostFromHit / 5 + 24, Id: "damageTimeIncrease")
                    {isVisible = RandomTeleport.settings.TriggersState[Triggers.Time]},

                new HorizontalOption("Time reduction from geo",
                    "How much time is removed from the timer when the player collects geo. Negative values will increase time.",
                    Enumerable.Range(-24, 49).Select(x => (x * 5).ToString()).ToArray(),
                    s => RandomTeleport.settings.timeLostFromGeo = (s - 24) * 5,
                    () => RandomTeleport.settings.timeLostFromGeo / 5 + 24, Id: "geoTimeIncrease")
                    {isVisible = RandomTeleport.settings.TriggersState[Triggers.Time]},
                
                new HorizontalOption("Chance of Teleport On Damage",
                    "When you take damage how likely are you to be teleported in %",
                    Enumerable.Range(0, 21).Select(x => (x * 5).ToString()).ToArray(),
                    s =>
                    {
                        RandomTeleport.settings.chanceOfTeleportOnDamage = s * 5;
                        ((HorizontalOption)MenuRef.Find("damageTrigger")).Description = GetDamageTriggerDesc;
                        ((HorizontalOption)MenuRef.Find("damageTrigger")).Update();
                    },
                    () => RandomTeleport.settings.chanceOfTeleportOnDamage / 5, Id: "damageTeleportChance")
                    {isVisible = RandomTeleport.settings.TriggersState[Triggers.Damage]},

                Blueprints.KeyAndButtonBind("Change Scene",
                RandomTeleport.settings.keybinds.keyRandomTeleport, RandomTeleport.settings.keybinds.buttonRandomTeleport,
                Id:"changeSceneKey"),
                
                Blueprints.KeyAndButtonBind("Go To Previous Teleport",
                RandomTeleport.settings.keybinds.keyPreviousTeleport, RandomTeleport.settings.keybinds.buttonPreviousTeleport,
                Id:"previousSceneKey"),
                
                Blueprints.NavigateToMenu("Extra Settings", 
                    "Some extra settings like RNG seed and on what rooms are included in the pool", 
                    () => ExtraMenu),
            });
            
            MenuRef.Find("changeSceneKey").OnVisibilityChange += (_, e) =>
                ((MenuRow)e.Target).Row.ForEach(elem => elem.isVisible = e.Target.isVisible);
            
            MenuRef.Find("previousSceneKey").OnVisibilityChange += (_, e) =>
                ((MenuRow)e.Target).Row.ForEach(elem => elem.isVisible = e.Target.isVisible);

            MainMenu = MenuRef.GetMenuScreen(modListMenu);
            ExtraMenu = CreateExtraSettings(MainMenu);

            return MainMenu;
        }

        private static string ResetTimerDescription => $"Click to reset timer back to {RandomTeleport.settings.teleportTime} seconds.";
        private static MenuScreen CreateExtraSettings(MenuScreen modListMenu)
        {
            ExtraMenuRef = new Menu("Extra Settings", new Element[]
            {
                new TextPanel("RNG Seed for new saves:", width: 1000f, Id: "Seed"),
                
                new HorizontalOption("Same Area Teleport",
                    "Randomly teleport you in the same area",
                    new [] { "Yes", "No" },
                    s => RandomTeleport.settings.sameAreaTeleport = s == 0,
                    () => RandomTeleport.settings.sameAreaTeleport ? 0 : 1),

                new HorizontalOption("Only Visited Scenes",
                    "Randomly teleport you in the scenes you have already visited",
                    new [] { "Yes", "No" },
                    s => RandomTeleport.settings.onlyVisitedScenes = s == 0,
                    () => RandomTeleport.settings.onlyVisitedScenes ? 0 : 1),

                new HorizontalOption("Allow Godhome bosses",
                    "Warning: Will lead to soft lock (and need use keybind)",
                    new [] { "Yes", "No" },
                    s => RandomTeleport.settings.AllowGodHomeBosses = s == 0,
                    () => RandomTeleport.settings.AllowGodHomeBosses ? 0 : 1),

                new HorizontalOption("Allow THK and Radiance",
                    "Warning: Will lead to soft lock (and need use keybind)",
                    new [] { "Yes", "No" },
                    s => RandomTeleport.settings.AllowTHK = s == 0,
                    () => RandomTeleport.settings.AllowTHK ? 0 : 1),
                
                new MenuButton("Reset Timer",
                    ResetTimerDescription,
                    _ => RandomTeleport.Instance.ResetTimer(),
                    Id:"resetTimer")
            });


            ExtraMenuRef.OnBuilt += (_, _) =>
            {
                ExtraMenuRef.Find("Seed").gameObject.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
                CreateInputField(ExtraMenuRef.Find("Seed").gameObject);
            };

            return ExtraMenuRef.GetMenuScreen(modListMenu);
        }

        private static void UpdateTriggerSubOptions()
        {
            foreach ((Triggers trigger, List<string> options) in DependantOptions)
            {
                if(RandomTeleport.settings.TriggersState[trigger]) options.ForEach(id => MenuRef.Find(id).Show());
                else options.ForEach(id => MenuRef.Find(id).Hide());
            } 
            MenuRef.Update();
        }

        //yet another instance of rando code
        private static void CreateInputField(GameObject parent)
        {
            UMenuButton backprefab = Object.Instantiate(UIManager.instance.playModeMenuScreen.defaultHighlight
                .FindSelectableOnDown().FindSelectableOnDown().gameObject).GetComponent<UMenuButton>();
            
            GameObject seedGameObject = backprefab.Clone("Seed", UMenuButton.MenuButtonType.Activate, parent).gameObject;
            Object.DestroyImmediate(seedGameObject.GetComponent<UMenuButton>());
            Object.DestroyImmediate(seedGameObject.GetComponent<EventTrigger>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<AutoLocalizeTextUI>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<FixVerticalAlign>());
            Object.DestroyImmediate(seedGameObject.transform.Find("Text").GetComponent<ContentSizeFitter>());

            RectTransform seedRect = seedGameObject.transform.Find("Text").GetComponent<RectTransform>();
            seedRect.anchorMin = seedRect.anchorMax = new Vector2(0.5f, 0.5f);
            seedRect.sizeDelta = new Vector2(337, 63.2f);

            InputField customSeedInput = seedGameObject.AddComponent<InputField>();
            customSeedInput.transform.localPosition = new Vector3(400, -50);
            customSeedInput.textComponent = seedGameObject.transform.Find("Text").GetComponent<Text>();

            customSeedInput.text = RandomTeleport.currentSeed.ToString();

            customSeedInput.caretColor = Color.white;
            customSeedInput.contentType = InputField.ContentType.IntegerNumber;
            customSeedInput.onEndEdit.AddListener(ParseSeedInput);
            customSeedInput.navigation = Navigation.defaultNavigation;
            customSeedInput.caretWidth = 8;
            customSeedInput.characterLimit = 6;

            customSeedInput.colors = new ColorBlock
            {
                highlightedColor = Color.yellow,
                pressedColor = Color.red,
                disabledColor = Color.black,
                normalColor = Color.white,
                colorMultiplier = 2f
            };
        }
        
        private static void ParseSeedInput(string input)
        {
            if (int.TryParse(input, out int newSeed))
            {
                RandomTeleport.currentSeed = newSeed;
            }
            else
            {
                RandomTeleport.Instance.LogWarn($"Seed input \"{input}\" could not be parsed to an integer");
            }
        }
        
        public static UMenuButton Clone(this UMenuButton self, 
            string name, 
            UMenuButton.MenuButtonType type, 
            GameObject parent)
        {
            // Set up duplicate of button
            UMenuButton newBtn = Object.Instantiate(self.gameObject).GetComponent<UMenuButton>();
            newBtn.name = name;
            newBtn.buttonType = type;

            Transform transform = newBtn.transform;
            transform.SetParent(parent.transform);
            transform.localScale = self.transform.localScale;

            // Change text on the button
            Transform textTrans = newBtn.transform.Find("Text");
            Object.Destroy(textTrans.GetComponent<AutoLocalizeTextUI>());
                
            return newBtn;
        }
    }
}