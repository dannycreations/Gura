using System;
using InControl;
using UnityEngine;

namespace Assets.Scripts.UI._2D.GlobalMap
{
	public class GlobeInputController
	{
		public float RotEarthSpeedAroundY
		{
			get
			{
				return this._speed.x;
			}
		}

		public Vector3 CamPos0
		{
			get
			{
				return this._camPos0;
			}
		}

		public void Rotation2Pond(Quaternion from, Quaternion to, float rotationTime)
		{
			this._from = from;
			this._to = to;
			this._rotationTime = rotationTime;
			this._actionTime = 0f;
		}

		public void SetEnable(bool flag)
		{
			this._isEnabled = flag;
		}

		public void Init0(Transform tCamera)
		{
			this._camRot0 = tCamera.rotation;
			this._camPos0 = tCamera.position;
		}

		public void Init(Transform tEarth, Transform tCamera, Camera camera, Transform tChangeHeightByZoom)
		{
			this._camera = camera;
			this._tEarth = tEarth;
			this._tCamera = tCamera;
			this._tChangeHeightByZoom = tChangeHeightByZoom;
			this._tChangeHeightByZoomY0 = tChangeHeightByZoom.localPosition.y;
		}

		public void Update(Vector2 speedRotateEarth, bool isLockEarthRotX, float maxZoom, float minZoom, float zoomSpeed, float cameraAngleMax, float zoomSpeedForStop)
		{
			if (!this._isEnabled || this._tEarth == null)
			{
				return;
			}
			this._zoomSpeedForStop = zoomSpeedForStop;
			this._maxZoom = maxZoom;
			this._minZoom = minZoom;
			this._zoomSpeed = zoomSpeed;
			this._cameraAngleMax = cameraAngleMax;
			bool isMouseActive = this.IsMouseActive;
			if (isMouseActive)
			{
				float axis = Input.GetAxis("Mouse X");
				float axis2 = Input.GetAxis("Mouse Y");
				Vector3 vector;
				vector..ctor(-axis * speedRotateEarth.x, axis2 * speedRotateEarth.y, 0f);
				this._newAngle = -this.GetVectorFromMouse(Time.deltaTime, true).y * speedRotateEarth.y;
				this._speed = vector;
				this._newZoom = 0f;
				this._camZoomZeroTime = 0f;
				if (Mathf.Abs(axis2) > Mathf.Abs(axis))
				{
					this._speed /= 10f;
				}
			}
			else
			{
				this._speed = Vector3.Lerp(this._speed, Vector3.zero, Time.deltaTime * 1f);
				this._newAngle = Mathf.Lerp(this._newAngle, 0f, Time.deltaTime * 1f);
			}
			Vector3 vector2 = this._tEarth.position - this._camPos0;
			Vector3 vector3 = this._tEarth.position - this._tCamera.position;
			float num = Vector3.Angle(vector2, vector3);
			bool flag = num + Mathf.Abs(this._newAngle) < this._cameraAngleMax || ((this._newAngle >= 0f || this._tCamera.rotation.x >= 0f) && (this._newAngle <= 0f || this._tCamera.rotation.x <= 0f));
			if (flag && isLockEarthRotX)
			{
				this._tCamera.RotateAround(this._tEarth.position, Vector3.right, this._newAngle);
			}
			this._tEarth.Rotate((!isLockEarthRotX) ? this._speed.y : 0f, this._speed.x, 0f, 1);
			this.Rotate(isMouseActive);
			float num2 = 0f;
			if (!UIHelper.IsMouse)
			{
				InputControl inputControl = InputManager.ActiveDevice.GetControl(InputControlType.RightStickUp);
				if (inputControl.IsPressed)
				{
					num2 = inputControl.Value;
				}
				else
				{
					inputControl = InputManager.ActiveDevice.GetControl(InputControlType.RightStickDown);
					if (inputControl.IsPressed)
					{
						num2 = -inputControl.Value;
					}
				}
			}
			if (UIHelper.IsMouse)
			{
				num2 = Input.GetAxis("Mouse ScrollWheel");
			}
			this.UpdateCameraZoom(num2);
		}

		public void ResetCamera()
		{
			this._mx = (this._my = 0f);
			this._camera.transform.position = this._camPos0;
			this._camera.transform.rotation = this._camRot0;
			this._camZoomZeroTime = (this._newZoom = (this._newAngle = 0f));
			this._speed = Vector3.zero;
			if (this._tChangeHeightByZoom != null)
			{
				this._tChangeHeightByZoom.localPosition = new Vector3(this._tChangeHeightByZoom.localPosition.x, this._tChangeHeightByZoomY0, this._tChangeHeightByZoom.localPosition.z);
			}
		}

