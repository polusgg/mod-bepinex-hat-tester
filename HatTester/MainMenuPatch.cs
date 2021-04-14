// #define FORCE_AUTOSTART
using System;
using HarmonyLib;
using InnerNet;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cosmetics {
#if FORCE_AUTOSTART
    // [HarmonyPatch(typeof(SplashLogoAnimator), nameof(SplashLogoAnimator.Start))]
    public static class SplashScreenPatch {
        private static bool started;

        private static GameModes GameMode = GameModes.LocalGame;
        private static MatchMakerModes MatchMakerMode = MatchMakerModes.HostAndClient;
        private static int TutorialMapId = 2;
        private static int GameCode = 32;
        private static string Ip = "127.0.0.1";
        private static ushort Port = 22023;

        [HarmonyPrefix]
        public static bool Start() {
            if (started && GameMode != GameModes.OnlineGame) {
                Application.Quit();
                return false;
            }

            started = true;

            AmongUsClient.Instance.TutorialMapId = TutorialMapId;
            AmongUsClient.Instance.GameMode = GameMode;
            if (GameMode != GameModes.OnlineGame) DestroyableSingleton<InnerNetServer>.Instance.StartAsServer();
            AmongUsClient.Instance.SetEndpoint(Ip, Port);
            AmongUsClient.Instance.GameId = GameCode;
            AmongUsClient.Instance.MainMenuScene = "MainMenu";
            AmongUsClient.Instance.OnlineScene = GameMode == GameModes.FreePlay ? "Tutorial" : "OnlineGame";
            AmongUsClient.Instance.Connect(MatchMakerMode);
            
            return true;
        }
    }
#endif
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuPatch {
        private static bool _started;
        [HarmonyPrefix]
        public static bool Start() {
#if FORCE_AUTOSTART
            return SplashScreenPatch.Start()
#else
            if (_started) return true;
            _started = true;
            
            // your code goes below
            try {
                GameObject gameObject = new("Lol") {hideFlags = HideFlags.HideAndDontSave};
                gameObject.AddComponent<CosmeticEditorBehaviour>();
                Object.DontDestroyOnLoad(gameObject);
            } catch (Exception e) {
                System.Console.WriteLine(e);
                throw;
            }

            return true;
#endif
        }
    }
}