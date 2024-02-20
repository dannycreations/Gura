using System;

namespace InControl
{
	public class MouseBindingSourceListener : BindingSourceListener
	{
		public void Reset()
		{
			this.detectFound = Mouse.None;
			this.detectPhase = 0;
		}

		public BindingSource Listen(BindingListenOptions listenOptions, InputDevice device)
		{
			if (!listenOptions.IncludeMouseButtons)
			{
				return null;
			}
			if (this.detectFound != Mouse.None && !MouseBindingSource.ButtonIsPressed(this.detectFound) && this.detectPhase == 2)
			{
				MouseBindingSource mouseBindingSource = new MouseBindingSource(this.detectFound);
				this.Reset();
				return mouseBindingSource;
			}
			Mouse mouse = this.ListenForControl();
			if (mouse != Mouse.None)
			{
				if (this.detectPhase == 1)
				{
					this.detectFound = mouse;
					this.detectPhase = 2;
				}
			}
			else if (this.detectPhase == 0)
			{
				this.detectPhase = 1;
			}
			return null;
		}

		private Mouse ListenForControl()
		{
			for (Mouse mouse = Mouse.None; mouse <= Mouse.Button9; mouse++)
			{
				if (MouseBindingSource.ButtonIsPressed(mouse))
				{
					return mouse;
				}
			}
			return Mouse.None;
		}

		private Mouse detectFound;

		private int detectPhase;
	}
}
