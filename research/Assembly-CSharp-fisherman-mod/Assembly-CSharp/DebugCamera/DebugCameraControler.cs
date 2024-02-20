using System;
using UnityEngine;

namespace DebugCamera
{
	[RequireComponent(typeof(Camera))]
	public class DebugCameraControler : MonoBehaviour
	{
		public Vector2 PanoramaPrc
		{
			get
			{
				return this._panoramaPrc;
			}
		}

		protected virtual void Awake()
		{
			this._camera = base.GetComponent<Camera>();
		}

		protected virtual void Start()
		{
			this.ForceUpdatePosition();
		}

		public void SetActive(bool flag, Transform connectedTo = null, float initialYaw = 0f, bool saveCameraPosition = false)
		{
			this._connectedTo = connectedTo;
			base.enabled = flag;
			if (flag)
			{
				if (saveCameraPosition)
				{
					this._cameraSavedLocalPosition = new Vector3?(this._camera.transform.localPosition);
				}
				this._curYaw = initialYaw;
				this._curPitch = 0f;
				this._panoramaPrc = Vector2.zero;
				this.ForceUpdatePosition();
			}
			else
			{
				this._camera.transform.localRotation = Quaternion.identity;
				Transform transform = this._camera.transform;
				Vector3? cameraSavedLocalPosition = this._cameraSavedLocalPosition;
				transform.localPosition = ((cameraSavedLocalPosition == null) ? Vector3.zero : cameraSavedLocalPosition.Value);
				this._cameraSavedLocalPosition = null;
			}
		}

		public void Disable()
		{
			base.enabled = false;
		}

		public void Restore(Transform connectedTo)
		{
			this.SetActive(true, connectedTo, this._curYaw, false);
		}

		public void StartScenario(int index)
		{
			if (index < 0 || index >= this._scenarios.Length)
			{
				LogHelper.Error("Invalid scenario index {0}", new object[] { index });
			}
			if (!base.gameObject.activeInHierarchy)
			{
				LogHelper.Error("You need to enable camera first", new object[0]);
			}
			this._scenarioController = new ScenarioController(this._scenarios[index], index);
			this._scenarioController.EFinished += this.OnScenarioFinished;
		}

		private void OnScenarioFinished()
		{
			LogHelper.Log("Scenario {0} finished", new object[] { this._scenarioController.ScenarioIndex });
			this._curYaw = this._scenarioController.CurYaw;
			this._curPitch = this._scenarioController.CurPitch;
			this.ForceUpdatePosition();
			this._scenarioController = null;
		}

		private void LateUpdate()
		{
			if (this._scenarioController != null)
			{
				this._scenarioController.Update();
				if (this._scenarioController != null)
				{
					this._curYaw = this._scenarioController.CurYaw;
					this._curPitch = this._scenarioController.CurPitch;
					this.ForceUpdatePosition();
				}
			}
			else
			{
				this.UpdateInput();
				if (this._wasUserInput)
				{
					this.UpdatePrecalculatedPosition(true);
				}
				else
				{
					this.GetNewPosition(this._curYaw, this._curPitch, this._curDist, this._panoramaPrc);
					this.UpdatePrecalculatedPosition(false);
				}
			}
		}

