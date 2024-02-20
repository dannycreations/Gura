using System;
using UnityEngine;

public class TelescopicLeg : MonoBehaviour
{
	public float MovableYMovement
	{
		get
		{
			return this.MovablePart.localPosition.y - this._zeroY;
		}
	}

	private void Awake()
	{
		this._zeroY = this.MovablePart.localPosition.y;
		this._localPos = base.transform.localPosition;
	}

	private void Start()
	{
		if (this.LinkedPivot != null)
		{
			this.LinkedPivot.parent = this.MovablePart;
		}
	}

	public bool LandTelescopicLeg(float maxPunctureDY)
	{
		base.transform.localPosition = this._localPos;
		Vector3 vector = this.GroundTip.forward * maxPunctureDY;
		RaycastHit maskedRayHit = Math3d.GetMaskedRayHit(this.GroundTip.position - vector, this.GroundTip.position + vector, GlobalConsts.GroundObstacleMask);
		if (maskedRayHit.transform != null && maskedRayHit.transform.gameObject.layer == GlobalConsts.TerrainLayer)
		{
			base.transform.position = maskedRayHit.point + this.GroundTip.forward * this.GroundTip.localPosition.y;
			this.MovablePart.localPosition = new Vector3(0f, this._zeroY, 0f);
			if (this.WaterMark.position.y > 0f || Mathf.Abs(this.WaterMark.position.y) < this.MaxMove)
			{
				if (this.WaterMark.position.y < 0f)
				{
					this.MovablePart.localPosition += new Vector3(0f, -this.WaterMark.position.y, 0f);
				}
				return true;
			}
		}
		return false;
	}

	public Transform GroundTip;

	public Transform MovablePart;

	public Transform WaterMark;

	public Transform RodTip;

	public Transform LinkedPivot;

	public float MaxMove;

	public float PressureMaxMove = 0.3f;

	private float _zeroY;

	private Vector3 _localPos;
}
