using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class RoomsChatInit : MenuBase
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> OnRoomsPopulation = delegate
	{
	};

	public int RoomsCount
	{
		get
		{
			return this._rooms.Count;
		}
	}

	private void Awake()
	{
		this.AlphaFade.ShowFinished += this.RoomsChatInit_ShowFinished;
	}

	private void OnDestroy()
	{
		if (this._isSubscribed && GameFactory.FishSpawner != null)
		{
			this._isSubscribed = false;
			GameFactory.FishSpawner.OnGamePaused -= this.FishSpawnerOnOnGamePaused;
		}
		if (this._isSubscribedRoomsPopulation)
		{
			this._isSubscribedRoomsPopulation = false;
			PhotonConnectionFactory.Instance.OnGotRoomsPopulation -= this.OnGotRoomsPopulation;
		}
	}

	private void Update()
	{
		if (!this._isSubscribed && GameFactory.FishSpawner != null)
		{
			this._isSubscribed = true;
			GameFactory.FishSpawner.OnGamePaused += this.FishSpawnerOnOnGamePaused;
		}
		if (SettingsManager.InputType == InputModuleManager.InputType.GamePad)
		{
			this.CheckPanel();
		}
		this._connectionTimer += Time.deltaTime;
		if (this._connectionTimer >= 10f)
		{
			if (StaticUserData.CurrentPond != null)
			{
				this.Init(StaticUserData.CurrentPond);
			}
			this._connectionTimer = 0f;
		}
	}

	protected override GameObject GetSelectedGameObject()
	{
		return this.toogle.gameObject;
	}

	protected override bool IsAnySelected()
	{
		return !this._rooms.Any((RoomChatHandler p) => p.Button.gameObject == UINavigation.CurrentSelectedGo);
	}

	protected override void ActivePanel(bool flag)
	{
		base.ActivePanel(flag);
		this.toogle.isOn = flag;
	}

	public void UpdateRoomsPopulation()
	{
		this._connectionTimer = 10f;
		this.Update();
	}

	private void Init(Pond pond)
	{
		if (GameFactory.FishSpawner != null && !GameFactory.FishSpawner.IsGamePaused && !this._isSubscribedRoomsPopulation)
		{
			LogHelper.Log("___kocha RoomsChatInit - GetRoomsPopulation for PondId:{0}", new object[] { pond.PondId });
			this._isSubscribedRoomsPopulation = true;
			PhotonConnectionFactory.Instance.OnGotRoomsPopulation += this.OnGotRoomsPopulation;
			PhotonConnectionFactory.Instance.GetRoomsPopulation(pond.PondId);
		}
	}

	private void OnGotRoomsPopulation(IList<RoomPopulation> roomsList)
	{
		string curRoomId = ((PhotonConnectionFactory.Instance.Room == null) ? null : PhotonConnectionFactory.Instance.Room.RoomId);
		LogHelper.Log("___kocha RoomsChatInit:OnGotRoomsPopulation curRoomId:{0} roomsList.Count:{1}", new object[]
		{
			curRoomId,
			(roomsList == null) ? 0 : roomsList.Count
		});
		PhotonConnectionFactory.Instance.OnGotRoomsPopulation -= this.OnGotRoomsPopulation;
		this._isSubscribedRoomsPopulation = false;
		if (roomsList == null || roomsList.Count == 0 || string.IsNullOrEmpty(curRoomId))
		{
			this.Clear();
		}
		else
		{
			List<RoomPopulation> list = roomsList.Where((RoomPopulation x) => x.RoomId != curRoomId).ToList<RoomPopulation>();
			if (!list.Select((RoomPopulation x) => x.RoomId).SequenceEqual(this._rooms.Select((RoomChatHandler x) => x.CurrentRoom.RoomId)))
			{
				this.Clear();
				for (int i = 0; i < list.Count; i++)
				{
					GameObject gameObject = GUITools.AddChild(this.ContentPanel, this.RoomChatItemPrefab);
					RoomChatHandler component = gameObject.GetComponent<RoomChatHandler>();
					component.Init(list[i], this.BackgroundPanel, i);
					this._rooms.Add(component);
				}
			}
		}
		this.OnRoomsPopulation(this._rooms.Count);
	}

	private void Clear()
	{
		this._rooms.ForEach(delegate(RoomChatHandler p)
		{
			p.Remove();
		});
		this._rooms.Clear();
	}

	private void RoomsChatInit_ShowFinished(object sender, EventArgs e)
	{
		this.Clear();
		if (StaticUserData.CurrentPond != null)
		{
			this.Init(StaticUserData.CurrentPond);
		}
	}

	private void FishSpawnerOnOnGamePaused(bool flag)
	{
		LogHelper.Log("___kocha RoomsChatInit:FishSpawnerOnOnGamePaused flag:{0}", new object[] { flag });
		if (!flag)
		{
			this.UpdateRoomsPopulation();
		}
	}

	public GameObject RoomChatItemPrefab;

	public GameObject ContentPanel;

	public Scrollbar Scrollbar;

	public GameObject BackgroundPanel;

	public Toggle toogle;

	private float _connectionTimer;

	private const float ConnectionTimerMax = 10f;

	private bool _isSubscribed;

	private bool _isSubscribedRoomsPopulation;

	private readonly List<RoomChatHandler> _rooms = new List<RoomChatHandler>();
}
