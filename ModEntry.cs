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
        PathWalker pathWalker;

        int targetX = 3;
        int targetY = 12;
        bool goToTarget = false;

        EventHandler<UpdateTickedEventArgs> walkingEvent = null;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {
            this.helper = helper;

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
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
                    //GoToWarp("Farm");
                    Play();
                    helper.Events.GameLoop.UpdateTicked += GlobeTrotter;
                }
                else if (playing == true) {
                    playing = false;
                    CleanupInputSimulator();
                    if (walkingEvent != null) {
                        walkingEvent = null;
                        helper.Events.GameLoop.UpdateTicked -= walkingEvent;
                    }
                    helper.Events.GameLoop.UpdateTicked -= GlobeTrotter;
                    Log("Stopped playing.");
                    return;
                }
            }

            if (e.Button == SButton.OemPipe) {
                CodeExplore();
            }
        }

        private void CodeExplore() {
            //foreach (var feature in Game1.currentLocation.terrainFeatures) {
            //    foreach (var f2 in feature) {
            //        Log($"{f2.Key}: {f2.Value}");
            //    }
            //}

            //foreach (var deb in Game1.currentLocation.Objects) {
            //    foreach (var ob2 in deb) {
            //        Log($"{ob2.Key}: {ob2.Value.Name}");
            //    }
            //}

            //Log($"current location name {Game1.currentLocation.Name}");
            //int playerX = (int)Game1.player.Tile.X;
            //int playerY = (int)Game1.player.Tile.Y;
            //Log($"current location coordinate: {playerX},{playerY}");
            //Log("warp locations:");
            //foreach (Warp warp in Game1.currentLocation.warps) {
            //    Log($"\t{warp.X},{warp.Y}: {warp.TargetName}");
            //    Log("\tPath from player: ");
            //    SimpleMap currentMap = new SimpleMap(Game1.currentLocation);
            //    PathFinder pathFinder = new PathFinder(currentMap);
            //    List<SimpleTile> routeToWarp = pathFinder.FindPath((playerX, playerY), (warp.X, warp.Y));
            //    SimpleMapVisualizer.VisualizeMap(currentMap, routeToWarp, this.Monitor);
            //}

            //Log("Layers:");
            //foreach (Layer layer in Game1.currentLocation.map.Layers) {
            //    Log($"\t{layer.Id}");
            //    Log($"\t\tDescription: {layer.Description}");
            //    Log($"\t\tSize: {layer.LayerWidth}x{layer.LayerHeight}");
            //    Log($"\t\tProperties:");
            //    foreach (var property in layer.Properties) {
            //        Log($"\t\t\t{property.Key}: {property.Value.ToString()}");
            //    }
            //}

            //Log($"Name: {Game1.currentLocation.Name}");
            //Log($"Unique Name: {Game1.currentLocation.uniqueName}");
            //Log($"NameOrUnique Name: {Game1.currentLocation.NameOrUniqueName}");
            //Log($"Display Name: {Game1.currentLocation.DisplayName}");
            //Log($"Parent Name: {Game1.currentLocation.parentLocationName}");

            Log($"Farmer width, height: {Game1.player.FarmerSprite.SpriteWidth}, {Game1.player.FarmerSprite.SpriteHeight}");
            Log($"Farmer pixel position: {Game1.player.Position.X}, {Game1.player.Position.Y}");
            var boundingBox = Game1.player.GetBoundingBox();
            Log($"Farmer bounding box - left,right,up,down: {boundingBox.Left},{boundingBox.Right},{boundingBox.Top},{boundingBox.Bottom}");
            Log($"Farm xoffset,yoffset: {Game1.player.xOffset},{Game1.player.yOffset}");
        }

        private void GlobeTrotter(object? sender, UpdateTickedEventArgs e) {
            // ignore if player hasn't loaded a save yet
            if (!Context.IsWorldReady)
                return;

            if (pathWalker != null && pathWalker.HasWalkingEnded() && walkingEvent != null) {
                Log("------------ removed walking event ------------");
                helper.Events.GameLoop.UpdateTicked -= walkingEvent;
                walkingEvent = null;
            }

            if (Context.IsPlayerFree && walkingEvent == null) {
                if (Game1.currentLocation.Name == Location.FARMHOUSE.Value) {
                    GoToWarp(Location.FARM);
                }
                if (Game1.currentLocation.Name == Location.FARM.Value) {
                    GoToWarp(Location.BUSSTOP);
                }
                if (Game1.currentLocation.Name == Location.BUSSTOP.Value) {
                    GoToWarp(Location.TOWN);
                }
                if (Game1.currentLocation.Name == Location.TOWN.Value) {
                    GoToWarp(Location.MOUNTAIN);
                }
                if (Game1.currentLocation.Name == Location.MOUNTAIN.Value) {
                    GoToWarp(Location.BACKWOODS);
                }
                if (Game1.currentLocation.Name == Location.BACKWOODS.Value) {
                    GoToWarp(Location.FARM);
                }
            }
        }

        private void GoToWarp(Location location) {
            if (Game1.currentLocation.Name == location.Value) {
                Log($"Already at {location.Value}. Not moving.");
                return;
            }

            List<Warp> warps = FindWarps(location);
            if (warps.Count == 0) {
                Log($"Could not find any warps to {location.Value}. Not moving.");
                return;
            }

            // Choose any of the warps, we don't care
            Warp warp = warps.First();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            SimpleMap currentMap = new SimpleMap(Game1.currentLocation);
            PathFinder pathFinder = new PathFinder(currentMap);
            var exitFarmhousePath = pathFinder.FindPath(((int)Game1.player.Tile.X, (int)Game1.player.Tile.Y), (warp.X, warp.Y));
            stopwatch.Stop();
            Log($"PathFinding for {location.Value} took {stopwatch.ElapsedMilliseconds}");

            WalkAlongPath(exitFarmhousePath);
        }

        /// <summary>
        /// Finds all warps to specified location from current game location.
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        private static List<Warp> FindWarps(Location targetName) {
            List<Warp> warpList = new();
            foreach (Warp warp in Game1.currentLocation.warps) {
                if (warp.TargetName == targetName.Value) {
                    warpList.Add(warp);
                }
            }
            return warpList;
        }

        private void WalkAlongPath(List<SimpleTile> pathTilesList) {
            if(pathTilesList.Count == 0) {
                Log($"Empth path provided to WalkAlongPath. Not moving.");
                return;
                }

            var path = new Queue<SimpleTile>(pathTilesList);

            SimpleTile origin = path.Peek();
            if (origin.X != Game1.player.Tile.X || origin.Y != Game1.player.Tile.Y) {
                throw new InvalidOperationException($"Current player location: {Game1.player.Tile.X}, {Game1.player.Tile.Y} did not match path origin: {origin.X},{origin.Y}");
            }

            pathWalker = new PathWalker(path, inputSimulator, this.Monitor);
            this.walkingEvent = pathWalker.GameLoop_UpdateTicked_WalkPath;
            helper.Events.GameLoop.UpdateTicked += this.walkingEvent;
        }

        private void Play() {


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

        private void CleanupInputSimulator() {
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
