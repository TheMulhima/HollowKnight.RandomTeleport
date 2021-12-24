using UnityEngine;

namespace RandomTeleport.TeleportTriggers
{
    public class KeyPressTeleport:MonoBehaviour
    {
        public void Update()
        {
            if (!RandomTeleport.enabled) return;

            if (RandomTeleport.settings.keybinds.wasPressed())
            {
                RandomTeleport.Instance.Teleport();
            }
        }
    }
}