- 1.12
  - Adds support for modifying sound effects.
  - Adds support for adding new sound effects.
  - Changes location music entries to be modification (if missing, nothing is changed).
  - Fixes custom musics not working as location music.

- 1.11
  - Fixes error near some locations. This was caused by incorrect default value for field `oneTime`.

- 1.10
  - Adds a message to the `ew_musics` command to indicate that the available music clips were printed to the log file.
  - Fixes config directory not being automatically created.

- 1.9
  - Fixes the same location having multiple location entries causing multiple location music entries. Now only the first entry is used.
  - Fixes multiple same location music entries causing an error. Now only the first entry is used.

- 1.8
  - Adds support for location music.
