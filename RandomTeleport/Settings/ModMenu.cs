using System;
using System.Collections.Generic;
using System.Linq;
using Modding;
using Satchel.BetterMenus;

namespace RandomTeleport
{
    public static class ModMenu
    {
        private static Menu MenuRef;
        private static readonly Dictionary<Triggers, List<string>> DependantOptions = new Dictionary<Triggers, List<string>>()
        {
            {Triggers.Time, new List<string> { "showTimer", "teleportTime", "damageTimeIncrease", "geoTimeIncrease" }},
            {Triggers.Damage, new List<string>() },
        };

        public static MenuScreen CreateMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            MenuRef = new Menu("Random Teleport", new Satchel.BetterMenus.Element[]
            {
                Blueprints.CreateToggle(toggleDelegates.GetValueOrDefault(), "Mod Toggle", ""),

                new HorizontalOption("Teleport Trigger",
                    "Choose which trigger should cause a random teleport",
                    Enum.GetNames(typeof(Triggers)),
                    s =>
                    {
                        RandomTeleport.settings.teleportTrigger = (Triggers) s;

                        foreach ((Triggers trigger, List<string>options) in DependantOptions)
                        {
                            if (trigger == (Triggers)s) options.ForEach(option => MenuRef.Find(option).Show());
                            else  options.ForEach(option => MenuRef.Find(option).Hide());
                        } 
                    },
                    () => (int)RandomTeleport.settings.teleportTrigger){
                },

               new HorizontalOption("Show Timer", "",
                    new string[] {"Yes", "No"},
                    s => RandomTeleport.settings.showTimer = s == 0,
                    () => RandomTeleport.settings.showTimer ? 0:1, Id: "showTimer"),

                new HorizontalOption("Random Teleport Time",
                    "Time between teleports in minutes",
                    Enumerable.Range(1, 40).Select(x => (x/4f).ToString()).ToArray(),
                    s => RandomTeleport.settings.teleportTime_minutes = (s + 1)/4f,
                    () => (int)(RandomTeleport.settings.teleportTime_minutes * 4) - 1, Id: "teleportTime"),

                new HorizontalOption("Time increase from damage",
                    "When you take damage how much of the time in the timer is increased",
                    Enumerable.Range(-20, 41).Select(x => (x/2f).ToString()).ToArray(),
                    s => RandomTeleport.settings.timeGainFromHit = (s - 20)/2f,
                    () => ((int)(RandomTeleport.settings.timeGainFromHit * 2) + 20), Id: "damageTimeIncrease"),

                new HorizontalOption("Time reduction from geo",
                    "When you collect geo how much of the time in the timer is reduced",
                    Enumerable.Range(-20, 41).Select(x => (x/2f).ToString()).ToArray(),
                    s => RandomTeleport.settings.timeLostFromGeo = (s - 20)/2f,
                    () => ((int)(RandomTeleport.settings.timeLostFromGeo * 2) + 20), Id: "geoTimeIncrease"),

                new HorizontalOption("Same Area Teleport",
                "Randomly teleport you in the same area",
                new string[] { "Yes", "No"},
                s => RandomTeleport.settings.sameAreaTeleport = s == 0,
                () => RandomTeleport.settings.sameAreaTeleport ? 0:1),

                new HorizontalOption("Only Visited Scenes",
                "Randomly teleport you in the scenes you have already visited",
                new string[] {"Yes", "No"},
                s => RandomTeleport.settings.onlyVisitedScenes = s == 0,
                () => RandomTeleport.settings.onlyVisitedScenes ? 0 : 1),

                new TextPanel("It is recommended to keep a keybind just incase you get infinite transistions"),
                Blueprints.KeyAndButtonBind("Change Scene",
                RandomTeleport.settings.keybinds.keyRandomTeleport, RandomTeleport.settings.keybinds.buttonRandomTeleport,
                Id:"changeSceneKey")

            }) ;

            return MenuRef.GetMenuScreen(modListMenu);
        }
    }
}