using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpandWorldData;
using UnityEngine;
using UnityEngine.Networking;

namespace ExpandWorld.Music;
public class Loader
{
  public static Dictionary<string, AudioClip> Clips = [];
  public static MusicMan.NamedMusic FromData(Data data)
  {
    return new()
    {
      m_alwaysFadeout = data.alwaysFadeOut,
      m_ambientMusic = data.ambientMusic,
      m_clips = data.clips.Select(PreloadClipCoroutine).Where(c => c != null).Cast<AudioClip>().ToArray(),
      m_enabled = true,
      m_fadeInTime = data.fadeInTime,
      m_loop = data.loop,
      m_name = data.name,
      m_resume = data.resume,
      m_volume = data.volume
    };
  }
  public static Data ToData(MusicMan.NamedMusic music)
  {
    return new()
    {
      alwaysFadeOut = music.m_alwaysFadeout,
      ambientMusic = music.m_ambientMusic,
      clips = music.m_clips.Select(c => c?.name ?? "empty").ToArray(),
      fadeInTime = music.m_fadeInTime,
      loop = music.m_loop,
      name = music.m_name,
      resume = music.m_resume,
      volume = music.m_volume
    };
  }
  public static void InitializeDefaultClips()
  {
    // Higher priority for actual music clips.
    Clips = MusicMan.instance.m_music.SelectMany(m => m.m_clips).Distinct(new Comparer()).ToDictionary(c => c?.name ?? "empty", c => c);
    var clips = Resources.FindObjectsOfTypeAll<AudioClip>();
    foreach (var clip in clips)
    {
      var name = clip.name;
      var order = 2;
      while (Clips.ContainsKey(name))
      {
        // Expected to have same clips as in the music manager.
        if (Clips[name] == clip)
          break;
        name = clip.name + "_" + order++;
      }
      if (Clips.ContainsKey(name))
        continue;

      Clips.Add(name, clip);
    }
  }
  private static AudioClip? PreloadClipCoroutine(string path)
  {
    if (Clips.ContainsKey(path))
      return Clips[path];
    if (!File.Exists(path))
      path = Path.Combine(EWD.YamlDirectory, path);
    if (Clips.ContainsKey(path))
      return Clips[path];
    if (!File.Exists(path))
    {
      EWM.LogWarning("Can't find audio clip at " + path);
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
      Clips.Add(path, clip);
      return clip;
    }
    catch
    {
      EWM.LogWarning("Failed to load audio clip at " + path);
    }
    return null;
  }
}

public class Comparer : IEqualityComparer<AudioClip>
{
  // Respawn has a null clip.
  public bool Equals(AudioClip x, AudioClip y) => x?.name == y?.name;

  public int GetHashCode(AudioClip obj) => obj?.name.GetStableHashCode() ?? 0;
}