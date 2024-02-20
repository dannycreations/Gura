using System;
using UnityEngine;

namespace Boats
{
	public abstract class BoatMouseController : CameraMouseLook
	{
		public Vector3 DriverCameraLocalPosition
		{
			get
			{
				return this._localMovement;
			}
		}

		public Vector3 CameraPosition
		{
			get
			{
				return this._playerCameraController.Camera.transform.position;
			}
		}

		public Vector3 CameraDirection
		{
			get
			{
				return this._playerCameraController.Camera.transform.forward;
			}
		}

		public bool IsTransitionActive
		{
			get
			{
				return this._curState != BoatMouseController.ChangeLocalMovementMode.Idle;
			}
		}

		public float CameraLocalPositionPrc
		{
			get
			{
				return this._cameraLocalSmoothPrc;
			}
		}

		protected abstract void InitMouseController(CameraController cameraController);

		public void TakeControll(CameraController playerCameraController, IBoatController owner)
		{
			this._owner = owner;
			this._camera = base.GetComponent<Camera>();
			this._playerCameraController = playerCameraController;
			this._prevCameraMovement = playerCameraController.NormalCameraLocalPosition;
			this._playerCameraController.CameraMouseLook.enabled = false;
			this.InitMouseController(playerCameraController);
			this._mouseLookController.TakeControll(owner);
			this.ResetOriginalRotation(Quaternion.identity);
			base.enabled = true;
			playerCameraController.enabled = false;
			this._cameraLocalPositionPrc = 1f;
			this._cameraLocalSmoothPrc = 1f;
			this._startSmothingFrom = Time.time;
		}

		public void ReleaseControll()
		{
			this._playerCameraController = null;
			base.transform.localRotation = Quaternion.identity;
			this._owner = null;
			this.Disable();
			this._camera = null;
			this._mouseLookController.ReleaseControll();
		}

		public void EnterMapMode()
		{
			if (this._isFishingMode)
			{
				this._mouseLookController.SetActive(false);
			}
			else
			{
				this.Disable();
				this._playerTranslation = GameFactory.Player.transform.parent.localPosition;
				GameFactory.Player.transform.parent.localRotation = base.transform.localRotation;
				GameFactory.Player.transform.parent.localPosition = base.transform.localPosition;
			}
		}

		public void LeaveMapMode()
		{
			if (this._isFishingMode)
			{
				this._mouseLookController.SetActive(true);
			}
			else
			{
				base.enabled = true;
				GameFactory.Player.transform.parent.localRotation = Quaternion.identity;
				GameFactory.Player.transform.parent.localPosition = this._playerTranslation;
			}
		}

		public void EnterFishingMode()
		{
			this._targetPitch = Math3d.ClampAngleTo180(base.transform.localRotation.eulerAngles.x);
			this._targetYaw = Math3d.ClampAngleTo180(base.transform.localRotation.eulerAngles.y);
			GameFactory.Player.SetHandsVisibility(false);
			this.ChangeState(BoatMouseController.ChangeLocalMovementMode.Clear, delegate
			{
				this._isFishingMode = true;
				this.Disable();
				this._mouseLookController.SetActive(true);
				GameFactory.Player.SetHandsVisibility(true);
			});
		}

		public void LeaveFishingMode(Action action)
		{
			this._isFishingMode = false;
			base.enabled = true;
			this._mouseLookController.SetActive(false);
			this._targetPitch = this._mouseLookController.Pitch;
			this._targetYaw = this._mouseLookController.Yaw;
			this.ChangeState(BoatMouseController.ChangeLocalMovementMode.Set, delegate
			{
				this.SetNewRotation(this._targetYaw, this._targetPitch);
				GameFactory.Player.SetHandsVisibility(true);
				action();
				this._startSmothingFrom = Time.time;
			});
		}

		public void PrepareDriverForUnboarding()
		{
			this._targetPitch = Math3d.ClampAngleTo180(base.transform.localRotation.eulerAngles.x);
			this._targetYaw = Math3d.ClampAngleTo180(base.transform.localRotation.eulerAngles.y);
			base.transform.localRotation = Quaternion.identity;
			this._mouseLookController.SetNewRotation(this._targetYaw, this._targetPitch);
		}

		private void Disable()
		{
			base.enabled = false;
		}

		private void ChangeState(BoatMouseController.ChangeLocalMovementMode state, Action eStateChanged = null)
		{
			this._eCameraMoved = eStateChanged;
			this._curState = state;
			if (state == BoatMouseController.ChangeLocalMovementMode.Set || state == BoatMouseController.ChangeLocalMovementMode.Clear)
			{
				this._cameraLocalPositionPrc = 1f - (base.transform.localPosition - this._localMovement).magnitude / (this._prevCameraMovement - this._localMovement).magnitude;
			}
		}

