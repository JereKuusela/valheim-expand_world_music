using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
namespace ExpandWorld.Music;

public class LocationManager
{
  public static string FileName = "expand_location_music.yaml";
  public static string Pattern = "expand_location_music*.yaml";
  public static Dictionary<string, LocationData> Data = [];

  public static bool IsServer() => ZNet.instance && ZNet.instance.IsServer();
  public static void ToFile()
  {
    if (!IsServer()) return;
    if (Yaml.Exists(FileName)) return;
    var yaml = Yaml.Write(ZoneSystem.instance.m_locations
      .Select(LocationLoader.ToData)
      .Where(m => m != null).Select(m => m!)
      .Distinct(new LocationDataComparer())
      .ToList());
    Yaml.WriteFile(FileName, yaml);
  }
  public static void FromFile(string lines)
  {
    if (!IsServer()) return;
    if (lines == "")
    {
      EWM.valueLocationMusicData.Value = [];
      return;
    }
    try
    {
      var data = Yaml.Read<LocationData>(lines, "LocationMusic", Log.Error).ToList();
      EWM.valueLocationMusicData.Value = data;
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
      Log.Error(e.StackTrace);
      EWM.valueLocationMusicData.Value = [];
    }
  }
  public static void FromSetting(List<LocationData> data)
  {
    Log.Info($"Reloading location music data ({data.Count} entries).");
    Data.Clear();
    foreach (var d in data)
    {
      if (Data.ContainsKey(d.name))
      {
        if (IsServer())
          Log.Warning($"Duplicate location music entry for '{d.name}' found.");
        continue;
      }
      Data[d.name] = d;
    }
  }
  public static void SetupWatcher()
  {
    Yaml.Setup(Pattern, FromFile);
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start)), HarmonyPriority(Priority.Last)]
public class InitializeLocationContent
{
  static void Postfix()
  {
    LocationManager.ToFile();
    LocationManager.FromFile(Yaml.ReadFiles(LocationManager.Pattern));
  }
}


[HarmonyPatch(typeof(MusicLocation), nameof(MusicLocation.Awake))]
public class MusicLocatioAwake
{
  static void Prefix(MusicLocation __instance)
  {
    var location = Location.GetLocation(__instance.transform.position);
    var prefab = Utils.GetPrefabName(location?.name ?? __instance.gameObject.name);
    if (!LocationManager.Data.TryGetValue(prefab, out var data)) return;
    __instance.m_forceFade = data.forceFade;
    __instance.m_notIfEnemies = data.notIfEnemies;
    __instance.m_oneTime = data.oneTime;
    __instance.m_radius = data.radius;
    __instance.m_addRadiusFromLocation = data.radiusFromLocation;
    var audioSource = __instance.GetComponent<AudioSource>();
    if (!audioSource) return;
    audioSource.volume = data.volume;
    audioSource.loop = data.loop;
    var clip = Clips.GetRandomClip(data.clips);
    if (clip != null)
      audioSource.clip = clip;
    audioSource.enabled = audioSource.clip != null;
  }

}
