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
      ZLog.Log(string.Join("\n", Loader.Clips.Keys.OrderBy(k => k)));
      args.Context.AddString("Available music clips printed to the log file.");
    }, true);
  }
}