using GlobalEnums;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace RandomTeleport1_4.TeleportTriggers
{
    class TimeTeleport : MonoBehaviour
    {
        public float timer = 0f;

        public void Awake()
        {
            //add a display timer
            gameObject.AddComponent<DisplayTimer>();
            ModHooks.Instance.AfterTakeDamageHook += HeroDamaged;
            On.GeoCounter.AddGeo += GeoGained;
            On.QuitToMenu.Start += ResetTimer;
        }

        private IEnumerator ResetTimer(On.QuitToMenu.orig_Start orig, QuitToMenu self)
        {
            timer = 0f;
            yield return orig(self);
        }

        private void GeoGained(On.GeoCounter.orig_AddGeo orig, GeoCounter self, int geo)
        {
            orig(self, geo);
            if (!RandomTeleport1_4.enabled) return;
            if (RandomTeleport1_4.Instance.settings.teleportTrigger == Triggers.Time) timer -= RandomTeleport1_4.Instance.settings.timeLostFromGeo;
        }

        private int HeroDamaged(int hazardType, int damageAmount)
        {
            if (!RandomTeleport1_4.enabled) return damageAmount;
            if (RandomTeleport1_4.Instance.settings.teleportTrigger == Triggers.Time)
            {
                if (damageAmount > 0)
                {
                    timer += RandomTeleport1_4.Instance.settings.timeGainFromHit;
                }
            }

            return damageAmount;
        }

        public void Update()
        {
            if (!RandomTeleport1_4.enabled) return;

            if (RandomTeleport1_4.Instance.settings.teleportTrigger == Triggers.Time)
            {
                //conditions for not incrementing timer
                if (GameManager.instance.GetSceneNameString() == "Menu_Title" ||
                    GameManager.instance.IsNonGameplayScene() ||
                    PlayerData.instance.atBench ||
                    HeroController.instance.cState.hazardRespawning ||
                    GameManager.instance.isPaused) return;
                
                float transitionTime = RandomTeleport1_4.Instance.settings.teleportTime_minutes * 60f;
                timer += Time.deltaTime;
                if (timer > transitionTime)
                {
                    timer = 0f;

                    RandomTeleport1_4.Instance.Teleport();
                }
            }
        }
    }
}
