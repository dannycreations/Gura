using System;
using System.Collections;
using InControl;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputFieldActivateWithButton : MonoBehaviour
{
	private void Start()
	{
		this._cg = ActivityState.GetParentActivityState(base.transform).CanvasGroup;
	}

	public void DeactivateAndWaitForPress()
	{
		this.inputField = this.inputField ?? base.GetComponent<InputField>();
		if (this.inputField != null)
		{
			base.StartCoroutine(this.DisableInNextFrame());
			base.StartCoroutine(this.WaitForPress());
		}
	}

	public void StopWaitingForPress()
	{
		base.StopAllCoroutines();
	}

	private IEnumerator WaitForPress()
	{
		yield return null;
		for (;;)
		{
			if (this._cg != null && !this._cg.interactable)
			{
				yield return null;
			}
			if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad && InputManager.ActiveDevice.GetControl(InputControlType.Action1).WasPressed)
			{
				if (this.inputField.isFocused)
				{
					yield return null;
					this.inputField.DeactivateInputField();
				}
				else
				{
					this.inputField.ActivateInputField();
					this.eventOnActivate.Invoke();
				}
			}
			yield return new WaitForEndOfFrame();
		}
		yield break;
	}

	private IEnumerator DisableInNextFrame()
	{
		yield return null;
		this.inputField.DeactivateInputField();
		yield break;
	}

	public UnityEvent eventOnActivate;

	private InputField inputField;

	private CanvasGroup _cg;
}
