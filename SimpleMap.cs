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

            generatedTiles = InitializeMap(generatedTiles);
            generatedTiles = AddWarps(generatedTiles);
            generatedTiles = AddTileContents(generatedTiles);

            return generatedTiles;
        }

        private SimpleTile[,] InitializeMap(SimpleTile[,] generatedTiles) {
            // Initialize from given items rather than Game1 item to avoid the small chance of underlying map changing between method calls.
            int simpleMapWidth = generatedTiles.GetLength(0);
            int simpleMapHeight = generatedTiles.GetLength(1);
            int gameMapWidth = simpleMapWidth - 2;
            int gameMapHeight = simpleMapHeight - 2;

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

            return generatedTiles;
        }

        private SimpleTile[,] AddWarps(SimpleTile[,] generatedTiles) {
            foreach (Warp warp in sdvLocation.warps) {
                HybridCoord warpCoords = HybridCoord.FromGameCoord(warp.X, warp.Y);
                generatedTiles[warpCoords.SimpleMap.X, warpCoords.SimpleMap.Y].Type = SimpleTile.TileType.WarpPoint;
            }
            return generatedTiles;
        }

        private SimpleTile[,] AddTileContents(SimpleTile[,] generatedTiles) {
            foreach (var gameObject in sdvLocation.Objects.Pairs) {
                if (gameObject.Value.Name == GameConstants.Objects.TWIG) {
                    HybridCoord coord = HybridCoord.FromGameCoord(gameObject.Key);
                    generatedTiles[coord.SimpleMap.X, coord.SimpleMap.Y].Content = SimpleTile.TileContent.Twig;
                }
                else if (gameObject.Value.Name == GameConstants.Objects.STONE) {
                    HybridCoord coord = HybridCoord.FromGameCoord(gameObject.Key);
                    generatedTiles[coord.SimpleMap.X, coord.SimpleMap.Y].Content = SimpleTile.TileContent.Stone;
                }
                else if (gameObject.Value.Name == GameConstants.Objects.WEEDS) {
                    HybridCoord coord = HybridCoord.FromGameCoord(gameObject.Key);
                    generatedTiles[coord.SimpleMap.X, coord.SimpleMap.Y].Content = SimpleTile.TileContent.Weeds;
                }
                else if (gameObject.Value.Name == GameConstants.Objects.SEEDSPOT) {
                    HybridCoord coord = HybridCoord.FromGameCoord(gameObject.Key);
                    generatedTiles[coord.SimpleMap.X, coord.SimpleMap.Y].Content = SimpleTile.TileContent.SeedSpot;
                }
            }

            foreach (var feature in sdvLocation.terrainFeatures.Pairs) {
                if (feature.Value is StardewValley.TerrainFeatures.Tree) {
                    HybridCoord coord = HybridCoord.FromGameCoord(feature.Key);
                    generatedTiles[coord.SimpleMap.X, coord.SimpleMap.Y].Content = SimpleTile.TileContent.Tree;
                }
            }

            return generatedTiles;
        }

        private static bool IsEndOfMap(int x, int y, int width, int height) {
            return x == 0 || y == 0 || x == width - 1 || y == height - 1;
        }
    }
}
