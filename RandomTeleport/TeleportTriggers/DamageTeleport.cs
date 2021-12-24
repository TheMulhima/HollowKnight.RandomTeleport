using Modding;
using UnityEngine;

namespace RandomTeleport.TeleportTriggers
{
    public class DamageTeleport:MonoBehaviour
    {
        public void Awake()
        {
            ModHooks.AfterTakeDamageHook += HeroDamaged;
        }

        private int HeroDamaged(int hazardtype, int damageamount)
        {
            if (!RandomTeleport.enabled) return damageamount;
            
            if (RandomTeleport.settings.teleportTrigger == Triggers.Damage)
            {
                if (damageamount > 0) RandomTeleport.Instance.Teleport();
            }

            return damageamount;
        }
    }
}