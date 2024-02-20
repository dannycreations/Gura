using System;
using System.Collections.Generic;
using RootMotion.FinalIK;
using TPM;
using UnityEngine;

[RequireComponent(typeof(LookAtIK))]
[RequireComponent(typeof(Animator))]
public class TPMPositionDrivenAnimator : MonoBehaviour
{
	public string UserId
	{
		get
		{
			return this._owner.UserId;
		}
	}

	private void Awake()
	{
		this._lookAtController = base.GetComponent<LookAtIK>();
		this.m_Animator = base.GetComponent<Animator>();
		this._forwardHash = Animator.StringToHash("Forward");
		this._sidewaysHash = Animator.StringToHash("Sideways");
		this._yawSignHash = Animator.StringToHash("YawRotationSign");
		this._yawEnabledHash = Animator.StringToHash("IsYawRotationEnabled");
		this._hashNames = new Dictionary<int, string>
		{
			{ this._forwardHash, "Forward" },
			{ this._sidewaysHash, "Sideways" }
		};
	}

	public void Init(bool isControlTarget, Transform partRoot, float defaultBodyWeight, float defaultHeadWeight, float headClampValue, HandsViewController owner = null, bool isUserInputEnabled = false)
	{
		if (owner != null)
		{
			this._owner = owner;
			this._owner.EEyesLockerChangeState += this.OwnerOnEyesLockerChangeState;
		}
		this._lookAtController.solver.clampWeightHead = headClampValue;
		this._lookAtController.solver.headWeight = defaultHeadWeight;
		this._lookAtController.solver.bodyWeight = defaultBodyWeight;
		this._defaultBodyWeight = defaultBodyWeight;
		this._defaultHeadWeight = defaultHeadWeight;
		this._isControlTarget = isControlTarget;
		this._isUserInputEnabled = isUserInputEnabled;
		this._aimExternal = this._lookAtController.solver.target;
		GameObject gameObject = new GameObject();
		this._aimTransition = gameObject.transform;
		this._aimTransition.parent = this._aimExternal.parent;
		TPMFullIKDebugSettings iksettings = TPMCharacterCustomization.Instance.IKSettings;
		this._eyes = new Transform[]
		{
			partRoot.Find(iksettings.leftEyePath),
			partRoot.Find(iksettings.rightEyePath)
		};
	}

	public void Restart()
	{
		this._wasTargetInitialized = false;
	}

	private void OwnerOnEyesLockerChangeState(bool b)
	{
		this._isEyesLockedByTackle = b;
	}

	private void Update()
	{
		if (this._aimExternal != null)
		{
			this.UpdateDebugUserInput();
			this._isMoving = Mathf.Abs(this.m_TargetForwardAmount) > 0.1f || Mathf.Abs(this.m_TargetSidewaysAmount) > 0.1f;
			if (this._isControlTarget)
			{
				this.UpdateAimTarget();
			}
			this.UpdateRotation();
			this.UpdateAnimatorMovement();
			if (this._isEyesLockedByTackle && GameFactory.Player.Tackle != null)
			{
				if (this._lookAtController.solver.target != GameFactory.Player.Tackle.transform)
				{
					this._lookAtController.solver.target = GameFactory.Player.Tackle.transform;
					this.restartEyesNoise();
				}
			}
			else if (GameFactory.Player != null)
			{
				bool flag = this.isPlayerTarget();
				if (flag)
				{
					if (this._lookAtController.solver.target != GameFactory.Player.transform)
					{
						if (this._lookAtController.solver.target == this._aimExternal)
						{
							this._aimTransition.position = this._aimExternal.position;
							this._lookAtController.solver.target = this._aimTransition;
							this.restartEyesNoise();
						}
						this.UpdateTransitionObject(GameFactory.Player.transform);
					}
				}
				else if (this._lookAtController.solver.target != this._aimExternal)
				{
					if (this._lookAtController.solver.target == GameFactory.Player.transform)
					{
						this._aimTransition.position = GameFactory.Player.transform.position;
						this._lookAtController.solver.target = this._aimTransition;
						this.restartEyesNoise();
					}
					this.UpdateTransitionObject(this._aimExternal);
				}
			}
			this.UpdateEyes();
		}
	}

