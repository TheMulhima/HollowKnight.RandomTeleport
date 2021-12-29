
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InControl;
using Modding;
using Newtonsoft.Json;
using Modding.Converters;

namespace RandomTeleport1_4
{
    public enum Triggers
    {
        Time = 0,
        Damage,
    }
    public class GlobalSettings: ModSettings
    {
        public float teleportTime_minutes = 2f;
        public Triggers teleportTrigger = Triggers.Time;
        public bool showTimer = true;
        public bool sameAreaTeleport = false;
        public bool onlyVisitedScenes = false;
        public float timeGainFromHit = 0f;
        public float timeLostFromGeo = 0f;
        public bool AllowGodHomeBosses = false;
        public bool AllowTHK = false;

        public string keyRandomTeleport = "";
    }
}
