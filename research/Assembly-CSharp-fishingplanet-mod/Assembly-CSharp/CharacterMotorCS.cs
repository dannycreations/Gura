using System;
using System.Collections;
using TPM;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[AddComponentMenu("Character/Character Motor")]
public class CharacterMotorCS : MonoBehaviour
{
	public Collider Collider
	{
		get
		{
			return this._controller;
		}
	}

	public Vector3 ColliderPosition
	{
		get
		{
			return base.transform.position;
		}
	}

	private void Awake()
	{
		this._controller = base.GetComponent<CharacterController>();
		this.tr = base.transform;
		this.tr.position = Vector3.zero;
		Transform transform = this.tr.Find("Player");
		transform.localPosition = Vector3.zero;
	}

	private void Start()
	{
		this.SetupCapsule();
	}

	private void SetupCapsule()
	{
		TPMModelSettings genderSettings = TPMCharacterCustomization.Instance.GetGenderSettings(TPMCharacterCustomization.Instance.GetMyGender());
		this._controller.center = new Vector3(0f, genderSettings.colliderCapsule.centerY, 0f);
		this._controller.height = genderSettings.colliderCapsule.height;
	}

	private void UpdateFunction()
	{
		Vector3 vector = this.movement.velocity;
		vector = this.ApplyInputVelocityChange(vector);
		vector = this.ApplyGravityAndJumping(vector);
		Vector3 vector2 = Vector3.zero;
		if (this.MoveWithPlatform())
		{
			Vector3 vector3 = this.movingPlatform.activePlatform.TransformPoint(this.movingPlatform.activeLocalPoint);
			vector2 = vector3 - this.movingPlatform.activeGlobalPoint;
			if (vector2 != Vector3.zero)
			{
				this._controller.Move(vector2);
			}
			Quaternion quaternion = this.movingPlatform.activePlatform.rotation * this.movingPlatform.activeLocalRotation;
			float y = (quaternion * Quaternion.Inverse(this.movingPlatform.activeGlobalRotation)).eulerAngles.y;
			if (y != 0f)
			{
				this.tr.Rotate(0f, y, 0f);
			}
		}
		Vector3 position = this.tr.position;
		Vector3 vector4 = vector * Time.deltaTime;
		float stepOffset = this._controller.stepOffset;
		Vector3 vector5;
		vector5..ctor(vector4.x, 0f, vector4.z);
		float num = Mathf.Max(stepOffset, vector5.magnitude);
		if (this.grounded)
		{
			vector4 -= num * Vector3.up;
		}
		this.movingPlatform.hitPlatform = null;
		this.groundNormal = Vector3.zero;
		this.movement.collisionFlags = this._controller.Move(vector4);
		this.movement.lastHitPoint = this.movement.hitPoint;
		this.lastGroundNormal = this.groundNormal;
		if (this.movingPlatform.enabled && this.movingPlatform.activePlatform != this.movingPlatform.hitPlatform && this.movingPlatform.hitPlatform != null)
		{
			this.movingPlatform.activePlatform = this.movingPlatform.hitPlatform;
			this.movingPlatform.lastMatrix = this.movingPlatform.hitPlatform.localToWorldMatrix;
			this.movingPlatform.newPlatform = true;
		}
		Vector3 vector6;
		vector6..ctor(vector.x, 0f, vector.z);
		this.movement.velocity = (this.tr.position - position) / Time.deltaTime;
		Vector3 vector7;
		vector7..ctor(this.movement.velocity.x, 0f, this.movement.velocity.z);
		if (vector6 == Vector3.zero)
		{
			this.movement.velocity = new Vector3(0f, this.movement.velocity.y, 0f);
		}
		else
		{
			float num2 = Vector3.Dot(vector7, vector6) / vector6.sqrMagnitude;
			this.movement.velocity = vector6 * Mathf.Clamp01(num2) + this.movement.velocity.y * Vector3.up;
		}
		if ((double)this.movement.velocity.y < (double)vector.y - 0.001)
		{
			if (this.movement.velocity.y < 0f)
			{
				this.movement.velocity.y = vector.y;
			}
			else
			{
				this.jumping.holdingJumpButton = false;
			}
		}
		if (this.grounded && !this.IsGroundedTest())
		{
			this.grounded = false;
			if (this.movingPlatform.enabled && (this.movingPlatform.movementTransfer == CharacterMotorCS.MovementTransferOnJump.InitTransfer || this.movingPlatform.movementTransfer == CharacterMotorCS.MovementTransferOnJump.PermaTransfer))
			{
				this.movement.frameVelocity = this.movingPlatform.platformVelocity;
				this.movement.velocity += this.movingPlatform.platformVelocity;
			}
			base.SendMessage("OnFall", 1);
			this.tr.position += num * Vector3.up;
		}
		else if (!this.grounded && this.IsGroundedTest())
		{
			this.grounded = true;
			this.jumping.jumping = false;
			this.SubtractNewPlatformVelocity();
			base.SendMessage("OnLand", 1);
		}
		if (this.MoveWithPlatform())
		{
			this.movingPlatform.activeGlobalPoint = this.tr.position + Vector3.up * (this._controller.center.y - this._controller.height * 0.5f + this._controller.radius);
			this.movingPlatform.activeLocalPoint = this.movingPlatform.activePlatform.InverseTransformPoint(this.movingPlatform.activeGlobalPoint);
			this.movingPlatform.activeGlobalRotation = this.tr.rotation;
			this.movingPlatform.activeLocalRotation = Quaternion.Inverse(this.movingPlatform.activePlatform.rotation) * this.movingPlatform.activeGlobalRotation;
		}
	}

