using System;
using System.Diagnostics;
using UnityEngine;

[Serializable]
public class CurveControlledBobFP
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event CurveControlledBobFP.OnFootStepDelegate OnFootStep = delegate
	{
	};

	public void Setup(Camera camera)
	{
		this.m_Time = this.Bobcurve[this.Bobcurve.length - 1].time;
		this._loBaseLocalPos = this.linkedObject.localPosition;
	}

	public Vector3 DoHeadBob(float speed, float speedK)
	{
		if (speed > 0.1f)
		{
			this._hCurveTime += Time.deltaTime * speedK;
			this._vCurveTime += Time.deltaTime * this.VerticaltoHorizontalRatio * speedK;
			if (this._hCurveTime > this.m_Time)
			{
				this._hCurveTime -= this.m_Time;
			}
			if (this._vCurveTime > this.m_Time)
			{
				this._vCurveTime -= this.m_Time;
			}
			float num = this.Bobcurve.Evaluate(this._hCurveTime) * this.HorizontalBobRange;
			float num2 = this.Bobcurve.Evaluate(this._vCurveTime);
			float num3 = num2 * this.VerticalBobRange;
			if (this._lastStepTimer > 0f)
			{
				this._lastStepTimer = Math.Max(this._lastStepTimer - Time.deltaTime, 0f);
			}
			else if (Math.Abs(num2 + 1f) < 0.25f)
			{
				this.OnFootStep();
				this._lastStepTimer = 0.3f;
			}
			Vector3 vector;
			vector..ctor(num, num3, 0f);
			this.linkedObject.localPosition = this._loBaseLocalPos + vector * this.loMovementMultiplier;
			this._lastSpeed = speed;
			return vector;
		}
		if (this._lastSpeed > 0.1f)
		{
			if (this._lastStepTimer == 0f)
			{
				this.OnFootStep();
			}
			this.linkedObject.localPosition = this._loBaseLocalPos;
			this._hCurveTime = 0f;
			this._vCurveTime = 0f;
			this._lastStepTimer = 0f;
		}
		this._lastSpeed = speed;
		return Vector3.zero;
	}

	private const float MIN_SPEED = 0.1f;

	private const float MIN_DELAY_BETWEEN_STEPS = 0.3f;

	private const float STEPS_PRECISION = 0.25f;

	[Tooltip("amplitude of camera's side movement")]
	public float HorizontalBobRange;

	[Tooltip("amplitude of camera's vertical movement")]
	public float VerticalBobRange = 0.01f;

	[Tooltip("curve for horizontal movement. Code assume it's normalized by 1")]
	public AnimationCurve Bobcurve = new AnimationCurve(new Keyframe[]
	{
		new Keyframe(0f, 0f),
		new Keyframe(0.5f, 1f),
		new Keyframe(1f, 0f),
		new Keyframe(1.5f, -1f),
		new Keyframe(2f, 0f)
	});

	[Tooltip("time multiplier to get vertical movement curve from horizontal curve")]
	public float VerticaltoHorizontalRatio = 2f;

	[Tooltip("Object moved with camera during player movement")]
	public Transform linkedObject;

	[Tooltip("Multiplier for linked object moving")]
	public float loMovementMultiplier = 0.5f;

	private float _hCurveTime;

	private float _vCurveTime;

	private float m_BobBaseInterval;

	private float m_Time;

	private Vector3 _loBaseLocalPos;

	private float _lastSpeed;

	private float _lastStepTimer;

	public delegate void OnFootStepDelegate();
}
