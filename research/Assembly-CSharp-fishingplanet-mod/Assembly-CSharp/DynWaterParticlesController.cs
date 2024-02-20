using System;
using UnityEngine;

public class DynWaterParticlesController : MonoBehaviour
{
	private void PlayInitialDisturb()
	{
		if (GameFactory.Water == null)
		{
			return;
		}
		float x = base.gameObject.transform.position.x;
		float z = base.gameObject.transform.position.z;
		float num = this.disturbSize;
		GameFactory.Water.AddWaterDisturb(new Vector3(x, 0f, z), 0.07f * num, 2.2f * num);
	}

	private void GenerateDrops(float radius, float distance, float force, int count = 6)
	{
		for (int i = 0; i < count; i++)
		{
			float num = this.disturbSize;
			float num2 = (Random.value - 0.5f) * 2f * distance * num;
			float num3 = (Random.value - 0.5f) * 2f * distance * num;
			float num4 = (Random.value - 0.5f) * 2f * radius * num;
			float num5 = base.gameObject.transform.position.x + num2;
			float num6 = base.gameObject.transform.position.z + num3;
			GameFactory.Water.AddWaterDisturb(new Vector3(num5, 0f, num6), num4, force * num);
		}
	}

	private void GenerateSmallDrop(float radius, float distance, float force, int count = 6)
	{
		float num = this.disturbSize;
		if (num < 1.9f)
		{
			return;
		}
		float num2 = (Random.value - 0.5f) * 2f * distance * num;
		float num3 = (Random.value - 0.5f) * 2f * distance * num;
		float num4 = (Random.value - 0.5f) * 2f * radius * num;
		float num5 = base.gameObject.transform.position.x + num2;
		float num6 = base.gameObject.transform.position.z + num3;
		DynWaterParticlesController.CreateSplash(this.cameraTransform, new Vector3(num5, 0f, num6), this.path, num / 4f, 1f, true, true, 1);
	}

	private void Disturb7()
	{
		float x = base.gameObject.transform.position.x;
		float z = base.gameObject.transform.position.z;
		float num = this.disturbSize;
		GameFactory.Water.AddWaterDisturb(new Vector3(x - 0.08f * num, 0f, z + 0.01f * num), 0.022f * num, 1.5f * num);
		GameFactory.Water.AddWaterDisturb(new Vector3(x + 0.1f * num, 0f, z + 0.03f * num), 0.02f * num, 1.5f * num);
		GameFactory.Water.AddWaterDisturb(new Vector3(x + 0.11f * num, 0f, z + 0.02f * num), 0.02f * num, 1.5f * num);
	}

	private void FixedUpdate()
	{
		if (base.gameObject == null || GameFactory.Water == null)
		{
			return;
		}
		int num = 0;
		float num2 = 1f / this.disturbSpeed;
		for (int i = 0; i < this.timeLine1.Length; i++)
		{
			if (i >= this.lastStep && Time.time - this.startTime > this.timeLine1[i] * num2)
			{
				num = i + 1;
				break;
			}
		}
		if (num > 0)
		{
			this.lastStep = num;
			switch (this.lastStep)
			{
			case 1:
				this.GenerateDrops(0.03f, 0.06f, 2f, 6);
				break;
			case 2:
				this.GenerateDrops(0.03f, 0.06f, 2f, 6);
				break;
			case 3:
				this.GenerateDrops(0.03f, 0.15f, 1.9f, 6);
				this.GenerateSmallDrop(0.03f, 0.15f, 1.9f, 6);
				break;
			case 4:
				this.GenerateDrops(0.03f, 0.15f, 1.8f, 6);
				break;
			case 5:
				this.GenerateDrops(0.02f, 0.15f, 1.5f, 6);
				this.GenerateSmallDrop(0.03f, 0.15f, 1.9f, 6);
				break;
			case 6:
				this.GenerateDrops(0.02f, 0.15f, 1.5f, 6);
				break;
			case 7:
				this.Disturb7();
				break;
			}
		}
	}

	private void Start()
	{
		this.startTime = Time.time;
		this.PlayInitialDisturb();
	}

	private void Update()
	{
	}

	public static GameObject CreateSplash(Transform CameraTransform, Vector3 coord, string prefabName, float inSize = 1f, float inSpeed = 1f, bool autodestruct = true, bool autostart = true, ParticleSystemSimulationSpace simulationSpace = 1)
	{
		float num = inSpeed;
		string text = prefabName;
		bool flag = true;
		bool flag2 = false;
		switch (SettingsManager.RenderQuality)
		{
		case RenderQualities.Fastest:
			flag2 = true;
			break;
		case RenderQualities.Fast:
			flag2 = true;
			break;
		case RenderQualities.Beautiful:
			flag = false;
			break;
		case RenderQualities.Fantastic:
		case RenderQualities.Ultra:
			flag = false;
			break;
		}
		if (((Application.platform == 2 && IntPtr.Size == 4) || (SystemInfo.systemMemorySize != 0 && SystemInfo.systemMemorySize <= 4100) || flag) && !flag2)
		{
			if (string.Compare(prefabName, "2D/Splashes/pSplash_universal", true) == 0)
			{
				text += "_lowres";
			}
		}
		else if (string.Compare(prefabName, "2D/Splashes/pSplash_universal", true) == 0 && Random.value > 0.8f && !flag2)
		{
			text = "2D/Splashes/pSplash_universal_2";
		}
		if (inSize != 1f && inSpeed == 1f)
		{
			num = 1.2f - (inSize - 1f) * 0.2f;
		}
		GameObject gameObject = (GameObject)Resources.Load(text, typeof(GameObject));
		if (gameObject == null)
		{
			throw new PrefabException(string.Format("splash: {0} prefab can't instantiate", text));
		}
		GameObject gameObject2 = Object.Instantiate<GameObject>(gameObject, coord, Quaternion.identity);
		if (CameraTransform != null)
		{
			gameObject2.transform.LookAt(CameraTransform);
		}
		DynWaterParticlesController component = gameObject2.GetComponent<DynWaterParticlesController>();
		if (component != null)
		{
			component.disturbSize = inSize;
			component.disturbSpeed = num;
			component.path = text;
			component.cameraTransform = CameraTransform;
		}
		ParticleSystem[] componentsInChildren = gameObject2.GetComponentsInChildren<ParticleSystem>();
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			particleSystem.playbackSpeed = num;
			if (particleSystem.name == "horizontal")
			{
				particleSystem.startRotation = (gameObject2.transform.rotation.eulerAngles.y - 90f) * 0.017453292f;
			}
			if (autostart)
			{
				particleSystem.Play();
			}
			particleSystem.startSize = inSize;
			particleSystem.simulationSpace = simulationSpace;
		}
		if (autodestruct)
		{
			Object.Destroy(gameObject2, 2f);
		}
		return gameObject2;
	}

	public float disturbSize = 1f;

	public float disturbSpeed = 1f;

	public string path;

	public Transform cameraTransform;

	private float startTime;

	private float lastTime;

	private float[] timeLine1 = new float[] { 0f, 0.2f, 0.4f, 0.6f, 0.7f, 0.9f, 1f, 1.1f };

	private int lastStep;

	private static GameObject cachedSplash;

	private static bool isCached;

	public enum ParticleType
	{
		Invalid,
		Splash,
		FishEmitter
	}

	public enum ParticleStart
	{
		Invalid,
		Manual,
		Auto
	}
}
