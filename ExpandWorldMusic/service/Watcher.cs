

using System;
using System.IO;
using System.Linq;
using BepInEx;

namespace Service;

public class Watcher
{
  private static void Setup(string folder, string pattern, Action<string> action)
  {
    FileSystemWatcher watcher = new(folder, pattern);
    watcher.Created += (s, e) => action(e.FullPath);
    watcher.Changed += (s, e) => action(e.FullPath);
    watcher.Renamed += (s, e) => action(e.FullPath);
    watcher.Deleted += (s, e) => action(e.FullPath);
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }

  public static void Setup(string directory, string backDirectory, string pattern, Action<string> action)
  {
    Setup(directory, pattern, file =>
    {
      BackupFile(file, backDirectory);
      action(ReadFiles(directory, pattern));
    });
  }
  private static void BackupFile(string backDirectory, string path)
  {
    if (!File.Exists(path)) return;
    if (!Directory.Exists(backDirectory))
      Directory.CreateDirectory(backDirectory);
    var stamp = DateTime.Now.ToString("yyyy-MM-dd");
    var name = $"{Path.GetFileNameWithoutExtension(path)}_{stamp}{Path.GetExtension(path)}.bak";
    File.Copy(path, Path.Combine(backDirectory, name), true);
  }
  public static string ReadFiles(string directory, string pattern)
  {
    if (!Directory.Exists(directory))
      Directory.CreateDirectory(directory);
    var data = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories).Reverse().Select(name =>
      string.Join("\n", File.ReadAllLines(name).ToList())
    );
    return string.Join("\n", data) ?? "";
  }

}