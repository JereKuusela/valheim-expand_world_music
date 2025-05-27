using Service;
using UnityEngine;

namespace ExpandWorld.Music;

public class LocationLoader
{
  public static LocationData? ToData(ZoneSystem.ZoneLocation location)
  {
    if (!location.m_prefab.m_assetID.IsValid) return null;
    location.m_prefab.Load();
    var locationMusic = location.m_prefab.Asset.GetComponentInChildren<MusicLocation>();
    if (!locationMusic)
    {
      location.m_prefab.Release();
      return null;
    }
    var audioSource = locationMusic.GetComponent<AudioSource>();
    if (!audioSource)
    {
      location.m_prefab.Release();
      return null;
    }
    LocationData data = new()
    {
      clips = [audioSource.clip?.name ?? ""],
      name = location.m_prefab.Asset.name,
      volume = audioSource.volume,
      forceFade = locationMusic.m_forceFade,
      loop = audioSource.loop,
      notIfEnemies = locationMusic.m_notIfEnemies,
      oneTime = locationMusic.m_oneTime,
      radius = locationMusic.m_radius,
      radiusFromLocation = locationMusic.m_addRadiusFromLocation
    };
    location.m_prefab.Release();
    return data;
  }
}