	private void FixedUpdate()
	{
		if (this.movingPlatform.enabled)
		{
			if (this.movingPlatform.activePlatform != null)
			{
				if (!this.movingPlatform.newPlatform)
				{
					this.movingPlatform.platformVelocity = (this.movingPlatform.activePlatform.localToWorldMatrix.MultiplyPoint3x4(this.movingPlatform.activeLocalPoint) - this.movingPlatform.lastMatrix.MultiplyPoint3x4(this.movingPlatform.activeLocalPoint)) / Time.deltaTime;
				}
				this.movingPlatform.lastMatrix = this.movingPlatform.activePlatform.localToWorldMatrix;
				this.movingPlatform.newPlatform = false;
			}
			else
			{
				this.movingPlatform.platformVelocity = Vector3.zero;
			}
		}
		if (this.useFixedUpdate)
		{
			this.UpdateFunction();
		}
	}

	private void Update()
	{
		if (!this.useFixedUpdate)
		{
			this.UpdateFunction();
		}
	}

	private Vector3 ApplyInputVelocityChange(Vector3 velocity)
	{
		if (!this.canControl)
		{
			this.inputMoveDirection = Vector3.zero;
		}
		Vector3 vector2;
		if (this.grounded && this.TooSteep())
		{
			Vector3 vector;
			vector..ctor(this.groundNormal.x, 0f, this.groundNormal.z);
			vector2 = vector.normalized;
			Vector3 vector3 = Vector3.Project(this.inputMoveDirection, vector2);
			vector2 = vector2 + vector3 * this.sliding.speedControl + (this.inputMoveDirection - vector3) * this.sliding.sidewaysControl;
			vector2 *= this.sliding.slidingSpeed;
		}
		else
		{
			vector2 = this.GetDesiredHorizontalVelocity();
		}
		if (this.movingPlatform.enabled && this.movingPlatform.movementTransfer == CharacterMotorCS.MovementTransferOnJump.PermaTransfer)
		{
			vector2 += this.movement.frameVelocity;
			vector2.y = 0f;
		}
		if (this.grounded)
		{
			vector2 = this.AdjustGroundVelocityToNormal(vector2, this.groundNormal);
		}
		else
		{
			velocity.y = 0f;
		}
		float num = this.GetMaxAcceleration(this.grounded) * Time.deltaTime;
		Vector3 vector4 = vector2 - velocity;
		if (vector4.sqrMagnitude > num * num)
		{
			vector4 = vector4.normalized * num;
		}
		if (this.grounded || this.canControl)
		{
			velocity += vector4;
		}
		if (this.grounded)
		{
			velocity.y = Mathf.Min(velocity.y, 0f);
		}
		return velocity;
	}

