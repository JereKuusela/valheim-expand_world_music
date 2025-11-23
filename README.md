# Expand World Music

Allows changing event and environment musics.

Install on all clients and the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Can also be used to customize your own music by only installing on the client.

## Configuration

Following files are created when loading a world:

- `expand_world/expand_music.yaml`
- `expand_world/expand_location_music.yaml`
- `expand_world/ref_expand_location_music.yaml`
- `expand_world/expand_sounds.yaml`
- `expand_world/ref_expand_sounds.yaml`

Location music and sound files are empty by default. Reference files contain copy of default data. This prevents unnecessary network traffic as most sounds are not changed.

Copy needed sounds from the reference files to your own files and modify them as needed.

Command `ew_musics` can be used list all available clips to the log file.

### expand_music.yaml

- name: Id of the music.
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
- volume (default `1.0`): Volume of the music.
- fadeInTime (default `0.0`): How many seconds remaining in the file when the music starts to fade in.
- alwaysFadeOut (default `false`): If true, fading is also used when there is no queued music file.
- ambientMusic (default `false`): If true, loop is set from the game settings.
- loop (default `false`): If true, the same file is played again when the music ends. If false, then a new file is randomly selected.
- resume (default `false`): If true and loop is true, the music continues from the last position.

### expand_location_music.yaml

- name: Id of the location.
- clips: List of music files.
- volume (default `0.0`): Volume of the music.
- loop (default `false`): If true, the music restarts automatically when it ends.
- oneTime (default `false`): If true, the music is played only once per location.
  - Note: Setting this to true causes errors for some locations.
- notIfEnemies (default `false`): If true, the music is not played if there are enemies nearby.
- forceFade (default `false`): If true, the volume is lowered when the music is near ending.
- radius (default `10.0`): Radius of the music area in meters.
- radiusFromLocation (default `false`): If true, radius of the location is added to the music radius.

### expand_sounds.yaml

- name: Id of the sound.
  - New spawnable object (prefab) is created if the id doesn't exist.
- clips: List of sound files (similar to music clips).
- persistent (default `false`): If true, the spawned sound object is saved to the world file.
  - This can be useful for long duration sounds but usually most sounds should be temporary.
- triggerTimeout (default `true`): If true, the timeout for automatic destroy is instantly started.
  - Usually you want to keep this true unless you have some other way for triggering the timeout.
- timeout: How many seconds until the object is automatically destroyed.
  - This should be at least the length of the longest clip to ensure full playback.
- playOnAwake (default `true`): If true, the sound plays immediately when loaded.
  - Usualy you want to keep this true unless you have some other way to trigger playback.
- closedCaptionToken: Text shown for closed caption display.
- minimumCaptionVolume (default `0.3`): Minimum volume for captions to appear.
- maxConcurrentSources: Maximum number of concurrent sound sources (0 = unlimited).
  - This can be used to prevent playback when too many sounds are already playing.
- ignoreConcurrencyDistance (default `false`): If true, ignores distance when checking concurrency.
  - Not sure when this would be useful. This would make this sound affect every concurrent check regardless of distance.
- maxPitch (default `1.0`): Maximum pitch.
- minPitch (default `1.0`): Minimum pitch.
- maxVol (default `0.0`): Maximum volume.
- minVol (default `0.0`): Minimum volume.
- fadeInDuration (default `0.0`): Duration in seconds for fade-in effect.
- fadeOutDuration (default `0.0`): Duration in seconds for fade-out effect.
- fadeOutDelay (default `0.0`): Delay before fade-out begins.
- fadeOutOnAwake (default `false`): If true, applies fade-out when sound starts.
- randomPan (default `false`): If true, applies random stereo panning.
- maxPan (default `1.0`): Maximum stereo panning.
- minPan (default `-1.0`): Minimum stereo panning.
- maxDelay (default `0.0`): Maximum delay before sound plays.
- minDelay (default `0.0`): Minimum delay before sound plays.
- distanceReverb (default `true`): If true, applies reverb based on distance.
- useCustomReverbDistance (default `false`): If true, uses custom reverb distance instead of default.
- customReverbDistance (default `10.0`): Custom distance for reverb calculation.

### Examples

#### Adding new music clips

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

#### Customizing sounds

```yaml
- name: "sfx_build_hammer"
  clips:
    - "sounds/custom_hammer.ogg"
    - "sounds/custom_hammer2.ogg"
  maxVol: 0.8
  minVol: 0.6
  maxPitch: 1.2
  minPitch: 0.8
  fadeInDuration: 0.1
  fadeOutDuration: 0.2
  randomPan: true
  maxConcurrentSources: 3
  timeout: 0.1
- name: "ambient_forest"
  clips:
    - "sounds/forest_ambient.ogg"
  persistent: true
  maxVol: 0.4
  minVol: 0.3
  distanceReverb: true
  customReverbDistance: 15.0
  useCustomReverbDistance: true
  fadeInDuration: 2.0
  fadeOutDuration: 3.0
```

## Credits

Thanks for probablykory for the clip loading code!

Sources: [GitHub](https://github.com/JereKuusela/valheim-expand_world_music)
Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)
