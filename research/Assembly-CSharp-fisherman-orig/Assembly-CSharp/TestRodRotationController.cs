using System;
using UnityEngine;

public class TestRodRotationController : MonoBehaviour
{
	public void Update()
	{
		this.ApplyMoveAndRotateToRod(this.root.position, this.root.rotation);
	}

	public void ApplyMoveAndRotateToRod(Vector3 rootPosition, Quaternion rootRotation)
	{
		Vector3? vector = this.priorRootPosition;
		if (vector != null)
		{
			Quaternion? quaternion = this.priorRootRotation;
			if (quaternion != null)
			{
				Vector3? vector2 = this.priorRootPosition;
				Vector3? vector3 = ((vector2 == null) ? null : new Vector3?(rootPosition - vector2.GetValueOrDefault()));
				Quaternion quaternion2 = rootRotation * Quaternion.Inverse(this.priorRootRotation.Value);
				this.CompensateRodRootMoveAndRotation(this.priorRootPosition.Value, vector3.Value, quaternion2);
			}
		}
		this.priorRootPosition = new Vector3?(rootPosition);
		this.priorRootRotation = new Quaternion?(rootRotation);
	}

	private void CompensateRodRootMoveAndRotation(Vector3 rootPosition, Vector3 rootPositionDelta, Quaternion rootRotationDelta)
	{
		Vector3 vector = Vector3.zero;
		if (rootPositionDelta != Vector3.zero || rootRotationDelta != Quaternion.identity)
		{
			for (int i = 0; i < this.nodes.Length; i++)
			{
				Transform transform = this.nodes[i];
				Vector3 vector2 = transform.position - rootPosition;
				Vector3 vector3 = rootRotationDelta * vector2;
				vector = rootPositionDelta + Vector3.Lerp(Vector3.zero, vector3 - vector2, this.RodRotationCompensation);
				transform.position += vector;
				transform.rotation *= rootRotationDelta;
			}
		}
	}

	public Transform root;

	public Transform node0;

	public Transform[] nodes;

	public float RodRotationCompensation = 1f;

	private Vector3? priorRootPosition;

	private Quaternion? priorRootRotation;
}
