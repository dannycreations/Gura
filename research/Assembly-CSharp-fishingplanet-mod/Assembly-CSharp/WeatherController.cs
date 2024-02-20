using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using UnityEngine;

[RequireComponent(typeof(WindController))]
public class WeatherController : MonoBehaviour
{
	public static WeatherController Instance { get; private set; }

	public float DebugWindSpeed
	{
		get
		{
			return this._debugWindSpeed;
		}
	}

	public SceneFX SceneFX { get; private set; }

	public static void RegisterSound(EnvironmentSound sound)
	{
		if (WeatherController.Instance == null)
		{
			WeatherController._waitingSounds.Add(sound);
			sound.OnMute(true);
		}
		else
		{
			WeatherController instance = WeatherController.Instance;
			instance.EVolumeChanged = (Action<float>)Delegate.Combine(instance.EVolumeChanged, new Action<float>(sound.OnEnvironmentVolumeChanged));
			WeatherController instance2 = WeatherController.Instance;
			instance2.EMute = (Action<bool>)Delegate.Combine(instance2.EMute, new Action<bool>(sound.OnMute));
			sound.OnMute(WeatherController.Instance._isMuted);
		}
	}

	public static void UnregisterSound(EnvironmentSound sound)
	{
		if (WeatherController.Instance == null)
		{
			WeatherController._waitingSounds.Remove(sound);
		}
		else
		{
			WeatherController instance = WeatherController.Instance;
			instance.EVolumeChanged = (Action<float>)Delegate.Remove(instance.EVolumeChanged, new Action<float>(sound.OnEnvironmentVolumeChanged));
			WeatherController instance2 = WeatherController.Instance;
			instance2.EMute = (Action<bool>)Delegate.Remove(instance2.EMute, new Action<bool>(sound.OnMute));
		}
	}

	public static void UpdateEnvironmentVolume()
	{
		if (WeatherController.Instance == null)
		{
			for (int i = 0; i < WeatherController._waitingSounds.Count; i++)
			{
				WeatherController._waitingSounds[i].OnEnvironmentVolumeChanged(0f);
			}
		}
		else
		{
			WeatherController.Instance.UpdateVolume();
		}
	}

	private void Awake()
	{
		for (int i = 0; i < 4; i++)
		{
			Dictionary<string, FishDisturbanceFreq> fishDisturbanceFreqStrToEnum = this._fishDisturbanceFreqStrToEnum;
			FishDisturbanceFreq fishDisturbanceFreq = (FishDisturbanceFreq)i;
			fishDisturbanceFreqStrToEnum[fishDisturbanceFreq.ToString()] = (FishDisturbanceFreq)i;
		}
		this._windController = base.GetComponent<WindController>();
		if (this._soundGroups.Count == 5)
		{
			WeatherController.GroupRecord[] array = this._soundGroups.OrderBy((WeatherController.GroupRecord g) => g.GroupType).ToArray<WeatherController.GroupRecord>();
			for (int j = 0; j < array.Length; j++)
			{
				this._orderedSounds[j] = array[j].Randomizers;
			}
		}
		else
		{
			this._orderedSounds = new SoundRandomizer[0][];
			LogHelper.Error("You must fill all sound groups to continue with WeatherController", new object[0]);
		}
		this._todObjects[0] = this.morningObjects;
		this._todObjects[1] = this.dayObjects;
		this._todObjects[2] = this.eveningObjects;
		this._todObjects[3] = this.nightObjects;
		this._todObjects[4] = new GameObject[0];
		WeatherController.Instance = this;
		for (int k = 0; k < WeatherController._waitingSounds.Count; k++)
		{
			this.EVolumeChanged = (Action<float>)Delegate.Combine(this.EVolumeChanged, new Action<float>(WeatherController._waitingSounds[k].OnEnvironmentVolumeChanged));
			this.EMute = (Action<bool>)Delegate.Combine(this.EMute, new Action<bool>(WeatherController._waitingSounds[k].OnMute));
		}
		WeatherController._waitingSounds.Clear();
		this.SceneFX = base.GetComponent<SceneFX>();
		this._fogColor = new Dictionary<string, Color>
		{
			{ "SoftFog", this.SoftFog },
			{ "Fog", this.MediumFog },
			{ "HeavyFog", this.HardFog }
		};
	}