		protected virtual void UpdateInput()
		{
			this._wasUserInput = false;
			if (this._isControlKeyEnabled && !Input.GetKey(this._controlKey))
			{
				return;
			}
			float num = ControlsController.ControlsActions.PhotoModeLooks.X * this._mouseSensitivity;
			float num2 = ControlsController.ControlsActions.PhotoModeLooks.Y * this._mouseSensitivity;
			if (!Mathf.Approximately(num, 0f) || !Mathf.Approximately(num2, 0f))
			{
				if (Input.GetMouseButton(1))
				{
					this.ChangeYaw(num);
					this.ChangePitch(-num2);
				}
				else if (Input.GetMouseButton(2))
				{
					this.UpdateXPanorama(num);
					this.UpdateYPanorama(num2 * 3f);
				}
			}
			if (ControlsController.ControlsActions.PHMResetUI.WasReleased)
			{
				this._panoramaPrc = Vector2.zero;
				this.ForceUpdatePosition();
			}
			if (ControlsController.ControlsActions.PHMOffsetUI.Y > 0f)
			{
				this.UpdateYPanorama(1f);
			}
			else if (ControlsController.ControlsActions.PHMOffsetUI.Y < 0f)
			{
				this.UpdateYPanorama(-1f);
			}
			if (ControlsController.ControlsActions.PHMOffsetUI.X > 0f)
			{
				this.UpdateXPanorama(1f);
			}
			else if (ControlsController.ControlsActions.PHMOffsetUI.X < 0f)
			{
				this.UpdateXPanorama(-1f);
			}
			if (ControlsController.ControlsActions.PHMOrbitUI.X < 0f)
			{
				this.ChangeYaw(1f);
			}
			else if (ControlsController.ControlsActions.PHMOrbitUI.X > 0f)
			{
				this.ChangeYaw(-1f);
			}
			if (ControlsController.ControlsActions.PHMOrbitUI.Y > 0f)
			{
				this.ChangePitch(1f);
			}
			else if (ControlsController.ControlsActions.PHMOrbitUI.Y < 0f)
			{
				this.ChangePitch(-1f);
			}
			if (ControlsController.ControlsActions.PHMDollyZoomInUI.IsPressedMandatory)
			{
				this.ChangeDist(-1f);
			}
			else if (ControlsController.ControlsActions.PHMDollyZoomOutUI.IsPressedMandatory)
			{
				this.ChangeDist(1f);
			}
			if (ControlsController.ControlsActions.PHMDollyZoomInByScrollUI.IsPressedMandatory)
			{
				this.ChangeDist(-this._distScrollSpeedK);
			}
			else if (ControlsController.ControlsActions.PHMDollyZoomOutByScrollUI.IsPressedMandatory)
			{
				this.ChangeDist(this._distScrollSpeedK);
			}
			this.UpdatePrecalculatedPosition(this._wasUserInput);
		}

		private void UpdateXPanorama(float sign)
		{
			this._wasUserInput = true;
			Vector2 vector;
			vector..ctor(Mathf.Clamp(this._panoramaPrc.x + sign * Time.unscaledDeltaTime * this._panoramaSpeed, -1f, 1f), this._panoramaPrc.y);
			if (this.GetNewPosition(this._curYaw, this._curPitch, this._curDist, vector))
			{
				this._panoramaPrc = vector;
			}
		}

		private void UpdateYPanorama(float sign)
		{
			this._wasUserInput = true;
			Vector2 vector;
			vector..ctor(this._panoramaPrc.x, Mathf.Clamp(this._panoramaPrc.y + sign * Time.unscaledDeltaTime * this._panoramaSpeed, -1f, 1f));
			if (this.GetNewPosition(this._curYaw, this._curPitch, this._curDist, vector))
			{
				this._panoramaPrc = vector;
			}
		}

		private void ChangeYaw(float magnitude)
		{
			this._wasUserInput = true;
			float num = this._curYaw + Mathf.Clamp(magnitude, -1f, 1f) * Time.unscaledDeltaTime * this._yawSpeed;
			if (this.GetNewPosition(num, this._curPitch, this._curDist, this._panoramaPrc))
			{
				this._curYaw = num;
			}
			else
			{
				this.ChangePitch(this._pitchSpeedWhenBlockedYaw);
			}
		}

		private void ChangePitch(float magnitude)
		{
			this._wasUserInput = true;
			float num = Math3d.ClampAngle(this._curPitch + Mathf.Clamp(magnitude, -1f, 1f) * Time.unscaledDeltaTime * this._pitchSpeed, this._minPitch, this._maxPitch);
			if (this.GetNewPosition(this._curYaw, num, this._curDist, this._panoramaPrc))
			{
				this._curPitch = num;
			}
		}

		private void ChangeDist(float magnitude)
		{
			this._wasUserInput = true;
			float num = Mathf.Clamp(this._curDist + magnitude * Time.unscaledDeltaTime * this._distSpeed, this._minDist, this._maxDist);
			if (this.GetNewPosition(this._curYaw, this._curPitch, num, this._panoramaPrc))
			{
				this._curDist = num;
			}
		}

		private void UpdatePrecalculatedPosition(bool isUserInput)
		{
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, this._helper.rotation, (!isUserInput) ? Mathf.Clamp01(Time.unscaledDeltaTime * this._followSpeed) : 1f);
			base.transform.position = Vector3.Lerp(base.transform.position, this._helper.position, (!isUserInput) ? Mathf.Clamp01(Time.unscaledDeltaTime * this._followSpeed) : 1f);
		}

		public void ForceUpdatePosition()
		{
			if (this._helper == null)
			{
				this._helper = new GameObject("LinkedCameraHelper").transform;
			}
			if (!this.GetNewPosition(this._curYaw, this._curPitch, this._curDist, this._panoramaPrc))
			{
				this._curYaw = 0f;
				this._curPitch = this._maxPitch;
				this._curDist = this._minDist;
				this._panoramaPrc = default(Vector2);
				this.GetNewPosition(this._curYaw, this._curPitch, this._curDist, this._panoramaPrc);
			}
			this.UpdatePrecalculatedPosition(true);
		}

