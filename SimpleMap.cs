using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;

namespace BotPlay {
    internal class SimpleMap {
        // TODO: Move HybridCoord out into it's own class and reuse it in PathFinder
        private class HybridCoord {
            public (int X, int Y) Game { get; }
            public (int X, int Y) SimpleMap { get; }

            public static HybridCoord FromGameCoord(int gameX, int gameY) {
                return new HybridCoord((gameX, gameY), (gameX + 1, gameY+1));
            }

            public static HybridCoord FromSimpleMapCoord(int simpleMapX, int simpleMapY) {
                // Note: it's valid to convert simple map index 0 to game index -1 because warp points etc are often represented by out of index numbers in game code.
                return new HybridCoord((simpleMapX-1, simpleMapY-1), (simpleMapX, simpleMapY));
            }

            private HybridCoord((int x, int y) game, (int x, int y) simpleMap) {
                // Pretty sure we don't need a deep copy here since we created the tuple ourselves from value types
                this.Game = game;
                this.SimpleMap = simpleMap;
            }
        }

        private readonly GameLocation sdvLocation;
        private SimpleTile[,]? mapTiles;

        public string LocationName {
            get;
        }

        public SimpleMap(GameLocation gameLocation) {
            this.sdvLocation = gameLocation;
            // TODO: Should this be Name or UniqueName or Display Name?
            this.LocationName = gameLocation.Name;
        }

        /// <summary>
        /// Returns map width from the game (i.e. not the extended map we store)
        /// </summary>
        private int GetGameMapWidth() {
            // All layers have the same dimensions I think, so which one we choose doesn't matter
            return sdvLocation.Map.Layers[0].LayerWidth;
        }

        /// <summary>
        /// Returns map height from the game (i.e. not the extended map we store)
        /// </summary>
        private int GetGameMapHeight() {
            // All layers have the same dimensions I think, so which one we choose doesn't matter
            return sdvLocation.Map.Layers[0].LayerHeight;
        }

        public SimpleTile[,] GetMapTiles() {
            this.mapTiles ??= GenerateTiles();
            return this.mapTiles;
        }

        private SimpleTile[,] GenerateTiles() {
            int gameMapWidth = GetGameMapWidth();
            int gameMapHeight = GetGameMapHeight();
            // We want to represent the outer boundary in our map because that's where the warp points are. In game, this often means warp points are at coordinates like (5,-1). 
            // For the purposes of pathfinding, we want these included in our matrix, so we adjust the height and width we are working with. We also use special tile types for warp and endofmap.
            int simpleMapWidth = gameMapWidth + 2;
            int simpleMapHeight = gameMapHeight + 2;

            SimpleTile[,] generatedTiles = new SimpleTile[simpleMapWidth, simpleMapHeight];

            for (int i = 0; i < simpleMapWidth; i++) {
                for (int j = 0; j < simpleMapHeight; j++) {
                    HybridCoord coords = HybridCoord.FromSimpleMapCoord(i, j);

                    if (IsEndOfMap(coords.SimpleMap.X, coords.SimpleMap.Y, simpleMapWidth, simpleMapHeight)) {
                        // We can add warp points later. For now mark as endofmap
                        generatedTiles[coords.SimpleMap.X, coords.SimpleMap.Y] = new SimpleTile(coords.Game.X, coords.Game.Y, SimpleTile.TileType.EndOfMap);
                        continue;
                    }

                    // This should never happen.
                    if (coords.Game.X < 0 || coords.Game.Y < 0 || coords.Game.Y >= gameMapWidth || coords.Game.Y >= gameMapHeight) {
                        throw new InvalidOperationException(
                            $"Game coordinates ({coords.Game.X},{coords.Game.Y}) are invalid given current map size of ({gameMapWidth},{gameMapHeight})");
                    }

                    if (sdvLocation.IsTileBlockedBy(new Vector2(coords.Game.X, coords.Game.Y), ignorePassables: CollisionMask.All) || sdvLocation.isWaterTile(coords.Game.X, coords.Game.Y)) {
                        generatedTiles[coords.SimpleMap.X, coords.SimpleMap.Y] = new SimpleTile(coords.Game.X, coords.Game.Y, SimpleTile.TileType.Blocked);
                    }
                    else {
                        generatedTiles[coords.SimpleMap.X, coords.SimpleMap.Y] = new SimpleTile(coords.Game.X, coords.Game.Y, SimpleTile.TileType.Empty);
                    }
                }
            }

            // Add warp tiles
            foreach (Warp warp in sdvLocation.warps) {
                HybridCoord warpCoords = HybridCoord.FromGameCoord(warp.X, warp.Y);
                generatedTiles[warpCoords.SimpleMap.X, warpCoords.SimpleMap.Y] = new SimpleTile(warpCoords.Game.X, warpCoords.Game.Y, SimpleTile.TileType.WarpPoint);
            }

            return generatedTiles;
        }

        private static bool IsEndOfMap(int x, int y, int width, int height) {
            return x == 0 || y == 0 || x == width - 1 || y == height - 1;
        }
    }
}