	private void Start()
	{
		this.ActivateAllDaySounds(true);
		if (this._isMuted)
		{
			for (int i = 0; i < this._allDaySounds.Length; i++)
			{
				this._allDaySounds[i].Mute(true);
			}
		}
		this.EVolumeChanged(this._windController.CurWindObjectVolumeMultiplier);
	}

	public void Activate()
	{
		this._windController = base.GetComponent<WindController>();
		base.gameObject.SetActive(true);
		this.Mute(false);
	}

	private void Update()
	{
		this.UpdateWater();
		if (TimeAndWeatherManager.CurrentTime != null && TimeAndWeatherManager.CurrentWeather != null)
		{
			bool flag = this.precipitation != TimeAndWeatherManager.CurrentWeather.Precipitation;
			if (flag)
			{
				this.precipitation = TimeAndWeatherManager.CurrentWeather.Precipitation;
				this.UpdateParticleFog(this.precipitation);
				this._fogInited = false;
			}
			TodGroup todGroup = this.GetTodGroup(TimeAndWeatherManager.CurrentTime.Value.ToTimeOfDay());
			if (flag || todGroup != this._objectsGroup)
			{
				this.UpdateTod((!this.IsRain(this.precipitation)) ? todGroup : TodGroup.Rain, todGroup);
			}
			if (!this._fogInited)
			{
				this._lerpValue += Time.deltaTime / 16f;
				this.ChangeFogWithoutBlending(this._lerpValue);
				this.ChangeWind(TimeAndWeatherManager.CurrentWeather);
				if (this._lerpValue >= 1f)
				{
					this._fogInited = true;
					this._lerpValue = 0f;
				}
			}
		}
		this._fogTimer += Time.deltaTime;
		if (this._fogTimer >= 5f)
		{
			this.ChangeFog();
			if (TimeAndWeatherManager.CurrentWeather != null)
			{
				this.ChangeWind(TimeAndWeatherManager.CurrentWeather);
			}
			this._fogTimer = 0f;
		}
	}

	private void UpdateWater()
	{
		if (GameFactory.Water == null)
		{
			return;
		}
		if (TimeAndWeatherManager.CurrentWeather == null)
		{
			GameFactory.Water.FishDisturbanceFreq = FishDisturbanceFreq.None;
			return;
		}
		string fishPlayFreq = TimeAndWeatherManager.CurrentWeather.FishPlayFreq;
		GameFactory.Water.FishDisturbanceFreq = ((fishPlayFreq == null || !this._fishDisturbanceFreqStrToEnum.ContainsKey(fishPlayFreq)) ? FishDisturbanceFreq.None : this._fishDisturbanceFreqStrToEnum[fishPlayFreq]);
	}

	[Conditional("UNITY_EDITOR")]
	private static void ManualChangeSky()
	{
		if (ControlsController.ControlsActions != null && ControlsController.ControlsActions.ChangeWeather.WasPressed)
		{
			int num = Array.IndexOf<string>(WeatherController._skies, TimeAndWeatherManager.CurrentWeather.Sky);
			if (num == -1)
			{
				num = 0;
			}
			else
			{
				num = ((num >= WeatherController._skies.Length) ? 0 : (num + 1));
			}
			TimeAndWeatherManager.DevSky = WeatherController._skies[num];
		}
	}

	private void ApplySSAO(float distance)
	{
		if (distance == 0f)
		{
			return;
		}
		if (GameFactory.Player != null && GameFactory.Player.CameraController != null && GameFactory.Player.CameraController.Camera != null)
		{
			float num = 0.3f / distance;
			num = Math.Min(150f, num);
			GameFactory.Player.CameraController.Camera.GetComponent<SSAOPro>().CutoffDistance = num;
			GameFactory.Player.CameraController.Camera.GetComponent<SSAOPro>().CutoffFalloff = num * 0.3f;
		}
	}

