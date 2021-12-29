using Modding;
using RandomTeleport1_4.TeleportTriggers;
using UnityEngine;
using UnityEngine.UI;

namespace RandomTeleport1_4
{
    public class DisplayTimer:MonoBehaviour
    {
        public CanvasText displayTimer;
        private TimeTeleport timeTeleport;

        public void Awake()
        {
            GameObject canvas = new GameObject("Display Timer Canvas");
            canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
            DontDestroyOnLoad(canvas);

            displayTimer = new CanvasText(canvas, new Vector2(Screen.width - 600f, 25), new Vector2(600, 50), CanvasUtil.TrajanBold, "", 40, alignment: TextAnchor.MiddleCenter);
        }

        public void Update()
        {
            if (!RandomTeleport1_4.enabled)
            {
                displayTimer.SetActive(false);
                return;
            }
            timeTeleport ??= gameObject.GetComponent<TimeTeleport>();

            //conditions for not showing timer
            if (!RandomTeleport1_4.enabled ||
                GameManager.instance.GetSceneNameString() == "Menu_Title" ||
                GameManager.instance.IsNonGameplayScene() ||
                RandomTeleport1_4.Instance.settings.teleportTrigger != Triggers.Time ||
                !RandomTeleport1_4.Instance.settings.showTimer)
            {
                displayTimer.SetActive(false);
                return;
            }

            displayTimer.SetActive(true);
            float transitionTime = RandomTeleport1_4.Instance.settings.teleportTime_minutes * 60f;

            displayTimer.UpdateText(
                $"Time remaining: {((int)(transitionTime - timeTeleport.timer) / 60).ToString()}:{((int)(transitionTime - timeTeleport.timer) % 60).ToString("00")}");
        }
    }
}