using HarmonyLib;

namespace Cosmetics {
    public class AccountManagementPatches {
        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
        public class DisableStupidBansPatch {
            [HarmonyPrefix]
            private static bool AmBannedGetter(out bool __result) {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(AuthManager._CoWaitForNonce_d__5), nameof(AuthManager._CoWaitForNonce_d__5.MoveNext))]
        public class DisableStupidNoncesPatch {
            [HarmonyPrefix]
            private static bool StupidNonce(AuthManager._CoWaitForNonce_d__5 __instance, out bool __result) {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(AuthManager._CoConnect_d__4), nameof(AuthManager._CoConnect_d__4.MoveNext))]
        public class DisableStupidConnectsPatch {
            [HarmonyPrefix]
            private static bool StupidConnects(out bool __result) {
                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(EOSManager), nameof(EOSManager.PLMCCAHGEHD))]
        public static class EosManagerInitializePlatformInterfacePatch {
            [HarmonyPrefix]
            public static bool Prefix(EOSManager __instance) {
                __instance.HasSignedIn();
                __instance.ageOfConsent = 0;
                __instance.loginFlowFinished = true;
                __instance.platformInitialized = true;
                __instance.gameObject.SetActive(false);
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.BirthDateYear), MethodType.Getter)]
        public static class SaveManagerGetBirthDateYearPatch {
            [HarmonyPrefix]
            public static bool Prefix(out int __result) {
                __result = 1990;
                return false;
            }
        }
    }
}