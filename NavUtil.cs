using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPlay {
    internal class NavUtil {
        /// <summary>
        /// Finds all warps to specified location from current game location.
        /// </summary>
        /// <param name="targetName"></param>
        /// <returns></returns>
        public static List<Warp> FindWarpsTo(Location targetName) {
            List<Warp> warpList = new();
            foreach (Warp warp in Game1.currentLocation.warps) {
                if (warp.TargetName == targetName.Value) {
                    warpList.Add(warp);
                }
            }
            return warpList;
        }

        /// <summary>
        /// Finds a warp to specified location and walks to it.
        /// </summary>
        /// <param name="location">Location type to warp to</param>
        /// <param name="inputSimulator">Input simulator object to pass to PathWalker</param>
        /// <param name="gameLoopEventHandler">IGameLoopEvents object to hook into</param>
        /// <param name="monitor">SMAPI monitor for logging</param>
        public static void GoToWarp(Location location, InputSimulator inputSimulator, IGameLoopEvents gameLoopEventHandler, IMonitor monitor) {
            if (Game1.currentLocation.Name == location.Value) {
                monitor.Log($"Already at {location.Value}. Not moving.",LogLevel.Debug);
                return;
            }

            List<Warp> warps = NavUtil.FindWarpsTo(location);
            if (warps.Count == 0) {
                monitor.Log($"Could not find any warps to {location.Value}. Not moving.", LogLevel.Debug);
                return;
            }

            // Choose any of the warps, we don't care
            Warp warp = warps.First();

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            SimpleMap currentMap = new SimpleMap(Game1.currentLocation);
            PathFinder pathFinder = new PathFinder(currentMap);
            var pathToWarp = pathFinder.FindPath(((int)Game1.player.Tile.X, (int)Game1.player.Tile.Y), (warp.X, warp.Y));
            stopwatch.Stop();
            monitor.Log($"PathFinding for {location.Value} took {stopwatch.ElapsedMilliseconds}");

            PathWalker pathWalker = new PathWalker(pathToWarp, inputSimulator, gameLoopEventHandler, monitor);
            pathWalker.InitiateWalk();
        }
    }
}
