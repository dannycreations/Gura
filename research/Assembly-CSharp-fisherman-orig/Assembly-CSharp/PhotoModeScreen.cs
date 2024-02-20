using System;
using UnityEngine;

[RequireComponent(typeof(AlphaFade))]
public abstract class PhotoModeScreen : MonoBehaviour
{
	public bool IsShow
	{
		get
		{
			return this._alphaFade.IsShow;
		}
	}

	public bool IsShowing
	{
		get
		{
			return this._alphaFade.IsShowing;
		}
	}

	public bool IsHiding
	{
		get
		{
			return this._alphaFade.IsHiding;
		}
	}

	protected virtual void Awake()
	{
		this._alphaFade = base.GetComponent<AlphaFade>();
	}

	public void Show()
	{
		this._alphaFade.ShowPanel();
		this.OnShow();
	}

	public void Hide()
	{
		this._alphaFade.HidePanel();
		this.OnHide();
	}

	public void FastHide()
	{
		this._alphaFade.FastHidePanel();
		this.OnHide();
	}

	protected virtual void OnShow()
	{
		for (int i = 0; i < this._actions.Length; i++)
		{
			this._actions[i].Activate();
		}
		for (int j = 0; j < this._axisActions.Length; j++)
		{
			this._axisActions[j].Activate();
		}
	}

	protected virtual void OnHide()
	{
		for (int i = 0; i < this._actions.Length; i++)
		{
			this._actions[i].Deactivate();
		}
		for (int j = 0; j < this._axisActions.Length; j++)
		{
			this._axisActions[j].Deactivate();
		}
	}

	protected AlphaFade _alphaFade;

	protected CustomPlayerAction[] _actions;

	protected CustomPlayerTwoAxisAction[] _axisActions;
}
