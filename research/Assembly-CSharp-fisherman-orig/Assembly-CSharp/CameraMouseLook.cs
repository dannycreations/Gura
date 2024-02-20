using System;
using Boats;
using UnityEngine;

[AddComponentMenu("Camera-Control/Camera Mouse Look")]
public class CameraMouseLook : MonoBehaviour
{
	public float RotationXDelta { get; protected set; }

	public float Yaw
	{
		get
		{
			return this.rotationX;
		}
	}

	public float Pitch
	{
		get
		{
			return -this.rotationY;
		}
	}

	protected virtual void Awake()
	{
		this.smoothResetAndStopTimeStamp = -1f;
		if (base.GetComponent<Rigidbody>())
		{
			base.GetComponent<Rigidbody>().freezeRotation = true;
		}
		this.originalSensitivityX = this.sensitivityX;
		this.originalSensitivityY = this.sensitivityY;
	}

	private float AdjustAngle(float angle, float sensitivity)
	{
		if (!Mathf.Approximately(angle, 0f))
		{
			float num = Mathf.Sign(angle);
			angle += -num * sensitivity * Time.deltaTime;
			if (num != Mathf.Sign(angle))
			{
				angle = 0f;
			}
			return angle;
		}
		return 0f;
	}

	protected virtual void Update()
	{
		if (this.smoothResetAndStopTimeStamp <= 0f)
		{
			Quaternion quaternion = Quaternion.identity;
			Quaternion quaternion2 = Quaternion.identity;
			float num = ((Mathf.Abs(Input.GetAxis("Mouse X")) >= 1E-08f) ? SettingsManager.MouseSensitivity : SettingsManager.ControllerSensitivity);
			if ((this.axes & CameraMouseLook.RotationAxes.MouseX) == CameraMouseLook.RotationAxes.MouseX)
			{
				this.velocity.x = this.velocity.x + ControlsController.ControlsActions.Looks.X * this.sensitivityX * (num + 0.5f);
				float num2 = this.rotationX;
				this.UpdateAxis(ref this.rotationX, ref this.velocity.x, this.minimumX, this.maximumX, num);
				this.RotationXDelta = this.rotationX - num2;
				this._yawToAdjust = this.AdjustAngle(this._yawToAdjust, this.sensitivityAdjustmentX);
				quaternion = Quaternion.AngleAxis(this.rotationX + this._yawToAdjust, Vector3.up);
			}
			int num3;
			if (Mathf.Abs(Input.GetAxis("Mouse Y")) < 1E-08f)
			{
				num3 = ((!SettingsManager.InvertController) ? 1 : (-1));
				num = SettingsManager.ControllerSensitivity * 2f;
			}
			else
			{
				num3 = ((!SettingsManager.InvertMouse) ? 1 : (-1));
				num = SettingsManager.MouseSensitivity;
			}
			if ((this.axes & CameraMouseLook.RotationAxes.MouseY) == CameraMouseLook.RotationAxes.MouseY)
			{
				this.velocity.y = this.velocity.y + ControlsController.ControlsActions.Looks.Y * this.sensitivityY * (float)num3 * (num + 0.5f);
				this.UpdateAxis(ref this.rotationY, ref this.velocity.y, this.minimumY, this.maximumY, num);
				this._pitchToAdjust = this.AdjustAngle(this._pitchToAdjust, this.sensitivityAdjustmentY);
				quaternion2 = Quaternion.AngleAxis(this.rotationY + this._pitchToAdjust, Vector3.left);
			}
			this.velocity *= Mathf.Pow(0.5f, Time.deltaTime * this.DampingMultiplier);
			base.transform.localRotation = this.originalRotation * quaternion * quaternion2;
		}
		if (this.smoothResetAndStopTimeStamp > 0f)
		{
			base.transform.localRotation = Quaternion.Slerp(this.startTransitionRotation, this.originalRotation, Mathf.Clamp01(Mathf.SmoothStep(0f, 1f, Time.time - this.smoothResetAndStopTimeStamp)));
			if (Time.time - this.smoothResetAndStopTimeStamp > 1f)
			{
				this.smoothResetAndStopTimeStamp = -1f;
				base.enabled = false;
			}
		}
	}

	protected Quaternion YawRotation
	{
		get
		{
			return Quaternion.AngleAxis(this.rotationX, Vector3.up);
		}
	}

