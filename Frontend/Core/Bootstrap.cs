using System;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace StationeersMods.Frontend.Core
{
    // Мінімальний бутстрапер: один раз за гру піднімає панель LaunchPad.
    internal static class Bootstrap
    {
        private static bool _done;
        private static ManualLogSource _log = Logger.CreateLogSource("LaunchPad-Bootstrap");

        public static void Ensure()
        {
            if (_done) return;
            _done = true;

            try
            {
                var go = new GameObject("StationeersLaunchPadRoot");
                UnityEngine.Object.DontDestroyOnLoad(go);

                // Якщо у фронтенді LaunchPad є свій компонент (наприклад, LaunchPadFrontend),
                // додаємо його напряму. Інакше — показуємо просту панель-заглушку.
                var frontType = AccessTools.TypeByName("StationeersMods.Frontend.LaunchPadFrontend");
                if (frontType != null && typeof(MonoBehaviour).IsAssignableFrom(frontType))
                {
                    go.AddComponent(frontType);
                    _log.LogInfo("LaunchPadFrontend attached.");
                }
                else
                {
                    go.AddComponent<SimpleBar>();
                    _log.LogWarning("Fallback SimpleBar attached (Frontend type not found).");
                }
            }
            catch (Exception e)
            {
                _log.LogError($"Ensure() error: {e}");
            }
        }

        // Дуже проста нижня панель на випадок, якщо фронтенд не підхопився
        private class SimpleBar : MonoBehaviour
        {
            Rect _r = new Rect(20, Screen.height - 60, 480, 40);
            bool _drag;

            void OnGUI()
            {
                _r.y = Screen.height - 60;
                GUI.Box(_r, "LaunchPad (compat)");
                GUILayout.BeginArea(new Rect(_r.x + 10, _r.y + 8, _r.width - 20, _r.height - 16));
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Open Mods Folder"))
                    System.Diagnostics.Process.Start(BepInEx.Paths.PluginPath);
                if (GUILayout.Button("Open Config Folder"))
                    System.Diagnostics.Process.Start(BepInEx.Paths.ConfigPath);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Press F6 for Difficulty UI");
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }
    }
}
