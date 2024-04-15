using StardewValley.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotPlay {
    /// <summary>
    /// InputSimulator that we will use for reflection inside SDV code. This is a singleton
    /// because there is no reason to have multiple of these, but also because it's used to
    /// create PathWalker which is itself a singleton.
    /// 
    /// TODO: It feels a little fishy to have this tightly coupled to PathWalker and be a singleton.
    /// </summary>
    internal class InputSimulator : IInputSimulator {

        private static InputSimulator? singletonInstance = null;
        private static readonly object mutex = new();

        private InputSimulator() {
        }

        public static InputSimulator Instance {
            get {
                if (singletonInstance == null) {
                    lock (mutex) {
                        singletonInstance ??= new InputSimulator();
                    }
                }
                return singletonInstance;
            }
        }

        public bool ActionButtonPressed { get; set; } = false;
        public bool SwitchToolButtonPressed { get; set; } = false;
        public bool UseToolButtonPressed { get; set; } = false;
        public bool UseToolButtonReleased { get; set; } = false;
        public bool AddItemToInventoryButtonPressed { get; set; } = false;
        public bool CancelButtonPressed { get; set; } = false;
        public bool MoveUpPressed { get; set; } = false;
        public bool MoveRightPressed { get; set; } = false;
        public bool MoveLeftPressed { get; set; } = false;
        public bool MoveDownPressed { get; set; } = false;
        public bool MoveUpReleased { get; set; } = false;
        public bool MoveRightReleased { get; set; } = false;
        public bool MoveLeftReleased { get; set; } = false;
        public bool MoveDownReleased { get; set; } = false;
        public bool MoveDownHeld { get; set; } = false;
        public bool MoveUpHeld { get; set; } = false;
        public bool MoveRightHeld { get; set; } = false;
        public bool MoveLeftHeld { get; set; } = false;
        public void SimulateInput(
            ref bool actionButtonPressed,
            ref bool switchToolButtonPressed,
            ref bool useToolButtonPressed,
            ref bool useToolButtonReleased,
            ref bool addItemToInventoryButtonPressed,
            ref bool cancelButtonPressed,
            ref bool moveUpPressed,
            ref bool moveRightPressed,
            ref bool moveLeftPressed,
            ref bool moveDownPressed,
            ref bool moveUpReleased,
            ref bool moveRightReleased,
            ref bool moveLeftReleased,
            ref bool moveDownReleased,
            ref bool moveUpHeld,
            ref bool moveRightHeld,
            ref bool moveLeftHeld,
            ref bool moveDownHeld) {
            actionButtonPressed = ActionButtonPressed;
            switchToolButtonPressed = SwitchToolButtonPressed;
            useToolButtonPressed = UseToolButtonPressed;
            useToolButtonReleased = UseToolButtonReleased;
            addItemToInventoryButtonPressed = AddItemToInventoryButtonPressed;
            cancelButtonPressed = CancelButtonPressed;
            moveUpPressed = MoveUpPressed;
            moveRightPressed = MoveRightPressed;
            moveLeftPressed = MoveLeftPressed;
            moveDownPressed = MoveDownPressed;
            moveUpReleased = MoveUpReleased;
            moveRightReleased = MoveRightReleased;
            moveLeftReleased = MoveLeftReleased;
            moveDownReleased = MoveDownReleased;
            moveUpHeld = MoveUpHeld;
            moveRightHeld = MoveRightHeld;
            moveLeftHeld = MoveLeftHeld;
            moveDownHeld = MoveDownHeld;
        }
    }
}
