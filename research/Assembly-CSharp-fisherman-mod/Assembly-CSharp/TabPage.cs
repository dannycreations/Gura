using System;
using System.Collections.Generic;
using System.Diagnostics;

public class TabPage : AlphaFade
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event TabPage.OnHidenDelegate OnHidden = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event TabPage.OnHidenDelegate OnShown = delegate
	{
	};

	protected override void Awake()
	{
		this.SetInteractable = true;
		base.Awake();
		base.GetComponentsInChildren<ActivityStateControlled>(true, this.children);
		foreach (ActivityStateControlled activityStateControlled in this.children)
		{
			activityStateControlled.OverrideCanvasGroup(base.CanvasGroup);
		}
		base.HideFinished += this.OnHideFinished;
		base.ShowFinished += this.OnShowFinished;
	}

	private void OnShowFinished(object sender, EventArgs eventArgs)
	{
		this.OnShown();
		foreach (ActivityStateControlled activityStateControlled in this.children)
		{
			activityStateControlled.OnActivityStateShown();
		}
	}

	private void OnHideFinished(object sender, EventArgsAlphaFade eventArgsAlphaFade)
	{
		this.OnHidden();
		foreach (ActivityStateControlled activityStateControlled in this.children)
		{
			activityStateControlled.HideEvent();
		}
	}

	public virtual void RefreshPage()
	{
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		base.HideFinished -= this.OnHideFinished;
		base.ShowFinished -= this.OnShowFinished;
	}

	private List<ActivityStateControlled> children = new List<ActivityStateControlled>();

	public delegate void OnHidenDelegate();
}
