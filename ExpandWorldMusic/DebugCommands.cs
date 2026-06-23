using System.Linq;
using HarmonyLib;

namespace ExpandWorld.Music;

[HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
public class SetCommands
{
  static void Postfix()
  {
    new Terminal.ConsoleCommand("ew_musics", "- Prints available music clips.", args =>
    {
      ZLog.Log(string.Join("\n", Clips.GetAllNames().OrderBy(k => k)));
      args.Context.AddString("Available music clips printed to the log file.");
    }, true);
    new Terminal.ConsoleCommand("ew_mixers", "- Prints available audio mixer groups.", args =>
    {
      ZLog.Log(string.Join("\n", AudioMan.instance.m_masterMixer.FindMatchingGroups("").Select(g => g.name).OrderBy(k => k)));
      args.Context.AddString("Available audio mixer groups printed to the log file.");
    }, true);
  }
}