using System;
using UnityEngine;

public class FishRotateController : MonoBehaviour
{
	internal void Start()
	{
		this.Initialize();
	}

	private void Initialize()
	{
	}

	public void OnUpdate()
	{
		this.yawAxisValue.update(Time.deltaTime);
		this.pitchAxisValue.update(Time.deltaTime);
		this.enabledValue.update(Time.deltaTime);
	}

	public void SetTargetRotation(Quaternion rotation)
	{
		Vector3 eulerAngles = rotation.eulerAngles;
		if (eulerAngles.x > 180f)
		{
			eulerAngles.x -= 360f;
		}
		if (eulerAngles.y > 180f)
		{
			eulerAngles.y -= 360f;
		}
		float num = ((eulerAngles.x <= 0f) ? ((eulerAngles.x >= 0f) ? 0f : (-1f)) : 1f);
		float value = this.pitchAxisValue.value;
		float num2 = -Mathf.Sign(value) * this.pitchAxisValue.changeSpeedUp / 2f;
		float num3 = value;
		float num4 = eulerAngles.x / 90f;
		if (num != 0f && value * num > 0f && Mathf.Abs(value / this.pitchAxisValue.changeSpeedUp) > this.getTimeToZero(num2, num3, num4))
		{
			num = 0f;
		}
		float num5 = ((eulerAngles.y <= 0f) ? ((eulerAngles.y >= 0f) ? 0f : (-1f)) : 1f);
		float value2 = this.yawAxisValue.value;
		float num6 = -Mathf.Sign(value2) * this.yawAxisValue.changeSpeedUp / 2f;
		float num7 = value2;
		float num8 = eulerAngles.y / 90f;
		if (num5 != 0f && value2 * num5 > 0f && Mathf.Abs(value2 / this.yawAxisValue.changeSpeedUp) > this.getTimeToZero(num6, num7, num8))
		{
			num5 = 0f;
		}
		this.yawAxisValue.target = num5;
		this.pitchAxisValue.target = num;
	}

	private float getTimeToZero(float a, float b, float c)
	{
		float num = b * b - 4f * a * c;
		if (num < 0f)
		{
			return 1E+10f;
		}
		float num2 = -b - Mathf.Sqrt(num) / 2f / a;
		float num3 = -b + Mathf.Sqrt(num) / 2f / a;
		return Mathf.Max(num2, num3);
	}

	public void SetEnabled(bool value)
	{
		this.enabledValue.target = ((!value) ? 0f : 1f);
	}

	public Quaternion GetRotate(float dt, float mul)
	{
		float num = mul * this.pitchAxisValue.value * this.enabledValue.value * dt * 180f;
		float num2 = mul * this.yawAxisValue.value * this.enabledValue.value * dt * 180f;
		return Quaternion.Euler(num, num2, 0f);
	}

	public const float maxAngle = 90f;

	public const float maxAngleSpeed = 180f;

	private const float RotateSpeedChange = 2f;

	private ValueChanger yawAxisValue = new ValueChanger(0f, 0f, 2f, null);

	private ValueChanger pitchAxisValue = new ValueChanger(0f, 0f, 2f, null);

	private ValueChanger enabledValue = new ValueChanger(1f, 1f, 2f, null);
}
