using System.Linq;
using InControl;
using UnityEngine;
using Logger = Modding.Logger;

namespace RandomTeleport.TeleportTriggers
{
    public class KeyPressTeleport:TeleportTrigger
    {
        public void Update()
        {
            if (!this.IsEnabled) return;
            if (RandomTeleport.settings.keybinds.RandomTeleportwasPressed())
            {
                RandomTeleport.Instance.Teleport();
            }
            if (RandomTeleport.settings.keybinds.PreviousTeleportwasPressed())
            {
                RandomTeleport.Instance.TeleportToPrevious();
            }
        }

        protected override void Enable()
        {}

        protected override void Disable()
        {}
    }
}