using System;
using UnityEngine;

public class RodPodLeg : MonoBehaviour
{
	public bool IsContactFound
	{
		get
		{
			return this._contactPoint != null;
		}
	}

	private void Awake()
	{
		this._initialPos = this._mesh.localPosition;
	}

	public void UpdateContactPoint()
	{
		Vector3 vector = this._mesh.parent.TransformPoint(this._initialPos);
		Vector3 vector2 = vector - this._mesh.up * this._castStartMovement;
		Vector3 vector3 = vector - this._mesh.up * (this._maxDistance + this._halfLength);
		RaycastHit maskedRayHit = Math3d.GetMaskedRayHit(vector2, vector3, GlobalConsts.GroundObstacleMask);
		if (maskedRayHit.transform != null)
		{
			this._contactPoint = new Vector3?(maskedRayHit.point);
			float num = (this._contactPoint.Value - vector2).magnitude + this._castStartMovement;
			float num2 = num - this._halfLength;
			if (num2 < 0f)
			{
				num2 = Math.Max(num2, -this._maxBackDistance);
			}
			this._mesh.position = vector - this._mesh.up * (num2 + ((maskedRayHit.transform.gameObject.layer != GlobalConsts.TerrainLayer) ? 0f : this._pierceDistance));
		}
		else
		{
			this._contactPoint = null;
			this._mesh.position = vector - this._mesh.up * this._maxDistance;
		}
	}

	private void OnDrawGizmos()
	{
		Vector3 vector = this._mesh.parent.TransformPoint(this._initialPos);
		Vector3 vector2 = vector - this._mesh.up * this._castStartMovement;
		Vector3 vector3 = vector - this._mesh.up * (this._maxDistance + this._halfLength);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(vector2, 0.02f);
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(vector3, 0.02f);
		Vector3? maskedRayContactPoint = Math3d.GetMaskedRayContactPoint(vector2, vector3, GlobalConsts.GroundObstacleMask);
		if (maskedRayContactPoint != null)
		{
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(maskedRayContactPoint.Value, 0.025f);
		}
	}

	[SerializeField]
	private Transform _mesh;

	[SerializeField]
	private float _halfLength = 0.26f;

	[SerializeField]
	private float _castStartMovement = -0.05f;

	[SerializeField]
	private float _maxBackDistance = 0.1f;

	[SerializeField]
	private float _maxDistance = 0.2f;

	[SerializeField]
	private float _pierceDistance = 0.02f;

	private Vector3 _initialPos;

	private Vector3? _contactPoint;
}
