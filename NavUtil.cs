using StardewValley;
using System;
using System.Collections.Generic;
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
    }
}
