using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile.Tiles;

namespace BotPlay {
    internal class PathWalker {
        private enum Direction {
            None, Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft
        }

        Queue<SimpleTile> path;
        InputSimulator inputSimulator;
        SimpleTile nextTile;
        Direction currentDirection;
        IMonitor monitor;

        public PathWalker(Queue<SimpleTile> path, InputSimulator inputSimulator, IMonitor monitor) {
            this.path = path;
            this.inputSimulator = inputSimulator;
            this.nextTile = this.path.Dequeue();
            this.currentDirection = Direction.None;
            this.monitor = monitor;
        }

        public bool HasWalkingEnded() {
            // the 1 is to account for warp points not being dequeued. But this needs to be more intelligent.
            if (this.path.Count <= 1 && this.currentDirection == Direction.None) {
                return true;
            }
            return false;
        }

        // diagonal walking is still a bit janky because the player keeps getting caught up on obstacles because of the float location for the player.
        // I think we need more smarts for "PlayerAtLocation" by checking the player is fully "inside the tile".
        public void GameLoop_UpdateTicked_WalkPath(object? sender, UpdateTickedEventArgs e) {
            (int x, int y) player = ((int)Math.Round(Game1.player.Tile.X), (int)Math.Round(Game1.player.Tile.Y));
            //monitor.Log($"Player location: {player.x},{player.y}");
            //monitor.Log($"Next tile: {nextTile.X},{nextTile.Y}");
            //monitor.Log($"Current direction: {currentDirection}");

            if (PlayerAtLocation(player, nextTile)) {
                if (path.Count > 0) {
                    nextTile = path.Dequeue();
                    monitor.Log($"\tUpdated next tile: {nextTile.X},{nextTile.Y}");

                    Direction directionToWalk = CalculateDirection(player, nextTile);
                    if (currentDirection != directionToWalk) {
                        UpdateInput(directionToWalk);
                        currentDirection = directionToWalk;
                        monitor.Log($"\t Updated current direction: {currentDirection}");
                    }
                }
                else if (path.Count == 0) {
                    UpdateInput(Direction.None);
                    currentDirection = Direction.None;
                    //monitor.Log($"Current direction: {currentDirection}");
                }
            }

            // kill switch if we end up of course
            if (PlayerOffCourse(player, nextTile)) {
                currentDirection = Direction.None;
                UpdateInput(Direction.None);
                monitor.Log($"Next tile ({nextTile.X},{nextTile.Y}) is not in a 1 tile circle of the player ({player.x},{player.y}). Setting direction to None.");
            }
        }

        private bool PlayerAtLocation((int x, int y) player, SimpleTile tile) {
            if (player.x == tile.X && player.y == tile.Y) {
                return true;
            }
            else {
                return false;
            }
        }

        private bool PlayerOffCourse((int x, int y) player, SimpleTile tile) {
            if (Math.Abs(player.x - tile.X) > 1 || Math.Abs(player.y - tile.Y)>1) {
                return true;
            }
            else {
                return false;
            }
        }

        private Direction CalculateDirection((int x, int y) player, SimpleTile simpleTile) {
            (int x, int y) tile = (simpleTile.X, simpleTile.Y);

            if (player.x == tile.x && player.y - 1 == tile.y) {
                return Direction.Up;
            }
            else if (player.x + 1 == tile.x && player.y - 1 == tile.y) {
                return Direction.UpRight;
            }
            else if (player.x + 1 == tile.x && player.y == tile.y) {
                return Direction.Right;
            }
            else if (player.x + 1 == tile.x && player.y + 1 == tile.y) {
                return Direction.DownRight;
            }
            else if (player.x == tile.x && player.y + 1 == tile.y) {
                return Direction.Down;
            }
            else if (player.x - 1 == tile.x && player.y + 1 == tile.y) {
                return Direction.DownLeft;
            }
            else if (player.x - 1 == tile.x && player.y == tile.y) {
                return Direction.Left;
            }
            else if (player.x - 1 == tile.x && player.y - 1 == tile.y) {
                return Direction.UpLeft;
            }
            else {
                monitor.Log($"Destination tile ({tile.x},{tile.y}) is not in a 1 tile circle of the player ({player.x},{player.y}). Setting direction to None.");
                return Direction.None;
            }
        }

        private void UpdateInput(Direction directionToWalk) {
            if (directionToWalk == Direction.None) {
                inputSimulator.MoveUpHeld = false;
                inputSimulator.MoveRightHeld = false;
                inputSimulator.MoveDownHeld = false;
                inputSimulator.MoveLeftHeld = false;
            }
            else if (directionToWalk == Direction.Up) {
                inputSimulator.MoveUpHeld = true;
                inputSimulator.MoveRightHeld = false;
                inputSimulator.MoveDownHeld = false;
                inputSimulator.MoveLeftHeld = false;
            }
            else if (directionToWalk == Direction.UpRight) {
                inputSimulator.MoveUpHeld = true;
                inputSimulator.MoveRightHeld = true;
                inputSimulator.MoveDownHeld = false;
                inputSimulator.MoveLeftHeld = false;
            }
            else if (directionToWalk == Direction.Right) {
                inputSimulator.MoveUpHeld = false;
                inputSimulator.MoveRightHeld = true;
                inputSimulator.MoveDownHeld = false;
                inputSimulator.MoveLeftHeld = false;
            }
            else if (directionToWalk == Direction.DownRight) {
                inputSimulator.MoveUpHeld = false;
                inputSimulator.MoveRightHeld = true;
                inputSimulator.MoveDownHeld = true;
                inputSimulator.MoveLeftHeld = false;
            }
            else if (directionToWalk == Direction.Down) {
                inputSimulator.MoveUpHeld = false;
                inputSimulator.MoveRightHeld = false;
                inputSimulator.MoveDownHeld = true;
                inputSimulator.MoveLeftHeld = false;
            }
            else if (directionToWalk == Direction.DownLeft) {
                inputSimulator.MoveUpHeld = false;
                inputSimulator.MoveRightHeld = false;
                inputSimulator.MoveDownHeld = true;
                inputSimulator.MoveLeftHeld = true;
            }
            else if (directionToWalk == Direction.Left) {
                inputSimulator.MoveUpHeld = false;
                inputSimulator.MoveRightHeld = false;
                inputSimulator.MoveDownHeld = false;
                inputSimulator.MoveLeftHeld = true;
            }
            else if (directionToWalk == Direction.UpLeft) {
                inputSimulator.MoveUpHeld = true;
                inputSimulator.MoveRightHeld = false;
                inputSimulator.MoveDownHeld = false;
                inputSimulator.MoveLeftHeld = true;
            }
        }
    }
}
