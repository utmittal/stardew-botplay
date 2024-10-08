﻿using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;

namespace BotPlay {
    internal class PathFinder {

        private static readonly float STRAIGHT_WEIGHT = 1;
        private static readonly float DIAGONAL_WEIGHT = 1.41f;  // length of diagonal in a square of side 1

        private readonly SimpleMap map;
        private readonly Dictionary<(SimpleTile, SimpleTile), float> adjacencyMatrix;
        private readonly Dictionary<SimpleTile, HashSet<SimpleTile>> tileNeighbours;

        public PathFinder(SimpleMap map) {
            this.map = map;
            // TODO: Refactor class to avoid doing so much work in the constructor. It feels fishy but also makes (future) testing hard.
            (this.adjacencyMatrix, this.tileNeighbours) = GenerateGraph();
        }

        IMonitor monitor;
        public PathFinder(SimpleMap map, IMonitor monitor) : this(map) {
            this.monitor = monitor;
        }

        public List<SimpleTile> FindPath((int x, int y) origin, (int x, int y) destination) {
            SimpleTile originTile = map.GetMapTiles()[origin.x + 1, origin.y + 1];
            SimpleTile destinationTile = map.GetMapTiles()[destination.x + 1, destination.y + 1];

            // Stores preceding tile on path
            Dictionary<SimpleTile, SimpleTile> prevTile = new();
            // Stores distance of tile from originTile
            Dictionary<SimpleTile, float> distanceFromOrigin = new();
            // Stores all the tiles we still need to visit
            HashSet<SimpleTile> tilesToVisit = new();
            tilesToVisit.Add(originTile);
            // Stores all the tiles we already visited
            // This way we don't need to add all the tiles to "tilesToVisit" right at the start.
            // Which reduces computation in GetClosestTile() for large grids.
            // Could potentially solve this by implementing our own minheap, but this is easier.
            HashSet<SimpleTile> visitedTiles = new();

            foreach (SimpleTile tile in map.GetMapTiles()) {
                distanceFromOrigin[tile] = float.MaxValue;
            }
            distanceFromOrigin[originTile] = 0;

            while (tilesToVisit.Count > 0) {
                SimpleTile currentTile = GetClosestTile(tilesToVisit, distanceFromOrigin);
                tilesToVisit.Remove(currentTile);
                visitedTiles.Add(currentTile);

                if (currentTile == destinationTile) {
                    break;
                }

                foreach (SimpleTile neighbour in tileNeighbours[currentTile]) {
                    float distanceNeighbourFromOrigin = distanceFromOrigin[currentTile] + adjacencyMatrix[(currentTile, neighbour)];
                    if (distanceNeighbourFromOrigin < distanceFromOrigin[neighbour]) {
                        distanceFromOrigin[neighbour] = distanceNeighbourFromOrigin;
                        prevTile[neighbour] = currentTile;
                    }
                    if (!visitedTiles.Contains(neighbour)) {
                        tilesToVisit.Add(neighbour);
                    }
                }
            }

            // if prevtile contains no entry for destinationTile, it means we didn't find a path.
            if (!prevTile.ContainsKey(destinationTile)) {
                return new List<SimpleTile>();
            }

            List<SimpleTile> path = new List<SimpleTile>();
            SimpleTile backtrackTile = destinationTile;
            while (backtrackTile != originTile) {
                path.Insert(0, backtrackTile);
                backtrackTile = prevTile[backtrackTile];
            }
            // Add origin tile into the path
            path.Insert(0, originTile);

            return path;
        }

