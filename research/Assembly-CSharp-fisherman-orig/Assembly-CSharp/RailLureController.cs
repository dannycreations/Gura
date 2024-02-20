using System;
using UnityEngine;

public class RailLureController : MonoBehaviour
{
	private void Start()
	{
		this.prevPosition = base.transform.position;
		this.basePosition = base.transform.localPosition;
		this.baseRotation = base.transform.localRotation;
		this.baseHook1Rotation = this.Hook1.localRotation;
		this.baseHook2Rotation = this.Hook2.localRotation;
		this.line = base.GetComponent<LineRenderer>();
	}

	private void Update()
	{
		if (this.KillSwitch)
		{
			return;
		}
		Vector3 vector = new Vector3(this.HighAmplitude * Mathf.Sin(Time.time * this.HighFrequency) + this.LowAmplitude * 2f * (Mathf.PerlinNoise(base.transform.parent.position.x * this.LowFrequency, base.transform.parent.position.z * this.LowFrequency) - 0.5f), 0f, 0f) + this.basePosition;
		Vector3 vector2 = base.transform.parent.TransformPoint(vector);
		Vector3 vector3 = vector - base.transform.localPosition;
		base.transform.localPosition = vector;
		Debug.DrawLine(base.transform.position, base.transform.position + vector3, Color.yellow);
		this.debug_ds = vector3.x;
		this.alpha = Mathf.Lerp(Mathf.Atan2(vector3.x, Time.deltaTime), this.alpha, Mathf.Exp(-Time.deltaTime * 20f));
		base.transform.localRotation = Quaternion.AngleAxis(-this.alpha * 57.29578f, Vector3.up) * this.baseRotation;
		this.prevPosition = base.transform.position;
		this.Hook1.localRotation = Quaternion.AngleAxis(this.alpha * this.RotationMultiplier * 57.29578f, Vector3.right) * Quaternion.AngleAxis(this.alpha * 57.29578f, Vector3.up) * this.baseHook1Rotation;
		this.Hook2.localRotation = Quaternion.AngleAxis(this.alpha * this.RotationMultiplier * 57.29578f, Vector3.right) * Quaternion.AngleAxis(this.alpha * 57.29578f, Vector3.up) * this.baseHook2Rotation;
	}

	public void UpdateLine()
	{
		this.line.SetPosition(0, this.LureLineAnchor.position);
		this.line.SetPosition(1, this.ExternalLineAnchor.position);
	}

	public float HighFrequency = 15f;

	public float HighAmplitude = 0.01f;

	public float LowFrequency = 5f;

	public float LowAmplitude = 0.1f;

	public float RotationMultiplier = 5f;

	public bool KillSwitch;

	public Transform Hook1;

	public Transform Hook2;

	public Transform LureLineAnchor;

	public Transform ExternalLineAnchor;

	private Vector3 prevPosition;

	private Vector3 basePosition;

	private Quaternion baseRotation;

	private Quaternion baseHook1Rotation;

	private Quaternion baseHook2Rotation;

	public float alpha;

	public float debug_ds;

	private LineRenderer line;
}
