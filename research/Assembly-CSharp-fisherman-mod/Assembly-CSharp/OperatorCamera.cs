using System;
using System.Diagnostics;
using UnityEngine;

public class OperatorCamera : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<float> EMovementSpeedChanged = delegate
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<float> ERotationSpeedChanged = delegate
	{
	};

	public float MovementSpeedK
	{
		get
		{
			return this._positionSpeedK;
		}
	}

	public float RotationSpeedK
	{
		get
		{
			return this._rotationSpeedK;
		}
	}

	public bool Controllable { get; set; }

	public void SetActive(bool flag)
	{
		base.enabled = flag;
		this._yaw = base.transform.rotation.eulerAngles.y;
		this._pitch = base.transform.rotation.eulerAngles.x;
	}

	private void Update()
	{
		if (Input.GetKey(354) || Input.GetKey(358) || Input.GetKey(359))
		{
			return;
		}
		if (Input.GetKeyUp(353) || Input.GetKeyUp(352))
		{
			this.UpdateSpeedModifier("movement speed", (!Input.GetKeyUp(353)) ? (-1) : 1, ref this._positionSpeedK);
			this.EMovementSpeedChanged(this._positionSpeedK);
		}
		if (Input.GetKeyUp(351) || Input.GetKeyUp(350))
		{
			this.UpdateSpeedModifier("rotation speed", (!Input.GetKeyUp(351)) ? (-1) : 1, ref this._rotationSpeedK);
			this.ERotationSpeedChanged(this._rotationSpeedK);
		}
		if (!this.Controllable)
		{
			return;
		}
		float unscaledDeltaTime = Time.unscaledDeltaTime;
		float num = Input.GetAxis("Left Stick Y Axis") * unscaledDeltaTime * this._forwardSpeed * this._positionSpeedK;
		float num2 = Input.GetAxis("Left Stick X Axis") * unscaledDeltaTime * this._sidewaySpeed * this._positionSpeedK;
		int num3 = ((!SettingsManager.InvertController) ? (-1) : 1);
		this._pitch += (float)num3 * Input.GetAxis("Right Stick Y Axis") * unscaledDeltaTime * this._angleSpeed * this._rotationSpeedK;
		this._yaw += Input.GetAxis("Right Stick X Axis") * unscaledDeltaTime * this._angleSpeed * this._rotationSpeedK;
		float num4 = -Input.GetAxis("Triggers") * unscaledDeltaTime * this._vSpeed * this._positionSpeedK;
		Vector3 vector = Math3d.ProjectOXZ(base.transform.forward);
		Vector3 vector2 = Math3d.ProjectOXZ(base.transform.right);
		base.transform.position += vector.normalized * num + vector2 * num2 + new Vector3(0f, num4, 0f);
		base.transform.rotation = Quaternion.Euler(new Vector3(this._pitch, this._yaw));
	}

	private void UpdateSpeedModifier(string modifierName, int dir, ref float current)
	{
		if (dir > 0)
		{
			current = ((!Mathf.Approximately(current, 1f) && current <= 1f) ? (current + 0.1f) : (current + 1f));
		}
		else
		{
			current = ((Mathf.Approximately(current, 1f) || current <= 1f) ? (current - 0.1f) : (current - 1f));
		}
	}

	public void SetPositionAndRotation(Vector3 pos, Quaternion rotation)
	{
		this._pitch = rotation.eulerAngles.x;
		this._yaw = rotation.eulerAngles.y;
		base.transform.rotation = rotation;
		base.transform.position = pos;
	}

	private const KeyCode LEFT_BUMPER = 354;

	private const KeyCode LEFT_STICK_CLICK = 358;

	private const KeyCode RIGHT_STICK_CLICK = 359;

	[SerializeField]
	private float _forwardSpeed = 10f;

	[SerializeField]
	private float _sidewaySpeed = 10f;

	[SerializeField]
	private float _vSpeed = 10f;

	[SerializeField]
	private float _angleSpeed = 45f;

	private float _positionSpeedK = 1f;

	private float _rotationSpeedK = 1f;

	private float _yaw;

	private float _pitch;
}
