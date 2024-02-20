using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class OVRPlayerController : MonoBehaviour
{
	private void Awake()
	{
		this.Controller = base.gameObject.GetComponent<CharacterController>();
		if (this.Controller == null)
		{
			Debug.LogWarning("OVRPlayerController: No CharacterController attached.");
		}
		OVRCameraRig[] componentsInChildren = base.gameObject.GetComponentsInChildren<OVRCameraRig>();
		if (componentsInChildren.Length == 0)
		{
			Debug.LogWarning("OVRPlayerController: No OVRCameraRig attached.");
		}
		else if (componentsInChildren.Length > 1)
		{
			Debug.LogWarning("OVRPlayerController: More then 1 OVRCameraRig attached.");
		}
		else
		{
			this.CameraController = componentsInChildren[0];
		}
		this.YRotation = base.transform.rotation.eulerAngles.y;
	}

	protected virtual void Update()
	{
		if (this.useProfileHeight)
		{
			OVRPose? initialPose = this.InitialPose;
			if (initialPose == null)
			{
				this.InitialPose = new OVRPose?(new OVRPose
				{
					position = this.CameraController.transform.localPosition,
					orientation = this.CameraController.transform.localRotation
				});
			}
			Vector3 localPosition = this.CameraController.transform.localPosition;
			localPosition.y = OVRManager.profile.eyeHeight - 0.5f * this.Controller.height;
			localPosition.z = OVRManager.profile.eyeDepth;
			this.CameraController.transform.localPosition = localPosition;
		}
		else
		{
			OVRPose? initialPose2 = this.InitialPose;
			if (initialPose2 != null)
			{
				this.CameraController.transform.localPosition = this.InitialPose.Value.position;
				this.CameraController.transform.localRotation = this.InitialPose.Value.orientation;
				this.InitialPose = null;
			}
		}
		this.UpdateMovement();
		Vector3 vector = Vector3.zero;
		float num = 1f + this.Damping * this.SimulationRate * Time.deltaTime;
		this.MoveThrottle.x = this.MoveThrottle.x / num;
		this.MoveThrottle.y = ((this.MoveThrottle.y <= 0f) ? this.MoveThrottle.y : (this.MoveThrottle.y / num));
		this.MoveThrottle.z = this.MoveThrottle.z / num;
		vector += this.MoveThrottle * this.SimulationRate * Time.deltaTime;
		if (this.Controller.isGrounded && this.FallSpeed <= 0f)
		{
			this.FallSpeed = Physics.gravity.y * (this.GravityModifier * 0.002f);
		}
		else
		{
			this.FallSpeed += Physics.gravity.y * (this.GravityModifier * 0.002f) * this.SimulationRate * Time.deltaTime;
		}
		vector.y += this.FallSpeed * this.SimulationRate * Time.deltaTime;
		if (this.Controller.isGrounded && this.MoveThrottle.y <= 0.001f)
		{
			float stepOffset = this.Controller.stepOffset;
			Vector3 vector2;
			vector2..ctor(vector.x, 0f, vector.z);
			float num2 = Mathf.Max(stepOffset, vector2.magnitude);
			vector -= num2 * Vector3.up;
		}
		Vector3 vector3 = Vector3.Scale(this.Controller.transform.localPosition + vector, new Vector3(1f, 0f, 1f));
		this.Controller.Move(vector);
		Vector3 vector4 = Vector3.Scale(this.Controller.transform.localPosition, new Vector3(1f, 0f, 1f));
		if (vector3 != vector4)
		{
			this.MoveThrottle += (vector4 - vector3) / (this.SimulationRate * Time.deltaTime);
		}
	}

	public virtual void UpdateMovement()
	{
		if (this.HaltUpdateMovement)
		{
			return;
		}
		bool flag = Input.GetKey(119) || Input.GetKey(273);
		bool flag2 = Input.GetKey(97) || Input.GetKey(276);
		bool flag3 = Input.GetKey(100) || Input.GetKey(275);
		bool flag4 = Input.GetKey(115) || Input.GetKey(274);
		bool flag5 = false;
		if (OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Up))
		{
			flag = true;
			flag5 = true;
		}
		if (OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.Down))
		{
			flag4 = true;
			flag5 = true;
		}
		this.MoveScale = 1f;
		if ((flag && flag2) || (flag && flag3) || (flag4 && flag2) || (flag4 && flag3))
		{
			this.MoveScale = 0.70710677f;
		}
		if (!this.Controller.isGrounded)
		{
			this.MoveScale = 0f;
		}
		this.MoveScale *= this.SimulationRate * Time.deltaTime;
		float num = this.Acceleration * 0.1f * this.MoveScale * this.MoveScaleMultiplier;
		if (flag5 || Input.GetKey(304) || Input.GetKey(303))
		{
			num *= 2f;
		}
		Transform transform = ((!this.HmdRotatesY) ? base.transform : this.CameraController.centerEyeAnchor);
		if (flag)
		{
			this.MoveThrottle += transform.TransformDirection(Vector3.forward * num * base.transform.lossyScale.z);
		}
		if (flag4)
		{
			this.MoveThrottle += transform.TransformDirection(Vector3.back * num * base.transform.lossyScale.z) * this.BackAndSideDampen;
		}
		if (flag2)
		{
			this.MoveThrottle += transform.TransformDirection(Vector3.left * num * base.transform.lossyScale.x) * this.BackAndSideDampen;
		}
		if (flag3)
		{
			this.MoveThrottle += transform.TransformDirection(Vector3.right * num * base.transform.lossyScale.x) * this.BackAndSideDampen;
		}
		bool flag6 = OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.LeftShoulder);
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		if (flag6 && !this.prevHatLeft)
		{
			eulerAngles.y -= this.RotationRatchet;
		}
		this.prevHatLeft = flag6;
		bool flag7 = OVRGamepadController.GPC_GetButton(OVRGamepadController.Button.RightShoulder);
		if (flag7 && !this.prevHatRight)
		{
			eulerAngles.y += this.RotationRatchet;
		}
		this.prevHatRight = flag7;
		if (Input.GetKeyDown(113))
		{
			eulerAngles.y -= this.RotationRatchet;
		}
		if (Input.GetKeyDown(101))
		{
			eulerAngles.y += this.RotationRatchet;
		}
		float num2 = this.SimulationRate * Time.deltaTime * this.RotationAmount * this.RotationScaleMultiplier;
		if (!this.SkipMouseRotation)
		{
			eulerAngles.y += Input.GetAxis("Mouse X") * num2 * 3.25f;
		}
		num = this.SimulationRate * Time.deltaTime * this.Acceleration * 0.1f * this.MoveScale * this.MoveScaleMultiplier;
		num *= 1f + OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftTrigger);
		float num3 = OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftXAxis);
		float num4 = OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.LeftYAxis);
		if (num4 > 0f)
		{
			this.MoveThrottle += num4 * transform.TransformDirection(Vector3.forward * num);
		}
		if (num4 < 0f)
		{
			this.MoveThrottle += Mathf.Abs(num4) * transform.TransformDirection(Vector3.back * num) * this.BackAndSideDampen;
		}
		if (num3 < 0f)
		{
			this.MoveThrottle += Mathf.Abs(num3) * transform.TransformDirection(Vector3.left * num) * this.BackAndSideDampen;
		}
		if (num3 > 0f)
		{
			this.MoveThrottle += num3 * transform.TransformDirection(Vector3.right * num) * this.BackAndSideDampen;
		}
		float num5 = OVRGamepadController.GPC_GetAxis(OVRGamepadController.Axis.RightXAxis);
		eulerAngles.y += num5 * num2;
		base.transform.rotation = Quaternion.Euler(eulerAngles);
	}

	public bool Jump()
	{
		if (!this.Controller.isGrounded)
		{
			return false;
		}
		this.MoveThrottle += new Vector3(0f, this.JumpForce, 0f);
		return true;
	}

	public void Stop()
	{
		this.Controller.Move(Vector3.zero);
		this.MoveThrottle = Vector3.zero;
		this.FallSpeed = 0f;
	}

	public void GetMoveScaleMultiplier(ref float moveScaleMultiplier)
	{
		moveScaleMultiplier = this.MoveScaleMultiplier;
	}

	public void SetMoveScaleMultiplier(float moveScaleMultiplier)
	{
		this.MoveScaleMultiplier = moveScaleMultiplier;
	}

	public void GetRotationScaleMultiplier(ref float rotationScaleMultiplier)
	{
		rotationScaleMultiplier = this.RotationScaleMultiplier;
	}

	public void SetRotationScaleMultiplier(float rotationScaleMultiplier)
	{
		this.RotationScaleMultiplier = rotationScaleMultiplier;
	}

	public void GetSkipMouseRotation(ref bool skipMouseRotation)
	{
		skipMouseRotation = this.SkipMouseRotation;
	}

	public void SetSkipMouseRotation(bool skipMouseRotation)
	{
		this.SkipMouseRotation = skipMouseRotation;
	}

	public void GetHaltUpdateMovement(ref bool haltUpdateMovement)
	{
		haltUpdateMovement = this.HaltUpdateMovement;
	}

	public void SetHaltUpdateMovement(bool haltUpdateMovement)
	{
		this.HaltUpdateMovement = haltUpdateMovement;
	}

	public void ResetOrientation()
	{
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		eulerAngles.y = this.YRotation;
		base.transform.rotation = Quaternion.Euler(eulerAngles);
	}

	public float Acceleration = 0.1f;

	public float Damping = 0.3f;

	public float BackAndSideDampen = 0.5f;

	public float JumpForce = 0.3f;

	public float RotationAmount = 1.5f;

	public float RotationRatchet = 45f;

	private float YRotation;

	public bool HmdRotatesY = true;

	public float GravityModifier = 0.379f;

	private float MoveScale = 1f;

	private Vector3 MoveThrottle = Vector3.zero;

	private float FallSpeed;

	private OVRPose? InitialPose;

	public bool useProfileHeight = true;

	protected CharacterController Controller;

	protected OVRCameraRig CameraController;

	private float MoveScaleMultiplier = 1f;

	private float RotationScaleMultiplier = 1f;

	private bool SkipMouseRotation;

	private bool HaltUpdateMovement;

	private bool prevHatLeft;

	private bool prevHatRight;

	private float SimulationRate = 60f;
}
