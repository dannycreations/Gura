using System;
using InControl;
using UnityEngine;

[RequireComponent(typeof(InControlInputModule))]
public class InputModuleActionAdapter : MonoBehaviour
{
	private void OnEnable()
	{
		this.CreateActions();
		InControlInputModule component = base.GetComponent<InControlInputModule>();
		if (component != null)
		{
			component.SubmitAction = this.actions.Submit;
			component.CancelAction = this.actions.Cancel;
			component.MoveAction = this.actions.Move;
		}
	}

	private void OnDisable()
	{
		this.DestroyActions();
	}

	private void CreateActions()
	{
		this.actions = new InputModuleActionAdapter.InputModuleActions();
		this.actions.Submit.AddDefaultBinding(InputControlType.Action1);
		this.actions.Submit.AddDefaultBinding(Key.Space, 1);
		this.actions.Submit.AddDefaultBinding(Key.Return, 1);
		this.actions.Cancel.AddDefaultBinding(InputControlType.Action2);
		this.actions.Cancel.AddDefaultBinding(Key.Escape, 1);
		this.actions.Up.AddDefaultBinding(Key.UpArrow, 1);
		this.actions.Down.AddDefaultBinding(Key.DownArrow, 1);
		this.actions.Left.AddDefaultBinding(Key.LeftArrow, 1);
		this.actions.Right.AddDefaultBinding(Key.RightArrow, 1);
	}

	private void DestroyActions()
	{
		this.actions.Destroy();
	}

	private InputModuleActionAdapter.InputModuleActions actions;

	public class InputModuleActions : PlayerActionSet
	{
		public InputModuleActions()
		{
			this.Submit = base.CreatePlayerAction("Submit");
			this.Cancel = base.CreatePlayerAction("Cancel");
			this.Left = base.CreatePlayerAction("Move Left");
			this.Right = base.CreatePlayerAction("Move Right");
			this.Up = base.CreatePlayerAction("Move Up");
			this.Down = base.CreatePlayerAction("Move Down");
			this.Move = base.CreateTwoAxisPlayerAction(this.Left, this.Right, this.Down, this.Up);
		}

		public PlayerAction Submit;

		public PlayerAction Cancel;

		public PlayerAction Left;

		public PlayerAction Right;

		public PlayerAction Up;

		public PlayerAction Down;

		public PlayerTwoAxisAction Move;
	}
}
