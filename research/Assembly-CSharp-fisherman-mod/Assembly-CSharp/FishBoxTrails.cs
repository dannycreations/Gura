using System;
using System.Collections.Generic;
using UnityEngine;

public class FishBoxTrails : FishBoxActivity, ISize
{
	protected override void Awake()
	{
		base.Awake();
		this._movers = new FishBoxTrails.MovementF[]
		{
			new FishBoxTrails.MovementF(this.MySin),
			new FishBoxTrails.MovementF(this.MyCos)
		};
		this._fishesPhase = new List<float>(this._maxFishesCount);
		this._movementAngle = this.GetRandomYaw();
	}

	public void SetWidth(float width)
	{
		Vector3 localScale = base.transform.localScale;
		base.transform.localScale = new Vector3(width, localScale.y, localScale.z);
	}

	public void SetDepth(float depth)
	{
		Vector3 localScale = base.transform.localScale;
		base.transform.localScale = new Vector3(localScale.x, localScale.y, depth);
	}

	public override bool ManualUpdate()
	{
		if (base.ManualUpdate())
		{
			if (this._nextBubbleAt > 0f && this._nextBubbleAt < Time.time)
			{
				this.CreateBubble();
			}
			return true;
		}
		return false;
	}

	protected override void StartAction()
	{
		base.StartAction();
		this._curMover = this._movers[Random.Range(0, this._movers.Length)];
		this._actionStartedAt = Time.time;
		this._actionStartPos = base.GetZoneCenterPos();
		if (!this._debugSameRotation)
		{
			this._movementAngle = this.GetRandomYaw();
		}
		this._fishesPhase.Clear();
		int num = Random.Range(this._minFishesCount, this._maxFishesCount + 1);
		for (int i = 0; i < num; i++)
		{
			this._fishesPhase.Add(Random.Range(0f, this._maxPhaseDelta));
		}
		this._bubblesForceChangingDuration = (this._actionDuration - this._bubblesFullForceDuration) / 2f;
		this.CreateBubble();
	}

	protected override void StopAction()
	{
		base.StopAction();
		this._actionStartedAt = -1f;
		this._nextBubbleAt = -1f;
	}

	private void CreateBubble()
	{
		this._nextBubbleAt = Time.time + this._timeBetweenBubbles;
		if (GameFactory.Water == null)
		{
			return;
		}
		float num = Time.time - this._actionStartedAt;
		float bublesForce = this.GetBublesForce(num);
		for (int i = 0; i < this._fishesPhase.Count; i++)
		{
			GameFactory.Water.AddWaterDisturb(this.GetFishCurPoint(i, num), this._bubblesRadius * bublesForce, (float)this._bubblesType * bublesForce);
		}
	}

	private Vector3 GetFishCurPoint(int fishIndex, float t)
	{
		float num = this._fishSpeed * (t + this._fishesPhase[fishIndex]) / this._r / this._k;
		float num2 = this._curMover(num);
		Vector3 vector = new Vector3(0f, 0f, (float)fishIndex * this._fishDist) + new Vector3(num, 0f, num2);
		Vector3 vector2 = Math3d.RotatePointAroundOY(vector, this._movementAngle - base.transform.eulerAngles.y * 0.017453292f);
		return this._actionStartPos + vector2;
	}

	private float GetBublesForce(float t)
	{
		if (t < this._bubblesForceChangingDuration)
		{
			return t / this._bubblesForceChangingDuration;
		}
		if (t > this._bubblesForceChangingDuration + this._bubblesFullForceDuration)
		{
			return 1f - (t - (this._bubblesForceChangingDuration + this._bubblesFullForceDuration)) / this._bubblesForceChangingDuration;
		}
		return 1f;
	}

	private float MySin(float phi)
	{
		return this._r * Mathf.Sin(phi);
	}

	private float MyCos(float phi)
	{
		return this._r * Mathf.Cos(phi);
	}

	[SerializeField]
	private int _minFishesCount = 3;

	[SerializeField]
	private int _maxFishesCount = 5;

	[SerializeField]
	private float _fishSpeed = 0.25f;

	[SerializeField]
	private float _fishDist = 0.1f;

	[Tooltip("radius of the trigonometric arc")]
	[SerializeField]
	private float _r = 0.3f;

	[Tooltip("trigonometric arc speed modifier")]
	[SerializeField]
	private float _k = 0.25f;

	[Tooltip("trigonometric arc phase's max delta")]
	[SerializeField]
	private float _maxPhaseDelta = 0.3f;

	[SerializeField]
	private float _timeBetweenBubbles = 0.02f;

	[SerializeField]
	private float _bubblesRadius = 0.1f;

	[SerializeField]
	private WaterDisturbForce _bubblesType = WaterDisturbForce.Small;

	[SerializeField]
	private float _bubblesFullForceDuration = 1f;

	[SerializeField]
	private bool _debugSameRotation;

	[SerializeField]
	private bool _debugDrawTrajectories;

	[SerializeField]
	private float _debugDrawTrajectoriesDuration = 3f;

	private FishBoxTrails.MovementF[] _movers;

	private float _actionStartedAt = -1f;

	private float _nextBubbleAt = -1f;

	private Vector3 _actionStartPos;

	private float _movementAngle;

	private FishBoxTrails.MovementF _curMover;

	private List<float> _fishesPhase;

	private float _bubblesForceChangingDuration;

	private delegate float MovementF(float x);
}
