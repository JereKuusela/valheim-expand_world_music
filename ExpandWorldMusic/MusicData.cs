using System;
using System.Collections.Generic;

namespace ExpandWorld.Music;

[Serializable]
public class Data
{
  public string name = "";
  public List<string> clips = [];
  public float volume = 0f;
  public float fadeInTime = 0f;
  public bool alwaysFadeOut = false;
  public bool ambientMusic = false;
  public bool loop = false;
  public bool resume = false;
}
