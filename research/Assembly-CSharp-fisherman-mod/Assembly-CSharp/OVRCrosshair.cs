using System;
using UnityEngine;

public class OVRCrosshair
{
	public void SetCrosshairTexture(ref Texture image)
	{
		this.ImageCrosshair = image;
	}

	public void SetOVRCameraController(ref OVRCameraRig cameraController)
	{
		this.CameraController = cameraController;
		this.UIAnchor = this.CameraController.centerEyeAnchor;
	}

	public void SetOVRPlayerController(ref OVRPlayerController playerController)
	{
		this.PlayerController = playerController;
	}

	public bool IsCrosshairVisible()
	{
		return this.FadeVal > 0f;
	}

	public void Init()
	{
		this.DisplayCrosshair = false;
		this.CollisionWithGeometry = false;
		this.FadeVal = 0f;
		this.ScreenWidth = (float)Screen.width;
		this.ScreenHeight = (float)Screen.height;
		this.XL = this.ScreenWidth * 0.5f;
		this.YL = this.ScreenHeight * 0.5f;
	}

	public void UpdateCrosshair()
	{
		if (this.ShouldDisplayCrosshair())
		{
			this.CollisionWithGeometryCheck();
		}
	}

	public void OnGUICrosshair()
	{
		if (this.DisplayCrosshair && !this.CollisionWithGeometry)
		{
			this.FadeVal += Time.deltaTime / this.FadeTime;
		}
		else
		{
			this.FadeVal -= Time.deltaTime / this.FadeTime;
		}
		this.FadeVal = Mathf.Clamp(this.FadeVal, 0f, 1f);
		if (this.PlayerController != null)
		{
			this.PlayerController.SetSkipMouseRotation(false);
		}
		if (this.ImageCrosshair != null && this.FadeVal != 0f)
		{
			if (this.PlayerController != null)
			{
				this.PlayerController.SetSkipMouseRotation(true);
			}
			GUI.color = new Color(1f, 1f, 1f, this.FadeVal * this.FadeScale);
			this.XL += Input.GetAxis("Mouse X") * this.ScaleSpeedX;
			if (this.XL < this.DeadZoneX)
			{
				if (this.PlayerController != null)
				{
					this.PlayerController.SetSkipMouseRotation(false);
				}
				this.XL = this.DeadZoneX - 0.001f;
			}
			else if (this.XL > (float)Screen.width - this.DeadZoneX)
			{
				if (this.PlayerController != null)
				{
					this.PlayerController.SetSkipMouseRotation(false);
				}
				this.XL = this.ScreenWidth - this.DeadZoneX + 0.001f;
			}
			this.YL -= Input.GetAxis("Mouse Y") * this.ScaleSpeedY;
			if (this.YL < this.DeadZoneY)
			{
				if (this.YL < 0f)
				{
					this.YL = 0f;
				}
			}
			else if (this.YL > this.ScreenHeight - this.DeadZoneY && this.YL > this.ScreenHeight)
			{
				this.YL = this.ScreenHeight;
			}
			bool flag = true;
			if (this.PlayerController != null)
			{
				this.PlayerController.GetSkipMouseRotation(ref flag);
			}
			if (flag)
			{
				GUI.DrawTexture(new Rect(this.XL - (float)this.ImageCrosshair.width * 0.5f, this.YL - (float)this.ImageCrosshair.height * 0.5f, (float)this.ImageCrosshair.width, (float)this.ImageCrosshair.height), this.ImageCrosshair);
			}
			GUI.color = Color.white;
		}
	}

	private bool ShouldDisplayCrosshair()
	{
		if (Input.GetKeyDown(this.CrosshairKey))
		{
			if (!this.DisplayCrosshair)
			{
				this.DisplayCrosshair = true;
				this.XL = this.ScreenWidth * 0.5f;
				this.YL = this.ScreenHeight * 0.5f;
			}
			else
			{
				this.DisplayCrosshair = false;
			}
		}
		return this.DisplayCrosshair;
	}

	private bool CollisionWithGeometryCheck()
	{
		this.CollisionWithGeometry = false;
		Vector3 position = this.UIAnchor.position;
		Vector3 vector = Vector3.forward;
		vector = this.UIAnchor.rotation * vector;
		vector *= this.CrosshairDistance;
		Vector3 vector2 = position + vector;
		RaycastHit raycastHit;
		if (Physics.Linecast(position, vector2, ref raycastHit) && !raycastHit.collider.isTrigger)
		{
			this.CollisionWithGeometry = true;
		}
		return this.CollisionWithGeometry;
	}

	public Texture ImageCrosshair;

	public OVRCameraRig CameraController;

	public OVRPlayerController PlayerController;

	public float FadeTime = 0.3f;

	public float FadeScale = 0.6f;

	public float CrosshairDistance = 1f;

	public KeyCode CrosshairKey = 99;

	private float DeadZoneX = 400f;

	private float DeadZoneY = 75f;

	private float ScaleSpeedX = 7f;

	private float ScaleSpeedY = 7f;

	private bool DisplayCrosshair;

	private bool CollisionWithGeometry;

	private float FadeVal;

	private Transform UIAnchor;

	private float XL;

	private float YL;

	private float ScreenWidth = 1280f;

	private float ScreenHeight = 800f;
}
