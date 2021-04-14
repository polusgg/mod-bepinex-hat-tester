#define UNITY_STANDALONE_WIN
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using B83.Win32;
using Newtonsoft.Json;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace Cosmetics {
    public class CosmeticEditorBehaviour : MonoBehaviour {
        public CosmeticEditorBehaviour(IntPtr ptr) : base(ptr) {}

        static CosmeticEditorBehaviour() {
            ClassInjector.RegisterTypeInIl2Cpp<CosmeticEditorBehaviour>();
        }
        
        private Rect _windowRect = new(20, 150, 400, 300);
        // private List<HatData> _hats = new();
        private HatData _selectedHat;
        // private HatData _stafaHat;
        private HatType _hatType;
        private bool _hooked;
        private bool _showFront = true;
        private int _runs;
        private bool _showUi;
        private bool _toggled = true;
        private bool _initialHat;
        // private bool dropforstafa = false;
        private HatBehaviour queuedHatBehaviour;
        private PlayerControl queuedPlayerControl;
        // private PlayerControl stafaControl;
        // private bool stafaCheckTutExisted = false;
        // private uint stafaCheckTut = 0;

        private void Start() {
            UnityDragAndDropHook.InstallHook();
            UnityDragAndDropHook.OnDroppedFiles += OnFiles;
            _selectedHat = new HatData(0);
            // _stafaHat = new HatData(55);

            HttpClient client = new();
            try {
                Task<HttpResponseMessage> task = client.GetAsync("https://google.ca");
                task.Wait();
                task.Exception.Log();
                task.Result.Content.ReadAsStringAsync().Result.Log();
            } catch (Exception e) {
                e.Log();
            }
        }

        private void OnDestroy() {
            UnityDragAndDropHook.UninstallHook();
        }

        // void LoadHats() {
        //     foreach (string file in Directory.EnumerateFiles("TestAssets/Hats", "*.json")) {
        //         HatData hatData = new();
        //         Hats.Add(hatData);
        //     }
        // }

        private void FixedUpdate() {
            _showUi = HudManager.InstanceExists && !DestroyableSingleton<HudManager>.Instance.KillOverlay.IsOpen && _toggled;
            if (Input.GetKeyDown(KeyCode.T)) {
                _toggled = !_toggled;
            }

            // if (!stafaCheckTutExisted && TutorialManager.InstanceExists) stafaCheckTut = 5; 
            // stafaCheckTutExisted = TutorialManager.InstanceExists;
            // if (PlayerControl.LocalPlayer && stafaCheckTut-- == 0) {
            //     _initialHat = false;
            //     queuedPlayerControl = stafaControl = (PlayerControl) PlayerControl.AllPlayerControls[(Index) 1];
            //     stafaControl.RpcSetColor(7);
            //     stafaControl.RpcSetHat(55);
            //     stafaControl.RpcSetName("Stafa");
            //     queuedHatBehaviour = HatManager.Instance.GetHatById(55);
            //     
            //     QueueRefresh();
            // }
            if (!_hooked && PlayerControl.LocalPlayer) {
                _hooked = true;
                UnityDragAndDropHook.InstallHook();
                UnityDragAndDropHook.OnDroppedFiles += OnFiles;
            } else if (_hooked && !PlayerControl.LocalPlayer) {
                _hooked = false;
                UnityDragAndDropHook.UninstallHook();
                UnityDragAndDropHook.OnDroppedFiles -= OnFiles;
            }
            RefreshHat();
        }

        void QueueRefresh() => _runs = 2;

        private void OnFiles(List<string> apathnames, POINT point) {
            "Got files LOL".Log();
            if (apathnames.Count == 1) {
                $"Loaded new hat from {apathnames[0]}".Log();
                // if (dropforstafa) {
                    // queuedHatBehaviour = HatManager.Instance.GetHatById(55);
                    // queuedHatBehaviour.FloorImage = LoadImage(apathnames[0]);
                    // queuedPlayerControl = stafaControl;
                // }
                // else {
                    queuedHatBehaviour = _selectedHat.HatBehaviour;
                    _selectedHat[_hatType] = LoadImage(apathnames[0]);
                    queuedPlayerControl = PlayerControl.LocalPlayer;
                // }
                QueueRefresh();
            }
        }

        private void RefreshHat() {
            if (_runs-- > 0) {
                $"Running RefreshHat with {_runs} runs left".Log();
                uint idFromHat = HatManager.Instance.GetIdFromHat(queuedHatBehaviour);
                SaveManager.LastHat = idFromHat;
                // if (CustomPlayerMenu.Instance != null) CustomPlayerMenu.Instance.Tabs[1].Tab.GetComponent<HatsTab>().HatImage.SetHat(idFromHat, PlayerControl.LocalPlayer.Data.ColorId);
                if (queuedPlayerControl) {
                    if (GameData.Instance) {
                        GameData.Instance.UpdateHat(queuedPlayerControl.PlayerId, idFromHat);
                    }

                    queuedPlayerControl.HatRenderer.SetHat(idFromHat, queuedPlayerControl.Data.ColorId);
                    // player.nameText.transform.localPosition = new Vector3(0f, (hatId == 0U) ? 0.7f : 1.05f, -0.5f);
                    queuedPlayerControl.nameText.transform.localPosition = new Vector3(0f, 1.05f, -0.5f);
                }
            }
        }

        private void OnGUI() {
            if (!_showUi) return;
            if (!PlayerControl.LocalPlayer) return;
            _windowRect = GUILayout.Window(0, _windowRect, new Action<int>(WindowFunction), "Hat Testing",
                new Il2CppReferenceArray<GUILayoutOption>(0));
        }

        void WindowFunction(int win) {
            // GUI.DragWindow(_windowRect);
            GUILayout.Label(
                _hatType switch {
                    HatType.Back => "Drop the back image of the hat",
                    HatType.Front => "Drop front image of the hat",
                    HatType.KillAnim => "Drop the image of the hat for the kill animation",
                    HatType.Climbing => "Drop the image of the hat for climbing",
                    _ => throw new ArgumentOutOfRangeException()
                } + " to view it ingame", new Il2CppReferenceArray<GUILayoutOption>(0));
            
            if (GUILayout.Button($"Switch to {(_showFront?"back":"front")} image",new Il2CppReferenceArray<GUILayoutOption>(0))) {
                _showFront = !_showFront;
                _hatType = _showFront ? HatType.Front : HatType.Back;
                _selectedHat.HatBehaviour.InFront = _showFront;
                QueueRefresh();
                // RefreshHat();
            }
            if (GUILayout.Button($"Drop front image",new Il2CppReferenceArray<GUILayoutOption>(0))) {
                _hatType = HatType.Front;
            }
            if (!_showFront && GUILayout.Button($"Drop back image",new Il2CppReferenceArray<GUILayoutOption>(0))) {
                _hatType = HatType.Back;
            }
            if (GUILayout.Button("Drop kill animation image",new Il2CppReferenceArray<GUILayoutOption>(0))) {
                _hatType = HatType.KillAnim;
            }
            if (GUILayout.Button("Drop climbing image",new Il2CppReferenceArray<GUILayoutOption>(0))) {
                _hatType = HatType.Climbing;
            }

            // if (GUILayout.Button($"Mustafa egg death {dropforstafa}", new Il2CppReferenceArray<GUILayoutOption>(0))) {
                // dropforstafa = !dropforstafa;
            // }
            if (GUILayout.Button($"{(_selectedHat.HatBehaviour.NoBounce?"Turn on":"Turn off")} hat bouncing",new Il2CppReferenceArray<GUILayoutOption>(0))) {
                // toggleBounce = !toggleBounce;
                _selectedHat.HatBehaviour.NoBounce = !_selectedHat.HatBehaviour.NoBounce;
                QueueRefresh();
            }

            if (GUILayout.Button("Play any kill animation (skin specific animations)", new Il2CppReferenceArray<GUILayoutOption>(0))) {
                HudManager.Instance.KillOverlay.ShowOne(PlayerControl.LocalPlayer.Data, PlayerControl.LocalPlayer.Data);
            }
            OverlayKillAnimation[] kanims = DestroyableSingleton<HudManager>.Instance.KillOverlay.KillAnims;
            foreach (OverlayKillAnimation overlayKillAnimation in kanims) {
                if (GUILayout.Button($"Play {overlayKillAnimation.name}",new Il2CppReferenceArray<GUILayoutOption>(0))) {
                    DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowOne(overlayKillAnimation, PlayerControl.LocalPlayer.Data,
                        // TutorialManager.InstanceExists ? stafaControl.Data : PlayerControl.LocalPlayer.Data);
                        PlayerControl.LocalPlayer.Data);
                }
            }
            
            GUILayout.Label("Zooming", new Il2CppReferenceArray<GUILayoutOption>(0));
            if (GUILayout.Button("Zoom In", new Il2CppReferenceArray<GUILayoutOption>(0))) {
                var main = Camera.main;
                var orthographicSize = main.orthographicSize;
                if (orthographicSize > 0.5f) orthographicSize -= 0.5f;
                main.orthographicSize = orthographicSize;
                HudManager.Instance.SetHudActive(Math.Abs(orthographicSize - 3) < 0.01f);
                HudManager.Instance.ShadowQuad.gameObject.SetActive(Math.Abs(orthographicSize) <= 3);
            }
            if (GUILayout.Button("Zoom Out", new Il2CppReferenceArray<GUILayoutOption>(0))) {
                var main = Camera.main;
                var orthographicSize = main.orthographicSize;
                orthographicSize += 0.5f;
                main.orthographicSize = orthographicSize;
                HudManager.Instance.SetHudActive(Math.Abs(orthographicSize - 3) < 0.01f);
                HudManager.Instance.ShadowQuad.gameObject.SetActive(Math.Abs(orthographicSize) <= 3);
            }
        }

        Sprite LoadImage(string file) {
            byte[] byteArray = File.ReadAllBytes(file);
            Texture2D tex = new(128, 128, TextureFormat.Alpha8, false);
            if (!tex.LoadImage(byteArray)) {
                "Failed to load hat!".Log();
                return null;
            }
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }

    public enum HatType {
        Front,
        Back,
        KillAnim,
        Climbing
    }

    public class HatData {
        public HatBehaviour HatBehaviour;
        public HatData(uint id) {
            // HatBehaviour = ScriptableObject.CreateInstance<HatBehaviour>();
            HatBehaviour = HatManager.Instance.GetHatById(id);
            HatBehaviour.NotInStore = true;
            HatBehaviour.NoBounce = false;
            HatBehaviour.InFront = true;
            // JsonSerializer serializer = JsonSerializer.Create();
            // serializer.Deserialize<HatData>(new JsonTextReader(File.OpenText("TestAssetOutput/Hats")));
        }

        public void Serialize() {
            if (!Directory.Exists("TestAssetOutput")) Directory.CreateDirectory("TestAssetOutput");
            if (!Directory.Exists("TestAssetOutput/Hats")) Directory.CreateDirectory("TestAssetOutput/Hats");
            
            JsonSerializer serializer = JsonSerializer.Create();
            serializer.Serialize(File.CreateText("TestAssetOutput/Hats"), this);
        }

        public Sprite this[HatType type] {
            get => type switch {
                HatType.Front => HatBehaviour.MainImage,
                HatType.Back => HatBehaviour.BackImage,
                HatType.KillAnim => HatBehaviour.FloorImage,
                HatType.Climbing => HatBehaviour.ClimbImage,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
            set {
                switch (type) {
                    case HatType.Front:
                        HatBehaviour.MainImage = value;
                        break;
                    case HatType.Back:
                        HatBehaviour.BackImage = value;
                        break;
                    case HatType.KillAnim:
                        HatBehaviour.FloorImage = value;
                        break;
                    case HatType.Climbing:
                        HatBehaviour.ClimbImage = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
        }
    }
}