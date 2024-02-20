using System;
using UnityEngine;

public class RailFallingLureController : MonoBehaviour
{
	private void Start()
	{
		this.basePosition = base.transform.localPosition;
		this.baseRotation = base.transform.localRotation;
		this.hook1BaseRotation = this.Hook1.localRotation;
		this.hook2BaseRotation = this.Hook2.localRotation;
		this.line = base.GetComponent<LineRenderer>();
		this.doRestart();
	}

	private void doRestart()
	{
		base.transform.localPosition = this.basePosition;
		base.transform.localRotation = this.baseRotation;
		this.velocity = this.StartVelocity;
		this.angularVelocity = this.StartAngularVelocity;
	}

	private void Update()
	{
		if (this.Restart)
		{
			this.doRestart();
		}
		Vector3 vector = -this.velocity * this.LinearDrag - this.velocity * this.velocity.magnitude * this.QuadraticDrag;
		this.velocity += vector * Time.deltaTime;
		base.transform.position += this.velocity * Time.deltaTime + vector * Time.deltaTime * Time.deltaTime * 0.5f;
		float num = Mathf.Pow(this.velocity.magnitude / this.StartVelocity.magnitude, 0.8f);
		this.angularVelocity = this.StartAngularVelocity * num;
		base.transform.rotation = Quaternion.Euler(this.angularVelocity * Time.deltaTime) * base.transform.rotation;
		this.Hook1.transform.localRotation = Quaternion.AngleAxis(Mathf.Sin(Time.time * 10f) * 100f * num, Vector3.right) * this.hook1BaseRotation;
		this.Hook2.transform.localRotation = Quaternion.AngleAxis(Mathf.Sin(Time.time * 7f) * 100f * num, Vector3.forward) * this.hook2BaseRotation;
		this.line.SetPosition(0, this.LureLineAnchor.position);
		this.line.SetPosition(1, this.ExternalLineAnchor.position);
	}

	public Vector3 StartVelocity;

	public Vector3 StartAngularVelocity;

	public float LinearDrag;

	public float QuadraticDrag;

	public Transform Hook1;

	public Transform Hook2;

	public Transform LureLineAnchor;

	public Transform ExternalLineAnchor;

	public bool Restart;

	private Vector3 basePosition;

	private Quaternion baseRotation;

	private Vector3 velocity;

	private Vector3 angularVelocity;

	private Quaternion hook1BaseRotation;

	private Quaternion hook2BaseRotation;

	private LineRenderer line;
}
