using System;
using UnityEngine;

public class QuadArcAnimator : MonoBehaviour
{
	private void Start()
	{
		this._center = base.transform.position;
		this._yaw = Math3d.ClampAngleTo360(base.transform.eulerAngles.y);
		this._rotationSign = Mathf.Sign(this._toAngle - this._fromAngle);
		this._renderer = base.GetComponent<Renderer>();
		this.StartDelay();
	}

	private void StartDelay()
	{
		this._curDelay = Random.Range(this._minDelay, this._maxDelay);
		this._renderer.enabled = false;
	}

	private void Update()
	{
		if (this._curDelay > 0f)
		{
			this._curDelay -= Time.deltaTime;
			if (this._curDelay < 0f)
			{
				this._curTime = 0f;
				this._renderer.enabled = true;
			}
		}
		else
		{
			this._curTime += Time.deltaTime;
			float num = this._fromAngle + this._curTime * this._w * this._rotationSign;
			if (this._rotationSign < 0f)
			{
				if (num < this._toAngle)
				{
					this.StartDelay();
					return;
				}
			}
			else if (num > this._toAngle)
			{
				this.StartDelay();
				return;
			}
			float num2 = this._a * Mathf.Cos(num * 0.017453292f);
			float num3 = num2 * Mathf.Cos(-this._trajectoryYaw * 0.017453292f);
			float num4 = num2 * Mathf.Sin(-this._trajectoryYaw * 0.017453292f);
			float num5 = this._b * Mathf.Sin(num * 0.017453292f);
			base.transform.rotation = Quaternion.Euler(0f, this._yaw, (!this._isRightToLeft) ? (90f - num) : (num - 90f));
			base.transform.position = this._center + new Vector3(num3, num5, num4);
		}
	}

	[SerializeField]
	private float _a = 500f;

	[SerializeField]
	private float _b = 200f;

	[SerializeField]
	private float _trajectoryYaw;

	[SerializeField]
	private float _fromAngle = 180f;

	[SerializeField]
	private float _toAngle;

	[SerializeField]
	private float _w = 1f;

	[SerializeField]
	private float _minDelay = 1f;

	[SerializeField]
	private float _maxDelay = 2f;

	[SerializeField]
	private bool _isRightToLeft;

	[SerializeField]
	private float _yaw;

	[SerializeField]
	private Vector3 _center;

	private Renderer _renderer;

	private float _curDelay;

	private float _curTime;

	private float _rotationSign;
}
