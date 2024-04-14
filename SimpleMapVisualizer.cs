using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BotPlay {
    internal class SimpleMapVisualizer {

        public static void VisualizeMap(SimpleMap map, IMonitor monitor) {
            char[,] debugMap = GetDebugMap(map);

            DrawMap(debugMap, monitor);
        }

        // Does not verify that the path actually belongs on the map. Though presumably it will blow up if the path has out of index tiles.
        public static void VisualizeMap(SimpleMap map, List<SimpleTile> path, IMonitor monitor) {
            char[,] debugMap = GetDebugMap(map);

            foreach (SimpleTile tile in path) {
                // Only draw over "empty" locations. This avoids us overwriting things like player origin and warp destination.
                // However, it opens up the problem that we might draw paths that go through obstacles (if the path is wrong)
                if (debugMap[tile.X + 1, tile.Y + 1] == ' ') {
                    debugMap[tile.X + 1, tile.Y + 1] = '*';
                }
            }

            DrawMap(debugMap, monitor);
        }

        private static char[,] GetDebugMap(SimpleMap map) {
            SimpleTile[,] tileMatrix = map.GetMapTiles();
            char[,] debugMap = new char[tileMatrix.GetLength(0), tileMatrix.GetLength(1)];

            for (int i = 0; i < tileMatrix.GetLength(0); i++) {
                for (int j = 0; j < tileMatrix.GetLength(1); j++) {
                    if (tileMatrix[i, j].Type == SimpleTile.TileType.Empty) {
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

            if (Game1.currentLocation.Name == map.LocationName) {
                int playerX = (int)Game1.player.Tile.X + 1;
                int playerY = (int)Game1.player.Tile.Y + 1;
                debugMap[playerX, playerY] = '@';
            }

            return debugMap;
        }

        private static void DrawMap(char[,] map, IMonitor monitor) {
            for (int i = 0; i < map.GetLength(1); i++) {
                String row = "";
                for (int j = 0; j < map.GetLength(0); j++) {
                    if (j != 0) {
                        row += " ";
                    }
                    row += map[j, i];
                }

                monitor.Log(row);
            }
        }
    }
}
