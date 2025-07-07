
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Service;

public class Yaml
{
  public static string YamlDirectory = Path.Combine(Paths.ConfigPath, "expand_world");
  public static string BackupDirectory = Path.Combine(Paths.ConfigPath, "expand_world_backups");
  public static string Write<T>(List<T> data) where T : class => Serializer().Serialize(data);
  public static List<T> Read<T>(string raw, string fileName, Action<string> error)
  {
    try
    {
      return Deserializer().Deserialize<List<T>>(raw) ?? [];
    }
    catch (Exception ex1)
    {
      error($"{fileName}: {ex1.Message}");
      try
      {
        return DeserializerUnSafe().Deserialize<List<T>>(raw) ?? [];
      }
      catch (Exception)
      {
        return [];
      }
    }
  }

  private static IDeserializer Deserializer() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
  private static IDeserializer DeserializerUnSafe() => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build();
  private static ISerializer Serializer() => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).DisableAliases()
    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitDefaults).Build();

  public static void WriteFile(string fileName, string content)
  {
    Directory.CreateDirectory(YamlDirectory);
    File.WriteAllText(Path.Combine(YamlDirectory, fileName), content);
  }
  public static bool Exists(string fileName)
  {
    return File.Exists(Path.Combine(YamlDirectory, fileName));
  }
  public static string ReadFile(string fileName)
  {
    var path = Path.Combine(YamlDirectory, fileName);
    if (!File.Exists(path)) return "";
    return File.ReadAllText(path);
  }

  private static void BackupFile(string filePath)
  {
    if (!File.Exists(filePath)) return;
    Directory.CreateDirectory(BackupDirectory);
    var stamp = DateTime.Now.ToString("yyyy-MM-dd");
    var name = $"{Path.GetFileNameWithoutExtension(filePath)}_{stamp}{Path.GetExtension(filePath)}.bak";
    File.Copy(filePath, Path.Combine(BackupDirectory, name), true);
  }

  private static void Setup(string folder, string pattern, Action<string> action)
  {
    Directory.CreateDirectory(folder);
    FileSystemWatcher watcher = new(folder, pattern);
    watcher.Created += (s, e) => action(e.FullPath);
    watcher.Changed += (s, e) => action(e.FullPath);
    watcher.Renamed += (s, e) => action(e.FullPath);
    watcher.Deleted += (s, e) => action(e.FullPath);
    watcher.IncludeSubdirectories = true;
    watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
    watcher.EnableRaisingEvents = true;
  }

  public static void Setup(string pattern, Action<string> action)
  {
    Setup(YamlDirectory, pattern, file =>
    {
      BackupFile(file);
      action(ReadFiles(pattern));
    });
  }
  public static string ReadFiles(string pattern)
  {
    Directory.CreateDirectory(YamlDirectory);
    var data = Directory.GetFiles(YamlDirectory, pattern, SearchOption.AllDirectories).Reverse().Select(name =>
      string.Join("\n", File.ReadAllLines(name).ToList())
    );
    return string.Join("\n", data) ?? "";
  }
}

