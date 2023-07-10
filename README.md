﻿# Quintessential
the Opus Magnum mod loader.

Quintessential extends the game Opus Magnum with the ability to load mods, and provides mods with the groundwork that makes compatibility possible.

To install or update Quintessential, use the installer tool [Opus Mutatum](https://github.com/QuintessentialOM/OpusMutatum). You can find a guide [here](https://github.com/QuintessentialOM/Quintessential/wiki/How-to-mod-the-game). To install a mod, place it zipped into the `Opus Magnum/Mods` folder generated by Quintessential.

The modded version of the game becomes a separate executable to the vanilla game, does not connect to Steam, and stores save data at a separate location, so you can safely use it alongside the ordinary game without breaking anything.

## For Users

Quintessential includes some user-facing features and changes.
- A "Mods" button appears on the pause screen, allowing you to view and configure installed mods.
- The puzzle editor becomes scrollable, and all puzzles can have their allowed instructions modified, and be converted to/from a more flexible modded format.
- Modded puzzles can also have reagent and product names modified, as well as any options introduced by mods (such as allowing the use of new parts).
- Exported GIFs include a Quintessence symbol marker at the top right plus the Quintessential version currently used.
- For a number of reasons, Steam integration is disabled. You cannot download Steam Workshop puzzles in-game, but they can be manually copied from the vanilla version and played in the modded version as custom puzzles.

## For Modders

Quintessential allows mods to hook into the game using [MonoMod](https://github.com/MonoMod/MonoMod) to perform arbitrary changes, and provides a number of helper functions and scaffolding to make modding easier and more compatible.

It can also load mods consisting only of texture replacements; any modded `Content` directory takes precedence over vanilla textures, or the textures of its dependencies.

Custom campaigns (and in the future, journals) can also be created using only puzzle files and assets, along with YAML files specifying their setup. These are still a work in progress.

For an up-to-date example of a mod with custom mechanics, see [Unstable Elements](https://github.com/l-Luna/UnstableElements).