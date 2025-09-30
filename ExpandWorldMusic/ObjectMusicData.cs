namespace ExpandWorld.Music;

public class ObjectData
{
  public string name = "";
  public bool playOnAwake = true;
  public string closedCaptionToken = "";
  public string secondaryCaptionToken = "";
  public int captionType = 0;
  public float minimumCaptionVolume = 0f;
  public string[] audioClips = [];
  public int maxConcurrentSources = 0;
  public bool ignoreConcurrencyDistance = false;
  public float maxPitch = 0f;
  public float minPitch = 0f;
  public float maxVol = 0f;
  public float minVol = 0f;
  public float fadeInDuration = 0f;
  public float fadeOutDuration = 0f;
  public float fadeOutDelay = 0f;
  public bool fadeOutOnAwake = false;
  public bool randomPan = false;
  public float minPan = 0f;
  public float maxPan = 0f;
  public float maxDelay = 0f;
  public float minDelay = 0f;
  public bool distanceReverb = false;
  public bool useCustomReverbDistance = false;
  public float customReverbDistance = 0f;
}
