using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
namespace ExpandWorld.Music;

public class Manager
{
  public static string FileName = "expand_music.yaml";
  public static string FilePath = Path.Combine(EWD.YamlDirectory, FileName);
  public static string Pattern = "expand_music*.yaml";
  public static List<MusicMan.NamedMusic> Originals = [];


  public static void ToFile()
  {
    if (Helper.IsClient()) return;
    if (File.Exists(FilePath)) return;
    var yaml = DataManager.Serializer().Serialize(MusicMan.instance.m_music.Select(Loader.ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    if (Helper.IsClient()) return;
    var yaml = DataManager.Read(Pattern);
    Set(yaml);
    Configuration.valueMusicData.Value = yaml;
  }
  public static void FromSetting(string yaml)
  {
    if (Helper.IsClient()) Set(yaml);
  }
  private static void Set(string yaml)
  {
    if (Helper.IsServer() && Originals.Count == 0)
      Originals = [.. MusicMan.instance.m_music];
    if (yaml == "") return;
    try
    {
      var data = DataManager.Deserialize<Data>(yaml, FileName).Select(Loader.FromData).ToList();
      if (data.Count == 0)
      {
        EWM.LogWarning($"Failed to load any music data.");
        return;
      }
      if (ExpandWorldData.Configuration.DataMigration && Helper.IsServer() && AddMissingEntries(data))
      {
        // Watcher triggers reload.
        return;
      }
      EWM.LogInfo($"Reloading music data ({data.Count} entries).");
      MusicMan.instance.m_music = data;
    }
    catch (Exception e)
    {
      EWM.LogError(e.Message);
      EWM.LogError(e.StackTrace);
    }
  }
  private static bool AddMissingEntries(List<MusicMan.NamedMusic> entries)
  {
    var missingKeys = Originals.Select(e => e.m_name).Distinct().ToHashSet();
    foreach (var item in entries)
      missingKeys.Remove(item.m_name);
    if (missingKeys.Count == 0) return false;
    var missing = Originals.Where(item => missingKeys.Contains(item.m_name)).ToList();
    EWM.LogWarning($"Adding {missing.Count} missing music to the expand_music.yaml file.");
    foreach (var item in missing)
      EWM.LogWarning(item.m_name);
    var yaml = File.ReadAllText(FilePath);
    var data = DataManager.Serializer().Serialize(missing.Select(Loader.ToData));
    // Directly appending is risky but necessary to keep comments, etc.
    yaml += "\n" + data;
    File.WriteAllText(FilePath, yaml);
    return true;
  }
  public static void SetupWatcher()
  {
    DataManager.SetupWatcher(Pattern, FromFile);
  }
}


[HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start)), HarmonyPriority(Priority.Last)]
public class InitializeContent
{
  static void Postfix()
  {
    Loader.InitializeDefaultClips();
    if (Helper.IsServer())
    {
      Manager.ToFile();
      Manager.FromFile();
    }
  }
}
