# Tilled Soil Decay

## Description
Prevents watered tilled soil from disappearing during the night. If you only want to change the decay rate, set the delays to 0. A rate of 0 means tilled soil will never disappear during the night. A rate of 1 means that it will always disappear (after the given delay).

## Config
*Note: run Stardew Valley once with this mod enabled to generate the `config.json` file.*

* `DryDecayRate`: Amount of tilled soil that will disappear. Normally this is 0.1 (=10%). Default = 0.5.
* `DryDecayRateFirstOfMonth`: Amount of tilled soil that will disappear at the start of a new month. Normally this is 0.8 (=80%). Default = 1.
* `DecayDelay`: Number of days that the patch must have been without water, before it can disappear during the night. Default = 2.
* `DecayDelayFirstOfMonth`: Number of days that the patch must have been without water, before it can disappear during the night at the end of the month. Default = 1.

## Dependencies
This mod requires the following mods to be installed:

* [SMAPI](https://www.nexusmods.com/stardewvalley/mods/2400)
* [StardewHack](https://www.nexusmods.com/stardewvalley/mods/3213)

## Known bugs
Please report bugs on [GitHub](https://github.com/bcmpinc/StardewHack/issues).

## Changes
#### 1.0:
* Added dependency on the StardewHack library mod.

#### 2.0:
* Updated for Stardew Valley 1.4
