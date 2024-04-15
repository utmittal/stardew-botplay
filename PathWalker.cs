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
        Direction currentDirection;
        IGameLoopEvents gameLoopEvents;
        IMonitor monitor;

        SimpleTile nextTile;

        public PathWalker(List<SimpleTile> path, InputSimulator inputSimulator, IGameLoopEvents gameLoopEvents,IMonitor monitor) {
            this.path = new(path);
            this.inputSimulator = inputSimulator;
            this.currentDirection = Direction.None;
            this.gameLoopEvents = gameLoopEvents;
            this.monitor = monitor;
        }

        public void InitiateWalk() {
            if (this.path.Count == 0) {
                monitor.Log($"Empth path provided. Not moving.",LogLevel.Warn);
                return;
            }

            nextTile = path.Dequeue();
            SimpleTile origin = nextTile;
            if (origin.X != Game1.player.Tile.X || origin.Y != Game1.player.Tile.Y) {
                monitor.Log($"Current player location: {Game1.player.Tile.X}, {Game1.player.Tile.Y} did not match path origin: {origin.X},{origin.Y}. Not moving.",LogLevel.Warn);
                return;
            }

            monitor.Log($"Adding walking event handler.");
            gameLoopEvents.UpdateTicked += this.GameLoop_UpdateTicked_WalkPath;
        }

        // diagonal walking is still a bit janky because the player keeps getting caught up on obstacles because of the float location for the player.
        // I think we need more smarts for "PlayerAtLocation" by checking the player is fully "inside the tile".
        private void GameLoop_UpdateTicked_WalkPath(object? sender, UpdateTickedEventArgs e) {
            (int x, int y) player = ((int)Math.Round(Game1.player.Tile.X), (int)Math.Round(Game1.player.Tile.Y));
            //monitor.Log($"Player location: {player.x},{player.y}");
            //monitor.Log($"Next tile: {nextTile.X},{nextTile.Y}");
            //monitor.Log($"Current direction: {currentDirection}");

            // Stop moving if we go through a warp point or end up off course
            if (PlayerOffCourse(player, nextTile)) {
                monitor.Log($"Next tile ({nextTile.X},{nextTile.Y}) is not in a 1 tile circle of the player ({player.x},{player.y}). Either player is off course or player used a warp point. Stopping movement.");
                StopGracefully();
            }

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
                    StopGracefully();
                    return;
                }
            }
        }

        private void StopGracefully() {
            currentDirection = Direction.None;
            UpdateInput(Direction.None);
            // TODO: What happens if the eventhandler is not already in the list? Does it just throw an exception?
            monitor.Log($"Removing walking event handler.");
            gameLoopEvents.UpdateTicked -= this.GameLoop_UpdateTicked_WalkPath;
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
