using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BotPlay {
    internal class Location {
        public static Location FARM = new("Farm");
        public static Location FARMHOUSE = new("FarmHouse");
        public static Location BUSSTOP = new("BusStop");
        public static Location TOWN = new("Town");
        public static Location MOUNTAIN = new("Mountain");
        public static Location BACKWOODS = new("Backwoods");
        public static Location FOREST = new("Forest");

        public string Value {
            get;
        }

        private Location(String val) {
            this.Value = val;
        }
    }
}
