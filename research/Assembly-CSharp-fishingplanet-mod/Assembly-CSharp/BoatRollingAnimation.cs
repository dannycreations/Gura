using System;
using UnityEngine;

public class BoatRollingAnimation : MonoBehaviour
{
	private void Start()
	{
		this.startPosition = base.transform.position;
		this.startRotation = base.transform.rotation;
		this.nowPosition = this.startPosition;
		this.nowRotation = this.startRotation;
	}

	private void FixedUpdate()
	{
		float num = Mathf.Sin(Time.fixedTime * 3.1415927f * this.heaveFrequency + 6.2831855f * this.phaseAngle);
		float num2 = Mathf.Sin(Time.fixedTime * 3.1415927f * this.pitchFrequency + 6.2831855f * this.phaseAngle);
		float num3 = Mathf.Sin(Time.fixedTime * 3.1415927f * this.rollFrequency + 6.2831855f * this.phaseAngle);
		this.nowPosition.y = this.startPosition.y + num * this.heaveAmplitude;
		base.transform.position = this.nowPosition;
		this.nowRotation.z = this.startRotation.z + num2 * this.rollAmplitude;
		this.nowRotation.x = this.startRotation.x + num3 * this.pitchAmplitude;
		base.transform.rotation = this.nowRotation;
	}

	private float heaveAmplitude = 0.01f;

	private float heaveFrequency = 1f;

	private float rollAmplitude = 0.001f;

	private float rollFrequency = 0.5f;

	private float pitchAmplitude = 0.001f;

	private float pitchFrequency = 0.25f;

	[Range(0f, 1f)]
	public float phaseAngle;

	private Vector3 startPosition = default(Vector3);

	private Vector3 nowPosition = default(Vector3);

	private Quaternion startRotation = default(Quaternion);

	private Quaternion nowRotation = default(Quaternion);
}
