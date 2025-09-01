using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace StationeersMods.Frontend.Patches
{
    // Гнучкий пошук цільового методу головного меню.
    [HarmonyPatch]
    internal static class MenuHook
    {
        static MethodBase TargetMethod()
        {
            // 1) Пробуємо старий цільовий метод (для сумісності зі старими білдами)
            var legacy = AccessTools.Method("WorkshopMenu:ManagerAwake");
            if (legacy != null) return legacy;

            // 2) Знаходимо актуальний клас меню (назва може змінюватись від білда до білда)
            var menuType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(SafeGetTypes)
                .FirstOrDefault(t =>
                    t.IsClass &&
                    // найтиповіші патерни назв меню
                    (t.Name.Contains("MainMenu", StringComparison.OrdinalIgnoreCase)
                     || t.Name.Contains("MenuController", StringComparison.OrdinalIgnoreCase)
                     || t.Name.Equals("Menu", StringComparison.OrdinalIgnoreCase)));

            if (menuType == null)
                return null;

            // 3) Чіпляємось до Awake або Start
            var awake = menuType.GetMethod("Awake", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (awake != null) return awake;

            var start = menuType.GetMethod("Start", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            return start;
        }

        static Type[] SafeGetTypes(Assembly a)
        {
            try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
        }

        // Після ініціалізації меню — гарантуємо підняття LaunchPad
        static void Postfix()
        {
            try
            {
                Core.Bootstrap.Ensure();
            }
            catch (Exception e)
            {
                BepInEx.Logging.Logger.CreateLogSource("LaunchPad-Fix").LogError($"Bootstrap failed: {e}");
            }
        }
    }
}
