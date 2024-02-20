using System;
using UnityEngine;

public class AboveWaterTransformAdjuster : MonoBehaviour
{
	public bool IsAdjusted
	{
		get
		{
			return this._deepMarker.position.y > 0f || Mathf.Approximately(this._deepMarker.position.y, 0f);
		}
	}

	private void Awake()
	{
		this._obj = this._lift.parent;
		this._deepMarker = new GameObject("deepMarker").transform;
		this._deepMarker.parent = this._lift;
		this._deepMarker.localPosition = this._lift.up * this._deepMarkPos0;
		this._liftLocalPos0 = this._lift.localPosition;
		base.transform.SetParent(this._lift, true);
	}

	private void Update()
	{
		Vector3 vector = this._obj.TransformPoint(this._liftLocalPos0);
		float num = Mathf.Asin(this._lift.up.y);
		if (!Mathf.Approximately(this._deepMarker.position.y, 0f))
		{
			float num2 = -vector.y / Mathf.Sin(num);
			float num3 = num2 - this._deepMarkPos0;
			float num4 = ((num3 >= 0f) ? Mathf.Min(num3, this._maxMovement) : 0f);
			this._lift.localPosition = this._liftLocalPos0 + Vector3.up * num4;
		}
	}

	private void OnDrawGizmos()
	{
		if (this._lift != null)
		{
			Vector3 vector = ((!(this._deepMarker != null)) ? (this._lift.position + this._lift.up * this._deepMarkPos0) : this._deepMarker.position);
			Gizmos.color = Color.magenta;
			Gizmos.DrawSphere(vector, 0.02f);
		}
	}

	[SerializeField]
	private Transform _lift;

	[SerializeField]
	private float _maxMovement = 0.45f;

	[SerializeField]
	private float _deepMarkPos0 = -0.2f;

	private Transform _obj;

	private Vector3 _liftLocalPos0;

	private Transform _deepMarker;
}