        public List<SimpleTile> FindPathToClosestDebris((int x, int y) origin) {
            SimpleTile originTile = map.GetMapTiles()[origin.x + 1, origin.y + 1];
            SimpleTile? destinationTile = null;

            // Stores preceding tile on path
            Dictionary<SimpleTile, SimpleTile> prevTile = new();
            // Stores distance of tile from originTile
            Dictionary<SimpleTile, float> distanceFromOrigin = new();
            // Stores all the tiles we still need to visit
            HashSet<SimpleTile> tilesToVisit = new();
            tilesToVisit.Add(originTile);
            // Stores all the tiles we already visited
            // This way we don't need to add all the tiles to "tilesToVisit" right at the start.
            // Which reduces computation in GetClosestTile() for large grids.
            // Could potentially solve this by implementing our own minheap, but this is easier.
            HashSet<SimpleTile> visitedTiles = new();

            foreach (SimpleTile tile in map.GetMapTiles()) {
                distanceFromOrigin[tile] = float.MaxValue;
            }
            distanceFromOrigin[originTile] = 0;

            while (tilesToVisit.Count > 0) {
                SimpleTile currentTile = GetClosestTile(tilesToVisit, distanceFromOrigin);
                tilesToVisit.Remove(currentTile);
                visitedTiles.Add(currentTile);

                // Debris tiles are mostly blocked tiles, so they won't be in the map graph at all.
                // But since we need to stand next to them, we just check ACTUAL neighbouring tiles.
                if (IsNextToDebris(currentTile)) {
                    destinationTile = currentTile;
                    break;
                }

                foreach (SimpleTile neighbour in tileNeighbours[currentTile]) {
                    float distanceNeighbourFromOrigin = distanceFromOrigin[currentTile] + adjacencyMatrix[(currentTile, neighbour)];
                    if (distanceNeighbourFromOrigin < distanceFromOrigin[neighbour]) {
                        distanceFromOrigin[neighbour] = distanceNeighbourFromOrigin;
                        prevTile[neighbour] = currentTile;
                    }
                    if (!visitedTiles.Contains(neighbour)) {
                        tilesToVisit.Add(neighbour);
                    }
                }
            }

            // if prevtile contains no entry for destinationTile, it means we didn't find a path.
            if (destinationTile == null) {
                return new List<SimpleTile>();
            }

            List<SimpleTile> path = new List<SimpleTile>();
            SimpleTile backtrackTile = (SimpleTile)destinationTile;
            while (backtrackTile != originTile) {
                path.Insert(0, backtrackTile);
                backtrackTile = prevTile[backtrackTile];
            }
            // Add origin tile into the path
            path.Insert(0, originTile);

            return path;
        }

