﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using xTile;

namespace BotPlay {
    internal class SimpleMap {

        private readonly Map Map;

        private bool[,]? WalkableTiles = null;
        public SimpleMap(Map map) {
            this.Map = map;
        }

        public int GetWidth() {
            // All layers have the same dimensions I think, so which one we choose doesn't matter
            return Map.Layers[0].LayerWidth;
        }

        public int GetHeight() {
            // All layers have the same dimensions I think, so which one we choose doesn't matter
            return Map.Layers[0].LayerHeight;
        }

        /// <summary>
        /// Visualizes the Map as a 2d character array with non-passable tiles and water marked as 'x' and everything else being empty. If the player is currently on this map, it also marks the player location with '@'.
        /// </summary>
        /// <param name="monitor">IMonitor object to log the visualized Map to.</param>
        public void VisualizeMap(IMonitor monitor) {
            if (WalkableTiles == null) {
                GenerateWalkableTiles();
            }

            int playerX = -1;
            int playerY = -1;
            if (Game1.player.currentLocation.Map.Id == this.Map.Id) {
                playerX = (int)Game1.player.Tile.X;
                playerY = (int)Game1.player.Tile.Y;
            }

            for (int i = 0; i < WalkableTiles?.GetLength(1); i++) {
                String row = "";
                for (int j = 0; j < WalkableTiles.GetLength(0); j++) {
                    if (j != 0) {
                        row += " ";
                    }

                    if (i == playerY && j == playerX) {
                        row += "@";
                    }
                    else if (WalkableTiles[j, i] == true) {
                        row += " ";
                    }
                    else if (WalkableTiles[j, i] == false) {
                        row += "x";
                    }
                }

                monitor.Log(row);
            }
        }


        /// <summary>
        /// Generates a boolean tile matrix representing the map where 'true' means walkable and 'false' means blocked/water tile.
        /// </summary>
        public void GenerateWalkableTiles() {
            int height = GetHeight();
            int width = GetWidth();

            WalkableTiles = new bool[width, height];
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    if (Game1.currentLocation.IsTileBlockedBy(new Vector2(i, j), ignorePassables: CollisionMask.All) || Game1.currentLocation.isWaterTile(i,j)) {
                        WalkableTiles[i, j] = false;
                    }
                    else {
                        WalkableTiles[i, j] = true;
                    }
                }
            }
        }
    }
}
