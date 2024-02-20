using System;
using ObjectModel;

public class ShowMap : PlayerStateBase
{
	protected override void OnCantOpenMenu(MainMenuPage page)
	{
		PlayerStateBase._savedPage = page;
	}

	protected override void onEnter()
	{
		base.Player.ShowSleeves = false;
		this._modeFsm = new TFSM<ShowMap.ShowMapState, ShowMap.ShowMapSignal>("ShowMap", ShowMap.ShowMapState.None);
		this._modeFsm.AddState(ShowMap.ShowMapState.None, null, null, null, 0f, ShowMap.ShowMapSignal.Start, null, null);
		this._modeFsm.AddState(ShowMap.ShowMapState.Enter, delegate
		{
			base.PlayTransitAnimation("ShowMapIn", 1, 1f);
		}, null, null, 0f, ShowMap.ShowMapSignal.Start, null, null);
		this._modeFsm.AddState(ShowMap.ShowMapState.Idle, delegate
		{
			this.PlayHandAnimation("ShowMapIdle", 1f);
		}, null, null, 0f, ShowMap.ShowMapSignal.Start, null, null);
		this._modeFsm.AddState(ShowMap.ShowMapState.Leave, delegate
		{
			base.PlayTransitAnimation("ShowMapOut", 3, 1f);
		}, null, null, 0f, ShowMap.ShowMapSignal.Start, null, null);
		this._modeFsm.AddTransition(ShowMap.ShowMapState.None, ShowMap.ShowMapState.Enter, ShowMap.ShowMapSignal.Start, null);
		this._modeFsm.AddTransition(ShowMap.ShowMapState.Enter, ShowMap.ShowMapState.Idle, ShowMap.ShowMapSignal.Ready, null);
		this._modeFsm.AddTransition(ShowMap.ShowMapState.Idle, ShowMap.ShowMapState.Leave, ShowMap.ShowMapSignal.Finishing, null);
		this._modeFsm.AddTransition(ShowMap.ShowMapState.Leave, ShowMap.ShowMapState.None, ShowMap.ShowMapSignal.Finished, delegate(ShowMap.ShowMapState f, ShowMap.ShowMapState t)
		{
			UIStatsCollector.ChangeGameScreen(GameScreenType.Game, GameScreenTabType.Undefined, null, null, null, null, null);
			base.PlaySound(PlayerStateBase.Sounds.TabletDrawOut);
			base.Player.Map.SetActive(false);
			ShowHudElements.Instance.ActivateGameMap(false);
		});
		UIStatsCollector.ChangeGameScreen(GameScreenType.Map, GameScreenTabType.Undefined, null, null, null, null, null);
		base.Player.Map.SetActive(true);
		if (base.Player.Tackle != null)
		{
			base.Player.Tackle.OnFsmUpdate();
		}
		ShowHudElements.Instance.ActivateGameMap(true);
		base.PlaySound(PlayerStateBase.Sounds.TabletDrawIn);
		this._modeFsm.SendSignal(ShowMap.ShowMapSignal.Start);
	}

	protected override Type onUpdate()
	{
		this._modeFsm.Update();
		ShowMap.ShowMapState curStateID = this._modeFsm.CurStateID;
		if (curStateID != ShowMap.ShowMapState.Enter)
		{
			if (curStateID != ShowMap.ShowMapState.Leave)
			{
				if (curStateID == ShowMap.ShowMapState.Idle)
				{
					if (base.Player.IsTransitionMapClosing)
					{
						this._modeFsm.SendSignal(ShowMap.ShowMapSignal.Finishing);
						base.Player.IsTransitionMapClosing = false;
					}
				}
			}
			else
			{
				base.UpdateTransitAnimation();
			}
		}
		else
		{
			base.UpdateTransitAnimation();
		}
		if (this._modeFsm.CurStateID != ShowMap.ShowMapState.None)
		{
			return null;
		}
		if (base.Player.IsSailing)
		{
			base.Player.OnCloseMap();
			return typeof(PlayerOnBoat);
		}
		return typeof(PlayerEmpty);
	}

	protected override void OnTransitAnimationFinished(byte signal)
	{
		base.OnTransitAnimationFinished(signal);
		this._modeFsm.SendSignal((ShowMap.ShowMapSignal)signal);
	}

	private TFSM<ShowMap.ShowMapState, ShowMap.ShowMapSignal> _modeFsm;

	private enum ShowMapState
	{
		None,
		Enter,
		Idle,
		Leave
	}

	private enum ShowMapSignal
	{
		Start,
		Ready,
		Finishing,
		Finished
	}
}
