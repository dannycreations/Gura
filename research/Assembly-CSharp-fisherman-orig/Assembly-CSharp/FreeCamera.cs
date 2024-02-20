using System;
using UnityEngine;

public class FreeCamera : MonoBehaviour
{
	private void Awake()
	{
		base.enabled = this.enableInputCapture;
	}

	private void OnValidate()
	{
		if (Application.isPlaying)
		{
			base.enabled = this.enableInputCapture;
		}
	}

	private void CaptureInput()
	{
		Cursor.lockState = 1;
		Cursor.visible = false;
		this.m_inputCaptured = true;
		this.m_yaw = base.transform.eulerAngles.y;
		this.m_pitch = base.transform.eulerAngles.x;
	}

	private void ReleaseInput()
	{
		Cursor.lockState = 0;
		Cursor.visible = true;
		this.m_inputCaptured = false;
	}

	private void OnApplicationFocus(bool focus)
	{
		if (this.m_inputCaptured && !focus)
		{
			this.ReleaseInput();
		}
	}

	private void Update()
	{
		if (!this.m_inputCaptured)
		{
			if (!this.holdRightMouseCapture && Input.GetMouseButtonDown(0))
			{
				this.CaptureInput();
			}
			else if (this.holdRightMouseCapture && Input.GetMouseButtonDown(1))
			{
				this.CaptureInput();
			}
		}
		if (!this.m_inputCaptured)
		{
			return;
		}
		if (this.m_inputCaptured)
		{
			if (!this.holdRightMouseCapture && Input.GetKeyDown(27))
			{
				this.ReleaseInput();
			}
			else if (this.holdRightMouseCapture && Input.GetMouseButtonUp(1))
			{
				this.ReleaseInput();
			}
		}
		float axis = Input.GetAxis("Mouse X");
		float axis2 = Input.GetAxis("Mouse Y");
		this.m_yaw = (this.m_yaw + this.lookSpeed * axis) % 360f;
		this.m_pitch = (this.m_pitch - this.lookSpeed * axis2) % 360f;
		base.transform.rotation = Quaternion.AngleAxis(this.m_yaw, Vector3.up) * Quaternion.AngleAxis(this.m_pitch, Vector3.right);
		float num = Time.deltaTime * ((!Input.GetKey(304)) ? this.moveSpeed : this.sprintSpeed);
		float num2 = num * Input.GetAxis("Vertical");
		float num3 = num * Input.GetAxis("Horizontal");
		float num4 = num * (((!Input.GetKey(101)) ? 0f : 1f) - ((!Input.GetKey(113)) ? 0f : 1f));
		base.transform.position += base.transform.forward * num2 + base.transform.right * num3 + Vector3.up * num4;
	}

	public bool enableInputCapture = true;

	public bool holdRightMouseCapture;

	public float lookSpeed = 5f;

	public float moveSpeed = 5f;

	public float sprintSpeed = 50f;

	private bool m_inputCaptured;

	private float m_yaw;

	private float m_pitch;
}
