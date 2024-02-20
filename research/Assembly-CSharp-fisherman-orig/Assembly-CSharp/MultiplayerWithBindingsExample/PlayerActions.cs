using System;
using InControl;

namespace MultiplayerWithBindingsExample
{
	public class PlayerActions : PlayerActionSet
	{
		public PlayerActions()
		{
			this.Green = base.CreatePlayerAction("Green");
			this.Red = base.CreatePlayerAction("Red");
			this.Blue = base.CreatePlayerAction("Blue");
			this.Yellow = base.CreatePlayerAction("Yellow");
			this.Left = base.CreatePlayerAction("Left");
			this.Right = base.CreatePlayerAction("Right");
			this.Up = base.CreatePlayerAction("Up");
			this.Down = base.CreatePlayerAction("Down");
			this.Rotate = base.CreateTwoAxisPlayerAction(this.Left, this.Right, this.Down, this.Up);
		}

		public static PlayerActions CreateWithKeyboardBindings()
		{
			PlayerActions playerActions = new PlayerActions();
			playerActions.Green.AddDefaultBinding(Key.A, 1);
			playerActions.Red.AddDefaultBinding(Key.S, 1);
			playerActions.Blue.AddDefaultBinding(Key.D, 1);
			playerActions.Yellow.AddDefaultBinding(Key.F, 1);
			playerActions.Up.AddDefaultBinding(Key.UpArrow, 1);
			playerActions.Down.AddDefaultBinding(Key.DownArrow, 1);
			playerActions.Left.AddDefaultBinding(Key.LeftArrow, 1);
			playerActions.Right.AddDefaultBinding(Key.RightArrow, 1);
			return playerActions;
		}

		public static PlayerActions CreateWithJoystickBindings()
		{
			PlayerActions playerActions = new PlayerActions();
			playerActions.Green.AddDefaultBinding(InputControlType.Action1);
			playerActions.Red.AddDefaultBinding(InputControlType.Action2);
			playerActions.Blue.AddDefaultBinding(InputControlType.Action3);
			playerActions.Yellow.AddDefaultBinding(InputControlType.Action4);
			playerActions.Up.AddDefaultBinding(InputControlType.LeftStickUp);
			playerActions.Down.AddDefaultBinding(InputControlType.LeftStickDown);
			playerActions.Left.AddDefaultBinding(InputControlType.LeftStickLeft);
			playerActions.Right.AddDefaultBinding(InputControlType.LeftStickRight);
			playerActions.Up.AddDefaultBinding(InputControlType.DPadUp);
			playerActions.Down.AddDefaultBinding(InputControlType.DPadDown);
			playerActions.Left.AddDefaultBinding(InputControlType.DPadLeft);
			playerActions.Right.AddDefaultBinding(InputControlType.DPadRight);
			return playerActions;
		}

		public PlayerAction Green;

		public PlayerAction Red;

		public PlayerAction Blue;

		public PlayerAction Yellow;

		public PlayerAction Left;

		public PlayerAction Right;

		public PlayerAction Up;

		public PlayerAction Down;

		public PlayerTwoAxisAction Rotate;
	}
}