	private Vector3 ApplyGravityAndJumping(Vector3 velocity)
	{
		if (!this.inputJump || !this.canControl)
		{
			this.jumping.holdingJumpButton = false;
			this.jumping.lastButtonDownTime = -100f;
		}
		if (this.inputJump && this.jumping.lastButtonDownTime < 0f && this.canControl)
		{
			this.jumping.lastButtonDownTime = Time.time;
		}
		if (this.grounded)
		{
			velocity.y = Mathf.Min(0f, velocity.y) - this.movement.gravity * Time.deltaTime;
		}
		else
		{
			velocity.y = this.movement.velocity.y - this.movement.gravity * Time.deltaTime;
			if (this.jumping.jumping && this.jumping.holdingJumpButton && Time.time < this.jumping.lastStartTime + this.jumping.extraHeight / this.CalculateJumpVerticalSpeed(this.jumping.baseHeight))
			{
				velocity += this.jumping.jumpDir * this.movement.gravity * Time.deltaTime;
			}
			velocity.y = Mathf.Max(velocity.y, -this.movement.maxFallSpeed);
		}
		if (this.grounded)
		{
			if (this.jumping.enabled && this.canControl && (double)(Time.time - this.jumping.lastButtonDownTime) < 0.2)
			{
				this.grounded = false;
				this.jumping.jumping = true;
				this.jumping.lastStartTime = Time.time;
				this.jumping.lastButtonDownTime = -100f;
				this.jumping.holdingJumpButton = true;
				if (this.TooSteep())
				{
					this.jumping.jumpDir = Vector3.Slerp(Vector3.up, this.groundNormal, this.jumping.steepPerpAmount);
				}
				else
				{
					this.jumping.jumpDir = Vector3.Slerp(Vector3.up, this.groundNormal, this.jumping.perpAmount);
				}
				velocity.y = 0f;
				velocity += this.jumping.jumpDir * this.CalculateJumpVerticalSpeed(this.jumping.baseHeight);
				if (this.movingPlatform.enabled && (this.movingPlatform.movementTransfer == CharacterMotorCS.MovementTransferOnJump.InitTransfer || this.movingPlatform.movementTransfer == CharacterMotorCS.MovementTransferOnJump.PermaTransfer))
				{
					this.movement.frameVelocity = this.movingPlatform.platformVelocity;
					velocity += this.movingPlatform.platformVelocity;
				}
				base.SendMessage("OnJump", 1);
			}
			else
			{
				this.jumping.holdingJumpButton = false;
			}
		}
		return velocity;
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.normal.y > 0f && hit.normal.y > this.groundNormal.y && hit.moveDirection.y < 0f)
		{
			if ((double)(hit.point - this.movement.lastHitPoint).sqrMagnitude > 0.001 || this.lastGroundNormal == Vector3.zero)
			{
				this.groundNormal = hit.normal;
			}
			else
			{
				this.groundNormal = this.lastGroundNormal;
			}
			this.movingPlatform.hitPlatform = hit.collider.transform;
			this.movement.hitPoint = hit.point;
			this.movement.frameVelocity = Vector3.zero;
		}
	}

	private IEnumerator SubtractNewPlatformVelocity()
	{
		if (this.movingPlatform.enabled && (this.movingPlatform.movementTransfer == CharacterMotorCS.MovementTransferOnJump.InitTransfer || this.movingPlatform.movementTransfer == CharacterMotorCS.MovementTransferOnJump.PermaTransfer))
		{
			if (this.movingPlatform.newPlatform)
			{
				Transform platform = this.movingPlatform.activePlatform;
				yield return new WaitForFixedUpdate();
				yield return new WaitForFixedUpdate();
				if (this.grounded && platform == this.movingPlatform.activePlatform)
				{
					yield break;
				}
			}
			this.movement.velocity -= this.movingPlatform.platformVelocity;
		}
		yield break;
	}

	private bool MoveWithPlatform()
	{
		return this.movingPlatform.enabled && (this.grounded || this.movingPlatform.movementTransfer == CharacterMotorCS.MovementTransferOnJump.PermaLocked) && this.movingPlatform.activePlatform != null;
	}

	private Vector3 GetDesiredHorizontalVelocity()
	{
		Vector3 vector = this.tr.InverseTransformDirection(this.inputMoveDirection);
		float num = ((vector.z <= 0f) ? this.movement.maxBackwardsSpeed : this.movement.maxForwardSpeed);
		Vector3 vector2;
		vector2..ctor(vector.x * this.movement.maxSidewaysSpeed, 0f, vector.z * num);
		return this.tr.TransformDirection(vector2);
	}

	private Vector3 AdjustGroundVelocityToNormal(Vector3 hVelocity, Vector3 groundNormal)
	{
		Vector3 vector = Vector3.Cross(Vector3.up, hVelocity);
		return Vector3.Cross(vector, groundNormal).normalized * hVelocity.magnitude;
	}

	private bool IsGroundedTest()
	{
		return (double)this.groundNormal.y > 0.01;
	}

	private float GetMaxAcceleration(bool grounded)
	{
		if (grounded)
		{
			return this.movement.maxGroundAcceleration;
		}
		return this.movement.maxAirAcceleration;
	}

	private float CalculateJumpVerticalSpeed(float targetJumpHeight)
	{
		return Mathf.Sqrt(2f * targetJumpHeight * this.movement.gravity);
	}

	private bool IsJumping()
	{
		return this.jumping.jumping;
	}

	private bool IsSliding()
	{
		return this.grounded && this.sliding.enabled && this.TooSteep();
	}

	private bool IsTouchingCeiling()
	{
		return (this.movement.collisionFlags & 2) != 0;
	}

	private bool IsGrounded()
	{
		return this.grounded;
	}

	private bool TooSteep()
	{
		return this.groundNormal.y <= Mathf.Cos(this._controller.slopeLimit * 0.017453292f);
	}

	private Vector3 GetDirection()
	{
		return this.inputMoveDirection;
	}

	public void SetControllable(bool controllable)
	{
		this.canControl = controllable;
	}

	private float MaxSpeedInDirection(Vector3 desiredMovementDirection)
	{
		if (desiredMovementDirection == Vector3.zero)
		{
			return 0f;
		}
		float num = ((desiredMovementDirection.z <= 0f) ? this.movement.maxBackwardsSpeed : this.movement.maxForwardSpeed) * this.ForwardSpeedK / (this.movement.maxSidewaysSpeed * this.SidewaysSpeedK);
		Vector3 vector;
		vector..ctor(desiredMovementDirection.x, 0f, desiredMovementDirection.z / num);
		Vector3 normalized = vector.normalized;
		Vector3 vector2;
		vector2..ctor(normalized.x, 0f, normalized.z * num);
		return vector2.magnitude * this.movement.maxSidewaysSpeed * this.SidewaysSpeedK;
	}

	private void SetVelocity(Vector3 velocity)
	{
		this.grounded = false;
		this.movement.velocity = velocity;
		this.movement.frameVelocity = Vector3.zero;
		base.SendMessage("OnExternalVelocity");
	}

	private float ForwardSpeedK
	{
		get
		{
			if (GameFactory.Player != null && GameFactory.Player.IsReeling)
			{
				return GameFactory.Player.PlayerSpeedParameters.ReelingForwardSpeed;
			}
			if (GameFactory.Player != null && this._isCatchingMode)
			{
				return GameFactory.Player.PlayerSpeedParameters.ThrowingForwardSpeed;
			}
			return 1f;
		}
	}

	private float SidewaysSpeedK
	{
		get
		{
			if (GameFactory.Player != null && GameFactory.Player.IsReeling)
			{
				return GameFactory.Player.PlayerSpeedParameters.ReelingSidewaysSpeed;
			}
			if (GameFactory.Player != null && this._isCatchingMode)
			{
				return GameFactory.Player.PlayerSpeedParameters.ThrowingSidewaysSpeed;
			}
			return 1f;
		}
	}

	public void SetCatchingMode(bool flag)
	{
		this._isCatchingMode = flag;
	}

	private void OnDestroy()
	{
		this._controller = null;
		this.tr = null;
	}

	public float THROWING_FORWARD_SPEED_K = 0.5f;

	public float THROWING_SIDEWAYS_SPEED_K = 0.25f;

	public float REELING_FORWARD_SPEED_K = 0.25f;

	public float REELING_SIDEWAYS_SPEED_K = 0.2f;

	private float _forwardSpeedK = 1f;

	private float _sidewaysSpeedK = 1f;

	public bool canControl = true;

	public bool useFixedUpdate = true;

	[NonSerialized]
	public Vector3 inputMoveDirection = Vector3.zero;

	[NonSerialized]
	public bool inputJump;

	public CharacterMotorCS.CharacterMotorMovement movement = new CharacterMotorCS.CharacterMotorMovement();

	public CharacterMotorCS.CharacterMotorJumping jumping = new CharacterMotorCS.CharacterMotorJumping();

	public CharacterMotorCS.CharacterMotorMovingPlatform movingPlatform = new CharacterMotorCS.CharacterMotorMovingPlatform();

	public CharacterMotorCS.CharacterMotorSliding sliding = new CharacterMotorCS.CharacterMotorSliding();

	[NonSerialized]
	public bool grounded = true;

	[NonSerialized]
	public Vector3 groundNormal = Vector3.zero;

	private Vector3 lastGroundNormal = Vector3.zero;

	private Transform tr;

	private CharacterController _controller;

	private bool _isCatchingMode;

	[Serializable]
	public class CharacterMotorMovement
	{
		public float maxForwardSpeed = 3f;

		public float maxSidewaysSpeed = 2f;

		public float maxBackwardsSpeed = 2f;

		public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(-90f, 1f),
			new Keyframe(0f, 1f),
			new Keyframe(90f, 0f)
		});

		public float maxGroundAcceleration = 30f;

		public float maxAirAcceleration = 20f;

		public float gravity = 9.81f;

		public float maxFallSpeed = 20f;

		[NonSerialized]
		public CollisionFlags collisionFlags;

		[NonSerialized]
		public Vector3 velocity;

		[NonSerialized]
		public Vector3 frameVelocity = Vector3.zero;

		[NonSerialized]
		public Vector3 hitPoint = Vector3.zero;

		[NonSerialized]
		public Vector3 lastHitPoint = new Vector3(float.PositiveInfinity, 0f, 0f);
	}

	public enum MovementTransferOnJump
	{
		None,
		InitTransfer,
		PermaTransfer,
		PermaLocked
	}

	[Serializable]
	public class CharacterMotorJumping
	{
		public bool enabled = true;

		public float baseHeight = 1f;

		public float extraHeight = 4.1f;

		public float perpAmount;

		public float steepPerpAmount = 0.5f;

		[NonSerialized]
		public bool jumping;

		[NonSerialized]
		public bool holdingJumpButton;

		[NonSerialized]
		public float lastStartTime;

		[NonSerialized]
		public float lastButtonDownTime = -100f;

		[NonSerialized]
		public Vector3 jumpDir = Vector3.up;
	}

	[Serializable]
	public class CharacterMotorMovingPlatform
	{
		public bool enabled = true;

		public CharacterMotorCS.MovementTransferOnJump movementTransfer = CharacterMotorCS.MovementTransferOnJump.PermaTransfer;

		[NonSerialized]
		public Transform hitPlatform;

		[NonSerialized]
		public Transform activePlatform;

		[NonSerialized]
		public Vector3 activeLocalPoint;

		[NonSerialized]
		public Vector3 activeGlobalPoint;

		[NonSerialized]
		public Quaternion activeLocalRotation;

		[NonSerialized]
		public Quaternion activeGlobalRotation;

		[NonSerialized]
		public Matrix4x4 lastMatrix;

		[NonSerialized]
		public Vector3 platformVelocity;

		[NonSerialized]
		public bool newPlatform;
	}

	[Serializable]
	public class CharacterMotorSliding
	{
		public bool enabled = true;

		public float slidingSpeed = 15f;

		public float sidewaysControl = 1f;

		public float speedControl = 0.4f;
	}
}
