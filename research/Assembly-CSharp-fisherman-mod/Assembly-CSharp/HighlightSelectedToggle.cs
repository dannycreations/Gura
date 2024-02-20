using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class HighlightSelectedToggle : MonoBehaviour, ISelectHandler, IDeselectHandler, IEventSystemHandler
{
	public void OnSelect(BaseEventData eventData)
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this.EnableGraphic(false);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		if (InputModuleManager.GameInputType == InputModuleManager.InputType.GamePad)
		{
			this.EnableGraphic(true);
		}
	}

	public void EnableGraphic(bool enable)
	{
		Toggle component = base.GetComponent<Toggle>();
		if (component.targetGraphic != null)
		{
			component.graphic.enabled = enable;
			component.graphic.canvasRenderer.SetAlpha((!enable || !component.isOn) ? 0f : 1f);
		}
	}
}
