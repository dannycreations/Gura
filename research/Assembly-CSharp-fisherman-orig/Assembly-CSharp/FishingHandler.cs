using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.PlayerProfile;
using ObjectModel;
using UnityEngine;

public class FishingHandler : MonoBehaviour
{
	public FeederFishingIndicator FeederFishingIndicator
	{
		get
		{
			return this._feederFishingIndicator;
		}
	}

	public BottomFishingIndicator BottomFishingIndicator
	{
		get
		{
			return this._bottomFishingIndicator;
		}
	}

	public FightIndicator CurFightIndicator
	{
		get
		{
			return this._currentFightIndicator;
		}
	}

	private void Start()
	{
		this._debugTrack = Object.Instantiate<DebugTrack>(this.DebugTrack, Vector3.zero, Quaternion.identity, base.transform);
		this.UpdateState();
		this.LurePositionHandlerContinuous.gameObject.SetActive(false);
		this.LurePositionHandlerContinuous.ShowDragStyleText(string.Empty, 0f);
		PhotonConnectionFactory.Instance.OnItemBroken += this.OnItemBroken;
		PhotonConnectionFactory.Instance.OnAchivementGained += this.OnAchivementGained;
	}

	private void OnDestroy()
	{
		PhotonConnectionFactory.Instance.OnItemBroken -= this.OnItemBroken;
		PhotonConnectionFactory.Instance.OnAchivementGained -= this.OnAchivementGained;
	}

	private void Update()
	{
		if (this.State != this._currentState || this._currentFightIndicator != SettingsManager.FightIndicator)
		{
			this.UpdateState();
		}
		if (ControlsController.ControlsActions.TrackShow.WasPressed)
		{
			this._debugTrack.gameObject.SetActive(!this._debugTrack.gameObject.activeSelf);
		}
	}

	public void ClearTrack()
	{
		this._debugTrack.ClearTrack();
	}

	public void DrawBottom(List<Vector2> points)
	{
		this._debugTrack.DrawBottom(points);
	}

	public void DrawPoint(Vector2 point, Vector2 pointFirst)
	{
		this._debugTrack.DrawPoint(point, pointFirst);
	}

	public void SetHintActive(bool flag)
	{
		this.BobberCtrl.HintActive = flag;
	}

	public void SetBobberColor(Color c)
	{
		this.BobberCtrl.SetBobberColor(c);
	}

	private void UpdateState()
	{
		this._currentState = this.State;
		switch (this._currentState)
		{
		case HudState.Fight:
			this.FrictionHandler.gameObject.SetActive(true);
			this._currentFightIndicator = SettingsManager.FightIndicator;
			this.CastSimpleHandler.gameObject.SetActive(false);
			this.CastTargetHandler.gameObject.SetActive(false);
			if (this._currentFightIndicator == FightIndicator.OneBand)
			{
				this.FightOneBandHandler.gameObject.SetActive(true);
				this.FightThreeBandHandler.gameObject.SetActive(false);
			}
			else
			{
				this.FightThreeBandHandler.gameObject.SetActive(true);
				this.FightOneBandHandler.gameObject.SetActive(false);
			}
			Debug.Log("Fishing indicator: Fight");
			break;
		case HudState.CastSimple:
			this.CastSimpleHandler.gameObject.SetActive(true);
			this.CastTargetHandler.gameObject.SetActive(false);
			this.FrictionHandler.gameObject.SetActive(true);
			this.FightOneBandHandler.gameObject.SetActive(false);
			this.FightThreeBandHandler.gameObject.SetActive(false);
			Debug.Log("Fishing indicator: CastSimple");
			break;
		case HudState.CastTarget:
			this.CastSimpleHandler.gameObject.SetActive(false);
			this.CastTargetHandler.gameObject.SetActive(true);
			this.FrictionHandler.gameObject.SetActive(true);
			this.FightOneBandHandler.gameObject.SetActive(false);
			this.FightThreeBandHandler.gameObject.SetActive(false);
			Debug.Log("Fishing indicator: CastTarget");
			break;
		case HudState.HandIdle:
			this.CastSimpleHandler.gameObject.SetActive(false);
			this.CastTargetHandler.gameObject.SetActive(false);
			this.FrictionHandler.gameObject.SetActive(false);
			this.FightOneBandHandler.gameObject.SetActive(false);
			this.FightThreeBandHandler.gameObject.SetActive(false);
			Debug.Log("Fishing indicator: HandIdle");
			break;
		case HudState.HandCastingSimple:
			this.CastSimpleHandler.gameObject.SetActive(true);
			this.CastTargetHandler.gameObject.SetActive(false);
			this.FrictionHandler.gameObject.SetActive(false);
			this.FightOneBandHandler.gameObject.SetActive(false);
			this.FightThreeBandHandler.gameObject.SetActive(false);
			Debug.Log("Fishing indicator: HandCastingSimple");
			break;
		}
	}

	private void OnAchivementGained(AchivementInfo achivement)
	{
		if (GameFactory.ChatListener != null)
		{
			GameFactory.ChatListener.OnLocalEvent(new LocalEvent
			{
				EventType = LocalEventType.Achivement,
				Player = PlayerProfileHelper.ProfileToPlayer(PhotonConnectionFactory.Instance.Profile),
				Achivement = achivement.AchivementName,
				AchivementStage = achivement.AchivementStageName,
				AchivementStageCount = achivement.AchivementStageCount
			});
		}
	}

	private void OnItemBroken(BrokenTackleType brokenTackleType, int rodSlot)
	{
		bool flag = false;
		if (flag)
		{
			GameFactory.ChatListener.OnLocalEvent(new LocalEvent
			{
				EventType = LocalEventType.TackleBroken,
				Player = new Player
				{
					UserName = PhotonConnectionFactory.Instance.Profile.Name,
					UserId = PhotonConnectionFactory.Instance.Profile.UserId.ToString(),
					Level = PhotonConnectionFactory.Instance.Profile.Level
				},
				TackleType = brokenTackleType
			});
		}
	}

	[SerializeField]
	private FeederFishingIndicator _feederFishingIndicator;

	[SerializeField]
	private BottomFishingIndicator _bottomFishingIndicator;

	[SerializeField]
	private DebugTrack DebugTrack;

	[SerializeField]
	private BobberIndicatorController BobberCtrl;

	private DebugTrack _debugTrack;

	public FrictionAndSpeedHandler FrictionHandler;

	public FightOneBandHandler FightOneBandHandler;

	public FightThreeBandHandler FightThreeBandHandler;

	public LineHandler LineHandler;

	public CastSimpleHandler CastSimpleHandler;

	public CastTargetHandler CastTargetHandler;

	public LurePositionHandler LurePositionHandler;

	public LurePositionHandlerContinuous LurePositionHandlerContinuous;

	public HudState State;

	private HudState _currentState;

	private FightIndicator _currentFightIndicator;
}