        private bool IsNextToDebris(SimpleTile tile) {
            foreach (var neighbour in GetMiddleNeighbourIndices(tile.X, tile.Y)) {
                SimpleTile ntile = this.map.GetMapTiles()[neighbour.Item1, neighbour.Item2];
                if (ntile.Content == SimpleTile.TileContent.Tree || ntile.Content == SimpleTile.TileContent.Weeds || ntile.Content == SimpleTile.TileContent.Twig || ntile.Content == SimpleTile.TileContent.Stone) {
                    return true;
                }
            }
            return false;
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

        private (Dictionary<(SimpleTile, SimpleTile), float>, Dictionary<SimpleTile, HashSet<SimpleTile>>) GenerateGraph() {
            Dictionary<(SimpleTile, SimpleTile), float> adjMatrix = new();
            Dictionary<SimpleTile, HashSet<SimpleTile>> tileNeigh = new();

            for (int i = 0; i < map.GetMapTiles().GetLength(0); i++) {
                for (int j = 0; j < map.GetMapTiles().GetLength(1); j++) {
                    SimpleTile currentTile = map.GetMapTiles()[i, j];

                    // Create an empty hashset of neighbours if it doesn't exist already because we want all tiles to be represented in the tileNeighbours dictionary even if they are unconnected.
                    tileNeigh.TryAdd(currentTile, new());

                    // If tile is blocked or is a warp point, it cannot have edges to any other tile
                    if (currentTile.Type == SimpleTile.TileType.Blocked || currentTile.Type == SimpleTile.TileType.WarpPoint || currentTile.Type == SimpleTile.TileType.EndOfMap) {
                        continue;
                    }
                    else if (currentTile.Type == SimpleTile.TileType.Empty) {
                        // Note: the only reason this doesn't blow up is because the boundary tiles for the map are garunteed to be endofmap or warppoint tiles which get caught in the earlier if 
                        // condition. If one of those tiles somehow falls through to this point, the neighbouring tiles would throw out of index errors and blow up the program.
                        // A risk I am willing to take.
                        (int, int)[] middleNeighbours = GetMiddleNeighbourIndices(i, j);
                        (int, int)[] cornerNeighbours = GetCornerNeighbourIndices(i, j);
                        foreach ((int ni, int nj) in middleNeighbours) {
                            SimpleTile neighbourTile = map.GetMapTiles()[ni, nj];

                            // If the adjacency matrix has the edge already in even one direction, it means this tuple of nodes has been processed already. We don't need to do it again.
                            // Warp points are the only exception to this but warp tiles only have unidirectional links which is handled in the else if condition and doesn't need to be handled again.
                            if (!adjMatrix.ContainsKey((currentTile, neighbourTile))) {
                                if (neighbourTile.Type == SimpleTile.TileType.Empty) {
                                    // Add edge in both directions
                                    adjMatrix.Add((currentTile, neighbourTile), STRAIGHT_WEIGHT);
                                    adjMatrix.Add((neighbourTile, currentTile), STRAIGHT_WEIGHT);
                                    // Add neighbours in both directions
                                    if (!tileNeigh.TryAdd(currentTile, new HashSet<SimpleTile>() { neighbourTile })) {
                                        tileNeigh[currentTile].Add(neighbourTile);
                                    }
                                    if (!tileNeigh.TryAdd(neighbourTile, new HashSet<SimpleTile>() { currentTile })) {
                                        tileNeigh[neighbourTile].Add(currentTile);
                                    }
                                }
                                else if (neighbourTile.Type == SimpleTile.TileType.WarpPoint) {
                                    // Add edge in one direction
                                    adjMatrix.Add((currentTile, neighbourTile), STRAIGHT_WEIGHT);
                                    // Add neighbour in one direction
                                    if (!tileNeigh.TryAdd(currentTile, new HashSet<SimpleTile>() { neighbourTile })) {
                                        tileNeigh[currentTile].Add(neighbourTile);
                                    }
                                }
                            }
                        }
                        foreach ((int ni, int nj) in cornerNeighbours) {
                            SimpleTile neighbourTile = map.GetMapTiles()[ni, nj];

                            // If the adjacency matrix has the edge already in even one direction, it means this tuple of nodes has been processed already. We don't need to do it again.
                            // Warp points are the only exception to this
                            if (!adjMatrix.ContainsKey((currentTile, neighbourTile))) {
                                // To walk diagonally in stardew, all 4 tiles in a square need to be empty. I.e. if the layout looks like (o is free, x is blocked):
                                //     x o
                                //     o o 
                                // the player cannot run diagonally across this. So we need to check that all 4 tiles are free.
                                SimpleTile crossingDiagonalTile1 = map.GetMapTiles()[i, nj];
                                SimpleTile crossingDiagonalTile2 = map.GetMapTiles()[ni, j];
                                if (neighbourTile.Type == SimpleTile.TileType.Empty && crossingDiagonalTile1.Type == SimpleTile.TileType.Empty && crossingDiagonalTile2.Type == SimpleTile.TileType.Empty) {
                                    // Add edge in both directions for BOTH diagonals
                                    adjMatrix.Add((currentTile, neighbourTile), DIAGONAL_WEIGHT);
                                    adjMatrix.Add((neighbourTile, currentTile), DIAGONAL_WEIGHT);
                                    adjMatrix.Add((crossingDiagonalTile1, crossingDiagonalTile2), DIAGONAL_WEIGHT);
                                    adjMatrix.Add((crossingDiagonalTile2, crossingDiagonalTile1), DIAGONAL_WEIGHT);
                                    // Add neighbours in both directions for both diagonals
                                    if (!tileNeigh.TryAdd(currentTile, new HashSet<SimpleTile>() { neighbourTile })) {
                                        tileNeigh[currentTile].Add(neighbourTile);
                                    }
                                    if (!tileNeigh.TryAdd(neighbourTile, new HashSet<SimpleTile>() { currentTile })) {
                                        tileNeigh[neighbourTile].Add(currentTile);
                                    }
                                    if (!tileNeigh.TryAdd(crossingDiagonalTile1, new HashSet<SimpleTile>() { crossingDiagonalTile2 })) {
                                        tileNeigh[crossingDiagonalTile1].Add(crossingDiagonalTile2);
                                    }
                                    if (!tileNeigh.TryAdd(crossingDiagonalTile2, new HashSet<SimpleTile>() { crossingDiagonalTile1 })) {
                                        tileNeigh[crossingDiagonalTile2].Add(crossingDiagonalTile1);
                                    }
                                }
                                // TODO: Consider implementing else if location for warp point diagonal. else if condition for warp point diagonal is more complex. It would require checking that
                                // the corresponding crossing tile in line with the warp point is either empty or a warp point itself. Given that a horizonal path will always exist if a diagonal 
                                // path exists, it's fine to skip this for now. It's a minor optimization.
                            }
                        }

                    }
                    else {
                        // Should never happen but I want to know if we get here
                        throw new InvalidOperationException($"Came across impossible tile: {currentTile}");
                    }
                }
            }

            return (adjMatrix, tileNeigh);
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