	private void restartEyesNoise()
	{
		this._eyesVNoise = 0f;
		this._eyesHNoise = 0f;
		this.PrepareNewMovement(this._eyesSettings.RestartMinDelay, this._eyesSettings.MovementMaxDelay);
	}

	private void UpdateEyes()
	{
		if (this._lookAtController.solver.target != null)
		{
			if (this._startMovementAt < 0f)
			{
				this.PrepareNewMovement(this._eyesSettings.MovementMinDelay, this._eyesSettings.MovementMaxDelay);
			}
			if (this._startMovementAt < Time.time)
			{
				this._eyesVNoise = Random.Range(-this._eyesSettings.MaxVAngle * 0.017453292f, this._eyesSettings.MaxVAngle * 0.017453292f);
				this._eyesHNoise = Random.Range(-this._eyesSettings.MaxHAngle * 0.017453292f, this._eyesSettings.MaxHAngle * 0.017453292f);
				this.PrepareNewMovement(this._eyesSettings.MovementMinDelay, this._eyesSettings.MovementMaxDelay);
			}
			float magnitude = (this._lookAtController.solver.target.position - base.transform.position).magnitude;
			Vector3 vector;
			vector..ctor(magnitude * Mathf.Tan(this._eyesHNoise), magnitude * Mathf.Tan(this._eyesVNoise), 0f);
			Vector3 vector2 = this._lookAtController.solver.target.position + base.transform.TransformDirection(vector);
			this._eyes[0].LookAt(vector2);
			this._eyes[1].localRotation = this._eyes[0].localRotation;
		}
	}

	private void PrepareNewMovement(float minDelay, float maxDelay)
	{
		this._startMovementAt = Time.time + Random.Range(minDelay, maxDelay);
	}

	private void UpdateTransitionObject(Transform target)
	{
		float vectorsYaw = Math3d.GetVectorsYaw(this._aimTransition.position - base.transform.position, target.position - base.transform.position);
		float num = Time.deltaTime * this._yawRotationSpeedOnPlayer;
		if (Mathf.Abs(vectorsYaw) < num)
		{
			this._lookAtController.solver.target = target;
			this._aimTransition.position = target.position;
		}
		else
		{
			float num2 = Mathf.Abs(num / vectorsYaw);
			Vector3 vector = target.position - this._aimTransition.position;
			float magnitude = vector.magnitude;
			this._aimTransition.position = this._aimTransition.position + vector.normalized * magnitude * num2;
		}
	}

	private bool isPlayerTarget()
	{
		if (this._isRotationsBlocked || this._isMoving || this._yawRotationSign != 0)
		{
			return false;
		}
		float magnitude = (GameFactory.Player.transform.position - base.transform.position).magnitude;
		if (float.IsNaN(magnitude) || GameFactory.Player == null || magnitude > this._maxDistToPlayer || magnitude < this._minDistToPlayer)
		{
			return false;
		}
		Vector3 vector = GameFactory.Player.transform.position - base.transform.position;
		float vectorsYaw = Math3d.GetVectorsYaw(base.transform.forward, vector);
		return Mathf.Abs(vectorsYaw) <= this.minDYawToStartRotation;
	}

	private void UpdateAnimatorMovement()
	{
		this.adjustAnimatorProperty(this._forwardHash, this.m_TargetForwardAmount);
		this.adjustAnimatorProperty(this._sidewaysHash, this.m_TargetSidewaysAmount);
	}

	private void adjustAnimatorProperty(int propertyHash, float targetValue)
	{
		float num = 1f / this.animPropChangeTime;
		float num2 = this.m_Animator.GetFloat(propertyHash);
		if (Mathf.Abs(num2 - targetValue) > 0.1f)
		{
			int num3 = ((num2 >= targetValue) ? (-1) : 1);
			num2 += (float)num3 * num * Time.deltaTime;
			if (num3 == 1)
			{
				num2 = Mathf.Min(num2, targetValue);
			}
			else
			{
				num2 = Mathf.Max(num2, targetValue);
			}
			this.m_Animator.SetFloat(propertyHash, num2);
		}
		else if (targetValue != num2)
		{
			this.m_Animator.SetFloat(propertyHash, targetValue);
		}
	}

