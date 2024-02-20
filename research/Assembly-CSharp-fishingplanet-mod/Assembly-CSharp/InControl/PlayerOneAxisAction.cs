using System;
using System.Diagnostics;

namespace InControl
{
	public class PlayerOneAxisAction : OneAxisInputControl
	{
		internal PlayerOneAxisAction(PlayerAction negativeAction, PlayerAction positiveAction)
		{
			this.negativeAction = negativeAction;
			this.positiveAction = positiveAction;
			this.Raw = true;
		}

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<BindingSourceType> OnLastInputTypeChanged;

		public object UserData { get; set; }

		internal void Update(ulong updateTick, float deltaTime)
		{
			this.ProcessActionUpdate(this.negativeAction);
			this.ProcessActionUpdate(this.positiveAction);
			float num = Utility.ValueFromSides(this.negativeAction, this.positiveAction);
			base.CommitWithValue(num, updateTick, deltaTime);
		}

		private void ProcessActionUpdate(PlayerAction action)
		{
			BindingSourceType bindingSourceType = this.LastInputType;
			if (action.UpdateTick > base.UpdateTick)
			{
				base.UpdateTick = action.UpdateTick;
				bindingSourceType = action.LastInputType;
			}
			if (this.LastInputType != bindingSourceType)
			{
				this.LastInputType = bindingSourceType;
				if (this.OnLastInputTypeChanged != null)
				{
					this.OnLastInputTypeChanged(bindingSourceType);
				}
			}
		}

		[Obsolete("Please set this property on device controls directly. It does nothing here.")]
		public new float LowerDeadZone
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		[Obsolete("Please set this property on device controls directly. It does nothing here.")]
		public new float UpperDeadZone
		{
			get
			{
				return 0f;
			}
			set
			{
			}
		}

		private PlayerAction negativeAction;

		private PlayerAction positiveAction;

		public BindingSourceType LastInputType;
	}
}
