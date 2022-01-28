using System;
using System.Linq;
using Modding;
using UnityEngine;

namespace RandomTeleport.TeleportTriggers
{
    public class DamageTeleport:TeleportTrigger
    {
        protected override void Enable()
        {
            ModHooks.AfterTakeDamageHook += HeroDamaged;
        }

        protected override void Disable()
        {
            ModHooks.AfterTakeDamageHook -= HeroDamaged;
        }

        private int HeroDamaged(int hazardtype, int damageamount)
        {
            int chance = UnityEngine.Random.Range(0, 100);
            if (damageamount > 0)
            {
                if ( chance <= RandomTeleport.settings.chanceOfTeleportOnDamage)
                {
                    RandomTeleport.Instance.Teleport();
                }
            }

            return damageamount;
        }
    }
}