	private void ChangeFog()
	{
		if (TimeAndWeatherManager.CurrentTime == null || TimeAndWeatherManager.CurrentWeather == null)
		{
			return;
		}
		if (TimeAndWeatherManager.NextWeather == null)
		{
			return;
		}
		if (!RenderSettings.fog || RenderSettings.fogMode != 2)
		{
			RenderSettings.fog = true;
			RenderSettings.fogMode = 2;
		}
		TimeSpan value = TimeAndWeatherManager.CurrentTime.Value;
		DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
		dateTime = dateTime.Add(value);
		TimeOfDay timeOfDay = TimeAndWeatherManager.CurrentTime.Value.ToTimeOfDay();
		string text = TimeAndWeatherManager.CurrentWeather.Precipitation;
		string text2 = TimeAndWeatherManager.NextWeather.Precipitation;
		if (TimeAndWeatherManager.CurrentTime.Value.AddHours(1.0).ToTimeOfDay() != timeOfDay)
		{
			Color color = Color.Lerp(this.GetFogColor(text, timeOfDay), this.GetFogColor(text2, timeOfDay), (float)dateTime.Minute / 60f);
			RenderSettings.fogColor = color;
			float num = Mathf.Lerp(this.GetFogDistance(text), this.GetFogDistance(text2), (float)dateTime.Minute / 60f);
			RenderSettings.fogDensity = num;
			this.ApplySSAO(num);
		}
		else if (RenderSettings.fogColor != this.GetFogColor(text, timeOfDay))
		{
			this._fogInited = false;
		}
	}

