using System;
using System.Collections;
using UnityEngine;

public class SchoolChild : MonoBehaviour
{
	public void Start()
	{
		if (this._model == null)
		{
			this._model = base.transform.Find("Model").GetComponent<Animation>();
		}
		this._animations = new AnimationState[this._model.GetClipCount()];
		int num = 0;
		IEnumerator enumerator = this._model.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				AnimationState animationState = (AnimationState)obj;
				this._animations[num++] = animationState;
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
		if (this._spawner != null)
		{
			this.SetRandomScale();
			this.LocateRequiredChildren();
			this.RandomizeStartAnimationFrame();
			this.SkewModelForLessUniformedMovement();
			this._speed = Random.Range(this._spawner._minSpeed, this._spawner._maxSpeed);
			this.Wander(0f);
			this.SetRandomWaypoint();
			this.CheckForBubblesThenInvoke();
			this._instantiated = true;
			this.UpdatePosition();
			this.FrameSkipSeedInit();
			this._spawner._activeChildren++;
			return;
		}
		base.enabled = false;
		Debug.Log(string.Concat(new object[] { base.gameObject, " found no school to swim in: ", this, " disabled... Standalone fish not supported, please use the SchoolController" }));
	}

	public void Update()
	{
		if (this._spawner._updateDivisor <= 1 || this._spawner._updateCounter == this._updateSeed)
		{
			this.CheckForDistanceToWaypoint();
			this.RotationBasedOnWaypointOrAvoidance();
			this.ForwardMovement();
			this.RayCastToPushAwayFromObstacles();
			this.SetAnimationSpeed();
		}
	}

	public void FrameSkipSeedInit()
	{
		if (this._spawner._updateDivisor > 1)
		{
			int num = this._spawner._updateDivisor - 1;
			SchoolChild._updateNextSeed++;
			this._updateSeed = SchoolChild._updateNextSeed;
			SchoolChild._updateNextSeed %= num;
		}
	}

	public void CheckForBubblesThenInvoke()
	{
		if (this._spawner._bubbles != null)
		{
			base.InvokeRepeating("EmitBubbles", this._spawner._bubbles._emitEverySecond * Random.value + 1f, this._spawner._bubbles._emitEverySecond);
		}
	}

	public void EmitBubbles()
	{
		if (this._spawner != null)
		{
			this._spawner._bubbles.EmitBubbles(base.transform.position, this._speed);
		}
	}

	public void OnDisable()
	{
		base.CancelInvoke();
		this._spawner._activeChildren--;
	}

	public void OnEnable()
	{
		if (this._instantiated)
		{
			this.CheckForBubblesThenInvoke();
			this._spawner._activeChildren++;
		}
	}

	public void LocateRequiredChildren()
	{
		if (this._scanner == null)
		{
			this._scanner = new GameObject().transform;
			this._scanner.parent = base.transform;
			this._scanner.localRotation = Quaternion.identity;
			this._scanner.localPosition = Vector3.zero;
		}
	}

	public void SkewModelForLessUniformedMovement()
	{
		Quaternion identity = Quaternion.identity;
		identity.eulerAngles = new Vector3(0f, 0f, (float)Random.Range(-25, 25));
		this._model.transform.rotation = identity;
	}

	public void SetRandomScale()
	{
		float num = Random.Range(this._spawner.MinFishScale, this._spawner.MaxFishScale);
		base.transform.localScale = Vector3.one * num;
	}

