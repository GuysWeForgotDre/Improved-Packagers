#if   Il2Cpp
using Il2CppFishNet;
using Il2CppFishNet.Object;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppScheduleOne.Management;
using Il2CppScheduleOne.NPCs.Behaviour;
using Il2CppScheduleOne.ObjectScripts;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.UI.Stations;
#elif Mono
using FishNet;
using FishNet.Object;
using ScheduleOne.Management;
using ScheduleOne.NPCs.Behaviour;
using ScheduleOne.ObjectScripts;
using ScheduleOne.Persistence;
using ScheduleOne.UI.Stations;
#endif
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

namespace ImprovedPackagers
{
    /*
     * PackagingStationCanvas Patches
     * */
    [HarmonyPatch(typeof(PackagingStationCanvas), nameof(PackagingStationCanvas.SetIsOpen))]
    static class PSCanvasSetIsOpenPatch
    {
        static void Postfix(PackagingStationCanvas __instance, PackagingStation station, bool open)
        {
            if (__instance is null || station is null || !open) return;
            StationModeRegistry.SetExplicit(station, __instance.CurrentMode);
        }
    }

    [HarmonyPatch(typeof(PackagingStationCanvas), nameof(PackagingStationCanvas.ToggleMode))]
    static class PSCanvasToggleModePatch
    {
        static void Postfix(PackagingStationCanvas __instance)
        {
            if (__instance is null || __instance.PackagingStation is null) return;
            StationModeRegistry.SetExplicit(__instance.PackagingStation, __instance.CurrentMode);
        }
    }

    /*
     * PackagingStationBehaviour Patches
     * */
    [HarmonyPatch(typeof(PackagingStationBehaviour), nameof(PackagingStationBehaviour.IsStationReady))]
    static class PSBehaviourIsStationReadyPatch
    {
        static bool Prefix(PackagingStationBehaviour __instance, PackagingStation station, ref bool __result)
        {
            if (station is null) { __result = false; return false; }

            var mode = PackagingStation.EMode.Package;
            StationModeRegistry.TryGetMode(station, out mode);

            if (station.GetState(mode) != PackagingStation.EState.CanBegin)
            {
                __result = false;
                return false;
            }
#if Il2Cpp
            IUsable usable = (station as Il2CppObjectBase)?.TryCast<IUsable>();
#elif Mono
            IUsable usable = station as IUsable;
#endif
            if (!(usable is null) && usable.IsInUse && station.NPCUserObject != __instance.Npc.NetworkObject)
            {
                __result = false;
                return false;
            }

            Vector3 target  = station.StandPoint.position;
            Vector3 backoff = -station.StandPoint.forward * 0.25f;
            Vector3 desired = target + backoff;
            Vector3 driveTo = NavMesh.SamplePosition(desired, out var hit, 0.6f, NavMesh.AllAreas) ? hit.position : desired;

            __result = __instance.Npc.Movement.CanGetTo(station.StandPoint.position);
            return false;
        }
    }

    /*
     * PackagingStation Patches
     * */
    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.Awake))]
    static class PackaginStationAwakePatch
    {
        static void Postfix(PackagingStation __instance) => StationModeRegistry.SetExplicitIfExists(__instance);
    }

    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.PackSingleInstance))]
    static class PackagingStationPackSingleInstancePatch
    {
        static bool Prefix(PackagingStation __instance)
        {
            if (__instance is null || __instance.NPCUserObject is null) return true;

            var mode = PackagingStation.EMode.Package;
            if (StationModeRegistry.TryGetMode(__instance, out var chosen))
                mode = chosen;

            if (InstanceFinder.IsServer && mode == PackagingStation.EMode.Unpackage)
            {
                __instance.Unpack();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.SetNPCUser))]
    static class PackagingStationSetNPCUserPatch
    {
        static void Postfix(PackagingStation __instance, NetworkObject npcObject)
        {
            if (__instance is null || npcObject is null) return;
            if (!StationModeRegistry.TryGetMode(__instance, out _))
            {
                bool canPack   = __instance.GetState(PackagingStation.EMode.Package)   == PackagingStation.EState.CanBegin;
                bool canUnpack = __instance.GetState(PackagingStation.EMode.Unpackage) == PackagingStation.EState.CanBegin;

                var chosen = canUnpack && !canPack ? PackagingStation.EMode.Unpackage : PackagingStation.EMode.Package;
                StationModeRegistry.SetStickyIfNone(__instance, chosen);
            }
        }
    }

    [HarmonyPatch(typeof(PackagingStation), nameof(PackagingStation.DestroyItem))]
    static class PackagingStationDestroyPatch
    {
        static void Postfix(PackagingStation __instance) => StationModeRegistry.Remove(__instance);
    }

    /*
     * SaveManager Patches
     * */
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Save), new[] { typeof(string) })]
    public class SaveManagerSavePatch
    {
        static void Prefix() => StationModeRegistry.Save();
    }
}