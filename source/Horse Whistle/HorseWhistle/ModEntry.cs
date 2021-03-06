﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HorseWhistle.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;

namespace HorseWhistle
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Properties
        *********/
        private TileData[] _tiles;
        private bool _gridActive;
        private ISoundBank _customSoundBank;
        private WaveBank _customWaveBank;
        private bool _hasAudio;
        private ModConfigModel _config;


        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // read config
            _config = helper.ReadConfig<ModConfigModel>();

            // set up sounds
            if (Constants.TargetPlatform == GamePlatform.Windows && _config.EnableWhistleAudio)
            {
                try
                {
                    _customSoundBank = new SoundBankWrapper(new SoundBank(Game1.audioEngine.Engine, Path.Combine(helper.DirectoryPath, "assets", "CustomSoundBank.xsb")));
                    _customWaveBank = new WaveBank(Game1.audioEngine.Engine, Path.Combine(helper.DirectoryPath, "assets", "CustomWaveBank.xwb"));
                    _hasAudio = true;
                }
                catch (ArgumentException ex)
                {
                    _customSoundBank = null;
                    _customWaveBank = null;
                    _hasAudio = false;

                    Monitor.Log("Couldn't load audio, so the whistle sound won't play.");
                    Monitor.Log(ex.ToString(), LogLevel.Trace);
                }
            }

            // add event listeners
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            if (_config.EnableGrid)
            {
                helper.Events.GameLoop.UpdateTicked += this.UpdateTicked;
                helper.Events.Display.Rendered += this.OnRendered;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsPlayerFree)
                return;

            if (e.Button == _config.TeleportHorseKey && !Game1.player.isRidingHorse())
            {
                var horse = FindHorse();
                if (horse == null) return;
                PlayHorseWhistle();
                Game1.warpCharacter(horse, Game1.currentLocation, Game1.player.getTileLocation());
            }
            else if (_config.EnableGrid && e.Button == _config.EnableGridKey)
                _gridActive = !_gridActive;
        }

        /// <summary>Raised after the game state is updated (≈60 times per second).</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(2))
                UpdateGrid();
        }

        /// <summary>Raised after the game draws to the sprite patch in a draw tick, just before the final sprite batch is rendered to the screen.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnRendered(object sender, RenderedEventArgs e)
        {
            DrawGrid(e.SpriteBatch);
        }

        /// <summary>Play the horse whistle sound.</summary>
        private void PlayHorseWhistle()
        {
            if (!_hasAudio || !_config.EnableWhistleAudio) return;

            var originalSoundBank = Game1.soundBank;
            var originalWaveBank = Game1.waveBank;
            try
            {
                Game1.soundBank = _customSoundBank;
                Game1.waveBank = _customWaveBank;
                Game1.audioEngine.Update();
                Game1.playSound("horseWhistle");
            }
            finally
            {
                Game1.soundBank = originalSoundBank;
                Game1.waveBank = originalWaveBank;
                Game1.audioEngine.Update();
            }
        }

        /// <summary>Get all available locations.</summary>
        private IEnumerable<GameLocation> GetLocations()
        {
            GameLocation[] mainLocations = (Context.IsMainPlayer ? Game1.locations : this.Helper.Multiplayer.GetActiveLocations()).ToArray();

            foreach (GameLocation location in mainLocations.Concat(MineShaft.activeMines))
            {
                yield return location;

                if (location is BuildableGameLocation buildableLocation)
                {
                    foreach (Building building in buildableLocation.buildings)
                    {
                        if (building.indoors.Value != null)
                            yield return building.indoors.Value;
                    }
                }
            }
        }

        /// <summary>Find the current player's horse.</summary>
        private Horse FindHorse()
        {
            foreach (GameLocation location in this.GetLocations())
            {
                foreach (Horse horse in location.characters.OfType<Horse>())
                {
                    if (horse.rider != null || horse.Name.StartsWith("tractor/"))
                        continue;

                    return horse;
                }
            }

            return null;
        }

        private void UpdateGrid()
        {
            if (!_gridActive || !Context.IsPlayerFree || Game1.currentLocation == null)
            {
                _tiles = null;
                return;
            }

            // get updated tiles
            var location = Game1.currentLocation;
            _tiles = CommonMethods
                .GetVisibleTiles(location, Game1.viewport)
                .Where(tile => location.isTileLocationTotallyClearAndPlaceableIgnoreFloors(tile))
                .Select(tile => new TileData(tile, Color.Red))
                .ToArray();
        }

        private void DrawGrid(SpriteBatch spriteBatch)
        {
            if (!_gridActive || !Context.IsPlayerFree || _tiles == null || _tiles.Length == 0)
                return;

            // draw tile overlay
            const int tileSize = Game1.tileSize;
            foreach (var tile in _tiles.ToArray())
            {
                var position = tile.TilePosition * tileSize - new Vector2(Game1.viewport.X, Game1.viewport.Y);
                RectangleSprite.DrawRectangle(spriteBatch, new Rectangle((int)position.X, (int)position.Y, tileSize, tileSize), tile.Color * .3f, 6);
            }
        }
    }
}