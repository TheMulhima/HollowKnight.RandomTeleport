
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InControl;
using Newtonsoft.Json;
using Modding.Converters;

namespace RandomTeleport
{
    public enum Triggers
    {
        Time = 0,
        Damage,
    }
    public class GlobalSettings
    {
        public float teleportTime_minutes = 2f;
        public Triggers teleportTrigger = Triggers.Time;
        public bool showTimer = true;
        public bool sameAreaTeleport = false;
        public bool onlyVisitedScenes = false;
        public float timeGainFromHit = 0f;
        public float timeLostFromGeo = 0f;

        [JsonConverter(typeof(PlayerActionSetConverter))]
        public KeyBinds keybinds = new KeyBinds();
    }
    public class KeyBinds : PlayerActionSet
    {
        public PlayerAction keyRandomTeleport;
        public PlayerAction buttonRandomTeleport;

        public KeyBinds()
        {
            keyRandomTeleport = CreatePlayerAction("keyRandomTeleport");
            buttonRandomTeleport = CreatePlayerAction("buttonRandomTeleport");
        }

        public bool wasPressed()
        {
            return keyRandomTeleport.WasPressed || buttonRandomTeleport.WasPressed;
        }
    }
}
