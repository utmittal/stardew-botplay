using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPlay {
    internal class SimpleTile {
        public enum TileType {
            Unknown,
            Empty,
            Blocked,
            WarpPoint,
            EndOfMap
        }

        public int X {
            get; set;
        }
        public int Y {
            get; set;
        }
        public TileType Type {
            get; set;
        }

        public SimpleTile(int X, int Y, TileType type) {
            this.X = X;
            this.Y = Y;
            this.Type = type;
        }

        public SimpleTile(int X, int Y) {
            this.X = X;
            this.Y = Y;
            this.Type = TileType.Unknown;
        }
    }
}
