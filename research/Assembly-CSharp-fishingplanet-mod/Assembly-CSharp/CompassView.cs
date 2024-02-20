using System;
using UnityEngine;
using UnityEngine.UI;

public class CompassView : MonoBehaviour, ICompassView
{
	public void SetNorthDegree(float degree)
	{
		this.compassArrow.transform.localEulerAngles = new Vector3(0f, 0f, degree);
	}

	[SerializeField]
	private Graphic compassArrow;
}
