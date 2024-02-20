using System;
using Phy;
using UnityEngine;

public class AttackingRailFishController : MonoBehaviour
{
	private void Start()
	{
		if (this.FollowPoint == null)
		{
			this.FollowPoint = this.Target.parent;
		}
		this.currentSeed = this.RandomSeed;
		this.startDirection = base.transform.forward;
		this.currentCurve = new BezierCurve(5);
		this.curveT = 0f;
		this.anchorPoints = new Vector3[6];
		this.updateCurve();
		this.railFish = base.GetComponent<RailFishController>();
		this.lureBasePosition = this.Lure.transform.localPosition;
		this.lureBaseRotation = this.Lure.transform.localRotation;
		this.lureBaseParent = this.Lure.transform.parent;
		this.railFish.Lure = this.Lure;
	}

	private float disprnd(float disp, float min, float signOverride = 0f)
	{
		float num = ((signOverride != 0f) ? signOverride : ((float)(2 * Random.Range(0, 2) - 1)));
		float value = Random.value;
		return num * (min + value * value * (disp - min));
	}

	private float getCurveRadius(Vector3 src, Vector3 dir, Vector3 dest)
	{
		Vector3 normalized = (dest - src).normalized;
		Vector3 normalized2 = Vector3.Cross(dir, normalized).normalized;
		Vector3 normalized3 = Vector3.Cross(normalized2, dir).normalized;
		return Mathf.Tan(Mathf.Acos(Vector3.Dot(normalized, normalized3)));
	}

	private void updateCurve()
	{
		Random.InitState(this.currentSeed);
		this.currentSeed = Random.Range(int.MinValue, int.MaxValue);
		Vector3 vector;
		if (this.attackState == AttackingRailFishController.AttackState.Escape)
		{
			vector = this.EscapePoint.position;
		}
		else if (this.attackState == AttackingRailFishController.AttackState.Follow)
		{
			vector = this.FollowPoint.position - this.FollowPoint.forward * this.SegmentDuration * this.TargetVelocity;
		}
		else
		{
			vector = this.Target.position;
		}
		Vector3 vector2 = vector;
		Vector3 normalized = (vector2 - this.anchorPoints[0]).normalized;
		if (this.attackState != AttackingRailFishController.AttackState.Throw)
		{
			float num = this.disprnd(this.WaypointDispersion.x, this.MinWaypointDispersion.x, this.alternatingSign);
			vector2 += this.FollowPoint.right * num + this.FollowPoint.up * this.disprnd(this.WaypointDispersion.y, this.MinWaypointDispersion.y, 0f) + this.FollowPoint.forward * (this.disprnd(this.WaypointDispersion.z, this.MinWaypointDispersion.z, 0f) + this.TargetMinDistance);
			this.alternatingSign = -this.alternatingSign;
		}
		float num2 = (base.transform.position - vector2).magnitude / (5f * this.turnSharpness);
		this.anchorPoints[0] = base.transform.position;
		this.anchorPoints[1] = this.anchorPoints[0] + this.startDirection / (this.SegmentDuration * 10f);
		for (int i = 2; i < 5; i++)
		{
			this.anchorPoints[i] = this.anchorPoints[i - 1] + Vector3.Slerp((this.anchorPoints[i - 1] - this.anchorPoints[i - 2]).normalized, (vector2 - this.anchorPoints[i - 1]).normalized, 0.5f) * num2;
		}
		this.anchorPoints[5] = vector2;
		for (int j = 0; j <= this.currentCurve.Order; j++)
		{
			this.currentCurve.AnchorPoints[j] = this.anchorPoints[j];
		}
		if (this.attackState == AttackingRailFishController.AttackState.Follow)
		{
			this.curveDT = 1f / (this.SegmentDuration + (Random.value - 0.5f) * Random.value * 2f * this.SpeedDispersion);
		}
		else if (this.attackState == AttackingRailFishController.AttackState.Throw)
		{
			this.curveDT = 0f;
		}
		else
		{
			this.curveDT = 1f / this.EscapeDuration;
		}
	}

