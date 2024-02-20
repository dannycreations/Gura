using System;
using InControl;

public class CustomPlayerTwoAxisAction : PlayerTwoAxisAction
{
	internal CustomPlayerTwoAxisAction(PlayerAction negativeXAction, PlayerAction positiveXAction, PlayerAction negativeYAction, PlayerAction positiveYAction, PlayerActionSet owner, bool isUnblockable)
		: base(negativeXAction, positiveXAction, negativeYAction, positiveYAction)
	{
		this.CAInstance = (ControlsActions)owner;
		this._isUnblockable = isUnblockable;
	}

	public void Activate()
	{
		this.negativeXAction.Activate();
		this.positiveXAction.Activate();
		this.negativeYAction.Activate();
		this.positiveYAction.Activate();
	}

	public void Deactivate()
	{
		base.ClearInputState();
		this.negativeXAction.Deactivate();
		this.positiveXAction.Deactivate();
		this.negativeYAction.Deactivate();
		this.positiveYAction.Deactivate();
	}

	public override float X
	{
		get
		{
			if (!this._isUnblockable && (this.CAInstance.IsBlockedAxis || (this.CAInstance.IsBlockedKeyboardInput && !this.CAInstance.ExcludeInputList.Contains("Move"))))
			{
				return 0f;
			}
			return base.X;
		}
		protected set
		{
			base.X = value;
		}
	}

	public override float Y
	{
		get
		{
			if (!this._isUnblockable && (this.CAInstance.IsBlockedAxis || (this.CAInstance.IsBlockedKeyboardInput && !this.CAInstance.ExcludeInputList.Contains("Move"))))
			{
				return 0f;
			}
			return (!InputManager.InvertYAxis) ? base.Y : (-base.Y);
		}
		protected set
		{
			base.Y = value;
		}
	}

	private ControlsActions CAInstance;

	private bool _isUnblockable;
}
