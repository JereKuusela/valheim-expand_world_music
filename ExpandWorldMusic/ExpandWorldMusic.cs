using System;
using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using ServerSync;
using Service;
using UnityEngine;
namespace ExpandWorld.Music;

[BepInPlugin(GUID, NAME, VERSION)]
public class EWM : BaseUnityPlugin
{
  public const string GUID = "expand_world_music";
  public const string NAME = "Expand World Music";
  public const string VERSION = "1.12";

#nullable disable
  public static CustomSyncedValue<List<Data>> valueMusicData;
  public static CustomSyncedValue<List<LocationData>> valueLocationMusicData;
  public static CustomSyncedValue<List<SoundData>> valueSoundData;
#nullable enable
  public static ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    IsLocked = true
  };
  public static GameObject ParentObj = new("ExpandWorldMusic");
  public void Awake()
  {
    ParentObj.SetActive(false);
    DontDestroyOnLoad(ParentObj);
    Log.Init(Logger);
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    valueMusicData = new(ConfigSync, "music_data");
    valueMusicData.ValueChanged += () => MusicManager.FromSetting(valueMusicData.Value);
    valueLocationMusicData = new(ConfigSync, "location_music_data");
    valueLocationMusicData.ValueChanged += () => LocationManager.FromSetting(valueLocationMusicData.Value);
    valueSoundData = new(ConfigSync, "object_music_data");
    valueSoundData.ValueChanged += () => SoundManager.FromSetting(valueSoundData.Value);
    try
    {
      MusicManager.SetupWatcher();
      LocationManager.SetupWatcher();
      SoundManager.SetupWatcher();
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
      Log.Error(e.StackTrace);
    }
  }
}
