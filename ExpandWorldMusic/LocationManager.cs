using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
namespace ExpandWorld.Music;

public class LocationManager
{
  public static string ReferenceFileName = "ref_expand_location_music.yaml";
  public static string FileName = "expand_location_music.yaml";
  public static string Pattern = "expand_location_music*.yaml";
  public static Dictionary<int, LocationData> Data = [];

  public static bool IsServer() => ZNet.instance && ZNet.instance.IsServer();
  public static void ToFile()
  {
    if (!IsServer()) return;
    if (!Yaml.Exists(FileName))
      Yaml.WriteFile(FileName, "");
    if (Yaml.Exists(ReferenceFileName)) return;
    var yaml = Yaml.Write(ZoneSystem.instance.m_locations
      .Select(LocationLoader.ToData)
      .Where(m => m != null).Select(m => m!)
      .Distinct(new LocationDataComparer())
      .ToList());
    Yaml.WriteFile(ReferenceFileName, yaml);
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
      var hash = d.name.GetStableHashCode();
      if (Data.ContainsKey(hash))
      {
        if (IsServer())
          Log.Warning($"Duplicate location music entry for '{d.name}' found.");
        continue;
      }
      Data[hash] = d;
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

// There are few cases to consider.
// 1. MusicLocation can appear as a separate object (inactive in the location): Location must be found by position.
// 2. MusicLocation can appear directly in the Location object: Must be reused, ZNetView can be bridged from LocationProxy to MusicLocation.
// 3. No music location: Must be added to LocationProxy (because blueprints won't have Location so can't use that).
[HarmonyPatch]
public class LocationPatch
{
  static readonly int ReferenceHash = "locationreference".GetStableHashCode();

  [HarmonyPatch(typeof(MusicLocation), nameof(MusicLocation.Awake)), HarmonyPostfix]
  static void HandleMusicLocation(MusicLocation __instance)
  {
    // Already handled directly in HandleProxy.
    if (__instance.GetComponent<LocationProxy>() || __instance.GetComponent<Location>()) return;
    // Separate component doesn't have direct link to Location, so have to find by position.
    var loc = Location.GetLocation(__instance.transform.position);
    if (!loc) return;
    var name = Utils.GetPrefabName(loc?.name ?? __instance.gameObject.name);
    var hash = name.GetStableHashCode();
    if (!LocationManager.Data.TryGetValue(hash, out var data)) return;
    Apply(__instance, data);
  }

  [HarmonyPatch(typeof(LocationProxy), nameof(LocationProxy.SpawnLocation)), HarmonyPostfix]
  static void HandleProxy(LocationProxy __instance)
  {
    if (!__instance.m_nview) return;
    // Location must be spawned before the correct way can be determined.
    // So if delayed, this will be called again after spawn.
    if (__instance.m_locationNeedsSpawn) return;
    var zdo = __instance.m_nview.GetZDO();
    // Expand World Data adds this so that clones and blueprints can be referenced.
    var hash = zdo.GetInt(ReferenceHash);
    if (hash == 0)
      hash = zdo.GetInt(ZDOVars.s_location);
    if (!LocationManager.Data.TryGetValue(hash, out var data)) return;
    var music = GetMusic(__instance);
    if (!music) return;
    Apply(music, data);
  }

  private static MusicLocation? GetMusic(LocationProxy proxy)
  {
    var go = proxy.gameObject;
    // Instance might already have MusicLocation component, no point adding another one.
    if (proxy.m_instance)
      go = proxy.m_instance.gameObject;
    var music = go.GetComponentInChildren<MusicLocation>(true);
    // Must check if the MusicLocation spawns as a separate object.
    if (music && music.GetComponent<ZNetView>() != proxy.m_nview)
      return null;
    if (!music)
    {
      var audio = go.AddComponent<AudioSource>();
      audio.maxDistance = 999f;
      audio.minDistance = 0f;
      audio.priority = 0;
      audio.reverbZoneMix = 0f;
      audio.playOnAwake = false;
      audio.rolloffMode = AudioRolloffMode.Linear;
      audio.outputAudioMixerGroup = AudioMan.instance.m_masterMixer.FindMatchingGroups("Music_ontop")[0];
      music = go.AddComponent<MusicLocation>();
    }
    if (!music.m_nview)
    {
      // Location probably doesn't have ZNetView but LocationProxy has, so bridge that to extend support.
      music.m_nview = proxy.m_nview;
      if (music.m_nview)
        music.m_nview.Register("SetPlayed", music.SetPlayed);
    }
    return music;
  }
  private static void Apply(MusicLocation music, LocationData data)
  {
    music.m_forceFade = data.forceFade;
    music.m_notIfEnemies = data.notIfEnemies;
    music.m_oneTime = data.oneTime;
    music.m_radius = data.radius;
    music.m_addRadiusFromLocation = data.radiusFromLocation;
    music.m_baseVolume = data.volume;
    var audioSource = music.m_audioSource;
    if (!audioSource) return;
    audioSource.volume = data.volume;
    audioSource.loop = data.loop;
    audioSource.pitch = data.pitch;
    if (data.mixerGroup != null)
      audioSource.outputAudioMixerGroup = AudioMan.instance.m_masterMixer.FindMatchingGroups(data.mixerGroup)[0];
    var clip = Clips.GetRandomClip(data.clips);
    if (clip != null)
      audioSource.clip = clip;
    audioSource.enabled = audioSource.clip != null;
  }

}
