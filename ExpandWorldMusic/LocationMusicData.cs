namespace ExpandWorld.Music;

public class LocationData
{
  public string name = "";
  public string[] clips = [];
  public float volume = 0f;
  public bool radiusFromLocation = false;
  public float radius = 10f;
  public bool loop = false;
  public bool oneTime = false;
  public bool notIfEnemies = false;
  public bool forceFade = false;
}

// Comparer for LocationData
public class LocationDataComparer : System.Collections.Generic.IEqualityComparer<LocationData>
{
  public bool Equals(LocationData? x, LocationData? y) => x?.name == y?.name;

  public int GetHashCode(LocationData obj)
  {
    if (obj is null) return 0;
    return obj.name?.GetHashCode() ?? 0;
  }
}
