using ServerSync;
using Service;

namespace ExpandWorld.Music;
public partial class Configuration
{
#nullable disable

  public static CustomSyncedValue<string> valueMusicData;
#nullable enable
  public static void Init(ConfigWrapper wrapper)
  {
    valueMusicData = wrapper.AddValue("music_data");
    valueMusicData.ValueChanged += () => Manager.FromSetting(valueMusicData.Value);
  }
}
