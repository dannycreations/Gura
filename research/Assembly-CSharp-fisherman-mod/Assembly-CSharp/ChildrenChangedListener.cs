using System;
using System.Diagnostics;

public class ChildrenChangedListener : ActivityStateControlled
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnChildrenChanged;

	public void OnTransformChildrenChanged()
	{
		this.updated = true;
	}

	private void Update()
	{
		if (!base.ShouldUpdate())
		{
			return;
		}
		if (this.updated)
		{
			this.updated = false;
			if (this.OnChildrenChanged != null)
			{
				this.OnChildrenChanged();
			}
		}
	}

	private bool updated;
}