		protected override void Update()
		{
			if (this._curState == BoatMouseController.ChangeLocalMovementMode.Set || this._curState == BoatMouseController.ChangeLocalMovementMode.Clear)
			{
				if (this._curState == BoatMouseController.ChangeLocalMovementMode.Set)
				{
					this._cameraLocalPositionPrc += this._changeLocalMovementSpeed * Time.deltaTime;
					if (this._cameraLocalPositionPrc > 1f)
					{
						this._cameraLocalPositionPrc = 1f;
						this._cameraLocalSmoothPrc = 1f;
						this.OnStateTransitionFinished();
						return;
					}
					this._cameraLocalSmoothPrc = Mathfx.SmoothStep2(0f, 1f, this._cameraLocalPositionPrc);
				}
				else
				{
					this._cameraLocalPositionPrc -= this._changeLocalMovementSpeed * Time.deltaTime;
					if (this._cameraLocalPositionPrc < 0f)
					{
						this._cameraLocalPositionPrc = 0f;
						this._cameraLocalSmoothPrc = 0f;
						this.OnStateTransitionFinished();
						return;
					}
					this._cameraLocalSmoothPrc = Mathfx.SmoothStep2(1f, 0f, 1f - this._cameraLocalPositionPrc);
				}
				this.UpdateCamera();
			}
			else if (this._curState == BoatMouseController.ChangeLocalMovementMode.Idle)
			{
				float magnitude = this.shakeHarmonicAmplitude.magnitude;
				if (this._owner.BoatCollisionImpulse.magnitude > this.prevBoatCollisionImpulse.magnitude)
				{
					this.shakeHarmonicAmplitude += (this._owner.BoatCollisionImpulse - this.prevBoatCollisionImpulse) * this._shakeHarmonicAmpFactor;
				}
				this.prevBoatCollisionImpulse = this._owner.BoatCollisionImpulse;
				if (this.shakeHarmonicAmplitude.magnitude - magnitude > this._shakeHarmonicAmpFactor)
				{
					this.shakeHarmonicStartTime = Time.time;
				}
				this.shakeHarmonicAmplitude *= Mathf.Exp(-Time.deltaTime * this._shakeHarmonicDamping);
				float num = Mathf.Sin((Time.time - this.shakeHarmonicStartTime) * this._shakeHarmonicFrequency);
				float num2 = Vector3.Dot(this.shakeHarmonicAmplitude, base.transform.forward) * num * 57.29578f;
				float num3 = Vector3.Dot(this.shakeHarmonicAmplitude, base.transform.right) * num * 57.29578f;
				float num4 = this._owner.BoatVelocity * this._shakeNoiseAmpFactor;
				Vector2 vector = new Vector2(this._owner.Position.x, this._owner.Position.z) * this._shakeNoiseFrequency;
				float num5 = 2f * (Mathf.PerlinNoise(vector.x, vector.y) - 0.5f) * num4;
				float num6 = 2f * (Mathf.PerlinNoise(vector.y, vector.x) - 0.5f) * num4;
				base.Update();
				Vector3 vector2 = this._owner.Rotation * Vector3.up;
				Quaternion quaternion = Quaternion.FromToRotation(vector2, Vector3.up);
				float num7 = Quaternion.Angle(Quaternion.identity, quaternion);
				if (num7 > 15f)
				{
					quaternion = Quaternion.Slerp(Quaternion.identity, quaternion, Mathf.SmoothStep(0f, 1f, 15f / num7));
				}
				Quaternion quaternion2 = quaternion * base.transform.rotation;
				quaternion2 = Quaternion.AngleAxis(num3 + num6, base.transform.forward) * Quaternion.AngleAxis(num2 + num5, base.transform.right) * quaternion2;
				float num8 = Time.time - this._startSmothingFrom;
				if (num8 > 0.5f)
				{
					base.transform.rotation = quaternion2;
				}
				else
				{
					base.transform.rotation = Quaternion.Slerp(base.transform.rotation, quaternion2, num8 / 0.5f);
				}
				Quaternion quaternion3 = Quaternion.FromToRotation(base.transform.up, vector2);
				float num9 = Quaternion.Angle(Quaternion.identity, quaternion3);
				float num10 = Mathf.Abs(this.minimumY);
			}
		}

		private void UpdateCamera()
		{
			base.transform.localRotation = Quaternion.Euler(new Vector3(Mathf.Lerp(0f, this._targetPitch, this._cameraLocalSmoothPrc), Mathf.Lerp(0f, this._targetYaw, this._cameraLocalSmoothPrc), 0f));
			this._mouseLookController.SetNewRotation(Mathf.Lerp(this._targetYaw, 0f, this._cameraLocalSmoothPrc), Mathf.Lerp(this._targetPitch, 0f, this._cameraLocalSmoothPrc));
			base.transform.localPosition = Vector3.Lerp(this._prevCameraMovement, this._localMovement, this._cameraLocalSmoothPrc);
		}

