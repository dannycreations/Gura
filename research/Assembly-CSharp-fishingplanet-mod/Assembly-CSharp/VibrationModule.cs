using System;
using InControl;
using UnityEngine;

public class VibrationModule : MonoBehaviour
{
	private void Update()
	{
		if (VibrationModule._vibrationTime > 0f)
		{
			VibrationModule._timeVibrated += Time.deltaTime;
			if (VibrationModule._timeVibrated >= VibrationModule._vibrationTime)
			{
				VibrationModule._vibrationTime = 0f;
				VibrationModule._timeVibrated = 0f;
				VibrationModule.StopVibrate();
			}
		}
	}

	public static void StopVibrate()
	{
		VibrationModule.Vibrate(0f, 0f);
	}

	public static void Vibrate(float leftMotor, float rightMotor)
	{
		try
		{
			InputManager.ActiveDevice.Vibrate(leftMotor, rightMotor);
		}
		catch (Exception ex)
		{
		}
	}

	public static void Vibrate(float motor)
	{
		VibrationModule.Vibrate(motor, motor);
	}

	public static void VibrateTimed(float leftMotor, float rightMotor, float duration)
	{
		VibrationModule._timeVibrated = 0f;
		VibrationModule._vibrationTime = duration;
		VibrationModule.Vibrate(leftMotor, rightMotor);
	}

	public static void VibrateTimed(float motor, float duration)
	{
		VibrationModule.VibrateTimed(motor, motor, duration);
	}

	private static float _timeVibrated;

	private static float _vibrationTime;
}
