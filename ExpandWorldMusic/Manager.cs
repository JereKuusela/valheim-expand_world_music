using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HarmonyLib;
using Service;
namespace ExpandWorld.Music;

public class Manager
{
  public static string FileName = "expand_music.yaml";
  public static string Pattern = "expand_music*.yaml";
  public static List<MusicMan.NamedMusic> Originals = [];

  // Check is different from EW Data to allow working on main menu.
  public static bool IsServer() => !ZNet.instance || ZNet.instance.IsServer();
  public static void ToFile()
  {
    if (!IsServer()) return;
    if (Yaml.Exists(FileName)) return;
    var yaml = Yaml.Write(MusicMan.instance.m_music.Select(Loader.ToData).ToList());
    Yaml.WriteFile(FileName, yaml);
  }
  public static void FromFile(string lines)
  {
    if (!IsServer()) return;
    if (lines == "")
    {
      EWM.valueMusicData.Value = [];
      return;
    }
    try
    {
      var data = Yaml.Read<Data>(lines, "Music", Log.Error).ToList();
      EWM.valueMusicData.Value = data;
    }
    catch (Exception e)
    {
      Log.Error(e.Message);
      Log.Error(e.StackTrace);
      EWM.valueMusicData.Value = [];
    }
  }
  public static void FromSetting(List<Data> data)
  {
    if (IsServer() && Originals.Count == 0)
      Originals = [.. MusicMan.instance.m_music];
    var musicList = data.Select(d => Loader.FromData(d, "Music")).ToList();
    if (IsServer() && AddMissingEntries(musicList))
    {
      // Watcher triggers reload.
      return;
    }
    Log.Info($"Reloading music data ({musicList.Count} entries).");
    MusicMan.instance.m_music = musicList;
    UpdateHashes();
    var current = MusicMan.instance.m_currentMusic?.m_name ?? "";
    MusicMan.instance.Reset();
    MusicMan.instance.StartMusic(current);
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
    var yaml = Yaml.ReadFile(FileName);
    var data = Yaml.Write(missing.Select(Loader.ToData).ToList());
    // Directly appending is risky but necessary to keep comments, etc.
    yaml += "\n" + data;
    Yaml.WriteFile(FileName, yaml);
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
    Yaml.Setup(Pattern, FromFile);
  }
}


[HarmonyPatch(typeof(MusicMan), nameof(MusicMan.Awake)), HarmonyPriority(Priority.Last)]
public class InitializeContent
{
  static void Postfix()
  {
    Loader.InitializeDefaultClips();
    Manager.ToFile();
    Manager.FromFile(Yaml.ReadFiles(Manager.Pattern));
  }
}
