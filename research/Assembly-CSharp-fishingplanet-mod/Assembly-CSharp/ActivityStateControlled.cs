using System;
using UnityEngine;

public class ActivityStateControlled : MonoBehaviour
{
	public void OnActivityStateShown()
	{
		if (base.gameObject.activeInHierarchy && base.enabled && !this.helpInited)
		{
			this.InitHelp(true);
		}
	}

	public void HideEvent()
	{
		if (this.helpInited)
		{
			this.InitHelp(false);
		}
	}

	public void OverrideCanvasGroup(CanvasGroup cg)
	{
		this._cg = cg;
		this._cgOverriden = true;
	}

	protected bool ShouldUpdate()
	{
		return base.gameObject.activeInHierarchy && (this._cg == null || this._cg.interactable);
	}

	protected virtual void OnDisable()
	{
		this.InitHelp(false);
	}

	protected virtual void OnEnable()
	{
		if (!this.activityStateChecked)
		{
			this.CheckActivityState();
		}
		this.InitHelp(this.ShouldUpdate());
	}

	protected virtual void Start()
	{
		if (!this.activityStateChecked || this._activityState == null)
		{
			this.CheckActivityState();
		}
	}

	protected void CheckActivityState()
	{
		this._activityState = ActivityState.GetParentActivityState(base.transform);
		if (this._activityState != null)
		{
			if (!this._cgOverriden)
			{
				this._cg = this._activityState.CanvasGroup;
			}
			this._activityState.OnShow += this.OnActivityStateShown;
			this._activityState.HideEvent += this.HideEvent;
		}
		this.activityStateChecked = true;
	}

	protected void InitHelp(bool init)
	{
		this.helpInited = init;
		if (init)
		{
			this.SetHelp();
		}
		else
		{
			this.HideHelp();
		}
	}

	protected virtual void SetHelp()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChangedAs;
	}

	protected virtual void HideHelp()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChangedAs;
	}

	protected virtual void OnDestroy()
	{
		if (this._activityState != null)
		{
			this._activityState.OnShow -= this.OnActivityStateShown;
			this._activityState.HideEvent -= this.HideEvent;
		}
	}

	protected virtual void OnInputTypeChangedAs(InputModuleManager.InputType inputType)
	{
	}

	protected virtual void OnUserChanged()
	{
	}

	protected CanvasGroup _cg;

	protected ActivityState _activityState;

	protected bool helpInited;

	protected bool activityStateChecked;

	protected bool _cgOverriden;
}
