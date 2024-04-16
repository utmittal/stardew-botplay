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
using xTile.Dimensions;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewValley.TerrainFeatures;
using StardewValley.Locations;

namespace BotPlay {
    /// <summary>The mod entry point.</summary>
    internal sealed class ModEntry : Mod {
        private bool playing = false;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

            Setup();
        }
        private void Setup() {
            PathWalker.InitPathWalker(InputSimulator.Instance, this.Helper.Events.GameLoop, this.Monitor);
        }

        private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e) {
            if (!playing) {
                return;
            }

            Play();
        }
        
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
                    //Play();
                    //helper.Events.GameLoop.UpdateTicked += GlobeTrotter;
                }
                else if (playing == true) {
                    playing = false;
                    CleanupInputSimulator();
                    //if (walkingEvent != null) {
                    //    walkingEvent = null;
                    //    helper.Events.GameLoop.UpdateTicked -= walkingEvent;
                    //}
                    //helper.Events.GameLoop.UpdateTicked -= GlobeTrotter;
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
            //        TerrainFeature tf = f2.Value;
            //        Log($"{f2.Key}: {f2.Value}");
            //        Log($"{f2.Value is StardewValley.TerrainFeatures.Tree}");
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

            Log($"current location name {Game1.currentLocation.Name}");
            int playerX = (int)Game1.player.Tile.X;
            int playerY = (int)Game1.player.Tile.Y;
            Log($"current location coordinate: {playerX},{playerY}");
            Log("warp locations:");
            SimpleMap currentMap = new SimpleMap(Game1.currentLocation);
            PathFinder pathFinder = new PathFinder(currentMap, this.Monitor);
            List<SimpleTile> route = pathFinder.FindPathToClosestDebris((playerX, playerY));
            SimpleMapVisualizer.VisualizeMap(currentMap, route, this.Monitor);


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

            //Log($"Farmer width, height: {Game1.player.FarmerSprite.SpriteWidth}, {Game1.player.FarmerSprite.SpriteHeight}");
            //Log($"Farmer pixel position: {Game1.player.Position.X}, {Game1.player.Position.Y}");
            //var boundingBox = Game1.player.GetBoundingBox();
            //Log($"Farmer bounding box - left,right,up,down: {boundingBox.Left},{boundingBox.Right},{boundingBox.Top},{boundingBox.Bottom}");
            //Log($"Farm xoffset,yoffset: {Game1.player.xOffset},{Game1.player.yOffset}");

            //foreach (var item in Game1.currentLocation.Objects) {
            //    foreach (var item2 in item) {
            //        Log($"Item: {item2.Key} - {item2.Value.Name}");
            //        if (item2.Value.Name == "Chest") {
            //            Log($"\t{item2.Value is Chest}");
            //            Log($"\t{((Chest)item2.Value).isEmpty()}");
            //        }
            //    }
            //}

            //// InputSimulator doesn't seem to provide a way to fake number keys for quick inventory switching. So we are directly going to switch the toolindex instead.
            //Log($"Current item: {Game1.player.CurrentItem.Name}");
            //Log($"Current tool: {Game1.player.CurrentTool.Name}");
            //Log($"Item count: {Game1.player.Items.Count}");
            //Game1.player.CurrentToolIndex = 2;

            //if (Game1.currentLocation is Town) {
            //    Stopwatch sw = new();
            //    float mapCreationAverage = 0;
            //    float graphCreationAverage = 0;
            //    float pathFindingAverage = 0;
            //    int sampleSize = 10;

            //    for (int i = 0; i < sampleSize; i++) {
            //        Log($"Run {i}");

            //        sw.Restart();
            //        SimpleMap simpleMap = new SimpleMap(Game1.currentLocation);
            //        // Forces initialization of the map tiles
            //        simpleMap.GetMapTiles();
            //        sw.Stop();
            //        Log($"\tMap creation:\t{sw.ElapsedMilliseconds}");
            //        mapCreationAverage += sw.ElapsedMilliseconds;

            //        sw.Restart();
            //        PathFinder pathFinder = new PathFinder(simpleMap);
            //        sw.Stop();
            //        Log($"\tGraph creation:\t{sw.ElapsedMilliseconds}");
            //        graphCreationAverage += sw.ElapsedMilliseconds;

            //        sw.Restart();
            //        var path = pathFinder.FindPath(((int)Game1.player.Tile.X, (int)Game1.player.Tile.Y), (81, -1));
            //        sw.Stop();
            //        Log($"\tPath finding:\t{sw.ElapsedMilliseconds}");
            //        pathFindingAverage += sw.ElapsedMilliseconds;

            //        if (i == sampleSize - 1) {
            //            SimpleMapVisualizer.VisualizeMap(simpleMap, path, this.Monitor);
            //        }
            //    }

            //    Log($"Map creation (sample size {sampleSize}):\t{mapCreationAverage/sampleSize}");
            //    Log($"Graph creation (sample size {sampleSize}):\t{graphCreationAverage / sampleSize}");
            //    Log($"Path finding (sample size {sampleSize}):\t{pathFindingAverage / sampleSize}");
            //}
        }

        private void Play() {
            // Go to farm if possible
            if (Game1.currentLocation.Name != Location.FARM.Value && !PathWalker.Instance.IsWalking) {
                NavUtil.GoToWarp(Location.FARM, this.Monitor);
            }

            // Once on farm
            if (Game1.currentLocation.Name == Location.FARM.Value) {
                //// Find an empty chest
                //(int x, int y) emptyChestLocation = GetEmptyChestLocation();
                //Log($"Found empty chest at: {emptyChestLocation.x},{emptyChestLocation.y}");
                // Equip pickaxe
                InventoryUtil.TryEquipPickaxe(this.Monitor);
            }
        }

        private static (int x, int y) GetEmptyChestLocation() {
            foreach (var obj in Game1.currentLocation.Objects.Pairs) {
                if (obj.Value is Chest && ((Chest)obj.Value).isEmpty()) {
                    return ((int)obj.Key.X, (int)obj.Key.Y);
                }
            }

            return (-1, -1);
        }

        //private void GlobeTrotter(object? sender, UpdateTickedEventArgs e) {
        //    // ignore if player hasn't loaded a save yet
        //    if (!Context.IsWorldReady)
        //        return;

        //    if (pathWalker != null && pathWalker.HasWalkingEnded() && walkingEvent != null) {
        //        Log("------------ removed walking event ------------");
        //        helper.Events.GameLoop.UpdateTicked -= walkingEvent;
        //        walkingEvent = null;
        //    }

        //    if (Context.IsPlayerFree && walkingEvent == null) {
        //        if (Game1.currentLocation.Name == Location.FARMHOUSE.Value) {
        //            GoToWarp(Location.FARM);
        //        }
        //        if (Game1.currentLocation.Name == Location.FARM.Value) {
        //            GoToWarp(Location.BUSSTOP);
        //        }
        //        if (Game1.currentLocation.Name == Location.BUSSTOP.Value) {
        //            GoToWarp(Location.TOWN);
        //        }
        //        if (Game1.currentLocation.Name == Location.TOWN.Value) {
        //            GoToWarp(Location.MOUNTAIN);
        //        }
        //        if (Game1.currentLocation.Name == Location.MOUNTAIN.Value) {
        //            GoToWarp(Location.BACKWOODS);
        //        }
        //        if (Game1.currentLocation.Name == Location.BACKWOODS.Value) {
        //            GoToWarp(Location.FARM);
        //        }
        //    }
        //}

        private void InitInputSimulator() {
            IReflectedField<IInputSimulator>? reflectedInputSimulator =
                this.Helper.Reflection.GetField<IInputSimulator>(typeof(Game1), "inputSimulator", true);
            if (reflectedInputSimulator?.GetValue() == null) {
                Log("Initializing reflected input simulator.");
                reflectedInputSimulator?.SetValue(InputSimulator.Instance);
            }
        }

        private void CleanupInputSimulator() {
            IReflectedField<IInputSimulator>? reflectedInputSimulator =
                this.Helper.Reflection.GetField<IInputSimulator>(typeof(Game1), "inputSimulator", true);
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
