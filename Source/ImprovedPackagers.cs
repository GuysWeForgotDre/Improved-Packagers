#if   Il2Cpp
using Il2CppScheduleOne.Delivery;
using Il2CppScheduleOne.Property;
#elif Mono
using ScheduleOne.Delivery;
using ScheduleOne.Property;
#endif
using MelonLoader;
using System;
using System.Collections.Generic;

namespace ImprovedPackagers
{
    public enum EDirection { Dual_Direction, Unload_Only, Load_Only }

    public class ImprovedPackagers : MelonMod
    {
        public const string ModName = "Improved Packagers";
        public const string Version = "2.0.0";
        public const string ModDesc = "Enables Packagers to unpack product at Packaging Stations, and load vehicles parked in Loading Bays";

        private MelonPreferences_Category PropertyGroup;
#if Il2Cpp
        private readonly List<MelonPreferences_Entry<EDirection>> LoadingBayPrefs = new List<MelonPreferences_Entry<EDirection>>();
        private const EDirection prefDefault = EDirection.Dual_Direction;
#elif Mono
        private readonly List<MelonPreferences_Entry<bool>> LoadingBayPrefs = new List<MelonPreferences_Entry<bool>>();
        private const bool prefDefault = true;
#endif
        private static readonly Dictionary<string, ImprovedPackagers> AllProperties = new Dictionary<string, ImprovedPackagers>();

        public override void OnLateInitializeMelon()
        {
            ReflectionHelper.Initialize();
            StationModeRegistry.Load();
        }

        public override void OnDeinitializeMelon()
        {
            ReflectionHelper.Deinitialize();
            StationModeRegistry.ClearAll();
        }

        public static ImprovedPackagers AddLoadingDocks(Property property)
        {
            string name = property.PropertyName;
            ImprovedPackagers prefs = new ImprovedPackagers { PropertyGroup = MelonPreferences.CreateCategory($"PackagersLoadVehicles_{name}", name) };

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
            if (AllProperties.TryGetValue(property, out ImprovedPackagers docks))
                foreach (var pref in docks.LoadingBayPrefs)
                    if (pref.DisplayName == dock)
                        return pref.Value;

            return prefDefault;
        }
    }
}