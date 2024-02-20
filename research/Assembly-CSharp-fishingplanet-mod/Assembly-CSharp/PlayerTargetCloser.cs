using System;
using System.Diagnostics;
using UnityEngine;

public class PlayerTargetCloser : MonoBehaviour
{
	public float Dy
	{
		get
		{
			return this._movementDisplacement.y;
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action EAdjusted = delegate
	{
	};

	private void Awake()
	{
		this._update = new UpdatePlayerPositionAndRotationDelegate(this.UpdateAction);
		this._targetHelper = new GameObject(string.Format("{0} TargetCloserHelper", base.name)).transform;
		this._targetHelper.SetParent(base.transform);
	}

	public void Setup(Transform player, Transform camera, UpdatePlayerPositionAndRotationDelegate updateF)
	{
		this._player = player;
		this._camera = camera;
		this._update = updateF;
		if (!this._isRotationOnly || !this._isNoRaycasts)
		{
			Vector3? maskedRayContactPoint = Math3d.GetMaskedRayContactPoint(this._player.position, this._player.position - new Vector3(0f, 2f, 0f), GlobalConsts.GroundObstacleMask);
			this._groundDy = ((maskedRayContactPoint == null) ? 1.55f : (this._player.position.y - maskedRayContactPoint.Value.y));
		}
	}

	private Vector3 GetPositionBehind(Transform target)
	{
		Vector3 normalized = Math3d.ProjectOXZ(target.forward).normalized;
		Vector3 normalized2 = Math3d.ProjectOXZ(target.right).normalized;
		return target.position + normalized * this._movementDisplacement.z + normalized2 * this._movementDisplacement.x;
	}

	public bool IsPossibleToAdjust(Transform target)
	{
		if (this._isRotationOnly || this._isNoRaycasts)
		{
			return true;
		}
		Vector3 positionBehind = this.GetPositionBehind(target);
		Vector3 vector;
		vector..ctor(target.position.x, positionBehind.y, target.position.z);
		Vector3 vector2 = positionBehind - vector;
		float num = vector2.magnitude + 0.5f;
		Vector3 normalized = Math3d.ProjectOXZ(target.right).normalized;
		return Math3d.GetMaskedRayContactPoint(vector, vector + vector2.normalized * num, GlobalConsts.WallsMask) == null && Math3d.GetMaskedRayContactPoint(vector, vector + normalized * 0.5f, GlobalConsts.WallsMask) == null && Math3d.GetMaskedRayContactPoint(vector, vector - normalized * 0.5f, GlobalConsts.WallsMask) == null;
	}

	public void AdjustPlayerImmediate(Transform target)
	{
		this._targetHelper.SetParent(target);
		this._targetHelper.localRotation = Quaternion.identity;
		this._targetHelper.localPosition = Vector3.zero;
		this._actionDuration = 0f;
		this._actionTill = Time.time;
	}

	public void AdjustPlayer(Transform target)
	{
		this._targetHelper.SetParent(target);
		this._cameraStartRotation = this._camera.rotation;
		Vector3 normalized = Math3d.ProjectOXZ(target.forward).normalized;
		float num = ((!(normalized == this._camera.forward)) ? Math3d.ClampAngleTo180(Mathf.Acos(Vector3.Dot(normalized, this._camera.forward)) * 57.29578f) : 0f);
		if (float.IsNaN(num))
		{
			num = 0f;
		}
		float num2 = Mathf.Abs(num / this._rotationSpeed);
		this._fromPos = this._player.position;
		Vector3 vector;
		if (this._isRotationOnly)
		{
			vector = this._fromPos;
		}
		else if (this._isNoRaycasts)
		{
			this._targetHelper.localRotation = Quaternion.identity;
			this._targetHelper.localPosition = Vector3.zero;
			vector = target.position;
		}
		else
		{
			vector = this.GetPositionBehind(target);
			if (!Mathf.Approximately(this._groundDy, 0f))
			{
				Vector3? maskedRayContactPoint = Math3d.GetMaskedRayContactPoint(vector + new Vector3(0f, 2f, 0f), vector - new Vector3(0f, 2f, 0f), GlobalConsts.GroundObstacleMask);
				if (maskedRayContactPoint != null)
				{
					vector = maskedRayContactPoint.Value + new Vector3(0f, this._groundDy, 0f);
				}
			}
			else
			{
				vector.y = this._player.position.y;
			}
		}
		float num3 = (vector - this._player.position).magnitude / this._movementSpeed;
		if (!this._isNoRaycasts)
		{
			this._targetHelper.rotation = Quaternion.Euler(0f, this._targetHelper.parent.eulerAngles.y, 0f);
			this._targetHelper.position = vector;
		}
		this._actionDuration = Mathf.Max(num3, num2);
		this._actionTill = Time.time + this._actionDuration;
	}

	private void Update()
	{
		if (this._actionTill > 0f)
		{
			if ((double)this._actionDuration > 0.05)
			{
				float num = Mathf.Clamp01(1f - (this._actionTill - Time.time) / this._actionDuration);
				float num2 = Mathf.SmoothStep(0f, 1f, num);
				this._update(Vector3.Lerp(this._fromPos, this._targetHelper.position, num2), Quaternion.Slerp(this._cameraStartRotation, this._targetHelper.rotation, num2), num2);
				if (this._actionTill < Time.time)
				{
					this._actionTill = -1f;
					this._targetHelper.SetParent(base.transform);
					this.EAdjusted();
				}
			}
			else
			{
				this._update(this._targetHelper.position, this._targetHelper.rotation, 1f);
				this._actionTill = -1f;
				this._targetHelper.SetParent(base.transform);
				this.EAdjusted();
			}
		}
	}

	private void UpdateAction(Vector3 pos, Quaternion rotation, float prc)
	{
		this._player.position = pos;
		Vector3 eulerAngles = rotation.eulerAngles;
		this._player.rotation = Quaternion.Euler(0f, eulerAngles.y, 0f);
		this._camera.localRotation = Quaternion.Euler(eulerAngles.x, 0f, 0f);
	}

	private void OnDrawGizmos()
	{
		if (this._targetHelper != null)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawSphere(this._targetHelper.position, 0.05f);
		}
	}

	[SerializeField]
	private Transform _camera;

	[SerializeField]
	private Transform _player;

	[SerializeField]
	private Vector3 _movementDisplacement = new Vector3(0f, 0f, -0.5f);

	[SerializeField]
	private float _rotationSpeed = 180f;

	[SerializeField]
	private float _movementSpeed = 3f;

	[SerializeField]
	private bool _isRotationOnly;

	[SerializeField]
	private bool _isNoRaycasts;

	private float _actionTill = -1f;

	private float _actionDuration;

	private Vector3 _fromPos;

	private Quaternion _cameraStartRotation;

	private UpdatePlayerPositionAndRotationDelegate _update;

	private float _groundDy;

	private const float playerCapsuleRadius = 0.5f;

	private Transform _targetHelper;
}
