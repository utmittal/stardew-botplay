using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPlay {
    internal class HybridCoord {
        // TODO: Some places like PathFinder don't use this class
        public (int X, int Y) Game {
            get;
        }
        public (int X, int Y) SimpleMap {
            get;
        }

        public static HybridCoord FromGameCoord(int gameX, int gameY) {
            return new HybridCoord((gameX, gameY), (gameX + 1, gameY + 1));
        }

        public static HybridCoord FromGameCoord(float gameX, float gameY) {
            return FromGameCoord((int)gameX, (int)gameY);
        }

        public static HybridCoord FromSimpleMapCoord(int simpleMapX, int simpleMapY) {
            // Note: it's valid to convert simple map index 0 to game index -1 because warp points etc are often represented by out of index numbers in game code.
            return new HybridCoord((simpleMapX - 1, simpleMapY - 1), (simpleMapX, simpleMapY));
        }

        public static HybridCoord FromSimpleMapCoord(float simpleMapX, float simpleMapY) {
            // Note: it's valid to convert simple map index 0 to game index -1 because warp points etc are often represented by out of index numbers in game code.
            return FromSimpleMapCoord((int)simpleMapX, (int)simpleMapY);
        }

        private HybridCoord((int x, int y) game, (int x, int y) simpleMap) {
            // Pretty sure we don't need a deep copy here since we created the tuple ourselves from value types
            this.Game = game;
            this.SimpleMap = simpleMap;
        }
    }
}
