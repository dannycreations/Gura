using System;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DpadSlider : MonoBehaviour, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	public void OnSelect(BaseEventData eventData)
	{
		this.selected = true;
	}

	public void OnDeselect(BaseEventData eventData)
	{
		this.selected = false;
	}

	private void Start()
	{
		this._cg = ActivityState.GetParentActivityState(base.transform).CanvasGroup;
	}

	private void Update()
	{
		if (this._cg != null && !this._cg.interactable)
		{
			return;
		}
		if (this.selected && InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			if (InputManager.ActiveDevice.DPadLeft.WasPressed || InputManager.ActiveDevice.DPadLeft.WasRepeated)
			{
				this.slider.value -= this.sliderChangeStep;
			}
			if (InputManager.ActiveDevice.DPadRight.WasPressed || InputManager.ActiveDevice.DPadRight.WasRepeated)
			{
				this.slider.value += this.sliderChangeStep;
			}
		}
	}

	[SerializeField]
	private Slider slider;

	private bool selected;

	private float sliderChangeStep = 0.1f;

	private CanvasGroup _cg;
}
