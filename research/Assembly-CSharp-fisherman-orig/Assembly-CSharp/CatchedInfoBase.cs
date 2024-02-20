using System;
using System.Diagnostics;
using InControl;
using UnityEngine;

public class CatchedInfoBase : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<bool, CatchedInfoTypes> ActivatePhotoMode = delegate
	{
	};

	protected virtual void OnEnable()
	{
		InputModuleManager.OnInputTypeChanged += this.OnInputTypeChanged;
		this.UpdateHelp();
	}

	protected virtual void OnDisable()
	{
		InputModuleManager.OnInputTypeChanged -= this.OnInputTypeChanged;
		base.StopAllCoroutines();
	}

	protected virtual void Update()
	{
		if (!this._isInPhotoMode && ControlsController.ControlsActions.ForceMouselook.IsPressed)
		{
			this._isInPhotoMode = true;
			this.ActivatePhotoMode(this._isInPhotoMode, this._catchedInfoType);
		}
		if (this._isInPhotoMode && !ControlsController.ControlsActions.ForceMouselook.IsPressed)
		{
			this._isInPhotoMode = false;
			this.ActivatePhotoMode(this._isInPhotoMode, this._catchedInfoType);
		}
	}

	protected virtual void OnInputTypeChanged(InputModuleManager.InputType type)
	{
		this.UpdateHelp();
	}

	protected virtual void UpdateHelp()
	{
	}

	protected virtual string GetKeyMapping(InputControlType ict)
	{
		return this.GetColored(HotkeyIcons.KeyMappings[ict]);
	}

	protected virtual string GetColored(string s)
	{
		return string.Format("<color=#FFDD77FF><size=24>{0}</size></color>", s);
	}

	public Action<bool> EClose = delegate
	{
	};

	protected bool _isInPhotoMode;

	protected CatchedInfoTypes _catchedInfoType;
}
