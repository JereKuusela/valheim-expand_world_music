using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Service;
namespace ExpandWorld.Music;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("expand_world_data", BepInDependency.DependencyFlags.HardDependency)]
public class EWM : BaseUnityPlugin
{
  public const string GUID = "expand_world_music";
  public const string NAME = "Expand World Music";
  public const string VERSION = "1.0";
#nullable disable
  public static ManualLogSource Log;
#nullable enable
  public static void LogWarning(string message) => Log.LogWarning(message);
  public static void LogError(string message) => Log.LogError(message);
  public static void LogInfo(string message) => Log.LogInfo(message);
  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    ModRequired = true,
    IsLocked = true
  };
  public void Awake()
  {
    Log = Logger;
    ConfigWrapper wrapper = new("expand_music_config", Config, ConfigSync, () => { });
    Configuration.Init(wrapper);
    Harmony harmony = new(GUID);
    harmony.PatchAll();
    try
    {
      if (ExpandWorldData.Configuration.DataReload)
      {
        Manager.SetupWatcher();
      }
    }
    catch (Exception e)
    {
      Log.LogError(e);
    }
  }
}
