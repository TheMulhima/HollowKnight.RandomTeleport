using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using Satchel.BetterMenus;
using UnityEngine;
using UnityEngine.Events;
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
        
        private static readonly string[] TimeOptions = new string[]
        {
            "30s", "1m", "1m 30s", "2m", "3m", "5m", "10m", "Custom"
        };
        
        private static readonly string[] TimeReductionOptions = new string[]
        {
            "5s", "10s", "30s", "1m", "2m", "5m"
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
                    "Time between teleports.",
                    //generate list from 20 to 900 with 20 step increments
                    TimeOptions,
                    s =>
                    {
                        RandomTeleport.settings.teleportTime = s switch
                        {
                            //"30s", "1m", "1m 30s", "2m", "3m", "5m", "10m", "Custom"
                            0 => 30,
                            1 => 60,
                            2 => 90,
                            3 => 120,
                            4 => 180,
                            5 => 300,
                            6 => 600,
                            7 => RandomTeleport.settings.customTime,
                            _ => int.MaxValue,
                        };

                        RandomTeleport.settings.chosenCustomTime = s == 7;
                        if (RandomTeleport.settings.chosenCustomTime)
                        {
                            MenuRef.Find("CustomTime").Show();
                        }
                        else
                        {
                            MenuRef.Find("CustomTime").Hide();
                        }
                        
                        RandomTeleport.Instance.ResetTimer();
                    },
                    () =>
                    {
                        if (RandomTeleport.settings.chosenCustomTime) return 7;
                        return RandomTeleport.settings.teleportTime switch
                        {
                            //"30s", "1m", "1m 30s", "2m", "3m", "5m", "10m", "Custom"
                            30 => 0,
                            60 => 1,
                            90 => 2,
                            120 => 3,
                            180 => 4,
                            300 => 5,
                            600 => 6,
                            _ => 7,
                        };
                    }, Id: "teleportTime")
                    {isVisible = RandomTeleport.settings.TriggersState[Triggers.Time]},
               
               Blueprints.IntInputField("Custom time (seconds)", 
                   s => RandomTeleport.settings.customTime = s,
                   () => RandomTeleport.settings.customTime,
                   _placeholder: "Write time here", Id:"CustomTime"),

               new HorizontalOption("Time reduction from damage",
                    "How much time is removed from the timer when the player takes damage",
                    TimeReductionOptions,
                    s =>
                    {
                        RandomTeleport.settings.timeLostFromHit = s switch
                        {
                            //"5s", "10s", "30s", "1m", "2m", "5m"
                            0 => 5,
                            1 => 10,
                            2 => 30,
                            3 => 60,
                            4 => 120,
                            5 => 300,
                            _ => 10
                        };
                    },
                    () =>
                    {
                        return RandomTeleport.settings.timeLostFromHit switch
                        {
                            //"30s", "10s", "30s", "1m", "2m", "5m"
                            5 => 0,
                            10 => 1,
                            30 => 2,
                            60 => 3,
                            120 => 4,
                            300 => 5,
                            _ => 0,
                        };
                    }, Id: "damageTimeIncrease")
                    {isVisible = RandomTeleport.settings.TriggersState[Triggers.Time]},

                new HorizontalOption("Time increase from geo",
                    "How much time is gained from the timer when the player collects geo.",
                    TimeReductionOptions,
                    s =>
                    {
                        RandomTeleport.settings.timeGainFromGeo = s switch
                        {
                            //"5s", "10s", "30s", "1m", "2m", "5m"
                            0 => 5,
                            1 => 10,
                            2 => 30,
                            3 => 60,
                            4 => 120,
                            5 => 300,
                            _ => 10,
                        };
                    },
                    () =>
                    {
                        return RandomTeleport.settings.timeLostFromHit switch
                        {
                            //"30s", "10s", "30s", "1m", "2m", "5m"
                            5 => 0,
                            10 => 1,
                            30 => 2,
                            60 => 3,
                            120 => 4,
                            300 => 5,
                            _ => 0,
                        };
                    }, Id: "geoTimeIncrease")
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
            
            MenuRef.Find("CustomTime").isVisible = RandomTeleport.settings.TriggersState[Triggers.Time] && RandomTeleport.settings.chosenCustomTime;

            MainMenu = MenuRef.GetMenuScreen(modListMenu);
            ExtraMenu = CreateExtraSettings(MainMenu);

            return MainMenu;
        }

        private static string ResetTimerDescription => $"Click to reset timer back to {RandomTeleport.settings.teleportTime} seconds.";
        private static MenuScreen CreateExtraSettings(MenuScreen modListMenu)
        {
            ExtraMenuRef = new Menu("Extra Settings", new Element[]
            {
                Blueprints.IntInputField("RNG Seed for new saves", 
                    s => RandomTeleport.saveSettings.seed = s,
                    () => RandomTeleport.saveSettings.seed,
                    _placeholder: "Write seed here",
                    _characterLimit: 9),

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
                
                new HorizontalOption("Only Spawn in Transitions",
                    "",
                    new [] { "Yes", "No" },
                    s => RandomTeleport.settings.OnlySpawnInTransitions = s == 0,
                    () => RandomTeleport.settings.OnlySpawnInTransitions ? 0 : 1),
                
                new MenuButton("Reset Timer",
                    ResetTimerDescription,
                    _ => RandomTeleport.Instance.ResetTimer(),
                    Id:"resetTimer")
            });

            return ExtraMenuRef.GetMenuScreen(modListMenu);
        }

        private static void UpdateTriggerSubOptions()
        {
            foreach ((Triggers trigger, List<string> options) in DependantOptions)
            {
                if(RandomTeleport.settings.TriggersState[trigger]) options.ForEach(id => MenuRef.Find(id).Show());
                else options.ForEach(id => MenuRef.Find(id).Hide());
            }

            if (RandomTeleport.settings.TriggersState[Triggers.Time] && RandomTeleport.settings.chosenCustomTime)
            {
                MenuRef.Find("CustomTime").Show();
            }
            else
            {
                MenuRef.Find("CustomTime").Hide();
            }
            MenuRef.Update();
        }
    }
}