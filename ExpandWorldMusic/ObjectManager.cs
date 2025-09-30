using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Service;
using UnityEngine;
namespace ExpandWorld.Music;

public class ObjectManager
{
  public static string FileName = "expand_object_music.yaml";
  public static string Pattern = "expand_object_music*.yaml";
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

  private static ObjectData? ToData(ZSFX zsfx)
  {
    if (zsfx == null) return null;

    return new ObjectData
    {
      name = Utils.GetPrefabName(zsfx.gameObject.name),
      playOnAwake = zsfx.m_playOnAwake,
      closedCaptionToken = zsfx.m_closedCaptionToken,
      secondaryCaptionToken = zsfx.m_secondaryCaptionToken,
      captionType = (int)zsfx.m_captionType,
      minimumCaptionVolume = zsfx.m_minimumCaptionVolume,
      audioClips = zsfx.m_audioClips?.Select(clip => clip?.name ?? "").Where(name => !string.IsNullOrEmpty(name)).ToArray() ?? [],
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
    EWM.valueObjectMusicData.Value = lines;
    Set(lines);
  }

  public static void FromSetting(string yaml)
  {
    if (!IsServer()) Set(yaml);
  }

  private static void Set(string yaml)
  {
    if (yaml == "") return;
    try
    {
      var data = Yaml.Read<ObjectData>(yaml, "ObjectMusic", Log.Error).ToList();
      if (data.Count == 0)
      {
        Log.Warning($"Failed to load any Object music data.");
        return;
      }
      Log.Info($"Reloading Object music data ({data.Count} entries).");
      DataKeys.Clear();
      var zs = ZNetScene.instance;
      foreach (var d in data)
      {
        if (DataKeys.Contains(d.name))
        {
          Log.Warning($"Duplicate object music entry for '{d.name}' found.");
          continue;
        }
        DataKeys.Add(d.name);
        var prefab = zs.GetPrefab(d.name);
        if (prefab == null)
        {
          Log.Warning($"No prefab found for '{d.name}'.");
          continue;
        }
        UpdatePrefab(prefab, d);
      }
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
      Log.Error(e.StackTrace);
    }
  }

  public static void SetupWatcher()
  {
    Yaml.Setup(Pattern, FromFile);
  }

  static void UpdatePrefab(GameObject go, ObjectData data)
  {
    var zsfx = go.GetComponent<ZSFX>();
    if (!zsfx) return;
    zsfx.m_playOnAwake = data.playOnAwake;
    zsfx.m_closedCaptionToken = data.closedCaptionToken;
    zsfx.m_secondaryCaptionToken = data.secondaryCaptionToken;
    zsfx.m_captionType = (ClosedCaptions.CaptionType)data.captionType;
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
    zsfx.m_audioClips = data.audioClips.Select(name => Loader.Clips.ContainsKey(name) ? Loader.Clips[name] : null).Where(clip => clip != null).ToArray();
  }
}

[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start)), HarmonyPriority(Priority.Last)]
public class InitializeObjectContent
{
  static void Postfix()
  {
    ObjectManager.ToFile();
    ObjectManager.FromFile(Yaml.ReadFiles(ObjectManager.Pattern));
  }
}
