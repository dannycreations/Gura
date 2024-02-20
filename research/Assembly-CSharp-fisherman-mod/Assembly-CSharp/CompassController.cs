using System;
using UnityEngine;

public class CompassController : MonoBehaviour
{
	public bool IsInited { get; private set; }

	public void Init(Transform viewTransform, ICompassView compassView, bool isWind = true)
	{
		this.viewTransform = viewTransform;
		this.compassView = compassView;
		this.isWindDirection = isWind;
		this.IsInited = true;
	}

	public void ChangeShift(float yRotationAngle)
	{
		this.compas_shift_degree = -yRotationAngle;
	}

	private void Update()
	{
		if (this.IsInited)
		{
			this.compassView.SetNorthDegree(this.viewTransform.eulerAngles.y + 180f + this.compas_shift_degree + this.additional_shift + 3f + ((!this.isWindDirection) ? 0f : CompassController.GetWindRotation()));
		}
	}

	public static float GetWindRotation()
	{
		if (TimeAndWeatherManager.CurrentWeather == null || string.IsNullOrEmpty(TimeAndWeatherManager.CurrentWeather.WindDirection))
		{
			return 0f;
		}
		string windDirection = TimeAndWeatherManager.CurrentWeather.WindDirection;
		switch (windDirection)
		{
		case "N":
			return 180f;
		case "S":
			return 0f;
		case "W":
			return -90f;
		case "E":
			return 90f;
		case "NE":
			return 135f;
		case "NW":
			return -135f;
		case "SE":
			return 45f;
		case "SW":
			return -45f;
		}
		return 0f;
	}

	public const float default_compass_shift_degree = 3f;

	public float additional_shift;

	private Transform viewTransform;

	private float compas_shift_degree;

	private bool isWindDirection;

	private ICompassView compassView;
}
