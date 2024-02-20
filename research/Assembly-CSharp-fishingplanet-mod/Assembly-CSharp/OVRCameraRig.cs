using System;
using UnityEngine;

[ExecuteInEditMode]
public class OVRCameraRig : MonoBehaviour
{
	public Transform leftEyeAnchor { get; private set; }

	public Transform centerEyeAnchor { get; private set; }

	public Transform rightEyeAnchor { get; private set; }

	private void Awake()
	{
		this.EnsureGameObjectIntegrity();
		if (!Application.isPlaying)
		{
			return;
		}
		this.needsCameraConfigure = true;
	}

	private void Start()
	{
		this.EnsureGameObjectIntegrity();
		if (!Application.isPlaying)
		{
			return;
		}
		this.UpdateCameras();
		this.UpdateAnchors();
	}

	private void LateUpdate()
	{
		this.EnsureGameObjectIntegrity();
		if (!Application.isPlaying)
		{
			return;
		}
		this.UpdateCameras();
		this.UpdateAnchors();
	}

	private void UpdateAnchors()
	{
		OVRPose eyePose = OVRManager.display.GetEyePose(OVREye.Left);
		OVRPose eyePose2 = OVRManager.display.GetEyePose(OVREye.Right);
		this.leftEyeAnchor.localRotation = eyePose.orientation;
		this.centerEyeAnchor.localRotation = eyePose.orientation;
		this.rightEyeAnchor.localRotation = eyePose2.orientation;
		this.leftEyeAnchor.localPosition = eyePose.position;
		this.centerEyeAnchor.localPosition = 0.5f * (eyePose.position + eyePose2.position);
		this.rightEyeAnchor.localPosition = eyePose2.position;
	}

	private void UpdateCameras()
	{
		if (this.needsCameraConfigure)
		{
			this.leftEyeCamera = this.ConfigureCamera(OVREye.Left);
			this.rightEyeCamera = this.ConfigureCamera(OVREye.Right);
			OVRManager.display.ForceSymmetricProj(true);
			this.needsCameraConfigure = false;
		}
	}

	private void EnsureGameObjectIntegrity()
	{
		if (this.leftEyeAnchor == null)
		{
			this.leftEyeAnchor = this.ConfigureEyeAnchor(OVREye.Left);
		}
		if (this.centerEyeAnchor == null)
		{
			this.centerEyeAnchor = this.ConfigureEyeAnchor(OVREye.Center);
		}
		if (this.rightEyeAnchor == null)
		{
			this.rightEyeAnchor = this.ConfigureEyeAnchor(OVREye.Right);
		}
		if (this.leftEyeCamera == null)
		{
			this.leftEyeCamera = this.leftEyeAnchor.GetComponent<Camera>();
			if (this.leftEyeCamera == null)
			{
				this.leftEyeCamera = this.leftEyeAnchor.gameObject.AddComponent<Camera>();
			}
		}
		if (this.rightEyeCamera == null)
		{
			this.rightEyeCamera = this.rightEyeAnchor.GetComponent<Camera>();
			if (this.rightEyeCamera == null)
			{
				this.rightEyeCamera = this.rightEyeAnchor.gameObject.AddComponent<Camera>();
			}
		}
	}

	private Transform ConfigureEyeAnchor(OVREye eye)
	{
		string text = eye.ToString() + "EyeAnchor";
		Transform transform = base.transform.Find(text);
		if (transform == null)
		{
			string text2 = "Camera" + eye.ToString();
			transform = base.transform.Find(text2);
		}
		if (transform == null)
		{
			transform = new GameObject(text).transform;
		}
		transform.parent = base.transform;
		transform.localScale = Vector3.one;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		return transform;
	}

	private Camera ConfigureCamera(OVREye eye)
	{
		Transform transform = ((eye != OVREye.Left) ? this.rightEyeAnchor : this.leftEyeAnchor);
		Camera component = transform.GetComponent<Camera>();
		OVRDisplay.EyeRenderDesc eyeRenderDesc = OVRManager.display.GetEyeRenderDesc(eye);
		component.fieldOfView = eyeRenderDesc.fov.y;
		component.aspect = eyeRenderDesc.resolution.x / eyeRenderDesc.resolution.y;
		component.rect = new Rect(0f, 0f, OVRManager.instance.virtualTextureScale, OVRManager.instance.virtualTextureScale);
		component.targetTexture = OVRManager.display.GetEyeTexture(eye);
		if (component.actualRenderingPath == 2)
		{
			QualitySettings.antiAliasing = 0;
		}
		return component;
	}

	private Camera leftEyeCamera;

	private Camera rightEyeCamera;

	private bool needsCameraConfigure;
}
