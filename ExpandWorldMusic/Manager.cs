using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpandWorldData;
using HarmonyLib;
using Service;
namespace ExpandWorld.Music;

public class Manager
{
  public static string FileName = "expand_music.yaml";
  public static string FilePath = Path.Combine(Yaml.Directory, FileName);
  public static string Pattern = "expand_music*.yaml";
  public static List<MusicMan.NamedMusic> Originals = [];

  // Check is different from EW Data to allow working on main menu.
  public static bool IsServer() => !ZNet.instance || ZNet.instance.IsServer();
  public static void ToFile()
  {
    if (!IsServer()) return;
    if (File.Exists(FilePath)) return;
    var yaml = Yaml.Serializer().Serialize(MusicMan.instance.m_music.Select(Loader.ToData).ToList());
    File.WriteAllText(FilePath, yaml);
  }
  public static void FromFile()
  {
    if (!IsServer()) return;
    var yaml = DataManager.Read(Pattern);
    Set(yaml);
    Configuration.valueMusicData.Value = yaml;
  }
  public static void FromSetting(string yaml)
  {
    if (!IsServer()) Set(yaml);
  }
  private static void Set(string yaml)
  {
    if (IsServer() && Originals.Count == 0)
      Originals = [.. MusicMan.instance.m_music];
    if (yaml == "") return;
    try
    {
      var data = Yaml.Deserialize<Data>(yaml, FileName).Select(Loader.FromData).ToList();
      if (data.Count == 0)
      {
        Log.Warning($"Failed to load any music data.");
        return;
      }
      if (ExpandWorldData.Configuration.DataMigration && Helper.IsServer() && AddMissingEntries(data))
      {
        // Watcher triggers reload.
        return;
      }
      Log.Info($"Reloading music data ({data.Count} entries).");
      MusicMan.instance.m_music = data;
      UpdateHashes();
      var current = MusicMan.instance.m_currentMusic?.m_name ?? "";
      MusicMan.instance.Reset();
      MusicMan.instance.StartMusic(current);
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
      Log.Error(e.StackTrace);
    }
  }
  private static bool AddMissingEntries(List<MusicMan.NamedMusic> entries)
  {
    var missingKeys = Originals.Select(e => e.m_name).Distinct().ToHashSet();
    foreach (var item in entries)
      missingKeys.Remove(item.m_name);
    if (missingKeys.Count == 0) return false;
    var missing = Originals.Where(item => missingKeys.Contains(item.m_name)).ToList();
    Log.Warning($"Adding {missing.Count} missing music to the expand_music.yaml file.");
    foreach (var item in missing)
      Log.Warning(item.m_name);
    var yaml = File.ReadAllText(FilePath);
    var data = Yaml.Serializer().Serialize(missing.Select(Loader.ToData));
    // Directly appending is risky but necessary to keep comments, etc.
    yaml += "\n" + data;
    File.WriteAllText(FilePath, yaml);
    return true;
  }
  private static void UpdateHashes()
  {
    var mm = MusicMan.instance;
    mm.m_musicHashes.Clear();
    foreach (MusicMan.NamedMusic music in mm.m_music)
    {
      if (!music.m_enabled) continue;
      if (music.m_clips.Length == 0 || music.m_clips[0] == null) continue;
      mm.m_musicHashes.Add(music.m_name.GetStableHashCode(), music);
    }
  }
  public static void SetupWatcher()
  {
    Yaml.SetupWatcher(Pattern, FromFile);
  }
}


[HarmonyPatch(typeof(MusicMan), nameof(MusicMan.Awake)), HarmonyPriority(Priority.Last)]
public class InitializeContent
{
  static void Postfix()
  {
    Loader.InitializeDefaultClips();
    Manager.ToFile();
    Manager.FromFile();
  }
}