		private void OnStateTransitionFinished()
		{
			this.UpdateCamera();
			this._curState = BoatMouseController.ChangeLocalMovementMode.Idle;
			if (this._eCameraMoved != null)
			{
				this._eCameraMoved();
				this._eCameraMoved = null;
			}
		}

		public void SetLookAtMode(Transform rootBone)
		{
			this.Disable();
			this.FollowBoneController.Activate(rootBone);
		}

		public void ClearLookAtMode()
		{
			base.enabled = true;
			this.FollowBoneController.Deactivate();
			base.AdaptCurrentRotation();
		}

		public void SetGlobalRotation(Quaternion rotation)
		{
			if (this._isFishingMode)
			{
				this._mouseLookController.SetActive(false);
				float y = rotation.eulerAngles.y;
				float x = rotation.eulerAngles.x;
				float y2 = base.transform.eulerAngles.y;
				float num = y - y2;
				this._mouseLookController.SetNewRotation(this._mouseLookController.Yaw + num, x);
			}
		}

		public void OnPlayerExternalControllReleased()
		{
			if (this._isFishingMode)
			{
				this._mouseLookController.SetActive(true);
			}
		}

		public void ClearFishingMode()
		{
			this._isFishingMode = false;
			this._mouseLookController.SetActive(false);
		}

		public void SetActive(bool flag)
		{
			if (this._isFishingMode)
			{
				this._mouseLookController.SetActive(flag);
			}
			else
			{
				base.enabled = flag;
			}
		}

		public virtual void FreezeCamera(bool flag)
		{
			this._mouseLookController.BlockInput(flag);
		}

		public virtual void SetFishPhotoMode(bool flag)
		{
			BoatPhotoMouseController component = base.transform.GetComponent<BoatPhotoMouseController>();
			if (flag)
			{
				component.enabled = true;
				component.SmoothStart();
				this._mouseLookController.axes = CameraMouseLook.RotationAxes.MouseX;
			}
			else
			{
				component.SmoothResetAndStop(Quaternion.identity);
				this._mouseLookController.axes = CameraMouseLook.RotationAxes.MouseXAndY;
			}
		}

		public void InitTransitionToZeroXRotation()
		{
			this._playerCameraController.InitTransitionToZeroXRotation(this._mouseLookController);
		}

		public void TransitToZeroXRotation()
		{
			this._playerCameraController.TransitToZeroXRotation();
		}

		public void FinalizeTransitionToZeroXRotation()
		{
			this._playerCameraController.FinalizeTransitionToZeroXRotation();
		}

		[SerializeField]
		private float _shakeHarmonicFrequency = 20f;

		[SerializeField]
		private float _shakeHarmonicDamping = 6f;

		[SerializeField]
		private float _shakeHarmonicAmpFactor = 0.02f;

		[SerializeField]
		private float _shakeNoiseFrequency = 0.5f;

		[SerializeField]
		private float _shakeNoiseAmpFactor = 0.01f;

		[SerializeField]
		protected Vector3 _localMovement = Vector3.zero;

		[SerializeField]
		protected float _changeLocalMovementSpeed = 2f;

		[SerializeField]
		protected float _changeRotationSpeed = 180f;

		protected CameraController _playerCameraController;

		protected BoatFishingMouseController _mouseLookController;

		protected FollowBoneMouseController FollowBoneController;

		private Vector3 _prevCameraMovement;

		private BoatMouseController.ChangeLocalMovementMode _curState;

		private float _cameraLocalPositionPrc;

		private float _cameraLocalSmoothPrc;

		private Action _eCameraMoved;

		private bool _isFishingMode;

		private float _targetYaw;

		private Camera _camera;

		private const float HIGH_INCLINE_ANGLE = 15f;

		private const float TRANSITION_SMOTHING_DURATION = 0.5f;

		private float _startSmothingFrom;

		private Vector3 shakeHarmonicAmplitude;

		private Vector3 prevBoatCollisionImpulse;

		private float shakeHarmonicStartTime;

		private DebugPlotter dp;

		private DebugPlotter.Value dp_deltaTime;

		private DebugPlotter.Value dp_amp;

		private DebugPlotter.Value dp_imp;

		private DebugPlotter.Value dp_pitch;

		private IBoatController _owner;

		private Vector3 _playerTranslation;

		private enum ChangeLocalMovementMode
		{
			Idle,
			Set,
			Clear
		}
	}
}
