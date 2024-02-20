using System;
using mset;
using UnityEngine;

public class FishBlendList : MonoBehaviour
{
	private void Start()
	{
		this.manager = SkyManager.Get();
		if (this.skyList.Length > 0)
		{
			this.manager.BlendToGlobalSky(this.skyList[this.currSky], this.blendTime);
			this.lightPos = this.skyList[this.currSky].transform.GetChild(0);
			if (this.lightPos)
			{
				this.light = this.lightPos.GetComponent<Light>();
			}
			this.oldLight = this.light;
			this.oldLightPos = this.lightPos;
			this.curLight = this.currentLightPos.GetComponent<Light>();
		}
		this.blendStamp = Time.time;
		this.transStamp = Time.time;
	}

	private void FixedUpdate()
	{
		if (this.skiesRoot)
		{
			this.skiesRoot.transform.Rotate(new Vector3(0f, 0.0004f, 0f));
		}
	}

	private void Update()
	{
		if (this.skyList.Length > 0)
		{
			if (Time.time - this.blendStamp > this.blendTime + this.waitTime)
			{
				this.currSky = (this.currSky + 1) % this.skyList.Length;
				this.blendStamp = Time.time;
				this.oldLightPos = this.lightPos;
				this.lightPos = this.skyList[this.currSky].transform.GetChild(0);
				this.oldLight = this.light;
				if (this.lightPos)
				{
					this.light = this.lightPos.GetComponent<Light>();
				}
				this.manager.BlendToGlobalSky(this.skyList[this.currSky], this.blendTime);
				this.transStamp = Time.time;
			}
			if (Time.time - this.transStamp < this.blendTime && Time.time - this.transStamp > 0f && this.oldLightPos && this.lightPos)
			{
				Vector3 position = this.oldLightPos.transform.position;
				Vector3 position2 = this.lightPos.position;
				Quaternion rotation = this.oldLightPos.transform.rotation;
				Quaternion rotation2 = this.lightPos.rotation;
				float num = (Time.time - this.transStamp) / this.blendTime;
				this.currentLightPos.transform.position = Vector3.Lerp(position, position2, num);
				this.currentLightPos.transform.rotation = Quaternion.Lerp(rotation, rotation2, num);
				this.curLight.intensity = Mathf.Lerp(this.oldLight.intensity, this.light.intensity, num);
				this.curLight.color = Vector4.Lerp(this.oldLight.color, this.light.color, num);
				if (this.waterBase && this.waterBase.sharedMaterial)
				{
					this.waterBase.sharedMaterial.SetColor("_SpecularColor", Vector4.Lerp(this.oldLight.color, this.light.color, num));
				}
			}
		}
	}

	public Sky[] skyList;

	public float blendTime = 3f;

	public float waitTime = 3f;

	public FishWaterBase waterBase;

	public GameObject clouds;

	public Vector3 direction;

	public GameObject camera;

	public float distance = 200f;

	public GameObject currentLightPos;

	public GameObject skiesRoot;

	private float blendStamp;

	private float transStamp;

	private int currSky;

	private Transform oldLightPos;

	private Transform lightPos;

	private Light oldLight;

	private Light light;

	private Light curLight;

	private SkyManager manager;
}
