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

        readonly Map Map;
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
        /// Visualizes the Map as a 2d character array with non-passable tiles marked as 'x' and everything else being empty. If the player is currently on this map, it also marks the player location with '@'.
        /// </summary>
        /// <param name="monitor">IMonitor object to log the visualized Map to.</param>
        public void VisualizeMap(IMonitor monitor) {
            int height = GetHeight();
            int width = GetWidth();

            char[,] visualizer = new char[width, height];
            for (int i = 0; i < width; i++) {
                for (int j = 0; j < height; j++) {
                    if (Game1.currentLocation.IsTileBlockedBy(new Vector2(i, j), ignorePassables: CollisionMask.All)) {
                        visualizer[i, j] = 'x';
                    }
                    else {
                        visualizer[i, j] = ' ';
                    }
                }
            }

            if (Game1.player.currentLocation.Map.Id == this.Map.Id) {
                visualizer[(int)Game1.player.Tile.X, (int)Game1.player.Tile.Y] = '@';
            }

            for (int i = 0; i < height; i++) {
                String row = "";
                for (int j = 0; j < width; j++) {
                    row += $"{visualizer[j, i]} ";
                }
                monitor.Log(row);
            }
        }
    }
}
