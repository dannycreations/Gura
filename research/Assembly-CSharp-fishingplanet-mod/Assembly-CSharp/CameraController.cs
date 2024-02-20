using System;
using UnityEngine;

[RequireComponent(typeof(CameraMouseLook))]
public class CameraController : MonoBehaviour
{
	public Vector3 CameraMovement { get; set; }

	internal void Awake()
	{
		this.player = base.transform.GetComponentInChildren<PlayerController>();
		if (this.player != null)
		{
			this.oldPlayerY = this.player.localHandsOffset;
		}
		this.CameraMouseLook = base.GetComponent<CameraMouseLookForObjectWithCollider>();
		if (this.CameraMouseLook != null)
		{
			this.sensX = this.CameraMouseLook.sensitivityX;
			this.sensY = this.CameraMouseLook.sensitivityY;
		}
		if (this.Camera != null)
		{
			this.oldCameraY = this.Camera.transform.localPosition.y;
			this.oldCameraZ = this.Camera.transform.localPosition.z;
		}
	}

	internal void Start()
	{
		float[] array = new float[32];
		array[12] = 40f;
		switch (SettingsManager.RenderQuality)
		{
		case RenderQualities.Fastest:
			array[12] = 10f;
			break;
		case RenderQualities.Fast:
			array[12] = 15f;
			break;
		case RenderQualities.Simple:
			array[12] = 20f;
			break;
		case RenderQualities.Good:
			array[12] = 25f;
			break;
		case RenderQualities.Beautiful:
			array[12] = 30f;
			break;
		case RenderQualities.Fantastic:
			array[12] = 40f;
			break;
		case RenderQualities.Ultra:
			array[12] = 40f;
			break;
		}
		if (this.Camera != null)
		{
			this.Camera.layerCullDistances = array;
			this.Camera.layerCullSpherical = true;
		}
	}

	private float FastRewind(float value)
	{
		return Mathf.Clamp(value * 2f, 0f, 1.2f);
	}

	internal void Update()
	{
		if (this.player != null && this.CameraMouseLook != null)
		{
			float num = (this.player.RodForceHand.magnitude - 1f) * 0.1f;
			num = Mathf.Clamp(num, 0f, 0.1f);
			float num2 = 1.5707964f / (num + 1f);
			num2 = Mathf.Sin(num2);
			this.CameraMouseLook.sensitivityX = this.sensX * num2;
			this.CameraMouseLook.sensitivityY = this.sensY * num2;
			this.player.ToolsOffset.y = -Mathf.Sin(this.FastRewind(this.player.BowDown)) * 1.25f;
			this.player.ToolsOffset.z = Mathf.Sin(this.FastRewind(this.player.BowDown)) * 1.5f;
			Vector3 vector;
			vector..ctor(this.Camera.transform.localPosition.x, this.oldCameraY - Mathf.Sin(this.player.MovedDown) * 0.5f, this.oldCameraZ);
			vector += new Vector3(this.player.ToolsOffset.x, this.player.ToolsOffset.y * 0.85f, this.player.ToolsOffset.z);
			vector += this.CameraMovement;
			this.player.localHandsOffset = this.oldPlayerY + Mathf.Sin(this.player.MovedDown) * 0.3f;
			this.Camera.transform.localPosition = vector;
		}
		if (!CameraController.setUp && Camera.current != null)
		{
			CameraController.setUp = true;
		}
	}

	public void CameraFreeze()
	{
		this.CameraMouseLook.enabled = false;
	}

	public void CameraUnFreeze()
	{
		this.CameraMouseLook.enabled = true;
	}

	public void InitTransitionToZeroXRotation(CameraMouseLookForObjectWithCollider transitionMouseLook)
	{
		this._transitionMouseLook = transitionMouseLook;
		this._timeToShowState = 0f;
		this._initialXAngle = base.transform.eulerAngles.x;
		this._targetXAngle = 0f;
		if (this._initialXAngle > 270f)
		{
			this._initialXAngle -= 360f;
		}
		this._transitionMouseLook.enabled = false;
	}

	public void TransitToZeroXRotation()
	{
		if (this._timeToShowState <= 1f)
		{
			this._timeToShowState += Time.deltaTime;
			float num = Mathf.Lerp(this._initialXAngle, this._targetXAngle, Mathf.Clamp01(this._timeToShowState));
			base.transform.localRotation = Quaternion.Euler(num, 0f, 0f);
		}
	}

	public void FinalizeTransitionToZeroXRotation()
	{
		base.transform.localRotation = Quaternion.identity;
		this._transitionMouseLook.ResetYAxis();
		this._transitionMouseLook.enabled = true;
		this._transitionMouseLook = null;
	}

	public readonly Vector3 NormalCameraLocalPosition = new Vector3(0f, 0.15169f, -0.35f);

	public Camera Camera;

	public Transform HUD;

	private static bool setUp;

	public CameraMouseLookForObjectWithCollider CameraMouseLook;

	public PlayerController player;

	[HideInInspector]
	private float sensX = 10f;

	private float sensY = 3f;

	private float oldCameraY;

	private float oldCameraZ;

	private float oldPlayerY = 0.15f;

	private float _timeToShowState;

	private float _initialXAngle;

	private float _targetXAngle;

	private CameraMouseLookForObjectWithCollider _transitionMouseLook;
}
