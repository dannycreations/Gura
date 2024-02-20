using System;
using UnityEngine;

public class BuoyBounceAnimation : MonoBehaviour
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
		if (GameFactory.Player != null && GameFactory.Player.CurrentBoat != null)
		{
			Vector3 vector = GameFactory.Player.CurrentBoat.Transform.InverseTransformPoint(base.transform.position);
			float num = GameFactory.Player.CurrentBoat.Width * 0.5f;
			float num2 = GameFactory.Player.CurrentBoat.Length * 0.5f;
			if (vector.x >= -num && vector.x <= num && vector.z >= -num2 && vector.z <= num2)
			{
				this.dive = Mathf.Lerp(1f, this.dive, Mathf.Exp(-Time.fixedDeltaTime * 10f));
			}
			else
			{
				this.dive = Mathf.Lerp(0f, this.dive, Mathf.Exp(-Time.fixedDeltaTime * 10f));
			}
		}
		float num3 = Mathf.Sin(Time.fixedTime * 3.1415927f * this.frequency + 6.2831855f * this.phaseAngle);
		float num4 = Mathf.Sin(Time.fixedTime * 3.1415927f * this.frequency * 0.5f + 1.5707964f + 3.1415927f * this.phaseAngle);
		this.rollingX = Mathf.Sin(this.rollingDirection * 3.1415927f) * this.rollingAmplitude;
		this.rollingZ = Mathf.Cos(this.rollingDirection * 3.1415927f) * this.rollingAmplitude;
		this.nowPosition.y = this.startPosition.y + num3 * this.amplitude - this.dive;
		base.transform.position = this.nowPosition;
		this.nowRotation.x = this.startRotation.x + num4 * this.rollingX;
		this.nowRotation.z = this.startRotation.z + num4 * this.rollingZ;
		base.transform.rotation = this.nowRotation;
	}

	private float amplitude = 0.001f;

	private float rollingAmplitude = 0.025f;

	private float frequency = 1f;

	[Range(0f, 1f)]
	public float phaseAngle;

	[Range(0f, 1f)]
	public float rollingDirection;

	private Vector3 startPosition = default(Vector3);

	private Vector3 nowPosition = default(Vector3);

	private Quaternion startRotation = default(Quaternion);

	private Quaternion nowRotation = default(Quaternion);

	private float rollingX;

	private float rollingZ;

	private float dive;
}
