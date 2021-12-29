using System;
using UnityEngine;
using Random = System.Random;

namespace RandomTeleport1_4.TeleportTriggers
{
    public class KeyPressTeleport:MonoBehaviour
    {
        public void Update()
        {
            if (!RandomTeleport1_4.enabled) return;
            if (RandomTeleport1_4.Instance.settings.keyRandomTeleport == "") return;

            bool input = false;
            
            try
            {
                KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode),RandomTeleport1_4.Instance.settings.keyRandomTeleport);
                
                //this wont fail cuz if it would fail it would've failed in the parse 
                input = Input.GetKeyDown(keyCode);
            }
            catch (Exception e1)
            {
                try
                {
                    input = Input.GetKeyDown(RandomTeleport1_4.Instance.settings.keyRandomTeleport);
                }
                catch (Exception e)
                {
                    RandomTeleport1_4.Instance.settings.keyRandomTeleport = "";
                    RandomTeleport1_4.Instance.LogError("Key for keyRandomTeleport is incorrect");
                }
            }
            
            
            if (input)
            {
                RandomTeleport1_4.Instance.Teleport();
            }
        }
    }
}