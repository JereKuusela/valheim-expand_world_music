using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace ExpandWorld.Music;

internal static class GameAccess
{
  internal static readonly AccessTools.FieldRef<MusicMan, Dictionary<int, MusicMan.NamedMusic>> MusicHashes =
    AccessTools.FieldRefAccess<MusicMan, Dictionary<int, MusicMan.NamedMusic>>("m_musicHashes");
  internal static readonly AccessTools.FieldRef<LocationProxy, bool> LocationProxyNeedsSpawn =
    AccessTools.FieldRefAccess<LocationProxy, bool>("m_locationNeedsSpawn");
  internal static readonly AccessTools.FieldRef<LocationProxy, GameObject> LocationProxyInstance =
    AccessTools.FieldRefAccess<LocationProxy, GameObject>("m_instance");
  internal static readonly AccessTools.FieldRef<LocationProxy, ZNetView> LocationProxyNetView =
    AccessTools.FieldRefAccess<LocationProxy, ZNetView>("m_nview");

  internal static readonly AccessTools.FieldRef<MusicLocation, ZNetView> MusicLocationNetView =
    AccessTools.FieldRefAccess<MusicLocation, ZNetView>("m_nview");
  internal static readonly AccessTools.FieldRef<MusicLocation, float> MusicLocationBaseVolume =
    AccessTools.FieldRefAccess<MusicLocation, float>("m_baseVolume");

  internal static readonly AccessTools.FieldRef<ZNetScene, Dictionary<int, GameObject>> NamedPrefabs =
    AccessTools.FieldRefAccess<ZNetScene, Dictionary<int, GameObject>>("m_namedPrefabs");

  private static readonly MethodInfo StartMusicByName = AccessTools.Method(
    typeof(MusicMan), "StartMusic", [typeof(string)]);
  private static readonly MethodInfo GetCurrentMusicMethod = AccessTools.Method(
    typeof(MusicMan), "GetCurrentMusic");
  private static readonly MethodInfo SetPlayedBySender = AccessTools.Method(
    typeof(MusicLocation), "SetPlayed", [typeof(long)]);

  internal static void StartMusic(MusicMan musicMan, string name) =>
    StartMusicByName.Invoke(musicMan, [name]);

  internal static string GetCurrentMusic(MusicMan musicMan) =>
    (string)GetCurrentMusicMethod.Invoke(musicMan, null);

  internal static void SetPlayed(MusicLocation musicLocation, long sender) =>
    SetPlayedBySender.Invoke(musicLocation, [sender]);
}
