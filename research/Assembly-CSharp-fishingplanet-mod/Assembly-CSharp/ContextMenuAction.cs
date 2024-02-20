using System;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

public class ContextMenuAction : MonoBehaviour
{
	private void Update()
	{
		if (ControlsController.ControlsActions.GetMouseButton(0) && !RectTransformUtility.RectangleContainsScreenPoint(base.GetComponent<RectTransform>(), Input.mousePosition, MenuHelpers.Instance.GUICamera) && this.isActive)
		{
			this.Hide();
		}
		if (this._isClosed && Mathf.FloorToInt(base.GetComponent<RectTransform>().sizeDelta.y) == 0)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Show()
	{
		if (this.isActive)
		{
			return;
		}
		this.isActive = true;
	}

	public void Hide()
	{
		if (!this.isActive)
		{
			return;
		}
		base.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 0f);
		this.isActive = false;
		this._isClosed = true;
	}

	internal void Show(Vector3 position)
	{
		if (this.isActive)
		{
			return;
		}
		this.Show();
	}

	[HideInInspector]
	internal bool isActive;

	private bool _isClosed;
}
