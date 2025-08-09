#if   Il2Cpp
using Il2CppScheduleOne.Delivery;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Property;

#elif Mono
using ScheduleOne.Delivery;
using ScheduleOne.ItemFramework;
using ScheduleOne.Property;

#endif
using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PackagersLoadVehicles
{
    public enum EDirection { Dual_Direction, Unload_Only, Load_Only }

    public class LoadVehicles : MelonMod
    {
        public const string ModName = "Packagers Load Vehicles";
        public const string Version = "1.0.0";
        public const string ModDesc = "Enables Schedule I Packagers to load vehicles parked in Loading Bays";

        private MelonPreferences_Category PropertyGroup;
#if Il2Cpp
        private readonly List<MelonPreferences_Entry<EDirection>> LoadingBayPrefs = new List<MelonPreferences_Entry<EDirection>>();
        private const EDirection prefDefault = EDirection.Dual_Direction;
#elif Mono
        private readonly List<MelonPreferences_Entry<bool>> LoadingBayPrefs = new List<MelonPreferences_Entry<bool>>();
        private const bool prefDefault = true;
#endif
        private static readonly Dictionary<string, LoadVehicles>  AllProperties   = new Dictionary<string, LoadVehicles>();

        public override void OnLateInitializeMelon() => ReflectionHelper.Initialize();
        public override void OnDeinitializeMelon()   => ReflectionHelper.Deinitialize();

        public static LoadVehicles AddLoadingDocks(Property property)
        {
            string name = property.PropertyName;
            LoadVehicles prefs = new LoadVehicles { PropertyGroup = MelonPreferences.CreateCategory($"PackagersLoadVehicles_{name}", name) };

            foreach (LoadingDock dock in property.LoadingDocks)
                prefs.LoadingBayPrefs.Add(prefs.PropertyGroup.CreateEntry(dock.Name.Replace(" ", ""), default_value: prefDefault, dock.Name));
            return prefs;
        }

        public static void AddProperty(Property property)
        {
            if (property.LoadingDockCount == 0 || AllProperties.TryGetValue(property.PropertyName, out _)) return;
            AllProperties.Add(property.PropertyName, AddLoadingDocks(property));
        }

#if Il2Cpp
        public static EDirection GetDockStatus(string property, string dock)
#elif Mono
        public static bool GetDockStatus(string property, string dock)
#endif
        {
            if (AllProperties.TryGetValue(property, out LoadVehicles docks))
                foreach (var pref in docks.LoadingBayPrefs)
                    if (pref.DisplayName == dock)
                        return pref.Value;

            return prefDefault;
        }
    }

    public static class ReflectionHelper
    {
        public static MethodInfo SetOccupant;
        private static EventInfo _event;
        private static Delegate _handler;
        private static bool _hooked;
        private static readonly BindingFlags _flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic;

        public static void Initialize()
        {
            if (_hooked) return;

            try
            {
                SetOccupant = AccessTools.Method(typeof(LoadingDock), "SetOccupant");
                Type eventsType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("ModManagerPhoneApp.ModSettingsEvents"))
                    .FirstOrDefault(t => t != null);
                if (eventsType == null) return;

                _event = eventsType.GetEvent("OnPreferencesSaved", _flags);
                if (_event == null) return;

                var actionType = typeof(Action);
                _handler = Delegate.CreateDelegate(actionType, typeof(ReflectionHelper).GetMethod(nameof(OnPrefsSaved), _flags));

                _event.AddEventHandler(null, _handler);
                _hooked = true;
                MelonLogger.Msg("Hooked ModManagerPhoneApp.OnPreferencesSaved");
            }
            catch (Exception ex) { MelonLogger.Error($"Failed to initialize Mod Manager autuo refresh {ex.Message}"); }

        }

        public static void Deinitialize()
        {
            try
            {
                if (_hooked && _event != null && _handler != null)
                    _event.RemoveEventHandler(null, _handler);
            }
            catch { }
            finally
            {
                _hooked  = false;
                _event   = null;
                _handler = null;
            }
        }

        private static void OnPrefsSaved()
        {
            try
            {
                foreach (var property in Property.OwnedProperties)
                    foreach (var dock in property.LoadingDocks)
                        SetOccupant.Invoke(dock, parameters: new object[] { null });
            }
            catch (Exception ex) { MelonLogger.Error($"OnPrefsSaved error {ex.Message}"); }
        }
    }

    [HarmonyPatch(typeof(LoadingDock), "SetOccupant")]
    public class LoadingDockSetOccupantPatch
    {
        static void Postfix(LoadingDock __instance)
        {
            if (__instance.DynamicOccupant != null)
            {
#if Il2Cpp
                EDirection status = LoadVehicles.GetDockStatus(__instance.ParentProperty.PropertyName, __instance.Name);
                if (status != EDirection.Unload_Only)
                    foreach (ItemSlot slot in __instance.DynamicOccupant.Storage.ItemSlots)
                        __instance.InputSlots.Add(slot);

                if (status == EDirection.Load_Only)
                    __instance.OutputSlots.Clear();
#elif Mono
                if (LoadVehicles.GetDockStatus(__instance.ParentProperty.PropertyName, __instance.Name))
                    foreach (ItemSlot slot in __instance.DynamicOccupant.Storage.ItemSlots)
                        __instance.InputSlots.Add(slot);
#endif
            }
        }
    }

    [HarmonyPatch(typeof(Property), "Start")]
    public class PropertyStartPatch
    {
        static void Postfix(Property __instance)
        {
            LoadVehicles.AddProperty(__instance);
        }
    }
}