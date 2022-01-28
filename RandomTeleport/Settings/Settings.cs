
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InControl;
using Newtonsoft.Json;
using Modding.Converters;
using RandomTeleport.Utils;

namespace RandomTeleport
{
    public enum Triggers
    {
        Time = 0,
        Damage,
        KeyPress,
    }
    public class GlobalSettings
    {
        public int teleportTime = 120;
        public Dictionary<Triggers,bool> TriggersState = new() 
        { 
            {Triggers.Time, true},
            { Triggers.Damage , false},
            {Triggers.KeyPress,true}
        };
        
        //Time
        public bool showTimer = true;
        public int timeLostFromHit = 0;
        public int timeLostFromGeo = 0;
        
        //Damage
        public int chanceOfTeleportOnDamage = 100;
        
        //extra
        public bool sameAreaTeleport = false;
        public bool onlyVisitedScenes = false;
        public bool AllowGodHomeBosses = false;
        public bool AllowTHK = false;

        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds keybinds = new KeyBinds();
    }
    public class KeyBinds : PlayerActionSet
    {
        public PlayerAction keyRandomTeleport;
        public PlayerAction buttonRandomTeleport;
        public PlayerAction keyPreviousTeleport;
        public PlayerAction buttonPreviousTeleport;

        public KeyBinds()
        {
            keyRandomTeleport = CreatePlayerAction("keyRandomTeleport");
            buttonRandomTeleport = CreatePlayerAction("buttonRandomTeleport");
            keyPreviousTeleport = CreatePlayerAction("keyPreviousTeleport");
            buttonPreviousTeleport = CreatePlayerAction("buttonPreviousTeleport");
        }

        public bool RandomTeleportwasPressed()
        {
            return keyRandomTeleport.WasPressed || buttonRandomTeleport.WasPressed;
        }
        public bool PreviousTeleportwasPressed()
        {
            return keyPreviousTeleport.WasPressed || buttonPreviousTeleport.WasPressed;
        }
    }

    public class SaveSettings
    {
        [JsonConverter(typeof(RandomConverter))]
        public Random RNG = new Random();

        //doesnt really have any use other than for recreating same seed again
        public int seed;

    }
}
