#if   Il2Cpp
using Il2CppScheduleOne.ObjectScripts;
#elif Mono
using ScheduleOne.ObjectScripts;
#endif
using MelonLoader;
using System;
using System.Collections.Generic;
using System.IO;

public static class StationModeRegistry
{
    private static readonly Dictionary<string, int> _explicit = new Dictionary<string, int>();
    private static readonly Dictionary<string, int> _sticky   = new Dictionary<string, int>();

    private static readonly string FilePath = Path.Combine("UserData", "ImprovedPackagers.json");

    public static void Load()
    {
        if (File.Exists(FilePath)) return;

        _explicit.Clear();
        _sticky.Clear();

        try
        {
            foreach (string line in File.ReadAllLines(FilePath))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0 || trimmed.StartsWith("#")) continue;
                string[] split = trimmed.Split(':');
                if (split.Length != 2) continue;
                string guid = split[0];
                if (int.TryParse(split[1], out var mode))
                    _explicit[guid] = mode;
            }
        }
        catch (Exception ex) { MelonLogger.Error($"Load data error {ex.Message}"); }
    }

    public static void Save()
    {
        try
        {
            using var w = new StreamWriter(FilePath, false);
            w.WriteLine("# Improve Packagers: Unpacking Stations");
            foreach (var kv in _explicit)
                w.WriteLine($"{kv.Key}:{kv.Value}");
        }
        catch (Exception ex) { MelonLogger.Error($"Save data error {ex.Message}"); }
    }

    public static void SetExplicit(PackagingStation station, PackagingStation.EMode mode)
    {
        _explicit[station.GUID.ToString()] = (int)mode;
        _sticky.Remove(station.GUID.ToString());
    }

    public static bool TryGetMode(PackagingStation station, out PackagingStation.EMode mode)
    {
        mode = PackagingStation.EMode.Package;
        if (!(station is null) && _explicit.TryGetValue(station.GUID.ToString(), out var m)) 
        { 
            mode = (PackagingStation.EMode)m; 
            return true;
        }

        if (!(station is null) && _sticky  .TryGetValue(station.GUID.ToString(), out m)) 
        {
            mode = (PackagingStation.EMode)m;
            return true;
        }
        return false;
    }

    public static void SetExplicitIfExists(PackagingStation station)
    {
        if (station is null) return;
        if (_explicit.TryGetValue(station.GUID.ToString(), out var m))
            _sticky[station.GUID.ToString()] = m;
    }

    public static void SetStickyIfNone(PackagingStation station, PackagingStation.EMode mode)
    {
        if (station is null) return;
        if (_explicit.ContainsKey(station.GUID.ToString())) return;
        if (!_sticky .ContainsKey(station.GUID.ToString())) _sticky[station.GUID.ToString()] = (int)mode;
    }

    public static void Remove(PackagingStation station)
    {
        if (station is null) return;
        _explicit.Remove(station.GUID.ToString());
        _sticky  .Remove(station.GUID.ToString());
    }

    public static void ClearAll()
    {
        _explicit.Clear();
        _sticky  .Clear();
    }
}