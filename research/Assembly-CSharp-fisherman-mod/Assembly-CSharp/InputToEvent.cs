﻿using System;
using UnityEngine;

public class InputToEvent : MonoBehaviour
{
	public static GameObject goPointedAt { get; private set; }

	public Vector2 DragVector
	{
		get
		{
			return (!this.Dragging) ? Vector2.zero : (this.currentPos - this.pressedPosition);
		}
	}

	private void Start()
	{
		this.m_Camera = base.GetComponent<Camera>();
	}

	private void Update()
	{
		if (this.DetectPointedAtGameObject)
		{
			InputToEvent.goPointedAt = this.RaycastObject(Input.mousePosition);
		}
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);
			this.currentPos = touch.position;
			if (touch.phase == null)
			{
				this.Press(touch.position);
			}
			else if (touch.phase == 3)
			{
				this.Release(touch.position);
			}
			return;
		}
		this.currentPos = Input.mousePosition;
		if (Input.GetMouseButtonDown(0))
		{
			this.Press(Input.mousePosition);
		}
		if (Input.GetMouseButtonUp(0))
		{
			this.Release(Input.mousePosition);
		}
		if (Input.GetMouseButtonDown(1))
		{
			this.pressedPosition = Input.mousePosition;
			this.lastGo = this.RaycastObject(this.pressedPosition);
			if (this.lastGo != null)
			{
				this.lastGo.SendMessage("OnPressRight", 1);
			}
		}
	}

	private void Press(Vector2 screenPos)
	{
		this.pressedPosition = screenPos;
		this.Dragging = true;
		this.lastGo = this.RaycastObject(screenPos);
		if (this.lastGo != null)
		{
			this.lastGo.SendMessage("OnPress", 1);
		}
	}

	private void Release(Vector2 screenPos)
	{
		if (this.lastGo != null)
		{
			GameObject gameObject = this.RaycastObject(screenPos);
			if (gameObject == this.lastGo)
			{
				this.lastGo.SendMessage("OnClick", 1);
			}
			this.lastGo.SendMessage("OnRelease", 1);
			this.lastGo = null;
		}
		this.pressedPosition = Vector2.zero;
		this.Dragging = false;
	}

	private GameObject RaycastObject(Vector2 screenPos)
	{
		RaycastHit raycastHit;
		if (Physics.Raycast(this.m_Camera.ScreenPointToRay(screenPos), ref raycastHit, 200f))
		{
			InputToEvent.inputHitPos = raycastHit.point;
			return raycastHit.collider.gameObject;
		}
		return null;
	}

	private GameObject lastGo;

	public static Vector3 inputHitPos;

	public bool DetectPointedAtGameObject;

	private Vector2 pressedPosition = Vector2.zero;

	private Vector2 currentPos = Vector2.zero;

	public bool Dragging;

	private Camera m_Camera;
}
