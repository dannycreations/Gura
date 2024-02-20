using System;
using System.Collections;
using System.Linq;
using ObjectModel;
using UnityEngine;

public class CameraWetness : MonoBehaviour
{
	public static bool IsRainActive { get; set; }

	private void Awake()
	{
		this.audioRainGO = new GameObject("Rain audio");
		this.audioRainGO.transform.position = base.gameObject.transform.position;
		this.audioRainGO.hideFlags = 61;
		AudioSource audioSource = this.audioRainGO.AddComponent(typeof(AudioSource)) as AudioSource;
		this.audioDropsGO = new GameObject("Drops audio");
		this.audioDropsGO.transform.position = base.gameObject.transform.position;
		this.audioDropsGO.hideFlags = 61;
		AudioSource audioSource2 = this.audioDropsGO.AddComponent(typeof(AudioSource)) as AudioSource;
		this.audioThunderGO = new GameObject("Thunder audio");
		this.audioThunderGO.transform.position = base.gameObject.transform.position;
		this.audioThunderGO.hideFlags = 61;
		AudioSource audioSource3 = this.audioThunderGO.AddComponent(typeof(AudioSource)) as AudioSource;
	}

	private void Start()
	{
		if (CameraWetness.IsRainActive)
		{
			this.StartRain();
		}
	}

	private void Update()
	{
		CameraWetness.frameskipped++;
		if (CameraWetness.frameskipped > 10)
		{
			CameraWetness.frameskipped = 0;
			this.audioRainGO.transform.position = base.gameObject.transform.position;
			this.audioDropsGO.transform.position = base.gameObject.transform.position;
			this.audioThunderGO.transform.position = base.gameObject.transform.position;
			if (this.RainSound && this.audioRainGO != null)
			{
				AudioSource component = this.audioRainGO.GetComponent<AudioSource>();
				if (component && component.volume != 0f)
				{
					component.volume = ((!GlobalConsts.InGameVolume) ? 0f : (0.12f * GlobalConsts.BgVolume));
				}
			}
		}
	}

	public void UpdateVolume(bool flag)
	{
		if (this.RainSound && this.audioRainGO != null)
		{
			AudioSource component = this.audioRainGO.GetComponent<AudioSource>();
			if (component != null)
			{
				component.volume = ((!GlobalConsts.InGameVolume) ? 0f : (0.12f * GlobalConsts.BgVolume));
			}
		}
	}

	private void OnDestroy()
	{
		this.StopRain();
	}

	public void StartRain()
	{
		bool flag = PhotonConnectionFactory.Instance.Profile.Inventory.Any((InventoryItem x) => x.ItemSubType == ItemSubTypes.Hat && x.Storage == StoragePlaces.Doll);
		if (this.Rain)
		{
			this.Rain.SetActive(true);
		}
		if (this.MainCamera)
		{
			Defocus component = this.MainCamera.GetComponent<Defocus>();
			ScreenWater component2 = this.MainCamera.GetComponent<ScreenWater>();
			if (component)
			{
				component.enabled = true;
			}
			if (component2 && !flag)
			{
				component2.enabled = true;
			}
		}
		if (this.RainDrops && !flag)
		{
			GlobalConsts.LensWetnessRainActive = true;
		}
		if (this.RainSound && this.audioRainGO != null)
		{
			AudioSource component3 = this.audioRainGO.GetComponent<AudioSource>();
			if (component3)
			{
				if (Time.timeScale != 0f)
				{
					component3.pitch = Time.timeScale;
				}
				component3.clip = this.RainSound;
				component3.spatialBlend = 0f;
				component3.minDistance = 0.01f;
				component3.volume = 0.12f * GlobalConsts.BgVolume;
				component3.loop = true;
				if (!component3.isPlaying)
				{
					component3.Play();
				}
			}
		}
		this.isRainActive = true;
	}

