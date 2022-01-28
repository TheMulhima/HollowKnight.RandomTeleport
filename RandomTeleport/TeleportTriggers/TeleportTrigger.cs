using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RandomTeleport.TeleportTriggers
{
    public abstract class TeleportTrigger : MonoBehaviour
    {
        public void Load()
        {
            if (IsEnabled) return; 

            isenabled = true;
            Enable();
        }

        public void Unload()
        {
            if (!isenabled) return;
            
            isenabled = false;
            Disable();
        }

        private bool isenabled = false;

        public bool IsEnabled => isenabled;


        protected abstract void Enable();
        protected abstract void Disable();

        public void Log(object s) => RandomTeleport.Instance.Log(s);
        public void LogDebug(object s) => RandomTeleport.Instance.LogDebug(s);
        
        public void LogError(object s) => RandomTeleport.Instance.LogError(s);
    }
}
