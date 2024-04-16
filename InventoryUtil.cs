using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPlay {
    internal class InventoryUtil {

        private static string PICKAXE = "Pickaxe";
        private static string AXE = "Axe";
        private static string SCYTHE = "Scythe";

        /// <summary>
        /// Tries to equip pickaxe. If not found, returns false.
        /// </summary>
        /// <returns></returns>
        public static bool TryEquipPickaxe(IMonitor monitor) {
            if (Game1.player.CurrentItem.Name == PICKAXE) {
                return true;
            }

            for (int i=0; i < Game1.player.Items.Count; i++) {
                if (Game1.player.Items[i].Name == PICKAXE) {
                    Game1.player.CurrentToolIndex = i;
                    return true;
                }
            }
            return false;
        }
    }
}