	private void Update()
	{
		if (this.dp == null && this.railFish.dp != null)
		{
			this.dp = this.railFish.dp;
			this.dp_target2fish = this.dp.AddValue("target2fish");
			this.dp_curveT = this.dp.AddValue("curveT");
			this.dp_deltaTime = this.dp.AddValue("deltaTime");
			this.dp_lurealpha = this.dp.AddValue("lureAlpha");
		}
		if (this.AttackCue)
		{
			if (this.attackState == AttackingRailFishController.AttackState.Follow)
			{
				this.attackState = AttackingRailFishController.AttackState.Throw;
				this.railFish.AnchorNode = this.Mouth;
				this.curveT = 0f;
				this.startDirection = (this.Target.position - this.Mouth.position).normalized;
				this.updateCurve();
			}
			if (this.attackState == AttackingRailFishController.AttackState.Throw)
			{
				if (this.curveT > 0.35f)
				{
					this.railFish.JawOpenCue = true;
				}
				this.railFish.AnchorFactor = this.curveT;
				this.railFish.ImmediateVelocity(Mathf.Pow(this.curveT, 0.3f) * 4f * this.railFish.NominalLocomotionVelocity);
				this.currentCurve.AnchorPoints[this.currentCurve.Order] = this.Target.position;
			}
		}
		else if (this.attackState != AttackingRailFishController.AttackState.Follow)
		{
			this.attackState = AttackingRailFishController.AttackState.Follow;
			this.railFish.AnchorFactor = 0f;
			this.Lure.transform.parent = this.lureBaseParent;
			this.Lure.transform.localPosition = this.lureBasePosition;
			this.Lure.transform.localRotation = this.lureBaseRotation;
			this.Lure.KillSwitch = false;
			this.turnSharpness = 1f;
		}
		this.currentCurve.SetT(this.curveT);
		if (this.attackState == AttackingRailFishController.AttackState.Throw)
		{
			base.transform.position = this.currentCurve.Point() + (base.transform.position - this.Mouth.position) * this.curveT;
			this.curveDT += 2f * Time.deltaTime / (this.ThrowDuration * this.ThrowDuration);
		}
		else
		{
			base.transform.position = this.currentCurve.Point();
		}
		this.curveT += this.curveDT * Time.deltaTime;
		if (this.curveT >= 1f)
		{
			if (this.AttackCue)
			{
				this.dist = (this.Mouth.position - this.Target.position).magnitude;
				if (this.attackState == AttackingRailFishController.AttackState.Throw)
				{
					this.attackState = AttackingRailFishController.AttackState.Escape;
					this.curveT = 0f;
					this.railFish.JawOpenCue = false;
					this.Lure.transform.parent = this.Mouth;
					this.Lure.transform.localPosition = Vector3.zero;
					this.Lure.KillSwitch = true;
					this.turnSharpness = 10f;
					this.railFish.ImmediateVelocity(0.5f * this.railFish.NominalLocomotionVelocity);
				}
			}
			this.curveT -= Mathf.Floor(this.curveT);
			this.startDirection = this.currentCurve.Derivative();
			this.updateCurve();
		}
		for (int i = 1; i < this.anchorPoints.Length; i++)
		{
			Debug.DrawLine(this.anchorPoints[i - 1], this.anchorPoints[i], Color.magenta);
		}
		if (this.dp != null)
		{
			this.dp_target2fish.Add(base.transform.position.x + base.transform.position.z);
			this.dp_curveT.Add(this.curveT);
			this.dp_deltaTime.Add(Time.deltaTime);
			this.dp_lurealpha.Add(this.Lure.alpha);
		}
	}

	private void OnApplicationQuit()
	{
		DebugPlotter.AutoSave();
	}

	public const int AnchorPointsCount = 6;

	public Transform Target;

	public float TargetVelocity = 0.3f;

	public Transform EscapePoint;

	public Transform FollowPoint;

	public float ThrowDuration = 1f;

	public float EscapeDuration = 5f;

	public bool AttackCue;

	public int RandomSeed;

	public Vector3 WaypointDispersion = new Vector3(0.5f, 0.3f, 0.5f);

	public Vector3 MinWaypointDispersion = new Vector3(0f, 0f, 0f);

	public float TargetMinDistance;

	public float SpeedDispersion;

	public float SegmentDuration = 1.5f;

	public Transform Mouth;

	public RailLureController Lure;

	private BezierCurve currentCurve;

	private Vector3 startDirection;

	private float curveT;

	private float curveDT;

	private Vector3[] anchorPoints;

	private float turnSharpness = 1f;

	public float dist;

	private AttackingRailFishController.AttackState attackState;

	private RailFishController railFish;

	private Vector3 lureBasePosition;

	private Quaternion lureBaseRotation;

	private Transform lureBaseParent;

	private DebugPlotter dp;

	private DebugPlotter.Value dp_target2fish;

	private DebugPlotter.Value dp_curveT;

	private DebugPlotter.Value dp_deltaTime;

	private DebugPlotter.Value dp_lurealpha;

	private int currentSeed;

	private float alternatingSign = 1f;

	public enum AttackState
	{
		Follow,
		Throw,
		Escape
	}
}
