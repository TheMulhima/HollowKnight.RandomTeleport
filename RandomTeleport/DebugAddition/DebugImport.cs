using System;
using MonoMod.ModInterop;

// Replace the namespace with your project's root namespace
namespace RandomTeleport
{
    internal static class DebugMod
    {
        [ModImportName("DebugMod")]
        private static class DebugImport
        {
            public static Action<Action, string, string> AddActionToKeyBindList = null;
            public static Action<string> LogToConsole = null;
        }
        static DebugMod()
        {
            // MonoMod will automatically fill in the actions in DebugImport the first time they're used
            typeof(DebugImport).ModInterop();
        }

        public static void AddActionToKeyBindList(Action method, string name, string category) => DebugImport.AddActionToKeyBindList?.Invoke(method, name, category);

        public static void LogToConsole(string msg) => DebugImport.LogToConsole?.Invoke(msg);
    }
}