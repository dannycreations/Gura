using System;
using UnityEngine;

public class CameraMouseLookForObjectWithCollider : CameraMouseLook
{
	protected override void Update()
	{
		if (this._isRotationToIdentity)
		{
			if (!Mathf.Approximately(this.rotationY, 0f))
			{
				float num = Mathf.Sign(this.rotationY);
				this.rotationY -= num * Time.deltaTime * this._rotationSpeed;
				if (Mathf.Sign(this.rotationY) != num)
				{
					this.rotationY = 0f;
				}
			}
			if (!Mathf.Approximately(this.rotationX, 0f))
			{
				float num2 = Mathf.Sign(this.rotationX);
				this.rotationX -= num2 * Time.deltaTime * this._rotationSpeed;
				if (Mathf.Sign(this.rotationX) != num2)
				{
					this.rotationX = 0f;
				}
			}
			this.SetRotation(this.originalRotation * base.YawRotation * base.PitchRotation);
			if (Mathf.Approximately(this.rotationY, 0f) && Mathf.Approximately(this.rotationX, 0f))
			{
				this._isRotationToIdentity = false;
				this._callback();
				this._callback = null;
			}
			return;
		}
		if (this.smoothResetAndStopTimeStamp <= 0f)
		{
			float num3 = ((Mathf.Abs(Input.GetAxis("Mouse X")) >= 1E-08f) ? SettingsManager.MouseSensitivity : SettingsManager.ControllerSensitivity);
			if ((this.axes & CameraMouseLook.RotationAxes.MouseX) == CameraMouseLook.RotationAxes.MouseX)
			{
				this.velocity.x = this.velocity.x + ControlsController.ControlsActions.Looks.X * this.sensitivityX * (num3 + 0.5f);
				float rotationX = this.rotationX;
				base.UpdateAxis(ref this.rotationX, ref this.velocity.x, this.minimumX, this.maximumX, num3);
				base.RotationXDelta = this.rotationX - rotationX;
			}
			int num4;
			if (Math.Abs(Input.GetAxis("Mouse Y")) < 1E-08f)
			{
				num4 = ((!SettingsManager.InvertController) ? 1 : (-1));
				num3 = SettingsManager.ControllerSensitivity;
			}
			else
			{
				num4 = ((!SettingsManager.InvertMouse) ? 1 : (-1));
				num3 = SettingsManager.MouseSensitivity;
			}
			if (this._isTargetedPitch)
			{
				if (!Mathf.Approximately(this.rotationY, this._targetPitch))
				{
					base.ClampAxis(ref this.rotationY, ref this.velocity.y, this._targetPitch, num3);
				}
			}
			else if ((this.axes & CameraMouseLook.RotationAxes.MouseY) == CameraMouseLook.RotationAxes.MouseY)
			{
				this.velocity.y = this.velocity.y + ControlsController.ControlsActions.Looks.Y * this.sensitivityY * (float)num4 * (num3 + 0.5f);
				base.UpdateAxis(ref this.rotationY, ref this.velocity.y, this.minimumY, this.maximumY, num3);
			}
			this.velocity *= Mathf.Pow(0.5f, Time.deltaTime * this.DampingMultiplier);
			this.SetRotation(this.originalRotation * base.YawRotation * base.PitchRotation);
		}
		else
		{
			this.SetRotation(Quaternion.Slerp(this.originalRotation, base.transform.localRotation, Mathf.Pow(0.5f, 10f * Time.deltaTime)));
			if (Time.time - this.smoothResetAndStopTimeStamp > 1f)
			{
				this.smoothResetAndStopTimeStamp = -1f;
				base.enabled = false;
			}
		}
	}

	public void SetTargetPitch(float angle)
	{
		this._isTargetedPitch = true;
		this._targetPitch = angle;
	}

	public void ClearTargetPitch()
	{
		this._isTargetedPitch = false;
	}

	public override void ResetOriginalRotation(Quaternion rotation)
	{
		this.SetRotation(rotation);
		this.originalRotation = rotation;
		this.rotationX = 0f;
		this.rotationY = 0f;
	}

	public override void SetNewRotation(float yaw, float pitch)
	{
		Quaternion quaternion = Quaternion.Euler(pitch, yaw, 0f);
		this.SetRotation(quaternion);
		this.originalRotation = Quaternion.identity;
		this.rotationX = Math3d.ClampAngleTo180(yaw);
		this.rotationY = -Math3d.ClampAngleTo180(pitch);
	}

	public void ResetYAxis()
	{
		this.rotationY = 0f;
	}

	protected void SetRotation(Quaternion rotation)
	{
		base.transform.parent.transform.localRotation = Quaternion.Euler(0f, rotation.eulerAngles.y, 0f);
		base.transform.localRotation = Quaternion.Euler(rotation.eulerAngles.x, 0f, rotation.eulerAngles.z);
	}

	public override void SmoothResetAndStop(Quaternion rotation)
	{
		this.originalRotation = rotation;
		this.rotationX = 0f;
		this.rotationY = 0f;
		this.smoothResetAndStopTimeStamp = Time.time;
	}

	public override void SmoothStart()
	{
		this.originalRotation = base.transform.localRotation;
		this.velocity = Vector2.zero;
		this.smoothResetAndStopTimeStamp = -1f;
	}

	public void RotateToIdentity(float speed, Action callback)
	{
		this._isRotationToIdentity = true;
		this._rotationSpeed = speed;
		this._callback = callback;
	}

	private bool _isRotationToIdentity;

	private Action _callback;

	private float _rotationSpeed;
}