	protected Quaternion PitchRotation
	{
		get
		{
			return Quaternion.AngleAxis(this.rotationY, Vector3.left);
		}
	}

	protected void UpdateAxis(ref float angle, ref float velocity, float minAngle, float maxAngle, float sensitivity)
	{
		float num = angle;
		angle = Math3d.ClampAngleTo360(angle + velocity * Time.deltaTime);
		if (num >= minAngle && num <= maxAngle)
		{
			angle = Mathf.Clamp(angle, minAngle, maxAngle);
		}
		else if (angle < minAngle)
		{
			if (velocity < 0f)
			{
				velocity = 0f;
			}
			this.ClampAxis(ref angle, ref velocity, minAngle, sensitivity);
		}
		else if (angle > maxAngle)
		{
			if (velocity > 0f)
			{
				velocity = 0f;
			}
			this.ClampAxis(ref angle, ref velocity, maxAngle, sensitivity);
		}
	}

	protected void ClampAxis(ref float angle, ref float velocity, float targetAngle, float sensitivity)
	{
		float num = Mathf.Sign(targetAngle - angle);
		velocity += num * (sensitivity + 0.5f) * 2f;
		angle += velocity * Time.deltaTime;
		if (Mathf.Sign(targetAngle - angle) != num)
		{
			angle = targetAngle;
		}
	}

	public void AdaptCurrentRotation()
	{
		Vector3 eulerAngles = (Quaternion.Inverse(this.originalRotation) * base.transform.localRotation).eulerAngles;
		this.rotationX = Math3d.ClampAngleTo180(eulerAngles.y);
		this.rotationY = -eulerAngles.x;
	}

	public void EnableFolowBoneController(Transform rootBone)
	{
		base.enabled = false;
		this._followBoneController.Activate(rootBone);
	}

	public void DisableFollowBoneController()
	{
		base.enabled = true;
		this._followBoneController.Deactivate();
		this._followBoneController.transform.localRotation = Quaternion.identity;
	}

	public void ResetSensitivity()
	{
		this.sensitivityX = this.originalSensitivityX;
		this.sensitivityY = this.originalSensitivityY;
	}

	public virtual void SetNewRotation(float yaw, float pitch)
	{
		base.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
		this.originalRotation = Quaternion.identity;
		this.rotationX = Math3d.ClampAngleTo180(yaw);
		this.rotationY = -Math3d.ClampAngleTo180(pitch);
	}

	public virtual void ResetOriginalRotation(Quaternion localRotation)
	{
		base.transform.localRotation = localRotation;
		this.originalRotation = localRotation;
		this.rotationX = 0f;
		this.rotationY = 0f;
		this.velocity = Vector2.zero;
	}

	public virtual void SmoothResetAndStop(Quaternion localRotation)
	{
		this.originalRotation = localRotation;
		this.startTransitionRotation = base.transform.localRotation;
		this.rotationX = 0f;
		this.rotationY = 0f;
		this.smoothResetAndStopTimeStamp = Time.time;
	}

	public virtual void SmoothStart()
	{
		this.originalRotation = base.transform.localRotation;
		this.velocity = Vector2.zero;
		this.smoothResetAndStopTimeStamp = -1f;
	}

	public CameraMouseLook.RotationAxes axes = CameraMouseLook.RotationAxes.MouseXAndY;

	public float sensitivityX = 50f;

	public float sensitivityY = 50f;

	public float sensitivityAdjustmentX = 150f;

	public float sensitivityAdjustmentY = 150f;

	public float DampingMultiplier = 10f;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -60f;

	public float maximumY = 60f;

	[SerializeField]
	protected FollowBoneMouseController _followBoneController;

	protected float rotationX;

	protected float rotationY;

	protected Quaternion originalRotation;

	protected Vector2 velocity = Vector2.zero;

	protected float originalSensitivityX;

	protected float originalSensitivityY;

	protected float smoothResetAndStopTimeStamp = -1f;

	protected Quaternion startTransitionRotation;

	protected float _yawToAdjust;

	protected float _pitchToAdjust;

	protected bool _isTargetedPitch;

	protected float _targetPitch;

	[Flags]
	public enum RotationAxes
	{
		None = 0,
		MouseX = 1,
		MouseY = 2,
		MouseXAndY = 3
	}
}
