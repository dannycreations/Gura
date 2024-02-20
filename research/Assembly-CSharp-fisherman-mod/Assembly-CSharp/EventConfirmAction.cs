using System;
using System.Diagnostics;
using UnityEngine;

public class EventConfirmAction : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ConfirmActionCalled;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> CancelActionCalled;

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ThirdButtonActionCalled;

	public void ConfirmAction()
	{
		if (this.ConfirmActionCalled != null)
		{
			this.ConfirmActionCalled(this, new EventArgs());
		}
	}

	public void CancelAction()
	{
		if (this.CancelActionCalled != null)
		{
			this.CancelActionCalled(this, new EventArgs());
		}
	}

	public void ThirdButtonAction()
	{
		if (this.ThirdButtonActionCalled != null)
		{
			this.ThirdButtonActionCalled(this, new EventArgs());
		}
	}
}
