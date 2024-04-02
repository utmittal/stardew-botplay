using System;
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

        // I don't think this should actually be shared because the underlying map could change
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

        public void FindPath((int x, int y) origin, (int x, int y) destination) {
            GenerateWalkableTiles();

            // WalkableTiles is not null here but it points to a problem in the class.
            // Weights represents the presence of a path between tiles. 0 means no path.
            // Since it's an int array, everything should be 0 by default.
            // This 4d matrix is dumb
            int[,,,] weights = new int[WalkableTiles.GetLength(0), WalkableTiles.GetLength(1), WalkableTiles.GetLength(0), WalkableTiles.GetLength(1)];

            for (int i = 0; i < WalkableTiles.GetLength(0); i++) {
                for (int j = 0; j < WalkableTiles.GetLength(1); j++) {
                    // up
                    if (i != 0) {
                        int upI = i - 1;
                        int upJ = j;

                        if (WalkableTiles[upI, upJ] == true) {
                            weights[i, j, upI, upJ] = 1;
                        }
                    }
                    // right
                    if (j != WalkableTiles.GetLength(1)-1) {
                        int rightI = i;
                        int rightJ = j+1;

                        if (WalkableTiles[rightI, rightJ] == true) {
                            weights[i, j, rightI, rightJ] = 1;
                        }
                    }
                    // down
                    if (i != WalkableTiles.GetLength(0) - 1) {
                        int downI = i+1;
                        int downj = j;

                        if (WalkableTiles[downI, downj] == true) {
                            weights[i, j, downI, downj] = 1;
                        }
                    }
                    // left
                    if (j != 0) {
                        int leftI = i;
                        int leftJ = j -1;

                        if (WalkableTiles[leftI, leftJ] == true) {
                            weights[i, j, leftI, leftJ] = 1;
                        }
                    }
                }
            }

            // BFS
            // This is kinda stupid. You now have to do up down left right all over again. Or I guess you can just search "row" (i,j) to find weight = 1 and those are your connected vertices.
            // But really, this should be cleaner.
        }

        /// <summary>
        /// Generates a boolean tile matrix representing the map where 'true' means walkable and 'false' means blocked/water tile.
        /// </summary>
        private void GenerateWalkableTiles() {
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