	public void StopRain()
	{
		if (this.Rain)
		{
			this.Rain.SetActive(false);
		}
		if (this.MainCamera)
		{
			Defocus component = this.MainCamera.GetComponent<Defocus>();
			ScreenWater component2 = this.MainCamera.GetComponent<ScreenWater>();
			if (!this.isDropsActive && component)
			{
				component.enabled = false;
			}
			if (component2)
			{
				component2.enabled = false;
			}
		}
		if (this.RainDrops)
		{
			this.RainDrops.SetActive(false);
			GlobalConsts.LensWetnessRainActive = false;
		}
		if (this.RainSound && this.audioRainGO != null)
		{
			AudioSource component3 = this.audioRainGO.GetComponent<AudioSource>();
			if (component3 && component3.isPlaying)
			{
				component3.Stop();
			}
		}
		this.isRainActive = false;
	}

	public void EnableCameraDrops()
	{
		if (this.MainCamera)
		{
			Defocus component = this.MainCamera.GetComponent<Defocus>();
			if (component)
			{
				component.enabled = true;
			}
		}
		if (this.FishDrops)
		{
			GlobalConsts.LensWetnessFishActive = true;
		}
		if (this.DropsSound && this.audioDropsGO != null)
		{
			AudioSource component2 = this.audioDropsGO.GetComponent<AudioSource>();
			if (component2)
			{
				if (Time.timeScale != 0f)
				{
					component2.pitch = Time.timeScale;
				}
				component2.clip = this.DropsSound;
				component2.spatialBlend = 0f;
				component2.minDistance = 0.1f;
				component2.volume = 1f * GlobalConsts.BgVolume;
				if (!component2.isPlaying)
				{
					component2.Play();
				}
			}
		}
		this.isDropsActive = true;
		CameraWetness.shakeCount++;
	}

	public void DelayedStart()
	{
		base.StartCoroutine("DelayedDisable");
	}

	public IEnumerator DelayedDisable()
	{
		yield return new WaitForSeconds(3f);
		this.DisableCameraDrops();
		CameraWetness.shakeCount--;
		yield break;
	}

	public void DisableCameraDrops()
	{
		if (this.MainCamera)
		{
			Defocus component = this.MainCamera.GetComponent<Defocus>();
			if (!this.isRainActive && component)
			{
				component.enabled = false;
			}
		}
		if (this.FishDrops)
		{
			this.FishDrops.SetActive(false);
			GlobalConsts.LensWetnessFishActive = false;
		}
		if (this.DropsSound && this.audioDropsGO != null)
		{
			AudioSource component2 = this.audioDropsGO.GetComponent<AudioSource>();
			if (component2 && component2.isPlaying)
			{
				component2.Stop();
			}
		}
		this.isDropsActive = false;
	}

	public void StartThunder()
	{
		if (this.ThunderSound && this.audioThunderGO != null)
		{
			AudioSource component = this.audioThunderGO.GetComponent<AudioSource>();
			if (component)
			{
				if (Time.timeScale != 0f)
				{
					component.pitch = Time.timeScale;
				}
				component.clip = this.ThunderSound;
				component.spatialBlend = 1f;
				component.minDistance = 0.1f;
				component.volume = 1f * GlobalConsts.BgVolume;
				if (!component.isPlaying)
				{
					component.Play();
				}
			}
		}
	}

	private const float RAIN_SOUND_MODIFIER = 0.12f;

	public GameObject Rain;

	public Camera MainCamera;

	public GameObject FishDrops;

	public GameObject RainDrops;

	private RainTypes rainType;

	public bool Thunders;

	public bool isRainActive;

	public bool isDropsActive;

	public AudioClip RainSound;

	public AudioClip DropsSound;

	public AudioClip ThunderSound;

	private GameObject audioRainGO;

	private GameObject audioDropsGO;

	private GameObject audioThunderGO;

	private static int shakeCount;

	private static int frameskipped;
}
