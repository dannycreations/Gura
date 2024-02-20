using System;
using UnityEngine;

public class RodPodSwivel : MonoBehaviour
{
	public float DistFromRootToBackTip
	{
		get
		{
			return this._distFromRootToBackTip;
		}
	}

	public bool IsContactFound
	{
		get
		{
			return this._contactPoint != null;
		}
	}

	private void Awake()
	{
		this._initialRodMovement = this._slot.localPosition;
		this._angles = new float[]
		{
			(this._minAngle + this._maxAngle) * 0.5f,
			this._maxAngle,
			this._minAngle
		};
		this._distFromRootToBackTip = this._rodTipDistance + this._slot.localPosition.z;
	}

	public void UpdateContactPoint()
	{
		for (int i = 0; i < this._angles.Length; i++)
		{
			base.transform.localEulerAngles = new Vector3(-this._angles[i], 0f, 0f);
			this._contactPoint = Math3d.GetMaskedRayContactPoint(base.transform.position, base.transform.position - this._slot.forward * (this._rodTipDistance + this._rodMaxBackMovement), GlobalConsts.GroundObstacleMask);
			if (this._contactPoint != null)
			{
				float magnitude = (base.transform.position - this._contactPoint.Value).magnitude;
				float num = this._rodTipDistance - magnitude;
				if (magnitude >= this._rodTipDistance || num <= this._rodMaxMovement)
				{
					this._slot.localPosition = this._initialRodMovement + num * Vector3.forward;
					return;
				}
			}
		}
		this._contactPoint = null;
	}

	private void OnDrawGizmos()
	{
		Vector3 position = base.transform.position;
		Vector3 vector = base.transform.position - this._slot.forward * (this._rodTipDistance + this._rodMaxBackMovement);
		Gizmos.color = Color.green;
		Gizmos.DrawSphere(position, 0.02f);
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(vector, 0.02f);
		Vector3? maskedRayContactPoint = Math3d.GetMaskedRayContactPoint(position, vector, GlobalConsts.GroundObstacleMask);
		if (maskedRayContactPoint != null)
		{
			float magnitude = (maskedRayContactPoint.Value - position).magnitude;
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(maskedRayContactPoint.Value, 0.025f);
		}
	}

	[SerializeField]
	private float _minAngle = 10f;

	[SerializeField]
	private float _maxAngle = 45f;

	[SerializeField]
	private float _rodTipDistance = 1.56f;

	[SerializeField]
	private float _rodMaxMovement = 0.5f;

	[SerializeField]
	private float _rodMaxBackMovement = 0.5f;

	[SerializeField]
	private Transform _slot;

	private Vector3 _initialRodMovement;

	private float[] _angles;

	private Vector3? _contactPoint;

	private float _distFromRootToBackTip;
}
