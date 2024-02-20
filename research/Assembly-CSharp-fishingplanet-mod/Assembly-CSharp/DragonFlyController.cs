using System;
using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class DragonFlyController : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event DragonFlyController.EscapeActionDelegate OnEscape = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event DragonFlyController.EscapeActionDelegate OnEscaped = delegate
	{
	};

	public void SetPosition(Vector3 newPos)
	{
		base.transform.position = newPos;
	}

	public void SetTarget(Transform targetTransform, Vector3 targetLocalPos)
	{
		if (this._curState == DragonFlyController.State.LANDED)
		{
			this.SetState(DragonFlyController.State.FLY_UP);
			this._newTarget.SetTarget(targetTransform, targetLocalPos);
		}
		else
		{
			this.SetState(DragonFlyController.State.FLYING);
			this._target.SetTarget(targetTransform, targetLocalPos);
		}
		base.transform.parent = null;
	}

	public void UpdateTargetLocalPos(Vector3 newLocalPos)
	{
		this._target.UpdateLocalPos(newLocalPos);
	}

	public void EscapeAction()
	{
		this.OnEscape();
		int randomSide = this.GetRandomSide();
		float num = (float)randomSide * this._maxSpeed * this._escapeTime;
		this._escapePoint = base.transform.position + num * this._cameraTransform.right;
		if (this._curState == DragonFlyController.State.LANDED)
		{
			this.SetState(DragonFlyController.State.FLY_UP);
		}
		else
		{
			this.SetState(DragonFlyController.State.ESCAPE);
		}
	}

	private void Awake()
	{
		this._audio = base.GetComponent<AudioSource>();
		this._audio.volume = this._maxSoundVolume * GlobalConsts.BgVolume;
		this._target = new DragonFlyController.Target(this);
		this._newTarget = new DragonFlyController.Target(this);
		this._animator = base.GetComponent<Animator>();
		this._flyHash = Animator.StringToHash("Fly");
		this.SetState(DragonFlyController.State.ESCAPED);
	}

	private void OnDestroy()
	{
		this._cameraTransform = null;
	}

	private int GetRandomSide()
	{
		return (Random.Range(0, 2) != 0) ? 1 : (-1);
	}

	private void SetState(DragonFlyController.State newState)
	{
		this._curState = newState;
		base.gameObject.SetActive(this._curState != DragonFlyController.State.ESCAPED);
		switch (newState)
		{
		case DragonFlyController.State.FLYING:
		case DragonFlyController.State.ESCAPE:
			this._curSpeed = 0f;
			this._ZigZagStartAt = Time.time + this._ZigZagSettings.FisrtDelay + DragonFlyController.GetNextRandomTime(this._ZigZagSettings.MediumTimeBetweenMovements);
			this._ZigZagStopAt = this._ZigZagStartAt + this._ZigZagSettings.Duration * 0.5f;
			this._ZigZagPauseTill = this._ZigZagStopAt + this._ZigZagSettings.PauseDuration;
			this._ZigZagLastSide = this.GetRandomSide();
			break;
		case DragonFlyController.State.LANDING:
			this._curSpeed = 1f;
			this._speedWhenLandingStarted = this._curSpeed;
			this._landingDeceleration = this._curSpeed / this._landingTime;
			this._isLandingWithFlipedYaw = Vector3.Dot(base.transform.forward, this._target.Transform.forward) < 0f;
			break;
		case DragonFlyController.State.FLY_UP:
			base.transform.parent = null;
			break;
		}
		this._curStateTime = 0f;
		this._curStateLastPosition = base.transform.position;
		this.ChangeFlyingFlag(this._curState != DragonFlyController.State.LANDED);
	}

	private void Update()
	{
		switch (this._curState)
		{
		case DragonFlyController.State.FLYING:
			this.UpdateFlying();
			break;
		case DragonFlyController.State.LANDING:
			this.UpdateLanding();
			break;
		case DragonFlyController.State.LANDED:
			this.UpdateLanded();
			break;
		case DragonFlyController.State.FLY_UP:
			this.UpdateFlyUp();
			break;
		case DragonFlyController.State.ESCAPE:
			this.UpdateEscape();
			break;
		}
	}

	private void UpdateFlying()
	{
		Vector3 landingPos = this._target.LandingPos;
		float num = this._maxSpeed;
		float num2 = ((this._curSpeed <= this._landingStartSpeed) ? (this._maxSpeed / this._fullSpeedAccelerationTime) : (this._maxSpeed / this._zeroSpeedSlowdownTime));
		float num3 = Mathf.Abs(this._curSpeed - this._landingStartSpeed) / num2;
		float magnitude = (landingPos - base.transform.position).magnitude;
		float num4 = 2f * magnitude / (this._curSpeed + this._landingStartSpeed);
		if (num3 >= num4)
		{
			num = this._landingStartSpeed;
		}
		if (!this.CommonFly(landingPos, num))
		{
			this.SetState(DragonFlyController.State.LANDING);
		}
	}

	private void UpdateLanding()
	{
		this._curSpeed -= this._landingDeceleration * Time.deltaTime;
		Vector3 landedPos = this._target.LandedPos;
		Vector3 vector = landedPos - base.transform.position;
		Vector3 vector2 = vector.normalized * Mathf.Max(this._curSpeed, 0f) * Time.deltaTime;
		this._audio.volume = Mathf.Max(this._maxSoundVolume * (this._curSpeed / this._landingStartSpeed), 0f) * GlobalConsts.BgVolume;
		if (this._curSpeed < 0f || vector2.magnitude > vector.magnitude)
		{
			this._curSpeed = 0f;
			base.transform.position = landedPos;
			this.SetState(DragonFlyController.State.LANDED);
		}
		else
		{
			base.transform.position += vector2;
		}
		float num = (this._speedWhenLandingStarted - this._curSpeed) / this._speedWhenLandingStarted;
		Quaternion quaternion = this._target.Transform.rotation;
		if (this._isLandingWithFlipedYaw)
		{
			quaternion = Quaternion.AngleAxis(180f, this._target.Transform.up) * quaternion;
		}
		if (this._target.Transform.up.y < 0f)
		{
			quaternion = Quaternion.AngleAxis(180f, this._target.Transform.forward) * quaternion;
		}
		base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, num);
		if (this._curState == DragonFlyController.State.LANDED)
		{
			base.transform.parent = this._target.Transform;
		}
	}

	private void UpdateFlyUp()
	{
		this._curSpeed += this.Acceleration * Time.deltaTime;
		base.transform.position += ((this._target.Transform.up.y >= 0f) ? this._target.Transform.up : (-this._target.Transform.up)) * this._curSpeed * Time.deltaTime;
		this._curStateTime += Time.deltaTime;
		this._audio.volume = Mathf.Min(this._maxSoundVolume * (this._curStateTime / this._flyUpTime), this._maxSoundVolume) * GlobalConsts.BgVolume;
		if (this._curStateTime > this._flyUpTime)
		{
			if (this._newTarget.Transform != null)
			{
				this._target.Copy(this._newTarget);
				this._newTarget.ClearTarget();
				this.SetState(DragonFlyController.State.FLYING);
			}
			else
			{
				this._target.ClearTarget();
				this.SetState(DragonFlyController.State.ESCAPE);
			}
		}
	}

	private void UpdateEscape()
	{
		this._curStateTime += Time.deltaTime;
		if (this._curStateTime > this._escapeTime || !this.CommonFly(this._escapePoint, this._maxSpeed))
		{
			this.SetState(DragonFlyController.State.ESCAPED);
			this.OnEscaped();
		}
	}

	private void UpdateLanded()
	{
		this._curStateTime += Time.deltaTime;
		if (this._curStateTime > this._landedStateDuration || (base.transform.position - this._curStateLastPosition).magnitude > this._escapeTickMovement)
		{
			this.EscapeAction();
			return;
		}
		this._curStateLastPosition = base.transform.position;
	}

	private float Acceleration
	{
		get
		{
			return this._maxSpeed / this._fullSpeedAccelerationTime;
		}
	}

	private float Deceleration
	{
		get
		{
			return this._maxSpeed / this._zeroSpeedSlowdownTime;
		}
	}

	private bool CommonFly(Vector3 targetPoint, float curMaxSpeed)
	{
		Vector3 vector = targetPoint - base.transform.position;
		float magnitude = vector.magnitude;
		bool flag = this._ZigZagSettings.IsEnabled;
		if (magnitude < this._ZigZagSettings.MaxDistToTarget)
		{
			flag = false;
		}
		else if (flag && this._ZigZagPauseTill < Time.time)
		{
			this._ZigZagStartAt = Time.time + DragonFlyController.GetNextRandomTime(this._ZigZagSettings.MediumTimeBetweenMovements);
			this._ZigZagStopAt = this._ZigZagStartAt + this._ZigZagSettings.Duration;
			this._ZigZagPauseTill = this._ZigZagStopAt + this._ZigZagSettings.PauseDuration;
			this._ZigZagLastSide = -this._ZigZagLastSide;
		}
		float num = curMaxSpeed;
		if (flag && this._ZigZagStartAt < Time.time && this._ZigZagStopAt > Time.time)
		{
			float num2 = this._curSpeed / this.Deceleration;
			float num3 = this._ZigZagStopAt - Time.time;
			if (num3 <= num2)
			{
				num = 0f;
			}
		}
		this._curSpeed = ((this._curSpeed >= num) ? Mathf.Max(this._curSpeed - this.Deceleration * Time.deltaTime, num) : Mathf.Min(this._curSpeed + this.Acceleration * Time.deltaTime, num));
		if (magnitude < this._curSpeed * Time.deltaTime)
		{
			Debug.DrawLine(base.transform.position, targetPoint, Color.red, 10f, false);
			base.transform.position = targetPoint;
			return false;
		}
		if (flag && this._ZigZagStartAt < Time.time)
		{
			if (this._ZigZagStartAt + Time.deltaTime >= Time.time)
			{
				float num4 = Random.Range(this._ZigZagSettings.MinAngle, this._ZigZagSettings.MaxAngle) * 0.017453292f;
				Vector3 vector2;
				vector2..ctor((float)this._ZigZagLastSide * Mathf.Sin(num4), Mathf.Sin(Random.Range(-this._ZigZagSettings.MaxVericalAngle, this._ZigZagSettings.MaxVericalAngle) * 0.017453292f), Mathf.Cos(num4));
				this._ZigZagCurDir = base.transform.TransformDirection(vector2.normalized);
			}
			if (this._ZigZagStopAt > Time.time)
			{
				float num5 = (float)(-(float)this._ZigZagLastSide) * this._curSpeed / curMaxSpeed * this.rollMaxAngle;
				Vector3 vector3 = Vector3.Cross(Vector3.up, base.transform.forward);
				int num6 = ((vector3.y >= base.transform.right.y) ? (-1) : 1);
				float num7 = (float)num6 * Vector3.Angle(vector3, base.transform.right);
				float num8 = num5 - num7;
				base.transform.Rotate(0f, 0f, num8);
				base.transform.position += this._ZigZagCurDir * this._curSpeed * Time.deltaTime;
				base.transform.position = new Vector3(base.transform.position.x, Mathf.Max(base.transform.position.y, this._ZigZagSettings.MinHeight), base.transform.position.z);
			}
			else if (this._ZigZagPauseTill > Time.time)
			{
				this._curSpeed = 0f;
				float num9 = (float)Random.Range(0, 360) * 0.017453292f;
				base.transform.position += new Vector3(this._ZigZagSettings.PauseHoveringR * Mathf.Sin(num9) * Time.deltaTime, 0f, this._ZigZagSettings.PauseHoveringR * Mathf.Cos(num9) * Time.deltaTime);
			}
		}
		else
		{
			Quaternion quaternion = Quaternion.LookRotation(vector);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, Time.deltaTime * this._damping);
			Debug.DrawLine(base.transform.position, base.transform.position + base.transform.forward * this._curSpeed * Time.deltaTime, (curMaxSpeed >= this._maxSpeed) ? Color.white : Color.green, 10f, false);
			base.transform.position += base.transform.forward * this._curSpeed * Time.deltaTime;
		}
		return true;
	}

	private void ChangeFlyingFlag(bool flag)
	{
		this._isFlying = flag;
		this._animator.SetBool(this._flyHash, this._isFlying);
	}

	private static float GetNextRandomTime(float commonTimeValue)
	{
		return -Mathf.Log(Random.Range(0.0001f, 1f)) * commonTimeValue;
	}

	public float _maxSpeed = 3f;

	[Tooltip("Time to get maximum speed")]
	public float _fullSpeedAccelerationTime = 1f;

	[Tooltip("Time to stop")]
	public float _zeroSpeedSlowdownTime = 1f;

	[Tooltip("Speed of rotation on target")]
	public float _damping = 10f;

	public float rollMaxAngle = 10f;

	public float _maxSoundVolume = 0.15f;

	[Tooltip("How long hop off")]
	public float _flyUpTime = 0.2f;

	[Tooltip("Target local movement above the shape before landing")]
	public Vector3 _landingStartPos = new Vector3(0f, 0.2f, 0.3f);

	[Tooltip("Desired landing speed. Dragonfly will use appropriate acceleration to reached this speed automatically just before the landing")]
	public float _landingStartSpeed = 1f;

	public float _landingTime = 0.72f;

	[Tooltip("Time to reach ESCAPED state")]
	public float _escapeTime = 5f;

	[Tooltip("Minimal tick movement of object we sit on to start ESCAPE scenario")]
	public float _escapeTickMovement = 0.1f;

	[Tooltip("Time in LANDED state to start ESCAPE scenario")]
	public float _landedStateDuration = 10f;

	public DragonFlyController.ZigZagSettings _ZigZagSettings;

	[Tooltip("Object to calculate direction to escape from")]
	public Transform _cameraTransform;

	private int _flyHash;

	private Animator _animator;

	private bool _isFlying;

	private DragonFlyController.State _curState;

	private float _curSpeed;

	private float _curStateTime;

	private Vector3 _curStateLastPosition;

	private float _speedWhenLandingStarted;

	private float _landingDeceleration;

	private bool _isLandingWithFlipedYaw;

	private DragonFlyController.Target _target;

	private DragonFlyController.Target _newTarget;

	private Vector3 _escapePoint;

	private int _ZigZagLastSide;

	private Vector3 _ZigZagCurDir;

	private float _ZigZagStartAt;

	private float _ZigZagStopAt;

	private float _ZigZagPauseTill;

	private const float _ZigZagFirstMovementTimeK = 0.5f;

	private AudioSource _audio;

	private enum State
	{
		FLYING,
		LANDING,
		LANDED,
		FLY_UP,
		ESCAPE,
		ESCAPED
	}

	[Serializable]
	public class ZigZagSettings
	{
		public bool IsEnabled = true;

		public float MediumTimeBetweenMovements = 0.3f;

		public float FisrtDelay = 2f;

		public float MaxDistToTarget = 2f;

		public float MinAngle = 60f;

		public float MaxAngle = 80f;

		public float MaxVericalAngle = 10f;

		public float Duration = 1f;

		public float PauseDuration = 1f;

		public float PauseHoveringR = 0.1f;

		public float MinHeight = 0.1f;
	}

	public delegate void EscapeActionDelegate();

	private class Target
	{
		public Target(DragonFlyController controller)
		{
			this._controller = controller;
			this.ClearTarget();
		}

		public Target(Transform transform, Vector3 localPos)
		{
			this.SetTarget(transform, localPos);
		}

		public Transform Transform
		{
			get
			{
				return this._transform;
			}
		}

		public Vector3 LandedPos
		{
			get
			{
				return this._transform.TransformPoint(this._localPos);
			}
		}

		public Vector3 LandingPos
		{
			get
			{
				return this._transform.TransformPoint(this._localPos + ((this._transform.up.y <= 0f) ? new Vector3(this._controller._landingStartPos.x, -this._controller._landingStartPos.y, this._controller._landingStartPos.z) : this._controller._landingStartPos));
			}
		}

		public void SetTarget(Transform transform, Vector3 localPos)
		{
			this._transform = transform;
			this._localPos = localPos;
		}

		public void UpdateLocalPos(Vector3 newLocalPos)
		{
			this._localPos = newLocalPos;
		}

		public void Copy(DragonFlyController.Target newTarget)
		{
			this._transform = newTarget._transform;
			this._localPos = newTarget._localPos;
		}

		public void ClearTarget()
		{
			this._transform = null;
		}

		private Transform _transform;

		private Vector3 _localPos;

		private DragonFlyController _controller;
	}
}
