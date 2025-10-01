using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Service;
using UnityEngine;
using UnityEngine.Networking;

namespace ExpandWorld.Music;

public class Clips
{
  private static Dictionary<string, AudioClip>? AudioClips;

  public static AudioClip? GetClip(string name)
  {
    AudioClips ??= InitializeDefaultClips();
    if (AudioClips.ContainsKey(name))
      return AudioClips[name];
    var clip = PreloadClipCoroutine(name);
    if (clip != null)
      AudioClips[name] = clip;
    return clip;
  }
  public static AudioClip? GetRandomClip(List<string> names)
  {
    var clipName = RandomizeClip(names);
    if (clipName == "") return null;
    return GetClip(clipName);
  }
  private static string RandomizeClip(List<string> clips)
  {
    if (clips.Count == 0) return "";
    if (clips.Count == 1) return clips[0];
    return clips[UnityEngine.Random.Range(0, clips.Count)];
  }
  public static AudioClip[] GetClips(List<string> names)
  {
    return names.Select(GetClip).Where(c => c != null).Cast<AudioClip>().ToArray();
  }
  public static List<string> GetNames(AudioClip[] clips)
  {
    return [.. clips.Where(c => c != null).Select(c => c.name)];
  }
  public static List<string> GetNames(AudioClip clip)
  {
    var name = clip?.name;
    if (name == null) return [];
    return [name];
  }
  public static List<string> GetAllNames() => AudioClips?.Keys.ToList() ?? [];

  private static Dictionary<string, AudioClip> InitializeDefaultClips()
  {
    // Higher priority for actual music clips.
    var audioClips = MusicMan.instance.m_music.SelectMany(m => m.m_clips).Where(c => c?.name != null).Distinct(new Comparer()).ToDictionary(c => c.name, c => c);
    // Old configs would have this, no need to show them a warning about it.
    audioClips["empty"] = null!;
    var clips = Resources.FindObjectsOfTypeAll<AudioClip>();
    foreach (var clip in clips)
    {
      var name = clip.name;
      var order = 2;
      while (audioClips.ContainsKey(name))
      {
        // Expected to have same clips as in the music manager.
        if (audioClips[name] == clip)
          break;
        name = clip.name + "_" + order++;
      }
      if (audioClips.ContainsKey(name))
        continue;

      audioClips.Add(name, clip);
    }
    return audioClips;
  }
  private static AudioClip? PreloadClipCoroutine(string path)
  {
    if (!File.Exists(path))
      path = Path.Combine(Yaml.YamlDirectory, path);
    if (!File.Exists(path))
    {
      Log.Warning($"Can't find audio clip at {path}");
      return null;
    }
    var uri = "file:///" + path.Replace("\\", "/");
    try
    {
      var loader = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.UNKNOWN) ?? throw new Exception();
      loader.SendWebRequest();
      while (!loader.isDone)
      {
      }
      var downloadHandlerAudioClip = (DownloadHandlerAudioClip)loader.downloadHandler;
      var clip = downloadHandlerAudioClip.audioClip ?? throw new Exception();
      clip.name = Path.GetFileNameWithoutExtension(path);
      return clip;
    }
    catch
    {
      Log.Warning("Failed to load audio clip at " + path);
    }
    return null;
  }
}

public class Comparer : IEqualityComparer<AudioClip>
{
  // Respawn has a null clip.
  public bool Equals(AudioClip x, AudioClip y) => x.name == y.name;

  public int GetHashCode(AudioClip obj) => obj.name.GetStableHashCode();
}