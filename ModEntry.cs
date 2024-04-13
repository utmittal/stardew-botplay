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
using System.Diagnostics;

namespace BotPlay {
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod {
        private bool playing = false;

        IModHelper? helper;

        private InputSimulator inputSimulator = new InputSimulator();

        int targetX = 3;
        int targetY = 12;
        bool goToTarget = false;

        EventHandler<UpdateTickedEventArgs> exitFarmEvent = null;

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
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            if (Game1.currentLocation.Name == "Farm" && this.exitFarmEvent != null) {
                helper.Events.GameLoop.UpdateTicked -= this.exitFarmEvent;
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
                    ExitFarmhouse();
                    Play();
                }
                else if (playing == true) {
                    playing = false;
                    cleanupInputSimulator();
                    Log("Stopped playing.");
                    return;
                }
            }

            if (e.Button == SButton.OemPipe) {
                CodeExplore();
            }
        }

        private void CodeExplore() {
            foreach (var feature in Game1.currentLocation.terrainFeatures) {
                foreach (var f2 in feature) {
                    Log($"{f2.Key}: {f2.Value}");
                }
            }

            foreach (var deb in Game1.currentLocation.Objects) {
                foreach (var ob2 in deb) {
                    Log($"{ob2.Key}: {ob2.Value.Name}");
                }
            }
        }

        private void ExitFarmhouse() {
            Warp? farmWarp = FindWarp("Farm");

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var exitFarmhousePath = MapGraph.FindPath(((int)Game1.player.Tile.X, (int)Game1.player.Tile.Y), (farmWarp.X, farmWarp.Y));
            stopwatch.Stop();
            Log($"PathFinding for {destinationWarp} took {stopwatch.ElapsedMilliseconds}");

            WalkAlongPath(exitFarmhousePath);
        }

        // Finds (any) warp point to specified target. Returns null if it can't find one.
        private Warp? FindWarp(string targetName) {
            foreach (Warp warp in Game1.currentLocation.warps) {
                if (warp.TargetName == targetName) {
                    return warp;
                }
            }
            return null;
        }

        private void WalkAlongPath(List<SimpleTile> pathTilesList) {
            var path = new Queue<SimpleTile>(pathTilesList);

            SimpleTile origin = path.Peek();
            if (origin.X != Game1.player.Tile.X || origin.Y != Game1.player.Tile.Y) {
                throw new InvalidOperationException($"Current player location: {Game1.player.Tile.X}, {Game1.player.Tile.Y} did not match path origin: {origin.X},{origin.Y}");
            }

            PathWalker pathWalker = new PathWalker(path, inputSimulator, this.Monitor);
            this.exitFarmEvent = pathWalker.GameLoop_UpdateTicked_WalkPath;
            helper.Events.GameLoop.UpdateTicked += this.exitFarmEvent;
        }

        private void Play() {
            Log($"current location name {Game1.currentLocation.Name}");
            int playerX = (int)Game1.player.Tile.X;
            int playerY = (int)Game1.player.Tile.Y;
            Log($"current location coordinate: {playerX},{playerY}");
            Log("warp locations:");
            foreach (Warp warp in Game1.currentLocation.warps) {
                Log($"\t{warp.X},{warp.Y}: {warp.TargetName}");
                Log("\tPath from player: ");
                MapGraph.VisualizeMap(this.Monitor, (playerX, playerY), (warp.X, warp.Y));
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

            //MapGraph.VisualizeMap(this.Monitor);
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
