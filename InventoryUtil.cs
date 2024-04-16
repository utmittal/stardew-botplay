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
            return TryEquipItem(PICKAXE, monitor);
        }

        public static bool TryEquipAxe(IMonitor monitor) {
            return TryEquipItem(AXE, monitor);
        }

        public static bool TryEquipScythe(IMonitor monitor) {
            return TryEquipItem(SCYTHE, monitor);
        }

        private static bool TryEquipItem(string itemName, IMonitor monitor) {
            if (Game1.player.CurrentItem.Name == itemName) {
                return true;
            }

            for (int i = 0; i < Game1.player.Items.Count; i++) {
                if (Game1.player.Items[i].Name == itemName) {
                    Game1.player.CurrentToolIndex = i;
                    monitor.Log($"Equipping item {itemName}");
                    return true;
                }
            }
            return false;
        }
    }
}
