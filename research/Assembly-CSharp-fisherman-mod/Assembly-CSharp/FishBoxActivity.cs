using System;
using System.Collections.Generic;
using UnityEngine;

public class FishBoxActivity : MonoBehaviour, IManualUpdate
{
	public ushort ServerId
	{
		get
		{
			return this._serverId;
		}
	}

	public GameObject GameObject
	{
		get
		{
			return base.gameObject;
		}
	}

	public Transform Scaler
	{
		get
		{
			return this._scaler;
		}
	}

	public bool IsPossibleToActivate
	{
		get
		{
			return this._serverId == 0 || this._isActivatedByServer;
		}
	}

	public bool IsCloseEnough
	{
		get
		{
			Transform playerTransform = GameFactory.PlayerTransform;
			return playerTransform != null && this._scaler != null && (this._scaler.position - playerTransform.position).magnitude <= 100f;
		}
	}

	public bool Choosen
	{
		get
		{
			return this._choosen;
		}
		set
		{
			this._choosen = value;
			base.gameObject.SetActive(value);
		}
	}

	protected virtual void Awake()
	{
		MeshRenderer component = base.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.enabled = false;
		}
		for (int i = 0; i < this._actionsSettings.Length; i++)
		{
			this._actionsMap[(byte)this._actionsSettings[i].activityType] = this._actionsSettings[i];
		}
		if (this._scaler == null)
		{
			this._scaler = base.transform;
		}
		else if (this._scaler != base.transform && base.transform.localScale != Vector3.one)
		{
			Vector3 localScale = base.transform.localScale;
			base.transform.localScale = Vector3.one;
			this._scaler.localScale = localScale;
		}
		this._isDebugFishActivityActive = false;
		this._isActivatedByServer = false;
	}

	public void SetServerVisibility(bool flag)
	{
		this._isActivatedByServer = flag;
	}

	public void MigrateToScaler()
	{
		if (this._scaler == null)
		{
			this._scaler = base.transform.Find("scaler");
		}
		if (this._scaler != null)
		{
			Vector3 localScale = base.transform.localScale;
			base.transform.localScale = Vector3.one;
			this._scaler.localScale = localScale;
		}
		else
		{
			LogHelper.Error("Scaler was not found", new object[0]);
		}
	}

	protected void OnDrawGizmos()
	{
	}

	protected void DrawZone(float rStartX, float rStartZ, float rWidth, float rHeight, Color color)
	{
		Color color2 = Gizmos.color;
		Gizmos.color = color;
		Vector3 globalPos = this.GetGlobalPos(rStartX, rStartZ);
		Vector3 globalPos2 = this.GetGlobalPos(rStartX + rWidth, rStartZ);
		Vector3 globalPos3 = this.GetGlobalPos(rStartX + rWidth, rStartZ + rHeight);
		Vector3 globalPos4 = this.GetGlobalPos(rStartX, rStartZ + rHeight);
		Gizmos.DrawLine(globalPos, globalPos2);
		Gizmos.DrawLine(globalPos2, globalPos3);
		Gizmos.DrawLine(globalPos3, globalPos4);
		Gizmos.DrawLine(globalPos4, globalPos);
		Gizmos.color = color2;
	}

	public virtual bool ManualUpdate()
	{
		float num = Time.time - this._lastUpdateAt;
		this._lastUpdateAt = Time.time;
		this._dt = ((num >= 0f && num <= 1f) ? num : Time.deltaTime);
		if (GameFactory.Player == null || (PhotonConnectionFactory.Instance.Game != null && GameFactory.GameIsPaused))
		{
			return false;
		}
		this.UpdateCurFishFrequency();
		if (this._stopActionAt > 0f && this._stopActionAt < Time.time)
		{
			this.StopAction();
		}
		if (!this._actionsMap.ContainsKey((byte)this._curFishDisturbanceFreq))
		{
			return true;
		}
		if (this._timeTillActionStart > 0f)
		{
			this._timeTillActionStart -= num;
			if (this._timeTillActionStart < 0f)
			{
				this.StartAction();
			}
		}
		else if (this._stopActionAt < 0f)
		{
			this.PrepareNextAction();
		}
		return true;
	}

	protected virtual void StartAction()
	{
		this._timeTillActionStart = -1f;
		this._stopActionAt = Time.time + this._actionDuration;
		this._zoneStartX = Random.Range(0f, 1f - this._actionZoneRelativeXSize);
		this._zoneStartZ = Random.Range(0f, 1f - this._actionZoneRelativeZSize);
	}

	protected virtual void StopAction()
	{
		this.PrepareNextAction();
	}

	protected virtual float GetRandomYaw()
	{
		return Random.Range(this._minYaw, this._maxYaw) * 0.017453292f;
	}

	protected Vector3 GetZoneCenterPos()
	{
		return this.GetGlobalPos(this._zoneStartX + this._actionZoneRelativeXSize / 2f, this._zoneStartZ + this._actionZoneRelativeZSize / 2f);
	}

	protected Vector3 GetZoneRandomPos()
	{
		float zoneRandomCoordinate = this.GetZoneRandomCoordinate(this._zoneStartX, this._actionZoneRelativeXSize);
		float zoneRandomCoordinate2 = this.GetZoneRandomCoordinate(this._zoneStartZ, this._actionZoneRelativeZSize);
		return this.GetGlobalPos(zoneRandomCoordinate, zoneRandomCoordinate2);
	}

	public static void PlayWaterContactEffect(Vector3 pos, string prefabName, float effectSize, string sound)
	{
		if (GameFactory.Player == null || !GlobalConsts.InGameVolume)
		{
			return;
		}
		DynWaterParticlesController.CreateSplash(GameFactory.PlayerTransform, pos, prefabName, effectSize, 1f, true, true, 1);
		RandomSounds.PlaySoundAtPoint(sound, pos, effectSize * 0.5f * SettingsManager.EnvironmentForcedVolume, false);
	}

	private float GetZoneRandomCoordinate(float zoneStartCoord, float zoneSize)
	{
		if (this._randomType == RandomType.MARSAGLIA)
		{
			return zoneStartCoord + zoneSize * 0.5f * (1f + RandomHelper.GetMarsaglia(1f, 0.2f, 0f));
		}
		return Random.Range(zoneStartCoord, zoneStartCoord + zoneSize);
	}

	private void PrepareNextAction()
	{
		this._stopActionAt = -1f;
		if (!this._actionsMap.ContainsKey((byte)this._curFishDisturbanceFreq))
		{
			this._timeTillActionStart = -1f;
			return;
		}
		FishActivitySettings fishActivitySettings = this._actionsMap[(byte)this._curFishDisturbanceFreq];
		this._timeTillActionStart = Random.Range(fishActivitySettings.minDelay, fishActivitySettings.maxDelay) * this._timeScale;
	}

	private Vector3 GetGlobalPos(float rx, float rz)
	{
		Vector3 vector;
		vector..ctor(rx - 0.5f, 0f, rz - 0.5f);
		return this._scaler.TransformPoint(vector);
	}

	private void UpdateCurFishFrequency()
	{
		if (this._serverId != 0)
		{
			this._curFishDisturbanceFreq = FishDisturbanceFreq.None;
			return;
		}
		this._curFishDisturbanceFreq = ((GameFactory.Water == null) ? FishDisturbanceFreq.Low : GameFactory.Water.FishDisturbanceFreq);
	}

	[Tooltip("Used for zone controlled by server")]
	[SerializeField]
	protected ushort _serverId;

	[SerializeField]
	protected float _distToPlayerToDisable = 6f;

	[Tooltip("Size proportional [0..1] to box full size in X dimension")]
	[SerializeField]
	protected float _actionZoneRelativeXSize = 0.1f;

	[Tooltip("Size proportional [0..1] to box full size in Z dimension")]
	[SerializeField]
	protected float _actionZoneRelativeZSize = 0.1f;

	[Tooltip("Yaw for direction sector")]
	[SerializeField]
	protected float _minYaw;

	[Tooltip("Yaw for direction sector")]
	[SerializeField]
	protected float _maxYaw = 360f;

	[Tooltip("Radius of the selected sector visualizer")]
	[SerializeField]
	protected float _debugArcR = 0.5f;

	[Tooltip("settings dependent on fish activity")]
	[SerializeField]
	protected FishActivitySettings[] _actionsSettings;

	[SerializeField]
	protected float _actionDuration;

	[SerializeField]
	protected RandomType _randomType;

	[Tooltip("Fill only if you have visible inner objects")]
	[SerializeField]
	private Transform _scaler;

	[SerializeField]
	private float _timeScale = 1f;

	[SerializeField]
	private bool _isDebugDraw;

	[Tooltip("debug feature to test each activity")]
	[SerializeField]
	public bool _isDebugFishActivityActive;

	[Tooltip("debug feature to test each activity")]
	[SerializeField]
	private FishDisturbanceFreq _debugFishActivity;

	private Dictionary<byte, FishActivitySettings> _actionsMap = new Dictionary<byte, FishActivitySettings>();

	private float _timeTillActionStart = -1f;

	private float _stopActionAt = -1f;

	private float _zoneStartX;

	private float _zoneStartZ;

	private FishDisturbanceFreq _curFishDisturbanceFreq;

	private bool _prevIsDebugFishActivityActive;

	[Tooltip("Don't change - will be updated by server")]
	[SerializeField]
	private bool _isActivatedByServer;

	private float _lastUpdateAt;

	protected float _dt;

	private bool _choosen;
}
