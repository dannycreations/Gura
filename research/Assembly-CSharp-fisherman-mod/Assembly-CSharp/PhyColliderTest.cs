using System;
using Phy;
using UnityEngine;

public class PhyColliderTest : MonoBehaviour
{
	private void Start()
	{
		this.pcollider = new BoxCollider(this.ucollider);
	}

	private void Update()
	{
		Vector3 zero = Vector3.zero;
		Vector3 vector = this.pcollider.TestPoint(this.point.position, this.prevPoint.position, out zero);
		Debug.DrawLine(this.point.position, this.prevPoint.position, Color.red);
		Debug.DrawLine(this.point.position, this.point.position + vector, Color.green);
		Debug.DrawLine(this.point.position + vector, this.point.position + vector + zero * 0.1f, Color.magenta);
	}

	public BoxCollider ucollider;

	public Transform point;

	public Transform prevPoint;

	private BoxCollider pcollider;
}
