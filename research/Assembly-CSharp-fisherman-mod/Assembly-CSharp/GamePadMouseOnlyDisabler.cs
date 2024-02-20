using System;
using UnityEngine;

public class GamePadMouseOnlyDisabler : MonoBehaviour
{
	private void Awake()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		this.OnInputTypeChanged(InputModuleManager.GameInputType);
	}

	private void Start()
	{
		ActivityState parentActivityState = ActivityState.GetParentActivityState(base.transform);
		if (parentActivityState != null)
		{
			this._cg = parentActivityState.CanvasGroup;
		}
	}

	private void OnDestroy()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
	}

	private void OnEnable()
	{
		this.OnInputTypeChanged(InputModuleManager.GameInputType);
	}

	private void FixedUpdate()
	{
		if (this.shouldCall && (this._cg == null || this._cg.interactable))
		{
			this.shouldCall = false;
			this.OnInputTypeChanged(this.lastType);
		}
	}

	public void SetShow(int isShow)
	{
		this._isShow = isShow == 1;
		this.OnInputTypeChanged(SettingsManager.InputType);
	}

	private void OnInputTypeChanged(InputModuleManager.InputType inputType)
	{
		if (this._cg != null && !this._cg.interactable && base.gameObject.activeSelf)
		{
			this.shouldCall = true;
			this.lastType = inputType;
			return;
		}
		if (((inputType == InputModuleManager.InputType.Mouse && this._mouseOnly) || (inputType == InputModuleManager.InputType.GamePad && this._gamePadOnly) || this._pcOnly) && !this._consoleOnly)
		{
			base.gameObject.SetActive(true);
		}
		else
		{
			base.gameObject.SetActive(false);
		}
		if (!this._isShow && base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(false);
		}
	}

	[SerializeField]
	private bool _gamePadOnly;

	[SerializeField]
	private bool _mouseOnly;

	[SerializeField]
	private bool _consoleOnly;

	[SerializeField]
	private bool _pcOnly;

	private CanvasGroup _cg;

	private bool _isShow = true;

	private bool shouldCall;

	private InputModuleManager.InputType lastType;
}
