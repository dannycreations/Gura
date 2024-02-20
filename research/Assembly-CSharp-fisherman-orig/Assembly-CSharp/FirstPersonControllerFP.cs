using System;
using ObjectModel;
using Phy;
using UnityEngine;
using UnityStandardAssets.Utility;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FirstPersonControllerFP : MonoBehaviour
{
	public bool canControl { get; private set; }

	public FootStepsHelper FootStepsHelper
	{
		get
		{
			return this._footSteps;
		}
	}

	private void Start()
	{
		this.canControl = false;
		this._footSteps = new FootStepsHelper(this._footstepsSounds, this._stepPixelsSize);
		this.m_CharacterController = base.GetComponent<CharacterController>();
		this.m_FovKick.Setup(this.m_Camera);
		this.m_HeadBob.Setup(this.m_Camera);
		this.m_HeadBob.OnFootStep += this.PlayFootStepAudio;
		this.m_StepCycle = 0f;
		this.m_NextStep = this.m_StepCycle / 2f;
		this.m_AudioSource = base.GetComponent<AudioSource>();
	}

	private void OnDestroy()
	{
		this._footSteps.Clean();
		this._footSteps = null;
		this.m_HeadBob.OnFootStep -= this.PlayFootStepAudio;
	}

	public void SetControllable(bool controllable)
	{
		this.canControl = controllable;
		if (this.m_CharacterController != null)
		{
			this.m_CharacterController.enabled = controllable;
		}
	}

	public void SetPlatform(Mass platform, bool enabled)
	{
		this.movingPlatformEnabled = enabled;
		this.MovingPlatform = platform;
	}

	private float ForwardSpeedK
	{
		get
		{
			if (GameFactory.Player != null && (GameFactory.Player.IsReeling || GameFactory.Player.State == typeof(PlayerIdleTarget)))
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
			if (GameFactory.Player != null && (GameFactory.Player.IsReeling || GameFactory.Player.State == typeof(PlayerIdleTarget)))
			{
				return GameFactory.Player.PlayerSpeedParameters.ReelingSidewaysSpeed;
			}
			if (GameFactory.Player != null && this._isCatchingMode)
			{
				return GameFactory.Player.PlayerSpeedParameters.ThrowingSidewaysSpeed;
			}
			return this._strafeK;
		}
	}

	public void SetCatchingMode(bool flag)
	{
		this._isCatchingMode = flag;
	}

	private void Update()
	{
		if (!this.m_CharacterController.enabled || (this.movingPlatformEnabled && this.MovingPlatform != null))
		{
			return;
		}
		float num = 0f;
		if (this.canControl)
		{
			this.GetInput(out num);
		}
		Vector3 vector;
		vector..ctor(this.m_Input.x * num * this.SidewaysSpeedK, 0f, this.m_Input.y * num * this.ForwardSpeedK);
		this.m_MoveDir = base.transform.TransformDirection(vector);
		if (this.m_CharacterController.isGrounded)
		{
			this.m_MoveDir.y = -this.m_StickToGroundForce;
		}
		else
		{
			this.m_MoveDir += Physics.gravity * this.m_GravityMultiplier * Time.deltaTime;
		}
		this.m_CollisionFlags = this.m_CharacterController.Move(this.m_MoveDir * Time.deltaTime);
		if (!this.m_UseHeadBob)
		{
			this.ProgressStepCycle(num);
		}
	}

	private void LateUpdate()
	{
		if (this.m_CharacterController.enabled)
		{
			this.UpdateCameraPosition();
		}
	}

	private void ProgressStepCycle(float speed)
	{
		if (this.m_CharacterController.velocity.sqrMagnitude > 0f && (this.m_Input.x != 0f || this.m_Input.y != 0f))
		{
			this.m_StepCycle += (this.m_CharacterController.velocity.magnitude + speed * ((!this.m_IsWalking) ? this.m_RunstepLenghten : 1f)) * Time.fixedDeltaTime;
		}
		if (this.m_StepCycle <= this.m_NextStep)
		{
			return;
		}
		this.m_NextStep = this.m_StepCycle + this.m_StepInterval;
		if (!this.m_CharacterController.isGrounded)
		{
			this.PlayFootStepAudio();
		}
	}

	private void PlayFootStepAudio()
	{
		this._footSteps.PlayFootStepAudio(base.transform.position, base.transform.position + Vector3.down * this.m_CharacterController.height, this.m_AudioSource);
	}

	private void UpdateCameraPosition()
	{
		if (this.m_UseHeadBob && this.m_CharacterController.isGrounded)
		{
			float num = ((!this.m_IsWalking) ? this.m_RunSpeed : this.m_WalkSpeed) / this.m_WalkSpeed;
			this.m_Camera.transform.localPosition += this.m_HeadBob.DoHeadBob(this.m_CharacterController.velocity.magnitude, num);
		}
	}

	private void GetInput(out float speed)
	{
		float x = ControlsController.ControlsActions.Move.X;
		float y = ControlsController.ControlsActions.Move.Y;
		bool isWalking = this.m_IsWalking;
		this.m_IsWalking = !ControlsController.ControlsActions.RunHotkey.IsPressed || GameFactory.Player.State == typeof(PlayerIdleTarget);
		speed = ((!this.m_IsWalking) ? this.m_RunSpeed : this.m_WalkSpeed);
		this.m_Input = new Vector2(x, y);
		if (this.m_Input.sqrMagnitude > 1f)
		{
			this.m_Input.Normalize();
		}
		if (isWalking != this.m_IsWalking && !StaticUserData.IS_IN_TUTORIAL)
		{
			PhotonConnectionFactory.Instance.ChangeSwitch(GameSwitchType.Sprint, !this.m_IsWalking, null);
		}
		if (this.m_IsWalking != isWalking && this.m_UseFovKick && this.m_CharacterController.velocity.sqrMagnitude > 0f)
		{
			base.StopAllCoroutines();
			base.StartCoroutine(this.m_IsWalking ? this.m_FovKick.FOVKickDown() : this.m_FovKick.FOVKickUp());
		}
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Rigidbody attachedRigidbody = hit.collider.attachedRigidbody;
		if (this.m_CollisionFlags == 4)
		{
			return;
		}
		if (attachedRigidbody == null || attachedRigidbody.isKinematic)
		{
			return;
		}
		attachedRigidbody.AddForceAtPosition(this.m_CharacterController.velocity * 0.1f, hit.point, 1);
	}

	[SerializeField]
	private bool m_IsWalking;

	[SerializeField]
	private float m_WalkSpeed;

	[SerializeField]
	private float m_RunSpeed;

	[SerializeField]
	private float _strafeK = 0.4f;

	[SerializeField]
	[Range(0f, 1f)]
	private float m_RunstepLenghten;

	[SerializeField]
	private float m_StickToGroundForce;

	[SerializeField]
	private float m_GravityMultiplier;

	[SerializeField]
	private bool m_UseFovKick;

	[SerializeField]
	private FOVKick m_FovKick = new FOVKick();

	[SerializeField]
	private bool m_UseHeadBob;

	[SerializeField]
	private CurveControlledBobFP m_HeadBob = new CurveControlledBobFP();

	[SerializeField]
	private FootstepsSounds[] _footstepsSounds;

	[Tooltip("Size in pixels for each step raycasting. You need to restart game when change it to apply!")]
	[SerializeField]
	private int _stepPixelsSize = 5;

	[Tooltip("Please ignore - used by old footsteps system")]
	[SerializeField]
	private float m_StepInterval;

	[SerializeField]
	private Camera m_Camera;

	private float m_YRotation;

	private Vector2 m_Input;

	private Vector3 m_MoveDir = Vector3.zero;

	private CharacterController m_CharacterController;

	private CollisionFlags m_CollisionFlags;

	private bool m_PreviouslyGrounded;

	private float m_StepCycle;

	private float m_NextStep;

	private AudioSource m_AudioSource;

	private bool movingPlatformEnabled;

	private FootStepsHelper _footSteps;

	public Mass MovingPlatform;

	private bool _isCatchingMode;
}
