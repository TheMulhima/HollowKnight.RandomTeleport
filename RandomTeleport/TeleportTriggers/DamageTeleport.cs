using Modding;
using UnityEngine;

namespace RandomTeleport1_4.TeleportTriggers
{
    public class DamageTeleport:MonoBehaviour
    {
        public void Awake()
        {
            ModHooks.Instance.AfterTakeDamageHook += HeroDamaged;
        }

        private int HeroDamaged(int hazardtype, int damageamount)
        {
            if (!RandomTeleport1_4.enabled) return damageamount;
            
            if (RandomTeleport1_4.Instance.settings.teleportTrigger == Triggers.Damage)
            {
                if (damageamount > 0) RandomTeleport1_4.Instance.Teleport();
            }

            return damageamount;
        }
    }
}