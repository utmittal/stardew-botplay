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
    // There are problems in this class. I think an ideal version of this class would clone the Map object so that the class doesn't change when the underlying Map changes, which is especially prone to happen if you are using
    // currentLocation.Map. But unsure whether that's useful from this mod's perspective. Also, we should probably hook up a bunch of event handlers here so that the internal map state, especially walkabletiles can be updated
    // whenever something changes in the map. That way we don't have to regenerate the map every single time. Whether this is worth doing will depend entirely on how often we need to do pathfinding I think.
    internal class PathFinder {

        private readonly Map Map;

        public PathFinder(Map map) {
            this.Map = map;
        }

        public int GetMapWidth() {
            // All layers have the same dimensions I think, so which one we choose doesn't matter
            return Map.Layers[0].LayerWidth;
        }

        public int GetMapHeight() {
            // All layers have the same dimensions I think, so which one we choose doesn't matter
            return Map.Layers[0].LayerHeight;
        }

        public void VisualizeMap(IMonitor monitor) {
            // Note the underlying map can change at any point, so this needs to be regenerated each time the method is called
            SimpleTile[,] walkableTiles = GenerateWalkableTiles();

            int playerX = -1;
            int playerY = -1;
            if (Game1.player.currentLocation.Map.Id == this.Map.Id) {
                playerX = (int)Game1.player.Tile.X+1;
                playerY = (int)Game1.player.Tile.Y+1;
            }

            for (int i = 0; i < walkableTiles.GetLength(1); i++) {
                String row = "";
                for (int j = 0; j < walkableTiles.GetLength(0); j++) {
                    if (j != 0) {
                        row += " ";
                    }

                    if (i == playerY && j == playerX) {
                        row += "@";
                    }
                    else if (walkableTiles[j, i].Type == SimpleTile.TileType.Empty) {
                        row += " ";
                    }
                    else if (walkableTiles[j, i].Type == SimpleTile.TileType.EndOfMap) {
                        row += "X";
                    }
                    else if (walkableTiles[j, i].Type == SimpleTile.TileType.Blocked) {
                        row += "o";
                    }
                    else if (walkableTiles[j, i].Type == SimpleTile.TileType.WarpPoint) {
                        row += "#";
                    }
                    else {
                        row += "?";
                    }
                }

                monitor.Log(row);
            }
        }

        private SimpleTile[,] GenerateWalkableTiles() {
            int gameMapWidth = GetMapWidth();
            int gameMapHeight = GetMapHeight();
            // We want to represent the outer boundary in our map because that's where the warp points are. In game, this often means warp points are at coordinates like (5,-1). 
            // For the purposes of pathfinding, we want these included in our matrix, so we adjust the height and width we are working with. We also use special tile types for warp and endofmap.
            int adjustedWidth = gameMapWidth + 2;
            int adjustedHeight = gameMapHeight + 2;

            SimpleTile[,] walkableTiles = new SimpleTile[adjustedWidth, adjustedHeight];

            for (int i = 0; i < adjustedWidth; i++) {
                for (int j = 0; j < adjustedHeight; j++) {
                    if (IsEndOfMap(i,j,adjustedWidth,adjustedHeight)) {
                        // We can add warp points later. For now mark as endofmap
                        walkableTiles[i, j] = new SimpleTile(i, j, SimpleTile.TileType.EndOfMap);
                        continue;
                    }

                    // Adjust i,j to game coordinates. It's only -1 because the other extra row/column is at the end, so we don't care.
                    int gameX = i - 1;
                    int gameY = j - 1;
                    if (gameX < 0 || gameY < 0 || gameX >= gameMapWidth || gameY >= gameMapHeight) {
                        throw new InvalidOperationException(
                            $"Game coordinates ({gameX},{gameY}) are invalid given current map size of ({gameMapWidth},{gameMapHeight})");
                    }

                    if (Game1.currentLocation.IsTileBlockedBy(new Vector2(gameX, gameY), ignorePassables: CollisionMask.All) || Game1.currentLocation.isWaterTile(gameX, gameY)) {
                        walkableTiles[i, j] = new SimpleTile(i, j, SimpleTile.TileType.Blocked);
                    }
                    else {
                        walkableTiles[i, j] = new SimpleTile(i, j, SimpleTile.TileType.Empty);
                    }
                }
            }

            // Add warp tiles
            foreach (Warp warp in Game1.currentLocation.warps) {
                // These translations between game coordinates and our coordinates are dangerous. Easy to forget/get it wrong.
                walkableTiles[warp.X+1, warp.Y+1].Type = SimpleTile.TileType.WarpPoint;
            }

            return walkableTiles;
        }

        private static bool IsEndOfMap(int x, int y, int width, int height) {
            return x == 0 || y == 0 || x == width - 1 || y == height - 1;
        }
    }
}
