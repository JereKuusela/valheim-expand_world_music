using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace ExpandWorld.Music;

[Serializable]
public class SoundData
{
  public string name = "";
  public bool persistent = false;
  [DefaultValue(true)]
  public bool triggerTimeout = true;
  public float timeout = 0f;
  [DefaultValue(true)]
  public bool playOnAwake = true;
  [DefaultValue("")]
  public string closedCaptionToken = "";
  [DefaultValue(0.3f)]
  public float minimumCaptionVolume = 0.3f;
  public List<string> clips = [];
  public int maxConcurrentSources = 0;
  public bool ignoreConcurrencyDistance = false;
  [DefaultValue(1f)]
  public float maxPitch = 1f;
  [DefaultValue(1f)]
  public float minPitch = 1f;
  [DefaultValue(0f)]
  public float maxVol = 0f;
  [DefaultValue(0f)]
  public float minVol = 0f;
  public float fadeInDuration = 0f;
  public float fadeOutDuration = 0f;
  public float fadeOutDelay = 0f;
  public bool fadeOutOnAwake = false;
  public bool randomPan = false;
  [DefaultValue(-1f)]
  public float minPan = -1f;
  [DefaultValue(1f)]
  public float maxPan = 1f;
  public float maxDelay = 0f;
  public float minDelay = 0f;
  [DefaultValue(true)]
  public bool distanceReverb = true;
  public bool useCustomReverbDistance = false;
  [DefaultValue(10f)]
  public float customReverbDistance = 10f;
}
