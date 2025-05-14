using System;
using System.IO;
using BepInEx;
using HarmonyLib;
using ServerSync;
using Service;
namespace ExpandWorld.Music;
[BepInPlugin(GUID, NAME, VERSION)]
public class EWM : BaseUnityPlugin
{
  public const string GUID = "expand_world_music";
  public const string NAME = "Expand World Music";
  public const string VERSION = "1.7";
  public static string YamlDirectory = Path.Combine(Paths.ConfigPath, "expand_world");
  public static string BackupDirectory = Path.Combine(Paths.ConfigPath, "expand_world_backups");

#nullable disable
  public static CustomSyncedValue<string> valueMusicData;
#nullable enable
  public static ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    IsLocked = true
  };
  public void Awake()
  {
    Log.Init(Logger);
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    valueMusicData = new(ConfigSync, "music_data");
    valueMusicData.ValueChanged += () => Manager.FromSetting(valueMusicData.Value);
    try
    {
      Manager.SetupWatcher();
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
      Log.Error(e.StackTrace);
    }
  }
}
