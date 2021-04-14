using System;
using System.Net.Http;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;

namespace Cosmetics {
    [BepInPlugin(Id)]
    public class CosmeticFrameworkPlugin : BasePlugin {
        public const string Id = "ca.sanae.cosmetics";
        public static ManualLogSource Logger;
        public static Harmony Harmony;

        public override void Load() {
            Logger = Log;
            Harmony = new Harmony(Id);
            Harmony.PatchAll();
        }
    }
}