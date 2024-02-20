using System;
using UnityEngine;

public class OVRVisionGuide : MonoBehaviour
{
	private void Start()
	{
		if (this.CameraController != null)
		{
			this.VisionGuide = Object.Instantiate(Resources.Load("OVRVisionGuideMessage")) as GameObject;
			Transform transform = this.VisionGuide.transform;
			this.VisionGuide.transform.parent = this.CameraController.centerEyeAnchor;
			this.VisionGuide.transform.localPosition = transform.position;
			this.VisionGuide.transform.localRotation = transform.rotation;
			this.VisionGuide.transform.localScale = transform.localScale;
			this.VisionGuide.SetActive(false);
			this.VisionGuide.layer = LayerMask.NameToLayer(this.LayerName);
		}
	}

	private void Update()
	{
		Vector3 zero = Vector3.zero;
		this.UpdateFadeValueFromRelCamPosition(ref zero);
		if (Input.GetKeyDown(116))
		{
			OVRManager.instance.timeWarp = !OVRManager.instance.timeWarp;
		}
		if (Input.GetKeyDown(102))
		{
			OVRManager.instance.freezeTimeWarp = !OVRManager.instance.freezeTimeWarp;
		}
	}

	private bool UpdateFadeValueFromRelCamPosition(ref Vector3 relCamPosition)
	{
		bool flag = false;
		this.FadeTextureAlpha = 0f;
		if (relCamPosition.x < this.CameraPositionClampMin.x && this.CalculateFadeValue(ref this.FadeTextureAlpha, relCamPosition.x, this.CameraPositionClampMin.x))
		{
			flag = true;
		}
		if (relCamPosition.y < this.CameraPositionClampMin.y && this.CalculateFadeValue(ref this.FadeTextureAlpha, relCamPosition.y, this.CameraPositionClampMin.y))
		{
			flag = true;
		}
		if (relCamPosition.z < this.CameraPositionClampMin.z && this.CalculateFadeValue(ref this.FadeTextureAlpha, relCamPosition.z, this.CameraPositionClampMin.z))
		{
			flag = true;
		}
		if (relCamPosition.x > this.CameraPositionClampMax.x && this.CalculateFadeValue(ref this.FadeTextureAlpha, this.CameraPositionClampMax.x, relCamPosition.x))
		{
			flag = true;
		}
		if (relCamPosition.y > this.CameraPositionClampMax.y && this.CalculateFadeValue(ref this.FadeTextureAlpha, this.CameraPositionClampMax.y, relCamPosition.y))
		{
			flag = true;
		}
		if (relCamPosition.z > this.CameraPositionClampMax.z && this.CalculateFadeValue(ref this.FadeTextureAlpha, this.CameraPositionClampMax.z, relCamPosition.z))
		{
			flag = true;
		}
		return flag;
	}

	private bool CalculateFadeValue(ref float curFade, float a, float b)
	{
		bool flag = false;
		float num = (b - a) / this.CameraPositionOverlap;
		if (num > 1f)
		{
			num = 1f;
		}
		num *= this.CameraPositionMaxFade;
		if (num > curFade)
		{
			curFade = num;
			if (num >= this.CameraPositionMaxFade)
			{
				flag = true;
			}
		}
		return flag;
	}

	public void SetOVRCameraController(ref OVRCameraRig cameraController)
	{
		this.CameraController = cameraController;
	}

	public void SetFadeTexture(ref Texture fadeTexture)
	{
		this.FadeTexture = fadeTexture;
	}

	public float GetFadeAlphaValue()
	{
		return this.FadeTextureAlpha;
	}

	public void SetVisionGuideLayer(ref string layer)
	{
		this.LayerName = layer;
	}

	public void OnGUIVisionGuide()
	{
		if (this.FadeTexture != null && this.FadeTextureAlpha > 0f)
		{
			GUI.color = new Color(0.1f, 0.1f, 0.1f, this.FadeTextureAlpha);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), this.FadeTexture);
			GUI.color = Color.white;
			if (this.VisionGuide != null)
			{
				this.VisionGuide.SetActive(true);
				float num = this.FadeTextureAlpha / this.CameraPositionMaxFade;
				num *= num;
				float num2 = num * ((Mathf.Sin(Time.time * this.VisionGuideFlashSpeed) + 1f) * 0.5f);
				Material material = this.VisionGuide.GetComponent<Renderer>().material;
				Color color = material.GetColor("_Color");
				color.a = num2;
				material.SetColor("_Color", color);
			}
		}
		else if (this.VisionGuide != null)
		{
			this.VisionGuide.SetActive(false);
		}
	}

	private Texture FadeTexture;

	private float FadeTextureAlpha;

	private Vector3 CameraPositionClampMin = new Vector3(-0.45f, -0.25f, -0.5f);

	private Vector3 CameraPositionClampMax = new Vector3(0.45f, 1.35f, 1f);

	private float CameraPositionOverlap = 0.125f;

	private float CameraPositionMaxFade = 0.65f;

	private OVRCameraRig CameraController;

	private GameObject VisionGuide;

	private float VisionGuideFlashSpeed = 5f;

	private string LayerName = "Default";
}