	public void RandomizeStartAnimationFrame()
	{
		IEnumerator enumerator = this._model.GetComponent<Animation>().GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				object obj = enumerator.Current;
				AnimationState animationState = (AnimationState)obj;
				animationState.time = Random.value * animationState.length;
			}
		}
		finally
		{
			IDisposable disposable;
			if ((disposable = enumerator as IDisposable) != null)
			{
				disposable.Dispose();
			}
		}
	}

	public void UpdatePosition()
	{
		base.transform.position = this._wayPoint - new Vector3(0.1f, 0.1f, 0.1f);
	}

	public void RegeneratePosition()
	{
		this.SetRandomWaypoint();
		this.UpdatePosition();
	}

	public void RayCastToPushAwayFromObstacles()
	{
		if (this._spawner._push)
		{
			this.RotateScanner();
			this.RayCastToPushAwayFromObstaclesCheckForCollision();
		}
	}

	public void RayCastToPushAwayFromObstaclesCheckForCollision()
	{
		RaycastHit raycastHit = default(RaycastHit);
		Vector3 forward = this._scanner.forward;
		if (Physics.Raycast(base.transform.position, forward, ref raycastHit, this._spawner._pushDistance, this._spawner._avoidanceMask))
		{
			SchoolChild component = raycastHit.transform.GetComponent<SchoolChild>();
			float num = (this._spawner._pushDistance - raycastHit.distance) / this._spawner._pushDistance;
			if (component != null)
			{
				base.transform.position -= forward * this._spawner._newDelta * num * this._spawner._pushForce;
			}
			else
			{
				this._speed -= 0.01f * this._spawner._newDelta;
				if (this._speed < 0.1f)
				{
					this._speed = 0.1f;
				}
				base.transform.position -= forward * this._spawner._newDelta * num * this._spawner._pushForce * 2f;
				this._scan = false;
			}
		}
		else
		{
			this._scan = true;
		}
	}

	public void RotateScanner()
	{
		if (this._scan)
		{
			this._scanner.rotation = Random.rotation;
			return;
		}
		this._scanner.Rotate(new Vector3(150f * this._spawner._newDelta, 0f, 0f));
	}

	public bool Avoidance()
	{
		if (!this._spawner._avoidance)
		{
			return false;
		}
		RaycastHit raycastHit = default(RaycastHit);
		Quaternion rotation = base.transform.rotation;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		Vector3 forward = base.transform.forward;
		Vector3 right = base.transform.right;
		if (Physics.Raycast(base.transform.position, -Vector3.up + forward * 0.1f, ref raycastHit, this._spawner._avoidDistance, this._spawner._avoidanceMask))
		{
			float num = (this._spawner._avoidDistance - raycastHit.distance) / this._spawner._avoidDistance;
			eulerAngles.x -= this._spawner._avoidSpeed * num * this._spawner._newDelta * (this._speed + 1f);
			rotation.eulerAngles = eulerAngles;
			base.transform.rotation = rotation;
		}
		if (Physics.Raycast(base.transform.position, Vector3.up + forward * 0.1f, ref raycastHit, this._spawner._avoidDistance, this._spawner._avoidanceMask))
		{
			float num = (this._spawner._avoidDistance - raycastHit.distance) / this._spawner._avoidDistance;
			eulerAngles.x += this._spawner._avoidSpeed * num * this._spawner._newDelta * (this._speed + 1f);
			rotation.eulerAngles = eulerAngles;
			base.transform.rotation = rotation;
		}
		if (Physics.Raycast(base.transform.position, forward + right * Random.Range(-0.1f, 0.1f), ref raycastHit, this._spawner._stopDistance, this._spawner._avoidanceMask))
		{
			float num = (this._spawner._stopDistance - raycastHit.distance) / this._spawner._stopDistance;
			eulerAngles.y -= this._spawner._avoidSpeed * num * this._spawner._newDelta * (this._targetSpeed + 3f);
			rotation.eulerAngles = eulerAngles;
			base.transform.rotation = rotation;
			this._speed -= num * this._spawner._newDelta * this._spawner._stopSpeedMultiplier * this._speed;
			if (this._speed < 0.01f)
			{
				this._speed = 0.01f;
			}
			return true;
		}
		if (Physics.Raycast(base.transform.position, forward + right * (this._spawner._avoidAngle + this._rotateCounterL), ref raycastHit, this._spawner._avoidDistance, this._spawner._avoidanceMask))
		{
			float num = (this._spawner._avoidDistance - raycastHit.distance) / this._spawner._avoidDistance;
			this._rotateCounterL += 0.1f;
			eulerAngles.y -= this._spawner._avoidSpeed * num * this._spawner._newDelta * this._rotateCounterL * (this._speed + 1f);
			rotation.eulerAngles = eulerAngles;
			base.transform.rotation = rotation;
			if (this._rotateCounterL > 1.5f)
			{
				this._rotateCounterL = 1.5f;
			}
			this._rotateCounterR = 0f;
			return true;
		}
		if (Physics.Raycast(base.transform.position, forward + right * -(this._spawner._avoidAngle + this._rotateCounterR), ref raycastHit, this._spawner._avoidDistance, this._spawner._avoidanceMask))
		{
			float num = (this._spawner._avoidDistance - raycastHit.distance) / this._spawner._avoidDistance;
			if (raycastHit.point.y < base.transform.position.y)
			{
				eulerAngles.y -= this._spawner._avoidSpeed * num * this._spawner._newDelta * (this._speed + 1f);
			}
			else
			{
				eulerAngles.x += this._spawner._avoidSpeed * num * this._spawner._newDelta * (this._speed + 1f);
			}
			this._rotateCounterR += 0.1f;
			eulerAngles.y += this._spawner._avoidSpeed * num * this._spawner._newDelta * this._rotateCounterR * (this._speed + 1f);
			rotation.eulerAngles = eulerAngles;
			base.transform.rotation = rotation;
			if (this._rotateCounterR > 1.5f)
			{
				this._rotateCounterR = 1.5f;
			}
			this._rotateCounterL = 0f;
			return true;
		}
		this._rotateCounterL = 0f;
		this._rotateCounterR = 0f;
		return false;
	}

	public void ForwardMovement()
	{
		base.transform.position += base.transform.TransformDirection(Vector3.forward) * this._speed * this._spawner._newDelta;
		if (this.tParam < 1f)
		{
			if (this._speed > this._targetSpeed)
			{
				this.tParam += this._spawner._newDelta * this._spawner._acceleration;
			}
			else
			{
				this.tParam += this._spawner._newDelta * this._spawner._brake;
			}
			this._speed = Mathf.Lerp(this._speed, this._targetSpeed, this.tParam);
		}
	}

	public void RotationBasedOnWaypointOrAvoidance()
	{
		Quaternion quaternion = Quaternion.identity;
		quaternion = Quaternion.LookRotation(this._wayPoint - base.transform.position);
		if (!this.Avoidance())
		{
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion, this._spawner._newDelta * this._damping);
		}
		float num = base.transform.localEulerAngles.x;
		num = ((num <= 180f) ? num : (num - 360f));
		Quaternion rotation = base.transform.rotation;
		Vector3 eulerAngles = rotation.eulerAngles;
		eulerAngles.x = SchoolChild.ClampAngle(num, -50f, 50f);
		rotation.eulerAngles = eulerAngles;
		base.transform.rotation = rotation;
	}

	public void CheckForDistanceToWaypoint()
	{
		if ((base.transform.position - this._wayPoint).magnitude < this._spawner._waypointDistance + this._stuckCounter)
		{
			this.Wander(0f);
			this._stuckCounter = 0f;
			this.CheckIfThisShouldTriggerNewFlockWaypoint();
			return;
		}
		this._stuckCounter += this._spawner._newDelta * (this._spawner._waypointDistance * 0.25f);
	}

	public void CheckIfThisShouldTriggerNewFlockWaypoint()
	{
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}

	public void SetAnimationSpeed()
	{
		for (int i = 0; i < this._animations.Length; i++)
		{
			this._animations[i].speed = Random.Range(this._spawner._minAnimationSpeed, this._spawner._maxAnimationSpeed) * this._spawner._schoolSpeed * this._speed + 0.1f;
		}
	}

	public void Wander(float delay)
	{
		this._damping = Random.Range(this._spawner._minDamping, this._spawner._maxDamping);
		this._targetSpeed = Random.Range(this._spawner._minSpeed, this._spawner._maxSpeed) * this._spawner._speedCurveMultiplier.Evaluate(Random.value) * this._spawner._schoolSpeed;
		base.Invoke("SetRandomWaypoint", delay);
	}

	public void SetRandomWaypoint()
	{
		this.tParam = 0f;
		this._wayPoint = this._spawner.GenerateFishWaypoint();
	}

	[HideInInspector]
	public SchoolController _spawner;

	private Vector3 _wayPoint;

	[HideInInspector]
	public float _speed = 10f;

	private float _stuckCounter;

	private float _damping;

	private Animation _model;

	private float _targetSpeed;

	private float tParam;

	private float _rotateCounterR;

	private float _rotateCounterL;

	public Transform _scanner;

	private bool _scan = true;

	private bool _instantiated;

	private static int _updateNextSeed;

	private int _updateSeed = -1;

	private AnimationState[] _animations;
}
