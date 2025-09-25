# Expand World Music

Allows changing event and environment musics.

Install on all clients and the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Can also be used to customize your own music by only installing on the client.

## Configuration

The files `expand_world/expand_music.yaml` and `expand_world/expand_location_music.yaml` are created when loading a world.

Command `ew_musics` can be used list all available clips to the log file.

### expand_music.yaml

- name: Identifier of the music.
  - This is what the game uses to find the music.
  - Adding new identifiers doesn't automatically do anything.
  - You need some other mod like Expand World Data to change used music in the `expand_biomes.yaml` file.
- clips: List of music files.
  - The file path is relative to `config/expand_world/` folder.
  - You can use `../` to go up one folder.
  - Remember to include the file extension.
  - Original music names can also be used.
  - If multiple files are given, the game randomly chooses one of them.
  - Note: The music files are not synced to clients. They must be provided in the mod pack.
- volume: Volume of the music.
- fadeInTime: How many seconds remaining in the file when the music starts to fade in.
- alwaysFadeOut: If true, fading is also used when there is no queued music file.
- ambientMusic: If true, loop is set from the game settings.
- loop: If true, the same file is played again when the music ends. If false, then a new file is randomly selected.
- resume: If true and loop is true, the music continues from the last position.

### expand_location_music.yaml

- name: Identifier of the location.
- clips: List of music files.
- volume: Volume of the music.
- loop: If true, the music restarts automatically when it ends.
- oneTime: If true, the music is played only once per location.
  - Note: Setting this to true causes errors for some locations.
- notIfEnemies: If true, the music is not played if there are enemies nearby.
- forceFade: If true, the volume is lowered when the music is near ending.
- radius: Radius of the music area in meters.
- radiusFromLocation: If true, radius of the location is added to the music radius.

### Example

Adding a new music clips:

```yaml
- name: "meadowsDay"
  clips:
  # This is in config/music/ folder.
    - "../music/epicSong.ogg"
  # This is in config/expand_world/music folder.
    - "music/epicSong2.ogg"
  volume: 0.5
  fadeInTime: 5
  alwaysFadeOut: true
  ambientMusic: false
  loop: true
  resume: false
- name: "meadowsNight"
  clips:
  # This is in config/expand_world folder.
    - "otherSong.ogg"
  # This is in music folder.
    - "../../music/some_music.ogg"
  volume: 0.5
  fadeInTime: 5
  alwaysFadeOut: true
  ambientMusic: false
  loop: true
  resume: false
```

Using the new music in `expand_biomes.yaml`:

```yaml
- biome: Meadows
  musicDay: meadowsDay
  musicNight: meadowsNight
  # Other fields...
```

## Credits

Thanks for probablykory for the clip loading code!

Sources: [GitHub](https://github.com/JereKuusela/valheim-expand_world_music)
Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)
