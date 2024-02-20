using System;
using System.Diagnostics;
using UnityEngine;

public class CompassMarkerBase : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool> OnActivated = delegate
	{
	};

	public bool IsActive
	{
		get
		{
			return this._alphaFade.IsShowing || this._alphaFade.IsShow;
		}
	}

	public virtual void SetActive(bool flag)
	{
		if (flag)
		{
			this._alphaFade.ShowFinished += this._alphaFade_ShowFinished;
			this._alphaFade.ShowPanel();
		}
		else
		{
			this._alphaFade.HideFinished += this._alphaFade_HideFinished;
			this._alphaFade.HidePanel();
		}
	}

	protected virtual void _alphaFade_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this._alphaFade.HideFinished -= this._alphaFade_HideFinished;
		this.OnActivated(false);
	}

	protected virtual void _alphaFade_ShowFinished(object sender, EventArgs e)
	{
		this._alphaFade.ShowFinished -= this._alphaFade_ShowFinished;
		this.OnActivated(true);
	}

	[SerializeField]
	protected AlphaFade _alphaFade;
}
