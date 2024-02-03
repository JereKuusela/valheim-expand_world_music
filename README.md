# Expand World Music

Allows changing event and environment musics.

Install on all clients and the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

Install [Expand World Data](https://valheim.thunderstore.io/package/JereKuusela/Expand_World_Data/).

## Configuration

The file `expand_world/expand_music.yaml` is created when loading a world.

Command `ew_musics` can be used list all available clips.

### expand_music.yaml

- name: Identifier of the music.
- clips: List of music files.
  - The file is relative to `config/expand_world/` folder.
  - Remember to include the file extension.
  - Adding for example "../music/" to the beginning of the file path can be used to change the folder.
  - Original music names can also be used.
  - If multiple files are given, the game randomly chooses one of them.
  - Note: The music files are not synced to clients. They must be provided in the mod pack.
- volume: Volume of the music.
- fadeInTime: How many seconds remaining in the file when the music starts to fade in.
- alwaysFadeOut: If true, fading is also used when there is no queued music file.
- ambientMusic: If true, loop is set from the game settings.
- loop: If true, the same file is played again when the music ends. If false, then a new file is randomly selected.
- resume: If true and loop is true, the music continues from the last position.

## Credits

Thanks for probablykory for the clip loading code!

Sources: [GitHub](https://github.com/JereKuusela/valheim-expand_world_music)
Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)