		public void UpdateHeightByZoom()
		{
			if (this._tChangeHeightByZoom != null)
			{
				float num = 50f;
				float num2 = Mathf.Abs(this._maxZoom);
				float num3 = num2 - num;
				float num4 = (this._camera.fieldOfView - num) / num3;
				float num5 = -1242.44f - this._tChangeHeightByZoomY0;
				float num6 = num4 * num5 + this._tChangeHeightByZoomY0;
				this._tChangeHeightByZoom.localPosition = new Vector3(this._tChangeHeightByZoom.localPosition.x, num6, this._tChangeHeightByZoom.localPosition.z);
			}
		}

		private void UpdateCameraZoom(float zoom)
		{
			zoom *= this._zoomSpeed;
			if (zoom == 0f)
			{
				this._camZoomZeroTime += Time.deltaTime;
				if (this._camZoomZeroTime > 0.15f)
				{
					this._newZoom = Mathf.Lerp(this._newZoom, 0f, Time.deltaTime * this._zoomSpeedForStop);
				}
			}
			else
			{
				this._camZoomZeroTime = 0f;
				if ((zoom > 0f && this._newZoom < 0f) || (zoom < 0f && this._newZoom > 0f))
				{
					this._newZoom = 0f;
				}
				else
				{
					this._newZoom = zoom * Time.deltaTime;
				}
			}
			float num = this._camera.fieldOfView - this._newZoom;
			this._camera.fieldOfView = Mathf.Clamp(num, this._minZoom, this._maxZoom);
			this.UpdateHeightByZoom();
		}

		private bool IsMouseActive
		{
			get
			{
				return Input.GetMouseButton(0);
			}
		}

		private void Rotate(bool isMouseActive)
		{
			if (isMouseActive)
			{
				this._actionTime = -1f;
			}
			if (this._actionTime >= 0f)
			{
				this._actionTime += Time.deltaTime;
				float num = this._actionTime / this._rotationTime;
				num = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(num));
				this._tEarth.rotation = Quaternion.Slerp(this._from, this._to, num);
				if (num >= 1f)
				{
					this._actionTime = -1f;
				}
			}
		}

		private Vector2 GetVectorFromMouse(float deltaTime, bool smoothed)
		{
			if (smoothed)
			{
				this._mx = this.ApplySmoothing(this._mx, Input.GetAxisRaw("mouse x") * 0.05f, deltaTime, 0.1f);
				this._my = this.ApplySmoothing(this._my, Input.GetAxisRaw("mouse y") * 0.05f, deltaTime, 0.1f);
			}
			else
			{
				this._mx = Input.GetAxisRaw("mouse x") * 0.05f;
				this._my = Input.GetAxisRaw("mouse y") * 0.05f;
			}
			return new Vector2(this._mx, this._my);
		}

		private float ApplySmoothing(float lastValue, float thisValue, float deltaTime, float sensitivity)
		{
			sensitivity = Mathf.Clamp(sensitivity, 0.001f, 1f);
			if (Mathf.Approximately(sensitivity, 1f))
			{
				return thisValue;
			}
			return Mathf.Lerp(lastValue, thisValue, deltaTime * sensitivity * 100f);
		}

		private Camera _camera;

		private Transform _tEarth;

		private Transform _tCamera;

		private Transform _tChangeHeightByZoom;

		private float _tChangeHeightByZoomY0;

		private const float TChangeHeightByZoomYMax = -1242.44f;

		private const float LerpSpeed = 1f;

		private Vector3 _speed = Vector3.zero;

		private bool _isEnabled = true;

		private float _rotationTime;

		private float _actionTime = -1f;

		private Quaternion _from;

		private Quaternion _to;

		private Quaternion _camRot0;

		private Vector3 _camPos0;

		private float _newAngle;

		private float _newZoom;

		private float _camZoomZeroTime;

		private float _maxZoom = 100f;

		private float _minZoom = 35f;

		private float _zoomSpeed = 1f;

		private float _cameraAngleMax = 34f;

		private float _zoomSpeedForStop = 4f;

		private const float Sensitivity = 0.1f;

		private const float MouseScale = 0.05f;

		private float _mx;

		private float _my;

		private const bool IsUpdateHeightByZoom = true;
	}
}
