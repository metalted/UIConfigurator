using BepInEx;
using UnityEngine;
using HarmonyLib;
using BepInEx.Configuration;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace UIConfigurator
{
    [BepInPlugin(pluginGUID, pluginName, pluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string pluginGUID = "com.metalted.zeepkist.uiconfigurator";
        public const string pluginName = "UI Configurator";
        public const string pluginVersion = "1.3";
        public UIConfigurator uiConfigurator;
        public static Plugin Instance;

        public ConfigEntry<KeyCode> editModeKey;
        public ConfigEntry<KeyCode> prevCycleKey;
        public ConfigEntry<KeyCode> nextCycleKey;
        public ConfigEntry<bool> resetAllButton;
        public ConfigEntry<string> borderColor;        
        
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            Harmony harmony = new Harmony(pluginGUID);
            harmony.PatchAll();

            Instance = this;

            editModeKey = Config.Bind("Settings", "Edit Mode Toggle", KeyCode.Keypad8);
            prevCycleKey = Config.Bind("Settings", "Next UI", KeyCode.Keypad4);
            nextCycleKey = Config.Bind("Settings", "Previous UI", KeyCode.Keypad6);
            borderColor = Config.Bind("Settings", "Border Color", "Red", new ConfigDescription("Selected Color", new AcceptableValueList<string>(UIConfiguratorUtils.colors)));
            resetAllButton = Config.Bind("Settings", "Reset All", true, "[Button] Reset All UI");

            resetAllButton.SettingChanged += ResetAllButton_SettingChanged;
            borderColor.SettingChanged += BorderColor_SettingChanged;

            GameObject uiObj = new GameObject("UIConfigurator");
            uiConfigurator = uiObj.AddComponent<UIConfigurator>();
            uiConfigurator.SetBorderColor(UIConfiguratorUtils.GetColor((string)borderColor.BoxedValue));
        }

        private void BorderColor_SettingChanged(object sender, EventArgs e)
        {
            Color c = UIConfiguratorUtils.GetColor((string)borderColor.BoxedValue);
            uiConfigurator.SetBorderColor(c);
        }

        private void ResetAllButton_SettingChanged(object sender, EventArgs e)
        {
            uiConfigurator.ResetAll();
        }

        public void OnSceneLoaded()
        {
            uiConfigurator.SceneChange();
        }

        public void AddRect(RectTransform rect)
        {
            RectTransformHandler rth = rect.GetComponent<RectTransformHandler>();
            if (rth == null)
            {
                rect.gameObject.AddComponent<RectTransformHandler>();
            }
        }

        public void AddRects(List<RectTransform> rects)
        {
            foreach(RectTransform rect in rects)
            {
                AddRect(rect);
            }
        }        
    }

    [HarmonyPatch(typeof(SpectatorCameraUI),"Awake")]
    public class SpectatorCameraUIAwakePatch
    {
        public static void Postfix(SpectatorCameraUI __instance)
        {
            Transform guiHolder = __instance.transform.Find("GUI Holder");
            Transform flyingCameraGUI = guiHolder.transform.Find("Flying Camera GUI");
            Transform smallLeaderboardHolder = guiHolder.transform.Find("Small Leaderboard Holder (false)");
            Transform DSLRRect = flyingCameraGUI.transform.Find("DSLR Rect");

            List<RectTransform> rectList = new List<RectTransform>();

            if (smallLeaderboardHolder != null)
            {
                Transform smallLeaderboard = smallLeaderboardHolder.GetChild(0);

                if (smallLeaderboard != null)
                {
                    RectTransform rt = smallLeaderboard.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rectList.Add(rt);
                    }
                }
            }
            
            if(DSLRRect != null)
            {
                foreach (Transform t in DSLRRect)
                {
                    RectTransform rt = t.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rectList.Add(rt);
                    }
                }
            }

            Plugin.Instance.AddRects(rectList);
        }
    }

    [HarmonyPatch(typeof(PlayerScreensUI), "Awake")]
    public class PlayerScreensUIAwakePatch
    {
        public static void Postfix(PlayerScreensUI __instance)
        {
            //Exclude CheckpointsPanel, Image, Debug, Center Shower, WR (for Saty)
            Transform playerPanel = __instance.transform.GetChild(0);

            List<RectTransform> rectList = new List<RectTransform>();

            if (playerPanel != null)
            {
                foreach (Transform t in playerPanel)
                {
                    RectTransform rt = t.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        string rectName = rt.name.ToLower();
                        switch(rectName)
                        {
                            default:
                                rectList.Add(rt);
                                break;
                            case "checkpointspanel":
                            case "image":
                            case "debug":
                            case "center shower":
                            case "wr (for saty)":
                                break;
                        }
                    }
                }
            }

            Plugin.Instance.AddRects(rectList);
        }
    }

    [HarmonyPatch(typeof(OnlineGameplayUI), "Awake")]
    public class OnlineGameplayUIAwakePatch
    {
        public static void Postfix(OnlineGameplayUI __instance)
        {
            Transform gameplayGUI = __instance.transform.GetChild(0);

            List<RectTransform> rectList = new List<RectTransform>();

            if (gameplayGUI != null)
            {
                foreach (Transform t in gameplayGUI)
                {
                    RectTransform rt = t.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rectList.Add(rt);
                    }
                }
            }

            Plugin.Instance.AddRects(rectList);
        }
    }

    [HarmonyPatch(typeof(OnlineChatUI), "Awake")]
    public class OnlineChatUIAwakePatch
    {
        public static void Postfix(OnlineChatUI __instance)
        {
            List<RectTransform> rectList = new List<RectTransform>();

            foreach (Transform t in __instance.transform)
            {
                RectTransform rt = t.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rectList.Add(rt);
                }
            }

            Plugin.Instance.AddRects(rectList);
        }
    }

    [HarmonyPatch(typeof(SceneManager), "LoadScene", new Type[] { typeof(string) })]
    public class SceneManagerLoadScenePatch
    {
        public static void Prefix()
        {
            // Place your code here
            Plugin.Instance.OnSceneLoaded();
        }
    }
}
