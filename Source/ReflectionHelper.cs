#if   Il2Cpp
using Il2CppScheduleOne.Delivery;
using Il2CppScheduleOne.Property;
#elif Mono
using ScheduleOne.Delivery;
using ScheduleOne.Property;
#endif
using HarmonyLib;
using MelonLoader;
using System;
using System.Linq;
using System.Reflection;

namespace ImprovedPackagers
{
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

                Type actionType = typeof(Action);
                _handler = Delegate.CreateDelegate(actionType, typeof(ReflectionHelper).GetMethod(nameof(OnPrefsSaved), _flags));

                _event.AddEventHandler(null, _handler);
                _hooked = true;
            }
            catch (Exception ex) { MelonLogger.Error($"Failed to initialize Mod Manager auto refresh {ex.Message}"); }

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
}