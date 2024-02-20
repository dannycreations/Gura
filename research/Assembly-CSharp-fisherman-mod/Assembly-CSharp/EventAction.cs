using System;
using System.Diagnostics;
using UnityEngine;

public class EventAction : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventArgs> ActionCalled;

	public void DoAction()
	{
		if (this.ActionCalled != null)
		{
			this.ActionCalled(this, new EventArgs());
		}
	}
}