	private void UpdateRotation()
	{
		Vector3 vector = this._aimExternal.position - base.transform.position;
		float num = Math3d.GetVectorsYaw(base.transform.forward, vector);
		if (this._isMoving)
		{
			if (this._yawRotationSign != 0)
			{
				this.StopRotation();
			}
			if (!this._isUserInputEnabled && Mathf.Abs(num) > this._yawAligningStartAngle)
			{
				base.transform.Rotate(0f, Mathf.Sign(num) * this._yawAligningSpeed * Time.deltaTime, 0f);
			}
		}
		else
		{
			if (!this._isRotationsBlocked && this._yawRotationSign == 0 && Mathf.Abs(num) > this.minDYawToStartRotation)
			{
				this._yawRotationSign = Math.Sign(num);
				this.m_Animator.SetFloat(this._yawSignHash, (float)this._yawRotationSign);
				this.m_Animator.SetBool(this._yawEnabledHash, true);
			}
			if (this._yawRotationSign != 0)
			{
				base.transform.Rotate(0f, (float)this._yawRotationSign * this.yawRotationSpeed * Time.deltaTime, 0f);
				num = Math3d.GetVectorsYaw(base.transform.forward, vector);
				if (Math.Sign(num) != this._yawRotationSign)
				{
					base.transform.Rotate(0f, num, 0f);
					this.StopRotation();
				}
			}
		}
	}

	private void StopRotation()
	{
		this._yawRotationSign = 0;
		this.m_Animator.SetBool(this._yawEnabledHash, false);
	}