	private void ChangeFogWithoutBlending(float lerpValue)
	{
		if (TimeAndWeatherManager.CurrentTime == null || TimeAndWeatherManager.CurrentWeather == null)
		{
			return;
		}
		if (TimeAndWeatherManager.NextWeather == null)
		{
			return;
		}
		if (!RenderSettings.fog || RenderSettings.fogMode != 2)
		{
			RenderSettings.fog = true;
			RenderSettings.fogMode = 2;
		}
		TimeSpan value = TimeAndWeatherManager.CurrentTime.Value;
		DateTime dateTime = new DateTime(2000, 1, 1, 0, 0, 0);
		dateTime = dateTime.Add(value);
		TimeOfDay timeOfDay = value.ToTimeOfDay();
		string text = TimeAndWeatherManager.CurrentWeather.Precipitation;
		string text2 = TimeAndWeatherManager.NextWeather.Precipitation;
		Color fogColor = this.GetFogColor(text, timeOfDay);
		if (RenderSettings.fogColor != fogColor)
		{
			float num2;
			if (TimeAndWeatherManager.CurrentTime.Value.AddHours(1.0).ToTimeOfDay() != timeOfDay)
			{
				Color color = Color.Lerp(fogColor, this.GetFogColor(text2, timeOfDay), (float)dateTime.Minute / 60f);
				float num = Mathf.Lerp(this.GetFogDistance(text), this.GetFogDistance(text2), (float)dateTime.Minute / 60f);
				RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, color, lerpValue);
				num2 = Mathf.Lerp(RenderSettings.fogDensity, num, lerpValue);
			}
			else
			{
				RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, fogColor, lerpValue);
				num2 = Mathf.Lerp(RenderSettings.fogDensity, this.GetFogDistance(text), lerpValue);
			}
			RenderSettings.fogDensity = num2;
			this.ApplySSAO(num2);
		}
	}

	private bool IsRain(string fog)
	{
		return this._rainFogs.Contains(fog);
	}

	private TodGroup GetTodGroup(TimeOfDay timeOfDay)
	{
		switch (timeOfDay)
		{
		case TimeOfDay.EarlyMorning:
		case TimeOfDay.Morning:
			return TodGroup.Morning;
		case TimeOfDay.Midday:
			return TodGroup.Day;
		case TimeOfDay.Evening:
		case TimeOfDay.LateEvening:
			return TodGroup.Evening;
		case TimeOfDay.EarlyNight:
		case TimeOfDay.MidNight:
		case TimeOfDay.LateNight:
			return TodGroup.Night;
		default:
			LogHelper.Error("Unsupported TOD {0}. Midday group used", new object[] { timeOfDay });
			return TodGroup.Day;
		}
	}

	private void UpdateTod(TodGroup soundsGroup, TodGroup objectsGroup)
	{
		if (soundsGroup != this._soundsGroup || objectsGroup != this._objectsGroup)
		{
			if (this._soundsGroup != null)
			{
				TodGroup? soundsGroup2 = this._soundsGroup;
				if ((int)((byte)soundsGroup2.Value) < this._orderedSounds.Length)
				{
					GameObject[][] todObjects = this._todObjects;
					TodGroup? objectsGroup2 = this._objectsGroup;
					this.ActivateObjectsGroup(todObjects[(int)((byte)objectsGroup2.Value)], false);
					this.ActiveSoundGroup(this._soundsGroup.Value, false);
				}
			}
			this._soundsGroup = new TodGroup?(soundsGroup);
			this._objectsGroup = new TodGroup?(objectsGroup);
			if ((int)((byte)soundsGroup) >= this._orderedSounds.Length)
			{
				LogHelper.Error("Can't set TOD ({0}, {1}) - not all sounds are defined", new object[] { soundsGroup, objectsGroup });
				return;
			}
			if (soundsGroup == TodGroup.Rain)
			{
				GameObject[][] todObjects2 = this._todObjects;
				TodGroup? objectsGroup3 = this._objectsGroup;
				this.ActivateObjectsGroupWhenRain(todObjects2[(int)((byte)objectsGroup3.Value)]);
			}
			else
			{
				GameObject[][] todObjects3 = this._todObjects;
				TodGroup? objectsGroup4 = this._objectsGroup;
				this.ActivateObjectsGroup(todObjects3[(int)((byte)objectsGroup4.Value)], true);
			}
			this.ActiveSoundGroup(soundsGroup, true);
			this.EnableBirds(soundsGroup != TodGroup.Night && soundsGroup != TodGroup.Rain);
			this.EnableNightLights(soundsGroup == TodGroup.Night);
			if (PhotonConnectionFactory.Instance.Profile != null && GameFactory.Player != null && GameFactory.Player.CameraWetness != null)
			{
				if (soundsGroup == TodGroup.Rain)
				{
					GameFactory.Player.CameraWetness.StartRain();
				}
				else
				{
					GameFactory.Player.CameraWetness.StopRain();
				}
			}
		}
	}

	public bool DebugRain
	{
		get
		{
			return this._isDebugRain;
		}
	}

	public void SetTod(TodGroup groupID)
	{
		this.UpdateTod((!this._isDebugRain) ? groupID : TodGroup.Rain, groupID);
	}

	public void SetRain(bool flag)
	{
		this._isDebugRain = flag;
		TodGroup? objectsGroup = this._objectsGroup;
		this.SetTod((objectsGroup == null) ? TodGroup.Morning : objectsGroup.Value);
	}

	public bool IsGroupActive(TodGroup groupID)
	{
		return this._objectsGroup == groupID;
	}

	public void ActivateAllDaySounds(bool flag)
	{
		for (int i = 0; i < this._allDaySounds.Length; i++)
		{
			this._allDaySounds[i].SetActive(flag);
		}
	}

	public bool IsAllDaySoundsActive()
	{
		return this._allDaySounds.Length > 0 && this._allDaySounds[0].Enabled;
	}

	private void ActiveSoundGroup(TodGroup groupID, bool flag)
	{
		CameraWetness.IsRainActive = groupID == TodGroup.Rain && flag;
		SoundRandomizer[] array = this._orderedSounds[(int)((byte)groupID)];
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetActive(flag);
			if (this._isMuted)
			{
				array[i].Mute(true);
			}
		}
	}

	private void ActivateObjectsGroup(GameObject[] objects, bool flag)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(flag);
		}
	}

	private void ActivateObjectsGroupWhenRain(GameObject[] objects)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].SetActive(!this.rainDamperedParticles.Contains(objects[i]));
		}
	}

	private void UpdateParticleFog(string fog)
	{
		bool flag = fog == "SoftFog" || fog == "Fog" || fog == "HeavyFog";
		for (int i = 0; i < this.fogParticles.Length; i++)
		{
			ParticleSystem particleSystem = this.fogParticles[i];
			if (!flag)
			{
				particleSystem.Stop();
			}
			else if (!particleSystem.isPlaying)
			{
				particleSystem.Play();
			}
		}
	}

	private Color GetFogColor(string fog, TimeOfDay timeOfDay)
	{
		if (timeOfDay.IsNightFrame())
		{
			return this.NightFog;
		}
		if (this.IsRain(fog))
		{
			return this.RainFog;
		}
		return (fog != null && this._fogColor.ContainsKey(fog)) ? this._fogColor[fog] : this.NoFog;
	}

	private float GetFogDistance(string fog)
	{
		return (fog != null && this._fogDistance.ContainsKey(fog)) ? this._fogDistance[fog] : 0.0013f;
	}

	public void UpdateDebugWind()
	{
		this._windController.SetWind(this._debugWindSpeed);
	}

	public void SetWind(WeatherDesc weather)
	{
		if (Application.isPlaying)
		{
			return;
		}
		this.ChangeWind(weather);
	}

	private void ChangeWind(WeatherDesc weather)
	{
		if (weather.WindDirection != this._currentWindDirection)
		{
			Vector3 windRotation = this.GetWindRotation(weather.WindDirection);
			base.transform.rotation = Quaternion.Euler(windRotation);
			this._currentWindDirection = weather.WindDirection;
			Vector3 vector = Quaternion.Euler(0f, windRotation.y + 3f, 0f) * Vector3.forward;
			vector = Vector3.Normalize(vector);
			this.SceneFX.Wind = new Vector4(vector.x, vector.y, vector.z, this.SceneFX.Wind.w);
			float num = 0f;
			if (this.Sky3D != null)
			{
				num = this.Sky3D.transform.eulerAngles.y;
			}
			this.SceneFX.WindAngle = num + 3f - CompassController.GetWindRotation();
		}
		if (Math.Abs(weather.WindSpeed - this._currentWindSpeed) > 0.001f)
		{
			this.SceneFX.Wind.w = Mathf.Lerp(0.1f, 1.5f, weather.WindSpeed / 3f);
			this._currentWindSpeed = weather.WindSpeed;
			if (SurfaceSettings.Terrain != null)
			{
				SurfaceSettings.Terrain.terrainData.wavingGrassStrength = Mathf.Lerp(0.05f, 0.5f, weather.WindSpeed / 3f);
			}
			this._windController.SetWind(this._currentWindSpeed);
			this.EVolumeChanged(this._windController.CurWindObjectVolumeMultiplier);
			if (this._isMuted)
			{
				this._windController.Mute(true);
			}
		}
	}

	private GameObject Sky3D
	{
		get
		{
			if (this._sky3D == null)
			{
				this._sky3D = GameObject.Find("3DSky");
			}
			return this._sky3D;
		}
	}

	private Vector3 GetWindRotation(string windDirection)
	{
		Vector3 vector = default(Vector3);
		GameObject sky3D = this.Sky3D;
		if (sky3D != null)
		{
			vector = sky3D.transform.rotation.eulerAngles;
		}
		switch (windDirection)
		{
		case "N":
			return new Vector3(vector.x, vector.y + 180f, vector.z);
		case "S":
			return new Vector3(vector.x, vector.y, vector.z);
		case "W":
			return new Vector3(vector.x, vector.y + 90f, vector.z);
		case "E":
			return new Vector3(vector.x, vector.y + 270f, vector.z);
		case "NE":
			return new Vector3(vector.x, vector.y + 225f, vector.z);
		case "NW":
			return new Vector3(vector.x, vector.y + 135f, vector.z);
		case "SE":
			return new Vector3(vector.x, vector.y + 315f, vector.z);
		case "SW":
			return new Vector3(vector.x, vector.y + 45f, vector.z);
		}
		return vector;
	}

	private void EnableNightLights(bool flag)
	{
		for (int i = 0; i < this.nightLights.Length; i++)
		{
			if (this.nightLights[i] != null)
			{
				this.nightLights[i].enabled = flag;
			}
		}
	}

	private void EnableBirds(bool flag)
	{
		for (int i = 0; i < this.birds.Length; i++)
		{
			this.birds[i].Enable(flag);
		}
	}

	public void UpdateVolume()
	{
		this.EVolumeChanged(this._windController.CurWindObjectVolumeMultiplier);
		this._windController.UpdateVolume();
		for (int i = 0; i < this._allDaySounds.Length; i++)
		{
			this._allDaySounds[i].UpdateVolume();
		}
		if (this._soundsGroup != null)
		{
			TodGroup? soundsGroup = this._soundsGroup;
			if ((int)((byte)soundsGroup.Value) < this._orderedSounds.Length)
			{
				SoundRandomizer[][] orderedSounds = this._orderedSounds;
				TodGroup? soundsGroup2 = this._soundsGroup;
				SoundRandomizer[] array = orderedSounds[(int)((byte)soundsGroup2.Value)];
				for (int j = 0; j < array.Length; j++)
				{
					array[j].UpdateVolume();
				}
				return;
			}
		}
	}

	public void Mute(bool flag)
	{
		this.EMute(flag);
		this._isMuted = flag;
		this._windController.Mute(flag);
		for (int i = 0; i < this.birds.Length; i++)
		{
			this.birds[i].Mute(flag);
		}
		for (int j = 0; j < this._allDaySounds.Length; j++)
		{
			this._allDaySounds[j].Mute(flag);
		}
		if (this._soundsGroup != null)
		{
			TodGroup? soundsGroup = this._soundsGroup;
			if ((int)((byte)soundsGroup.Value) < this._orderedSounds.Length)
			{
				SoundRandomizer[][] orderedSounds = this._orderedSounds;
				TodGroup? soundsGroup2 = this._soundsGroup;
				SoundRandomizer[] array = orderedSounds[(int)((byte)soundsGroup2.Value)];
				for (int k = 0; k < array.Length; k++)
				{
					array[k].Mute(flag);
				}
				return;
			}
		}
	}

	private void OnDestroy()
	{
		CameraWetness.IsRainActive = false;
		WeatherController.Instance = null;
	}

	[Range(0f, 5f)]
	[SerializeField]
	private float _debugWindSpeed;

	private const float MinWindZoneMainByWater = 0.1f;

	private const float MaxWindZoneMainByWater = 1.5f;

	private const float MinWindZoneMainByGrass = 0.05f;

	private const float MaxWindZoneMainByGrass = 0.5f;

	private Dictionary<string, Color> _fogColor;

	private float _fogTimer;

	private const float FogTimerMax = 5f;

	private bool _fogInited;

	private string _currentWindDirection = string.Empty;

	private float _currentWindSpeed = -1f;

	private float _lerpValue;

	private bool _isMuted = true;

	private TodGroup? _soundsGroup;

	private TodGroup? _objectsGroup;

	public Color HardFog = new Color(0.58431375f, 0.6156863f, 0.64705884f, 1f);

	public Color MediumFog = new Color(0.5647059f, 0.6156863f, 0.64705884f, 1f);

	public Color SoftFog = new Color(0.53333336f, 0.6f, 0.6784314f, 1f);

	public Color NoFog = new Color(0.5019608f, 0.6f, 0.69803923f, 1f);

	public Color NightFog = new Color(0.10980392f, 0.10980392f, 0.10980392f, 1f);

	public Color RainFog = new Color(0.24705882f, 0.25490198f, 0.26666668f, 1f);

	public float FogScale = 1f;

	public ParticleSystem[] fogParticles;

	public GameObject[] rainDamperedParticles;

	public FlockController[] birds;

	public GameObject[] dayObjects;

	public GameObject[] morningObjects;

	public GameObject[] eveningObjects;

	public GameObject[] nightObjects;

	public Light[] nightLights;

	public Action<float> EVolumeChanged = delegate
	{
	};

	public Action<bool> EMute = delegate
	{
	};

	private WindController _windController;

	[SerializeField]
	private SoundRandomizer[] _allDaySounds;

	[SerializeField]
	private List<WeatherController.GroupRecord> _soundGroups;

	private SoundRandomizer[][] _orderedSounds = new SoundRandomizer[5][];

	private GameObject[][] _todObjects = new GameObject[5][];

	private static List<EnvironmentSound> _waitingSounds = new List<EnvironmentSound>();

	private Dictionary<string, FishDisturbanceFreq> _fishDisturbanceFreqStrToEnum = new Dictionary<string, FishDisturbanceFreq>();

	private static readonly string[] _skies = new string[] { "Clear", "Cloudy", "HeavyCloudy", "Unclear" };

	private string precipitation = string.Empty;

	private readonly HashSet<string> _rainFogs = new HashSet<string> { "Rain", "HeavyRain", "ThunderStorm" };

	private bool _isDebugRain;

	private readonly Dictionary<string, float> _fogDistance = new Dictionary<string, float>
	{
		{ "SoftFog", 0.013f },
		{ "Fog", 0.016f },
		{ "HeavyFog", 0.035f },
		{ "Rain", 0.013f },
		{ "HeavyRain", 0.016f },
		{ "ThunderStorm", 0.018f }
	};

	private GameObject _sky3D;

	[Serializable]
	private class GroupRecord
	{
		public TodGroup GroupType;

		public SoundRandomizer[] Randomizers;
	}
}
