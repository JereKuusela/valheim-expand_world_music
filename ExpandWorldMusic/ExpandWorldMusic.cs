using System;
using BepInEx;
using HarmonyLib;
using Service;
namespace ExpandWorld.Music;
[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("expand_world_data", "1.50")]
public class EWM : BaseUnityPlugin
{
  public const string GUID = "expand_world_music";
  public const string NAME = "Expand World Music";
  public const string VERSION = "1.5";
  public static ServerSync.ConfigSync ConfigSync = new(GUID)
  {
    DisplayName = NAME,
    CurrentVersion = VERSION,
    ModRequired = true,
    IsLocked = true
  };
  public void Awake()
  {
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
      Log.Error(e.StackTrace);
    }
  }
}
