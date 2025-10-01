namespace ExpandWorld.Music;

public class MusicLoader
{
  public static MusicMan.NamedMusic FromData(Data data, string fileName)
  {
    return new()
    {
      m_alwaysFadeout = data.alwaysFadeOut,
      m_ambientMusic = data.ambientMusic,
      m_clips = Clips.GetClips(data.clips),
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
      clips = Clips.GetNames(music.m_clips),
      fadeInTime = music.m_fadeInTime,
      loop = music.m_loop,
      name = music.m_name,
      resume = music.m_resume,
      volume = music.m_volume
    };
  }

}
