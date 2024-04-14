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

        private static readonly float STRAIGHT_WEIGHT = 1;
        private static readonly float DIAGONAL_WEIGHT = 1.41f;  // length of diagonal in a square of side 1

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
        private string locationName;
        private SimpleTile[,]? mapTiles;

        public SimpleMap(GameLocation gameLocation) {
            this.sdvLocation = gameLocation;
            // TODO: Should this be Name or UniqueName or Display Name?
            this.locationName = gameLocation.Name;
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

        private SimpleTile[,] getMapTiles() {
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

        public void VisualizeMap(IMonitor monitor) {
            // Note the underlying map can change at any point, so this needs to be regenerated each time the method is called
            char[,] debugMap = GetDebugMap();

            for (int i = 0; i < debugMap.GetLength(1); i++) {
                String row = "";
                for (int j = 0; j < debugMap.GetLength(0); j++) {
                    if (j != 0) {
                        row += " ";
                    }
                    row += debugMap[j, i];
                }

                monitor.Log(row);
            }
        }

        public void VisualizeMap(IMonitor monitor, (int x, int y) origin, (int x, int y) destination) {
            List<SimpleTile> path = FindPath(origin, destination);
            if (path.Count == 0) {
                monitor.Log("Couldn't generate path. Printing normal map: ");
                VisualizeMap(monitor);
                return;
            }
            char[,] debugMap = GetDebugMap();

            foreach (SimpleTile tile in path) {
                if (tile.X == origin.x && tile.Y == origin.y) {
                    // don't draw over origin
                    continue;
                }
                if (tile.X == destination.x && tile.Y == destination.y) {
                    // don't draw over destination
                    continue;
                }
                debugMap[tile.X + 1, tile.Y + 1] = '*';
            }

            for (int i = 0; i < debugMap.GetLength(1); i++) {
                String row = "";
                for (int j = 0; j < debugMap.GetLength(0); j++) {
                    if (j != 0) {
                        row += " ";
                    }
                    row += debugMap[j, i];
                }

                monitor.Log(row);
            }
        }

        private char[,] GetDebugMap() {
            SimpleTile[,] tileMatrix = GenerateTiles();
            char[,] debugMap = new char[tileMatrix.GetLength(0), tileMatrix.GetLength(1)];

            int playerX = (int)Game1.player.Tile.X + 1;
            int playerY = (int)Game1.player.Tile.Y + 1;

            for (int i = 0; i < tileMatrix.GetLength(0); i++) {
                for (int j = 0; j < tileMatrix.GetLength(1); j++) {
                    if (i == playerX && j == playerY) {
                        debugMap[i, j] = '@';
                    }
                    else if (tileMatrix[i, j].Type == SimpleTile.TileType.Empty) {
                        debugMap[i, j] = ' ';
                    }
                    else if (tileMatrix[i, j].Type == SimpleTile.TileType.EndOfMap) {
                        debugMap[i, j] = 'X';
                    }
                    else if (tileMatrix[i, j].Type == SimpleTile.TileType.Blocked) {
                        debugMap[i, j] = 'o';
                    }
                    else if (tileMatrix[i, j].Type == SimpleTile.TileType.WarpPoint) {
                        debugMap[i, j] = '#';
                    }
                    else {
                        debugMap[i, j] = '?';
                    }
                }
            }

            return debugMap;
        }

        public List<SimpleTile> FindPath((int x, int y) origin, (int x, int y) destination) {
            SimpleTile[,] tileMatrix = GenerateTiles();
            var (adjacencyMatrix, tileNeighbours) = GenerateGraph(tileMatrix);
            SimpleTile originTile = tileMatrix[origin.x + 1, origin.y + 1];
            SimpleTile destinationTile = tileMatrix[destination.x + 1, destination.y + 1];

            // Stores preceding tile on path
            Dictionary<SimpleTile, SimpleTile> prevTile = new Dictionary<SimpleTile, SimpleTile>();
            // Stores distance of tile from originTile
            Dictionary<SimpleTile, float> distanceFromOrigin = new Dictionary<SimpleTile, float>();
            // Stores all the tiles we still need to visit
            HashSet<SimpleTile> tilesToVisit = new HashSet<SimpleTile>();

            foreach (SimpleTile tile in tileMatrix) {
                distanceFromOrigin[tile] = float.MaxValue;
                tilesToVisit.Add(tile);
            }
            distanceFromOrigin[originTile] = 0;

            while (tilesToVisit.Count > 0) {
                SimpleTile currentTile = GetClosestTile(tilesToVisit, distanceFromOrigin);
                tilesToVisit.Remove(currentTile);

                if (currentTile == destinationTile) {
                    break;
                }

                if (!tileNeighbours.ContainsKey(currentTile)) {
                    // this happens if we end up iterating over an endofmap tile or warp tile that is not the destination.
                    // We know these cannot be part of the solution, so we simply move on
                    continue;
                }

                foreach (SimpleTile neighbour in tileNeighbours[currentTile]) {
                    float distanceNeighbourFromOrigin = distanceFromOrigin[currentTile] + adjacencyMatrix[(currentTile, neighbour)];
                    if (distanceNeighbourFromOrigin < distanceFromOrigin[neighbour]) {
                        distanceFromOrigin[neighbour] = distanceNeighbourFromOrigin;
                        prevTile[neighbour] = currentTile;
                    }
                }
            }

            // if prevtile contains no entry for destinationTile, it means we didn't find a path.
            if(!prevTile.ContainsKey(destinationTile)) {
                return new List<SimpleTile>();
            }

            List<SimpleTile> path = new List<SimpleTile>();
            // Add origin tile into the path
            path.Insert(0, new SimpleTile(origin.x, origin.y, SimpleTile.TileType.Empty));
            SimpleTile backtrackTile = destinationTile;
            while (backtrackTile != originTile) {
                path.Insert(0, backtrackTile);
                backtrackTile = prevTile[backtrackTile];
            }
            path.Insert(0, originTile);

            return path;
        }

        private static SimpleTile GetClosestTile(HashSet<SimpleTile> tilesToVisit, Dictionary<SimpleTile, float> distanceFromOrigin) {
            SimpleTile? closestTile = null;
            float closestTileDistance = float.MaxValue;
            foreach (SimpleTile tile in tilesToVisit) {
                if (distanceFromOrigin[tile] <= closestTileDistance) {
                    closestTile = tile;
                    closestTileDistance = distanceFromOrigin[tile];
                }
            }

            if (closestTile == null) {
                throw new InvalidOperationException("Could not find closest tile. Did you pass an empty hashset or dictionary?");
            }
            else {
                return (SimpleTile)closestTile;
            }
        }

        private static (Dictionary<(SimpleTile, SimpleTile), float>, Dictionary<SimpleTile, HashSet<SimpleTile>>) GenerateGraph(SimpleTile[,] tileMatrix) {
            Dictionary<(SimpleTile, SimpleTile), float> adjacencyMatrix = new();
            Dictionary<SimpleTile, HashSet<SimpleTile>> tileNeighbours = new();

            // We iterate only over the actual game map. We exclude the first and last row/column because it's filled with end of map tiles or warp tiles both of which cannot have edges.
            // Warp tiles can have a directed edge from an empty tile but the processing of the empty tile will handle that.
            // Iterating in this way also means we can check for tiles surrounding the current tile without having to worry about going out of bounds
            for (int i = 1; i < tileMatrix.GetLength(0) - 1; i++) {
                for (int j = 1; j < tileMatrix.GetLength(1) - 1; j++) {
                    SimpleTile currentTile = tileMatrix[i, j];

                    // Create an empty hashset of neighbours if it doesn't exist already because we want all tiles to be represented in the tileNeighbours dictionary even if they are unconnected.
                    tileNeighbours.TryAdd(currentTile, new());

                    // If tile is blocked or is a warp point, it cannot have edges to any other tile
                    if (currentTile.Type == SimpleTile.TileType.Blocked || currentTile.Type == SimpleTile.TileType.WarpPoint) {
                        continue;
                    }
                    else if (currentTile.Type == SimpleTile.TileType.Empty) {
                        (int, int)[] middleNeighbours = GetMiddleNeighbourIndices(i, j);
                        (int, int)[] cornerNeighbours = GetCornerNeighbourIndices(i, j);
                        foreach ((int ni, int nj) in middleNeighbours) {
                            SimpleTile neighbourTile = tileMatrix[ni, nj];

                            // If the adjacency matrix has the edge already in even one direction, it means this tuple of nodes has been processed already. We don't need to do it again.
                            // Warp points are the only exception to this but we don't need to worry about those if we check the ((i,j),(ni,nj)) pair.
                            if (!adjacencyMatrix.ContainsKey((currentTile, neighbourTile))) {
                                if (neighbourTile.Type == SimpleTile.TileType.Empty) {
                                    // Add edge in both directions
                                    adjacencyMatrix.Add((currentTile, neighbourTile), STRAIGHT_WEIGHT);
                                    adjacencyMatrix.Add((neighbourTile, currentTile), STRAIGHT_WEIGHT);
                                    // Add neighbours in both directions
                                    if (!tileNeighbours.TryAdd(currentTile, new HashSet<SimpleTile>() { neighbourTile })) {
                                        tileNeighbours[currentTile].Add(neighbourTile);
                                    }
                                    if (!tileNeighbours.TryAdd(neighbourTile, new HashSet<SimpleTile>() { currentTile })) {
                                        tileNeighbours[neighbourTile].Add(currentTile);
                                    }
                                }
                                else if (neighbourTile.Type == SimpleTile.TileType.WarpPoint) {
                                    // Add edge in one direction
                                    adjacencyMatrix.Add((currentTile, neighbourTile), STRAIGHT_WEIGHT);
                                    // Add neighbour in one direction
                                    if (!tileNeighbours.TryAdd(currentTile, new HashSet<SimpleTile>() { neighbourTile })) {
                                        tileNeighbours[currentTile].Add(neighbourTile);
                                    }
                                }
                            }
                        }
                        foreach ((int ni, int nj) in cornerNeighbours) {
                            SimpleTile neighbourTile = tileMatrix[ni, nj];

                            // If the adjacency matrix has the edge already in even one direction, it means this tuple of nodes has been processed already. We don't need to do it again.
                            // Warp points are the only exception to this but we don't need to worry about those if we check the ((i,j),(ni,nj)) pair.
                            if (!adjacencyMatrix.ContainsKey((currentTile, neighbourTile))) {
                                // To walk diagonally in stardew, all 4 tiles in a square need to be empty. I.e. if the layout looks like (o is free, x is blocked):
                                //     x o
                                //     o o 
                                // the player cannot run diagonally across this. So we need to check that all 4 tiles are free.
                                SimpleTile crossingDiagonalTile1 = tileMatrix[i, nj];
                                SimpleTile crossingDiagonalTile2 = tileMatrix[ni, j];
                                if (neighbourTile.Type == SimpleTile.TileType.Empty && crossingDiagonalTile1.Type == SimpleTile.TileType.Empty && crossingDiagonalTile2.Type == SimpleTile.TileType.Empty) {
                                    // Add edge in both directions for BOTH diagonals
                                    adjacencyMatrix.Add((currentTile, neighbourTile), DIAGONAL_WEIGHT);
                                    adjacencyMatrix.Add((neighbourTile, currentTile), DIAGONAL_WEIGHT);
                                    adjacencyMatrix.Add((crossingDiagonalTile1, crossingDiagonalTile2), DIAGONAL_WEIGHT);
                                    adjacencyMatrix.Add((crossingDiagonalTile2, crossingDiagonalTile1), DIAGONAL_WEIGHT);
                                    // Add neighbours in both directions for both diagonals
                                    if (!tileNeighbours.TryAdd(currentTile, new HashSet<SimpleTile>() { neighbourTile })) {
                                        tileNeighbours[currentTile].Add(neighbourTile);
                                    }
                                    if (!tileNeighbours.TryAdd(neighbourTile, new HashSet<SimpleTile>() { currentTile })) {
                                        tileNeighbours[neighbourTile].Add(currentTile);
                                    }
                                    if (!tileNeighbours.TryAdd(crossingDiagonalTile1, new HashSet<SimpleTile>() { crossingDiagonalTile2 })) {
                                        tileNeighbours[crossingDiagonalTile1].Add(crossingDiagonalTile2);
                                    }
                                    if (!tileNeighbours.TryAdd(crossingDiagonalTile2, new HashSet<SimpleTile>() { crossingDiagonalTile1 })) {
                                        tileNeighbours[crossingDiagonalTile2].Add(crossingDiagonalTile1);
                                    }
                                }
                                // else if condition for warp point diagonal is more complex. It would require checking that the corresponding crossing tile in line with the warp point is either empty or a warp point itself.
                            }
                        }

                    }
                    else {
                        // Should never happen but I want to know if we get here
                        throw new InvalidOperationException($"Came across impossible tile: {currentTile}");
                    }
                }
            }

            return (adjacencyMatrix, tileNeighbours);
        }

        private static (int, int)[] GetMiddleNeighbourIndices(int x, int y) {
            // left, right, up, down only. No corners.
            return new (int, int)[] { (x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1) };
        }

        private static (int, int)[] GetCornerNeighbourIndices(int x, int y) {
            // corners only.
            return new (int, int)[] { (x - 1, y - 1), (x - 1, y + 1), (x + 1, y - 1), (x + 1, y + 1) };
        }

    }
}
