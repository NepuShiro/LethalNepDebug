using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using BepInEx.Logging;
using Logger = BepInEx.Logging.Logger;

namespace LethalNepDebug
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Dictionary<string, bool> toggles = new();

        private Harmony harmonymain;
        public static ManualLogSource Log;

        public static bool QuotaToggledOnce = false;
        public static bool VersionToggledOnce;
        public static int Version;

        private Rect _windowRect = new(0, 0, 190, 200);
        private bool _windowOpen = false;

        private void Awake()
        {
            // Hardcoded Version because funny game doesn't initialize this on startup.
            Version = 40;
            Log = Logger;

            InitializeToggles();

            harmonymain = new Harmony(PluginInfo.PLUGIN_GUID);
            harmonymain.PatchAll();

            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} has loaded!");
        }

        private void InitializeToggles()
        {
            ToggleManager.AddToggle("NoKill", "F1");
            ToggleManager.AddToggle("NoHurty", "F2");
            ToggleManager.AddToggle("Speed", "F3");
            ToggleManager.AddToggle("Heavy", "F4");
            ToggleManager.AddToggle("Charge", "F5");
            ToggleManager.AddToggle("Quota", "F6");
            ToggleManager.AddToggle("VersionSpoof", "F10");
            ToggleManager.AddToggle("Debug", "F12");

            ToggleManager.EnableToggleInputActions();

            Logger.LogInfo($"{PluginInfo.PLUGIN_GUID} toggles Initialized");
        }

        private void Update()
        {
            if (FindObjectOfType<QuickMenuManager>())
            {
                _windowOpen = FindObjectOfType<QuickMenuManager>().isMenuOpen;
            }

            if (GameNetworkManager.Instance != null && GameNetworkManager.Instance.gameVersionNum != 0)
            {
                Version = GameNetworkManager.Instance.gameVersionNum;
                enabled = false;
            }

            if (toggles["Debug"])
            {
                LoggerManager.LogAllToggles();
                ToggleManager.ToggleFunction("Debug");
            }
        }

        private void OnGUI()
        {
            if (!_windowOpen)
            {
                return;
            }

            _windowRect = GUI.Window(0, _windowRect, WindowRoutine, "FunnyStuff Menu");
        }

        private void WindowRoutine(int windowId)
        {
            GUILayout.Label("Toggle Options:");

            float windowHeight = 30f * toggles.Count + 30f;
            _windowRect.height = windowHeight;

            List<string> keysToToggle = new(toggles.Keys);

            foreach (var toggleKey in keysToToggle)
            {
                if (toggleKey == "VersionSpoof" || toggleKey == "Debug")
                {
                    continue;
                }

                GUILayout.BeginHorizontal();

                bool newValue = GUILayout.Toggle(toggles[toggleKey], $"{ToggleManager.GetToggleDisplayName(toggleKey)} [{ToggleManager.GetToggleKeybinding(toggleKey)}]", GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));

                if (newValue != toggles[toggleKey])
                {
                    ToggleManager.ToggleFunction(toggleKey);
                }

                GUILayout.EndHorizontal();

                if (toggleKey == "Quota" && toggles[toggleKey])
                {
                    GUILayout.Label($"{toggleKey} is Host Only!");
                }
            }

            GUI.DragWindow();
        }
    }

    public static class ToggleManager
    {
        private static InputAction[] toggleActions;

        static ToggleManager()
        {
            toggleActions = new InputAction[0];
        }

        public static void AddToggle(string toggleKey, string keybinding)
        {
            Plugin.toggles.Add(toggleKey, false);
            CreateToggleInputAction(toggleKey, keybinding);
        }

        private static void CreateToggleInputAction(string toggleKey, string keybinding)
        {
            Array.Resize(ref toggleActions, toggleActions.Length + 1);

            toggleActions[^1] = new InputAction($"Toggle{toggleKey}", InputActionType.Button, $"<Keyboard>/{keybinding}");
            toggleActions[^1].performed += ctx => ToggleFunction(toggleKey);
        }

        public static void EnableToggleInputActions()
        {
            foreach (var toggleAction in toggleActions)
            {
                toggleAction.Enable();
            }
        }

        public static void ToggleFunction(string toggleKey)
        {
            Plugin.toggles[toggleKey] = !Plugin.toggles[toggleKey];

            if (Plugin.toggles[toggleKey])
            {
                Plugin.Log.LogInfo($"{GetToggleDisplayName(toggleKey)} Enabled");
            }
            else
            {
                Plugin.Log.LogInfo($"{GetToggleDisplayName(toggleKey)} Disabled");
            }

            if (toggleKey == "Quota" && Plugin.toggles[toggleKey])
            {
                Plugin.Log.LogError($"{GetToggleDisplayName(toggleKey)} is Host Only!");
            }
        }

        public static string GetToggleDisplayName(string toggleKey)
        {
            return toggleKey switch
            {
                "NoKill" => "No Kill",
                "NoHurty" => "No Damage",
                "Speed" => "Infinite Stamina",
                "Heavy" => "Always 0lbs",
                "Charge" => "Infinite Battery",
                "Quota" => "Reach Quota",
                "Debug" => "Debug",
                _ => toggleKey,
            };
        }

        public static string GetToggleKeybinding(string toggleKey)
        {
            return toggleKey switch
            {
                "NoKill" => "F1",
                "NoHurty" => "F2",
                "Speed" => "F3",
                "Heavy" => "F4",
                "Charge" => "F5",
                "Quota" => "F6",
                _ => toggleKey,
            };
        }
    }

    public static class LoggerManager
    {
        //public void LogAllInitialToggles()
        //{
        //    foreach (var toggle in Plugin.toggles)
        //    {
        //        Plugin.Log.LogInfo($"{toggle.Key} is {toggle.Value}");
        //    }
        //}

        public static void LogAllToggles()
        {
            foreach (var toggle in Plugin.toggles)
            {
                Plugin.Log.LogInfo($"{toggle.Key} is {toggle.Value}");
            }
        }

        //public void LogToggle(string toggledKey)
        //{
        //    Plugin.Log.LogInfo($"{toggledKey} toggled to {Plugin.toggles[toggledKey]}");
        //}
    }
}
