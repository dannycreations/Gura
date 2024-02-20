using System;
using UnityEngine;

public class IKTargetMover : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKey(260))
		{
			this.ChangeYaw(-1);
		}
		else if (Input.GetKey(262))
		{
			this.ChangeYaw(1);
		}
		if (Input.GetKey(264))
		{
			this.ChangePitch(1);
		}
		else if (Input.GetKey(261))
		{
			this.ChangePitch(-1);
		}
		this.UpdatePosition();
	}

	private void ChangeYaw(int sign)
	{
		this.curYaw += (float)sign * this.yawSpeed * Time.deltaTime;
	}

	private void ChangePitch(int sign)
	{
		this.curPitch = Mathf.Clamp(this.curPitch + (float)sign * this.pitchSpeed * Time.deltaTime, this.minPitch, this.maxPitch);
	}

	private void UpdatePosition()
	{
		float num = this.r * Mathf.Sin(this.curPitch * 0.017453292f);
		float num2 = this.r * Mathf.Cos(this.curPitch * 0.017453292f);
		float num3 = num2 * Mathf.Sin(this.curYaw * 0.017453292f);
		float num4 = num2 * Mathf.Cos(this.curYaw * 0.017453292f);
		base.transform.localPosition = new Vector3(num3, num, num4);
	}

	public float r = 3f;

	public float curYaw = 90f;

	public float yawSpeed = 90f;

	public float curPitch = 45f;

	public float pitchSpeed = 45f;

	public float minPitch = 15f;

	public float maxPitch = 60f;
}
