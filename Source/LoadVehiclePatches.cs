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

namespace ImprovedPackagers
{
    /*
     * LoadingDock Patches
     * */
    [HarmonyPatch(typeof(LoadingDock), "SetOccupant")]
    static class LoadingDockSetOccupantPatch
    {
        static void Postfix(LoadingDock __instance)
        {
            if (__instance.DynamicOccupant != null)
            {
#if Il2Cpp
                EDirection status = ImprovedPackagers.GetDockStatus(__instance.ParentProperty.PropertyName, __instance.Name);
                if (status != EDirection.Unload_Only)
                    foreach (ItemSlot slot in __instance.DynamicOccupant.Storage.ItemSlots)
                        __instance.InputSlots.Add(slot);

                if (status == EDirection.Load_Only)
                    __instance.OutputSlots.Clear();
#elif Mono
                if (ImprovedPackagers.GetDockStatus(__instance.ParentProperty.PropertyName, __instance.Name))
                    foreach (ItemSlot slot in __instance.DynamicOccupant.Storage.ItemSlots)
                        __instance.InputSlots.Add(slot);
#endif
            }
        }
    }

    /*
     * Property Patches
     * */
    [HarmonyPatch(typeof(Property), "Start")]
    static class PropertyStartPatch
    {
        static void Postfix(Property __instance) => ImprovedPackagers.AddProperty(__instance);
    }
}