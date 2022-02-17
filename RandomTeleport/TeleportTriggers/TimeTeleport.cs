using GlobalEnums;
using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MagicUI.Core;
using MagicUI.Elements;
using UnityEngine;
using UnityEngine.UI;

namespace RandomTeleport.TeleportTriggers
{
    class TimeTeleport : TeleportTrigger
    {
        public float timer = 0f;
        private tk2dSpriteAnimator HCanimator;
        private LayoutRoot layout;
        private TextObject displayTimer;

        public void Awake()
        {
            //add a display timer
            if (layout == null)
            {
                layout = new(true, false, "RandomGravityChange Display Timer");

                displayTimer = new TextObject(layout)
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Top,
                    FontSize = 40,
                    Font = UI.TrajanBold,
                    Text = ""
                };
            }
        }

        protected override void Enable()
        {
            displayTimer.Visibility = Visibility.Visible;
            ModHooks.AfterTakeDamageHook += HeroDamaged;
            On.GeoCounter.AddGeo += GeoGained;
            On.QuitToMenu.Start += ResetTimer;
        }
        protected override void Disable()
        {
            displayTimer.Visibility = Visibility.Hidden;
            ModHooks.AfterTakeDamageHook -= HeroDamaged;
            On.GeoCounter.AddGeo -= GeoGained;
            On.QuitToMenu.Start -= ResetTimer;
        }

        private IEnumerator ResetTimer(On.QuitToMenu.orig_Start orig, QuitToMenu self)
        {
            timer = 0f;
            yield return orig(self);
        }

        private void GeoGained(On.GeoCounter.orig_AddGeo orig, GeoCounter self, int geo)
        {
            orig(self, geo);
            timer += RandomTeleport.settings.timeLostFromGeo;
        }

        private int HeroDamaged(int hazardType, int damageAmount)
        {
            if (damageAmount > 0)
            {
                timer += RandomTeleport.settings.timeLostFromHit;
            }
            
            return damageAmount;
        }

        private static readonly List<string> NoTeleportAnimations = new List<string>()
        {
            "Collect",
            "Roar Lock",
            "Stun",
            "Super Hard Land",
        };

        //to prevent funny storages
        private bool CurrentAnimationisNonTeleportAnim()
        {
            bool contains = false;
            if (!HeroController.instance) return false;
            HCanimator ??= HeroController.instance.gameObject.GetComponent<tk2dSpriteAnimator>();
            string currentClip = HCanimator.CurrentClip.name;
            NoTeleportAnimations.ForEach(anim =>
            {
                if (currentClip.Contains(anim)) contains = true;
            });

            return contains;
        }

        public void Update()
        {
            HandleTimer();
            HandleTimerDisplay();
        }

        private void HandleTimer()
        {
            if (!this.IsEnabled) return;
            //conditions for not incrementing timer
            if (TimerShouldBePaused()) return;

            timer += Time.deltaTime;
            if (timer > RandomTeleport.settings.teleportTime)
            {
                timer = 0f;

                RandomTeleport.Instance.Teleport();
            }
        }
        private void HandleTimerDisplay()
        {
            //conditions for not showing timer
            if (!this.IsEnabled ||
                GameManager.instance.GetSceneNameString() == "Menu_Title" ||
                GameManager.instance.IsNonGameplayScene() ||
                !RandomTeleport.settings.TriggersState[Triggers.Time] ||
                !RandomTeleport.settings.showTimer)
            {
                displayTimer.Visibility = Visibility.Hidden;
            }
            else
            {
                displayTimer.Visibility = Visibility.Visible;
                displayTimer.Text =
                    $"Time remaining: {((int)(RandomTeleport.settings.teleportTime - timer) / 60).ToString()}:{((int)(RandomTeleport.settings.teleportTime - timer) % 60).ToString("00")}";

            }
        }

        private static bool lookForTeleporting;
        private static GameState lastGameState;
        private static readonly FieldInfo cameraControlTeleporting = typeof(CameraController).GetField("teleporting", BindingFlags.NonPublic | BindingFlags.Instance);
        
        //timer mod code for removing timer in loads
        private bool TimerShouldBePaused()
            {
            if(GameManager.instance == null) 
            {
                lookForTeleporting = false;
                lastGameState = GameState.INACTIVE;
                return false;
            }

            var nextScene = GameManager.instance.nextSceneName;
            var sceneName = GameManager.instance.sceneName;
            var uiState = GameManager.instance.ui.uiState;
            var gameState = GameManager.instance.gameState;

            bool loadingMenu = (string.IsNullOrEmpty(nextScene) && sceneName != "Menu_Title") || (nextScene == "Menu_Title" && sceneName != "Menu_Title");
            if(gameState == GameState.PLAYING && lastGameState == GameState.MAIN_MENU)
            {
                lookForTeleporting = true;
            }
            bool teleporting = (bool) cameraControlTeleporting.GetValue(GameManager.instance.cameraCtrl);
            if(lookForTeleporting && (teleporting || (gameState != GameState.PLAYING && gameState != GameState.ENTERING_LEVEL))) 
            {
                lookForTeleporting = false;
            }

            var shouldPause =
                (
                    gameState == GameState.PLAYING
                    && teleporting
                    && !(GameManager.instance.hero_ctrl == null ? false : GameManager.instance.hero_ctrl.cState.hazardRespawning)
                )
                || lookForTeleporting
                || ((gameState == GameState.PLAYING || gameState == GameState.ENTERING_LEVEL) &&
                    uiState != UIState.PLAYING)
                || (gameState != GameState.PLAYING && !GameManager.instance.inputHandler.acceptingInput)
                || gameState == GameState.EXITING_LEVEL
                || gameState == GameState.LOADING
                || gameState == GameState.PAUSED
                || gameState == GameState.MAIN_MENU
                || (GameManager.instance.hero_ctrl == null ? false : GameManager.instance.hero_ctrl.transitionState == HeroTransitionState.WAITING_TO_ENTER_LEVEL)
                || (
                    uiState != UIState.PLAYING
                    && (uiState != UIState.PAUSED || loadingMenu)
                    && (!string.IsNullOrEmpty(nextScene) || sceneName == "_test_charms" || loadingMenu)
                    && nextScene != sceneName
                )
                || GameManager.instance.IsNonGameplayScene()
                || PlayerData.instance.atBench
                || CurrentAnimationisNonTeleportAnim(); //to not punish people changing charms and stuff


            lastGameState = gameState;

            return shouldPause;
        }
    }
}
