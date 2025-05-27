namespace ExpandWorld.Music;

public class LocationData
{
  public string name = "";
  public string[] clips = [];
  public float volume = 0f;
  public bool radiusFromLocation = true;
  public float radius = 10f;
  public bool loop = false;
  public bool oneTime = true;
  public bool notIfEnemies = true;
  public bool forceFade = false;
}