	private void UpdateDebugUserInput()
	{
		if (this._isUserInputEnabled && !Input.GetKey(306))
		{
			Vector3 vector;
			vector..ctor(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
			if (vector.magnitude > 0f)
			{
				Vector3 vector2;
				vector2..ctor(vector.x * 1.765f * Time.deltaTime, 0f, vector.z * 1.765f * Time.deltaTime);
				Vector3 vector3 = base.transform.TransformDirection(vector2);
				this.Move(vector3, Time.deltaTime);
				base.transform.Translate(vector2);
			}
			else if (this.m_TargetForwardAmount != 0f || this.m_TargetSidewaysAmount != 0f)
			{
				this.m_TargetForwardAmount = 0f;
				this.m_TargetSidewaysAmount = 0f;
			}
		}
	}

	private void Move(Vector3 globalMovement, float dt)
	{
		if (this._isRotationsBlocked)
		{
			base.transform.localRotation = Quaternion.identity;
			this.StopRotation();
			this._lookAtController.solver.bodyWeight = 0f;
		}
		if (this._isMovementsBlocked)
		{
			this.m_TargetForwardAmount = 0f;
			this.m_TargetSidewaysAmount = 0f;
		}
		else
		{
			this._lookAtController.solver.bodyWeight = this._defaultBodyWeight;
			this._lookAtController.solver.headWeight = this._defaultHeadWeight;
			Vector3 vector = base.transform.InverseTransformDirection(globalMovement) / dt;
			bool flag = Mathf.Abs(vector.z) > 0.1f;
			bool flag2 = Mathf.Abs(vector.x) > 0.3f;
			this.m_TargetForwardAmount = ((!flag) ? 0f : Mathf.Sign(vector.z));
			this.m_TargetSidewaysAmount = ((!flag2) ? 0f : Mathf.Sign(vector.x));
		}
	}

	public void MoveAndLook(Vector3 move, Vector3 newPos, Quaternion targetRotation, float dt, bool isRotationsBlocked, bool isMovementsBlocked)
	{
		this._isRotationsBlocked = isRotationsBlocked;
		this._isMovementsBlocked = isMovementsBlocked;
		if (this._aimExternal == null)
		{
			return;
		}
		this.Move(move, dt);
		if (this._isControlTarget)
		{
			if (!this._wasTargetInitialized)
			{
				this._wasTargetInitialized = true;
				this._prevPosition = newPos;
				this._prevRotation = targetRotation;
				base.transform.rotation = Quaternion.AngleAxis(targetRotation.eulerAngles.y, Vector3.up);
			}
			else
			{
				this._prevPosition = this._targetPosition;
				this._prevRotation = this._targetRotation;
			}
			this._targetPosition = newPos;
			this._targetRotation = targetRotation;
			this._updatedAt = Time.time;
			this._dt = dt;
		}
	}

	private void UpdateAimTarget()
	{
		if (this._wasTargetInitialized)
		{
			float num = Mathf.Min((Time.time - this._updatedAt) / this._dt, 1f);
			Vector3 vector = Vector3.Lerp(this._prevPosition, this._targetPosition, num);
			Vector3 eulerAngles = Quaternion.Slerp(this._prevRotation, this._targetRotation, num).eulerAngles;
			float y = eulerAngles.y;
			float num2 = ((eulerAngles.x <= 180f) ? (-eulerAngles.x) : (360f - eulerAngles.x));
			this._aimExternal.position = vector + new Vector3(this.lookDist * Mathf.Sin(y * 0.017453292f), this.lookDist * Mathf.Sin(num2 * 0.017453292f), this.lookDist * Mathf.Cos(y * 0.017453292f));
		}
	}

	private void OnDestroy()
	{
		if (this._aimTransition != null)
		{
			Object.Destroy(this._aimTransition.gameObject);
		}
		if (this._owner != null)
		{
			this._owner.EEyesLockerChangeState -= this.OwnerOnEyesLockerChangeState;
			this._owner = null;
		}
	}

	private const float PRECISION = 0.1f;

	private const float SIDEWAY_PRECISION = 0.3f;

	public float minDYawToStartRotation = 45f;

	public float yawRotationSpeed = 90f;

	public float lookDist = 3f;

	public float animPropChangeTime = 0.1f;

	[SerializeField]
	private float _minDistToPlayer = 4f;

	[SerializeField]
	private float _maxDistToPlayer = 10f;

	[SerializeField]
	private float _yawRotationSpeedOnPlayer = 60f;

	[SerializeField]
	private EyesSettings _eyesSettings;

	[Tooltip("Rotation speed in degrees to align 3rd player rotation with 1st player rotation during movement")]
	[SerializeField]
	private float _yawAligningSpeed = 45f;

	[SerializeField]
	private float _yawAligningStartAngle = 5f;

	private bool _isUserInputEnabled;

	private bool _isControlTarget;

	private float _startMovementAt = -1f;

	private float _targetYaw;

	private int _yawRotationSign;

	private const float FORWARD_MAX_SPEED = 1.765f;

	private const float SIDEWAYS_MAX_SPEED = 1.765f;

	private float RA = 180f;

	private float H = 2.96f;

	private Animator m_Animator;

	private int _forwardHash;

	private int _sidewaysHash;

	private int _yawSignHash;

	private int _yawEnabledHash;

	private float m_TargetSidewaysAmount;

	private float m_TargetForwardAmount;

	private bool _isMoving;

	private float m_curRotationSpeed;

	private Transform _aimExternal;

	private Transform _aimTransition;

	private Dictionary<int, string> _hashNames;

	private LookAtIK _lookAtController;

	private bool _isRotationsBlocked;

	private bool _isMovementsBlocked;

	private bool _wasTargetInitialized;

	private Vector3 _prevPosition;

	private Vector3 _targetPosition;

	private Quaternion _prevRotation;

	private Quaternion _targetRotation;

	private float _updatedAt;

	private float _dt;

	private Transform[] _eyes;

	private float _eyesVNoise;

	private float _eyesHNoise;

	private HandsViewController _owner;

	private bool _isEyesLockedByTackle;

	private float _defaultBodyWeight;

	private float _defaultHeadWeight;
}
