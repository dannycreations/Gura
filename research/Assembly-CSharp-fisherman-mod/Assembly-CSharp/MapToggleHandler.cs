using System;
using UnityEngine;
using UnityEngine.UI;

public class MapToggleHandler : MonoBehaviour
{
	public void Change(bool flag)
	{
		if (this._active.IsShowing || this._active.IsHiding || this._notActive.IsShowing || this._notActive.IsHiding)
		{
			return;
		}
		if (flag)
		{
			this._notActive.HideFinished += this._notActive_HideFinished;
			this._notActive.HidePanel();
		}
		else
		{
			this._active.HideFinished += this._active_HideFinished;
			this._active.HidePanel();
		}
	}

	private void _notActive_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this._notActive.HideFinished -= this._notActive_HideFinished;
		this._imColorChange.color = this._activeColor;
		this._active.ShowPanel();
	}

	private void _active_HideFinished(object sender, EventArgsAlphaFade e)
	{
		this._active.HideFinished -= this._active_HideFinished;
		this._imColorChange.color = this._notActiveColor;
		this._notActive.ShowPanel();
	}

	[SerializeField]
	private AlphaFade _active;

	[SerializeField]
	private AlphaFade _notActive;

	[SerializeField]
	private Color _activeColor;

	[SerializeField]
	private Color _notActiveColor;

	[SerializeField]
	private Image _imColorChange;
}
