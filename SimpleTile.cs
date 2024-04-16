using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPlay {
    internal record struct SimpleTile {
        public enum TileType {
            Unknown,
            Empty,
            Blocked,
            WarpPoint,
            EndOfMap
        }

        public enum TileContent {
            Unknown,
            Empty,
            Tree,
            Twig,
            Stone,
            Weeds,
            SeedSpot    // Seed spot is where you dig to find seasonal wild seeds. It's not where a tree seed exists.
        }

        // X coordinate of tile in-game
        public int X {
            get;
        }
        // Y coordinate of tile in-game
        public int Y {
            get;
        }
        public TileType Type {
            get; set;
        }

        public TileContent Content {
            get; set;
        } = TileContent.Unknown;

        public SimpleTile(int x, int y, TileType type) {
            this.X = x;
            this.Y = y;
            this.Type = type;
        }

        public override readonly string ToString() {
            return $"({this.X}, {this.Y}, {this.Type})";
        }
    }
}
