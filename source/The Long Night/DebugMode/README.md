**Debug Mode** is an open-source [Stardew Valley](http://stardewvalley.net/) mod which lets you
press `~` to view useful metadata and unlock the game's debug commands (including teleportation
and time manipulation).

## Contents
* [Install](#install)
* [Use](#use)
* [Configure](#configure)
* [Compatibility](#compatibility)
* [See also](#see-also)

## Install
1. [Install the latest version of SMAPI](https://smapi.io/).
2. [Install this mod from Nexus mods](http://www.nexusmods.com/stardewvalley/mods/679/).
3. Run the game using SMAPI.

## Use
Press the `~` key (configurable) to enable or disable debug mode. This will...

1. Show cursor crosshairs, the map name and cursor's tile position, and the game's built-in debug info:

   ![screenshot](screenshots/world.png)

2. When a menu is open, it'll also show the menu name and (if applicable) submenu name:

   ![screenshot](screenshots/menu.png)

3. When an event in progress, it'll also show the internal event ID and event command progress:

   ![screenshot](screenshots/event.png)

4. When a festival is in progress, it'll show the internal festival name:

   ![screenshot](screenshots/festival.png)

5. Unlock the game's built-in debug commands:

   hotkey | action | multiplayer notes
   :----- | :----- | -------------------
   `T`    | Add one hour to the clock. | Main player only, affects all players.
   `SHIFT` + `Y` | Subtract 10 minutes from the clock. | Main player only, affects all players.
   `Y`    | Add 10 minutes to the clock. | Main player only, affects all players.
   `1`    | Warp to the mountain (facing Robin's house). | Affects current player.
   `2`    | Warp to the town (on the path between the town and community center). | Affects current player.
   `3`    | Warp to the farm (at your farmhouse door). | Affects current player.
   `4`    | Warp to the forest (near the traveling cart). | Affects current player.
   `5`    | Warp to the beach (left of Elliott's house). | Affects current player.
   `6`    | Warp to the mine (at the inside entrance). | Affects current player.
   `7`    | Warp to the desert (in Sandy's shop). | Affects current player.
   `K`    | Move down one mine level. If not currently in the mine, warp to it. | Affects current player.
   `F5`   | Show or hide all player sprites. | Visible to current player.
   `F7`   | Draw a tile grid. | Visible to current player.
   `F8`   | Show a debug command input window (not currently documented). | Depends on the command used.
   `B`    | Shift the toolbar to show the next higher inventory row. | Affects current player.
   `N`    | Shift the toolbar to show the next lower inventory row. | Affects current player.

6. If you set `AllowDangerousCommands: true` in the [configuration](#configuration) (disabled by
   default), also unlock these debug commands:

   hotkey | action | multiplayer notes
   :----- | :----- | -----------------
   `P`    | Immediately go to bed and start the next day. | Affects current player. Will take effect when you change location.
   `M`    | Immediately go to bed and start the next season. | Affects all players if used by main player; else equivalent to `P`. Will take effect when you change location.
   `H`    | Randomise the player's hat. | Affects current player.
   `I`    | Randomise the player's hair. | Affects current player.
   `J`    | Randomise the player's shirt and pants. | **Affects all players!**
   `L`    | Randomise the player. | **Affects all players!**
   `U`    | Randomise the farmhouse wallpaper and floors. | Affects main player's farmhouse (even if used by another player).
   `F10`  | Starts a multiplayer server for the current save (if not already started). | No meaningful effect.

## Configure
The mod creates a `config.json` file in its mod folder the first time you run it. You can open that
file in a text editor to configure the mod.

These are the available settings:

setting           | what it affects
:---------------- | :------------------
`Controls`        | The configured controller, keyboard, and mouse buttons (see [key bindings](https://stardewvalleywiki.com/Modding:Key_bindings)). You can separate multiple buttons with commas. The default value is `~` to toggle debug mode.
`AllowDangerousCommands` | Default `false`. This allows debug commands which end the current day/season & save, randomise your player or farmhouse decorations, or crash the game. Only change this if you're aware of the consequences.

## Compatibility
Debug Mode is compatible with Stardew Valley 1.3+ on Linux/Mac/Windows, both single-player and
multiplayer. Commands may have different effects in multiplayer; see multiplayer notes for each
command.

## See also
* [Release notes](release-notes.md)
* [Nexus mod](http://www.nexusmods.com/stardewvalley/mods/679)
* [Discussion thread](http://www.nexusmods.com/stardewvalley/mods/679)