		private bool GetNewPosition(float yaw, float pitch, float dist, Vector2 panorama)
		{
			if (this._connectedTo == null)
			{
				return false;
			}
			float num = dist;
			float num2 = dist * Mathf.Sin(pitch * 0.017453292f);
			if (num2 < this._minLocalY)
			{
				num = this._minLocalY / Mathf.Sin(pitch * 0.017453292f);
				num2 = num * Mathf.Sin(pitch * 0.017453292f);
			}
			else if (num2 > this._maxLocalY)
			{
				num = this._maxLocalY / Mathf.Sin(pitch * 0.017453292f);
				num2 = num * Mathf.Sin(pitch * 0.017453292f);
			}
			float num3 = this._curDist * Mathf.Cos(pitch * 0.017453292f);
			float num4 = num3 * Mathf.Sin(yaw * 0.017453292f);
			float num5 = num3 * Mathf.Cos(yaw * 0.017453292f);
			this._helper.position = this._connectedTo.TransformPoint(new Vector3(num4, num2, num5));
			this._helper.LookAt(this._connectedTo);
			float num6 = 1f - Mathf.Clamp01((num - this._maxDist) / (this._minDist - this._maxDist));
			float num7 = (this._minXZPanorama * (1f - num6) + this._maxXZPanorama * num6) * panorama.x;
			float num8 = (this._minYPanorama * (1f - num6) + this._maxYPanorama * num6) * panorama.y;
			this._helper.position += num7 * this._helper.right + num8 * this._helper.up;
			if (this._helper.position.y < 0.1f)
			{
				this._helper.position = new Vector3(this._helper.position.x, 0.1f, this._helper.position.z);
				return false;
			}
			Vector3? cameraRayContactPoint = this.GetCameraRayContactPoint(this._helper.position);
			if (cameraRayContactPoint != null)
			{
				this._helper.position = cameraRayContactPoint.Value;
				return false;
			}
			return true;
		}

		private Vector3? GetCameraRayContactPoint(Vector3 endPosition)
		{
			return Math3d.GetMaskedCylinderContactPoint(this._connectedTo.position, endPosition, this._terrainColliderDist, GlobalConsts.TerrainMask | GlobalConsts.WaterMask | (1 << GlobalConsts.PhotoModeLayer));
		}

		private const float WATER_HEIGHT = 0.1f;

		[SerializeField]
		protected Transform _connectedTo;

		[SerializeField]
		protected bool _isControlKeyEnabled;

		[SerializeField]
		protected KeyCode _controlKey = 306;

		[SerializeField]
		protected float _minDist = 1f;

		[SerializeField]
		protected float _minXZPanorama = 1f;

		[SerializeField]
		protected float _minYPanorama = 0.3f;

		[SerializeField]
		protected float _maxDist = 5f;

		[SerializeField]
		protected float _maxXZPanorama = 5f;

		[SerializeField]
		protected float _maxYPanorama = 1f;

		[SerializeField]
		protected float _curDist = 3f;

		[SerializeField]
		protected float _distSpeed = 2f;

		[Tooltip("Multiplier for DistSpeed used for mouse scroll only!")]
		[SerializeField]
		protected float _distScrollSpeedK = 3f;

		[SerializeField]
		protected float _yawSpeed = 180f;

		[SerializeField]
		protected float _pitchSpeed = 180f;

		[SerializeField]
		protected float _pitchSpeedWhenBlockedYaw = 0.5f;

		[SerializeField]
		protected float _mouseRollK = 100f;

		[SerializeField]
		protected float _mouseSensitivity = 4f;

		[SerializeField]
		protected float _panoramaSpeed = 1f;

		[SerializeField]
		protected float _minPitch = -30f;

		[SerializeField]
		protected float _maxPitch = 30f;

		[SerializeField]
		protected float _minLocalY = -1f;

		[SerializeField]
		protected float _maxLocalY = 2f;

		[SerializeField]
		protected float _terrainColliderDist = 0.01f;

		[SerializeField]
		protected float _followSpeed = 2f;

		[SerializeField]
		protected ScenarioSettings[] _scenarios;

		protected Camera _camera;

		protected float _curPitch;

		protected float _curYaw;

		protected Vector2 _panoramaPrc;

		private ScenarioController _scenarioController;

		private Transform _helper;

		private Vector3? _cameraSavedLocalPosition;

		private bool _wasUserInput;
	}
}
