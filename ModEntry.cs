using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Util;
using System.Diagnostics.CodeAnalysis;
using xTile.Layers;

namespace BotPlay {
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod {
        private bool playing = false;

        IModHelper? helper;

        private InputSimulator inputSimulator = new InputSimulator();

        int targetX = 3;
        int targetY = 12;
        bool goToTarget = false;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {
            this.helper = helper;

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;

            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
        }

        private void GameLoop_UpdateTicked(object? sender, UpdateTickedEventArgs e) {
            if (goToTarget == false || !Context.IsWorldReady) {
                return;
            }

            Log($"current location coordinate: {Game1.player.Tile.X},{Game1.player.Tile.Y}");

            if (Game1.player.Tile.X != targetX) {
                inputSimulator.MoveLeftHeld = true;
            }
            else if(Game1.player.Tile.X == targetX) {
                inputSimulator.MoveLeftHeld = false;
            }

            if (Game1.player.Tile.Y != targetY) {
                inputSimulator.MoveDownHeld = true;
            }
            else if (Game1.player.Tile.Y == targetY) {
                inputSimulator.MoveDownHeld = false;
            }
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object? sender, ButtonPressedEventArgs e) {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            // print button presses to the console window
            this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            if (e.Button == SButton.OemTilde) {
                if (playing == false) {
                    playing = true;
                    Log("Started playing.");
                    InitInputSimulator();
                    Play();
                }
                else if (playing == true) {
                    playing = false;
                    cleanupInputSimulator();
                    Log("Stopped playing.");
                    return;
                }
            }
        }

        private void Play() {
            Log($"current location name {Game1.currentLocation.Name}");
            Log($"current location coordinate: {Game1.player.Tile.X},{Game1.player.Tile.Y}");
            Log("warp locations:");
            foreach (Warp warp in Game1.currentLocation.warps) {
                Log($"\t{warp.X},{warp.Y}: {warp.TargetName}");
            }

            Log("Layers:");
            foreach (Layer layer in Game1.currentLocation.map.Layers) {
                Log($"\t{layer.Id}");
                Log($"\t\tDescription: {layer.Description}");
                Log($"\t\tSize: {layer.LayerWidth}x{layer.LayerHeight}");
                Log($"\t\tProperties:");
                foreach (var property in layer.Properties) {
                    Log($"\t\t\t{property.Key}: {property.Value.ToString()}");
                }
            }

            SimpleMap testMap = new SimpleMap(Game1.currentLocation.map);
            testMap.VisualizeMap(this.Monitor);
            //goToTarget = true;
        }

        private void InitInputSimulator() {
            IReflectedField<IInputSimulator>? reflectedInputSimulator = 
                helper?.Reflection.GetField<IInputSimulator>(typeof(Game1), "inputSimulator", true);
            if (reflectedInputSimulator?.GetValue() == null) {
                Log("Initializing reflected input simulator.");
                reflectedInputSimulator?.SetValue(inputSimulator);
            }
        }

        private void cleanupInputSimulator() {
            IReflectedField<IInputSimulator>? reflectedInputSimulator =
                helper?.Reflection.GetField<IInputSimulator>(typeof(Game1), "inputSimulator", true);
            if (reflectedInputSimulator?.GetValue() != null) {
                Log("Deinitializing reflected input simulator.");
                reflectedInputSimulator?.FieldInfo.SetValue(reflectedInputSimulator.GetValue(), null);
            }
        }

        private void Log(string message) {
            this.Monitor.Log(message, LogLevel.Debug);
        }
    }
}
