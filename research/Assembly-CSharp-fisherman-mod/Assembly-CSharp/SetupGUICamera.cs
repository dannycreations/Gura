using System;
using System.Collections.Generic;
using Assets.Scripts.Common.Managers.Helpers;
using UnityEngine;

public class SetupGUICamera : MonoBehaviour
{
	private void Awake()
	{
		this.Setup();
	}

	public void Setup()
	{
		this._canvas = base.GetComponent<Canvas>();
		if (this._canvas != null && this._canvas.worldCamera == null)
		{
			Camera guicamera = MenuHelpers.Instance.GUICamera;
			if (guicamera != null)
			{
				this._canvas.worldCamera = guicamera;
				this._canvas.planeDistance = 99f;
			}
		}
		if (this._canvas != null && this._canvas.renderMode == null && base.gameObject.layer != LayerMask.NameToLayer("Customization"))
		{
			this._canvasGroup = this._canvas.GetComponent<CanvasGroup>();
			if (this._canvasGroup == null)
			{
				this._canvasGroup = base.gameObject.AddComponent(typeof(CanvasGroup)) as CanvasGroup;
			}
			if (this._canvasGroup != null && !SetupGUICamera.ActiveCanvasGroups.Contains(this._canvasGroup))
			{
				SetupGUICamera.ActiveCanvasGroups.Add(this._canvasGroup);
			}
		}
	}

	private void OnDestroy()
	{
		if (this._canvasGroup != null && SetupGUICamera.ActiveCanvasGroups.Contains(this._canvasGroup))
		{
			SetupGUICamera.ActiveCanvasGroups.Remove(this._canvasGroup);
		}
	}

	public static List<CanvasGroup> ActiveCanvasGroups = new List<CanvasGroup>();

	private Canvas _canvas;

	private CanvasGroup _canvasGroup;
}
