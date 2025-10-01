using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
namespace ExpandWorld.Music;

public class SoundManager
{
  public static string FileName = "expand_sounds.yaml";
  public static string Pattern = "expand_sounds*.yaml";
  private readonly static HashSet<string> DataKeys = [];

  public static bool IsServer() => ZNet.instance && ZNet.instance.IsServer();

  public static void ToFile()
  {
    if (!IsServer()) return;
    if (Yaml.Exists(FileName)) return;

    var zsfxComponents = ZNetScene.instance.m_namedPrefabs.Values.Select(go => go.GetComponent<ZSFX>()).Where(zsfx => zsfx).ToList();
    var objectDataList = zsfxComponents
      .Select(ToData)
      .Where(m => m != null).Select(m => m!)
      .ToList();

    if (objectDataList.Count > 0)
    {
      var yaml = Yaml.Write(objectDataList);
      Yaml.WriteFile(FileName, yaml);
    }
  }

  private static SoundData? ToData(ZSFX zsfx)
  {
    if (zsfx == null) return null;

    return new SoundData
    {
      name = zsfx.name,
      playOnAwake = zsfx.m_playOnAwake,
      closedCaptionToken = zsfx.m_closedCaptionToken,
      minimumCaptionVolume = zsfx.m_minimumCaptionVolume,
      clips = Clips.GetNames(zsfx.m_audioClips),
      maxConcurrentSources = zsfx.m_maxConcurrentSources,
      ignoreConcurrencyDistance = zsfx.m_ignoreConcurrencyDistance,
      maxPitch = zsfx.m_maxPitch,
      minPitch = zsfx.m_minPitch,
      maxVol = zsfx.m_maxVol,
      minVol = zsfx.m_minVol,
      fadeInDuration = zsfx.m_fadeInDuration,
      fadeOutDuration = zsfx.m_fadeOutDuration,
      fadeOutDelay = zsfx.m_fadeOutDelay,
      fadeOutOnAwake = zsfx.m_fadeOutOnAwake,
      randomPan = zsfx.m_randomPan,
      minPan = zsfx.m_minPan,
      maxPan = zsfx.m_maxPan,
      maxDelay = zsfx.m_maxDelay,
      minDelay = zsfx.m_minDelay,
      distanceReverb = zsfx.m_distanceReverb,
      useCustomReverbDistance = zsfx.m_useCustomReverbDistance,
      customReverbDistance = zsfx.m_customReverbDistance
    };
  }

  public static void FromFile(string lines)
  {
    if (!IsServer()) return;
    if (lines == "")
    {
      EWM.valueSoundData.Value = [];
      return;
    }
    try
    {
      var data = Yaml.Read<SoundData>(lines, "Sounds", Log.Error).ToList();
      EWM.valueSoundData.Value = data;
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
      Log.Error(e.StackTrace);
      EWM.valueSoundData.Value = [];
    }
  }

  public static void FromSetting(List<SoundData> data)
  {
    Log.Info($"Reloading sound data ({data.Count} entries).");
    DataKeys.Clear();
    var zs = ZNetScene.instance;
    foreach (var d in data)
    {
      if (DataKeys.Contains(d.name))
      {
        if (IsServer())
          Log.Warning($"Duplicate sound entry for '{d.name}' found.");
        continue;
      }
      DataKeys.Add(d.name);
      var prefab = zs.GetPrefab(d.name);
      if (prefab == null)
      {
        if (IsServer())
          Log.Warning($"No prefab found for '{d.name}'.");
        continue;
      }
      UpdatePrefab(prefab, d);
    }
  }

  public static void SetupWatcher()
  {
    Yaml.Setup(Pattern, FromFile);
  }

  static void UpdatePrefab(GameObject go, SoundData data)
  {
    var zsfx = go.GetComponent<ZSFX>();
    if (!zsfx) return;
    zsfx.m_playOnAwake = data.playOnAwake;
    zsfx.m_closedCaptionToken = data.closedCaptionToken;
    zsfx.m_minimumCaptionVolume = data.minimumCaptionVolume;
    zsfx.m_maxConcurrentSources = data.maxConcurrentSources;
    zsfx.m_ignoreConcurrencyDistance = data.ignoreConcurrencyDistance;
    zsfx.m_maxPitch = data.maxPitch;
    zsfx.m_minPitch = data.minPitch;
    zsfx.m_maxVol = data.maxVol;
    zsfx.m_minVol = data.minVol;
    zsfx.m_fadeInDuration = data.fadeInDuration;
    zsfx.m_fadeOutDuration = data.fadeOutDuration;
    zsfx.m_fadeOutDelay = data.fadeOutDelay;
    zsfx.m_fadeOutOnAwake = data.fadeOutOnAwake;
    zsfx.m_randomPan = data.randomPan;
    zsfx.m_minPan = data.minPan;
    zsfx.m_maxPan = data.maxPan;
    zsfx.m_maxDelay = data.maxDelay;
    zsfx.m_minDelay = data.minDelay;
    zsfx.m_distanceReverb = data.distanceReverb;
    zsfx.m_useCustomReverbDistance = data.useCustomReverbDistance;
    zsfx.m_customReverbDistance = data.customReverbDistance;
    zsfx.m_audioClips = Clips.GetClips(data.clips);
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start)), HarmonyPriority(Priority.Last)]
public class InitializeObjectContent
{
  static void Postfix()
  {
    SoundManager.ToFile();
    SoundManager.FromFile(Yaml.ReadFiles(SoundManager.Pattern));
  }
}
