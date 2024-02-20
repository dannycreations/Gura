using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using UnityEngine;

public class TournamentManager : MonoBehaviour
{
	public static TournamentManager Instance
	{
		get
		{
			if (CompetitionMapController.Instance != null)
			{
				return CompetitionMapController.Instance.TournamentMgr;
			}
			return SetPondsOnGlobalMap.TournamentMgr;
		}
	}

	public List<UserCompetitionPublic> UserGeneratedCompetitions
	{
		get
		{
			return TournamentManager._lastFiltered;
		}
	}

	public static UserCompetitionPublic CurrentUserGeneratedCompetition
	{
		get
		{
			return TournamentManager._lastFiltered.FirstOrDefault<UserCompetitionPublic>();
		}
	}

	public List<Tournament> MyTournaments
	{
		get
		{
			return this._myTournaments;
		}
	}

	public void Awake()
	{
		this._registeredFilter.RequestId = this._requestIdGetUserCompetitions;
		this._registeredFilter.IamRegisteredIn = true;
		this._registeredFilter.IamStillParticipating = true;
		base.InvokeRepeating("UpdateOnceASecond", 1f, 1f);
	}

	private void OnDestroy()
	{
		this.Unsubscribe();
		this.UnsubscribeRemoveOwnerCompetition();
		this.UnsubscribeOnUnregisterFromCompetition();
		this.UnsubscribeMyTournaments();
		this.UnsubscribeUnregisteredFromTournament();
	}

	private void UpdateOnceASecond()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null)
		{
			return;
		}
		ProfileTournament tournament = profile.Tournament;
		if (tournament != null)
		{
			bool flag = false;
			if (PhotonConnectionFactory.Instance.TournamentRemainingTime != null && PhotonConnectionFactory.Instance.TournamentRemainingTimeReceived != null)
			{
				flag = PhotonConnectionFactory.Instance.TournamentRemainingTime.Value.Subtract(DateTime.UtcNow - PhotonConnectionFactory.Instance.TournamentRemainingTimeReceived.Value).TotalSeconds <= 0.0;
			}
			if ((tournament.IsEnded || tournament.EndDate < PhotonConnectionFactory.Instance.ServerUtcNow || flag) && (this._tournamentIdOnEnd == null || this._tournamentIdOnEnd.Value != tournament.TournamentId))
			{
				this.TournamentEndedOnClient(tournament);
			}
		}
	}

	private void TournamentEndedOnClient(ProfileTournament tournament)
	{
		if (PondControllers.Instance != null && !PondControllers.Instance.IsInMenu && GameFactory.Message != null)
		{
			GameFactory.Message.ShowEntryTimeIsOver();
		}
		this._tournamentIdOnEnd = new int?(tournament.TournamentId);
		PhotonConnectionFactory.Instance.EndTournament(false);
		if (this.OnTournamentEndedOnCient != null)
		{
			this.OnTournamentEndedOnCient();
		}
	}

	public void FullRefresh()
	{
		this.Refresh();
		this.RefreshMyTournaments();
	}

	public void Refresh()
	{
		LogHelper.Log("___kocha TM:Refresh()");
		TournamentManager._lastFiltered.Clear();
		this.Subscribe();
		PhotonConnectionFactory.Instance.GetUserCompetitions(this._registeredFilter);
	}

	public void RefreshMyTournaments()
	{
		LogHelper.Log("___kocha TM:RefreshMyTournaments()");
		this._myTournaments.Clear();
		this.SubscribeMyTournaments();
		PhotonConnectionFactory.Instance.GetMyTournaments(null);
	}

	private void OnGetUserCompetitionsFailed(UserCompetitionFailure failure)
	{
		this.Unsubscribe();
		Debug.LogError(failure.FullErrorInfo);
	}

	private void Instance_OnGetUserCompetitions(Guid requestId, List<UserCompetitionPublic> list)
	{
		if (this._requestIdGetUserCompetitions != requestId)
		{
			return;
		}
		this.Unsubscribe();
		TournamentManager._lastFiltered = list.Where((UserCompetitionPublic x) => x.IsRegistered && ((x.ApproveStatus == UserCompetitionApproveStatus.Approved || !x.IsSponsored) & !x.IsDisqualified) && !x.IsDone).ToList<UserCompetitionPublic>();
		LogHelper.Log("___kocha TM:GetUserCompetitions count--> {0}", new object[] { TournamentManager._lastFiltered.Count });
		if (this.OnRefreshed != null)
		{
			this.OnRefreshed();
		}
	}

	private void Subscribe()
	{
		PhotonConnectionFactory.Instance.OnGetUserCompetitions += this.Instance_OnGetUserCompetitions;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetitions += this.OnGetUserCompetitionsFailed;
	}

	private void Unsubscribe()
	{
		PhotonConnectionFactory.Instance.OnGetUserCompetitions -= this.Instance_OnGetUserCompetitions;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetitions -= this.OnGetUserCompetitionsFailed;
	}

	private void SubscribeMyTournaments()
	{
		PhotonConnectionFactory.Instance.OnGotMyTournaments += this.GotMyTournaments;
		PhotonConnectionFactory.Instance.OnGettingMyTournamentsFailed += this.GettingMyTournamentsFailed;
	}

	private void UnsubscribeMyTournaments()
	{
		PhotonConnectionFactory.Instance.OnGotMyTournaments -= this.GotMyTournaments;
		PhotonConnectionFactory.Instance.OnGettingMyTournamentsFailed -= this.GettingMyTournamentsFailed;
	}

	private void GotMyTournaments(List<Tournament> tournaments)
	{
		this.UnsubscribeMyTournaments();
		this._myTournaments = tournaments.Where((Tournament t) => !t.IsUgc()).ToList<Tournament>();
		LogHelper.Log("___kocha TM:GotMyTournaments count--> {0}", new object[] { tournaments.Count });
		this.OnRefreshedTournaments();
	}

	private void GettingMyTournamentsFailed(Failure failure)
	{
		this.UnsubscribeMyTournaments();
		Debug.LogError(failure.FullErrorInfo);
	}

	public void UnregFromTournament(Tournament t, string messageId, string confirmButtonTextLocId, Action cancelCalled)
	{
		string yellowTan = UgcConsts.GetYellowTan(t.Name);
		if (t.IsStarted)
		{
			cancelCalled();
			UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("UGC_AlreadyHaveActiveCompCaption"), yellowTan), TournamentCanceledInit.MessageTypes.Warning, null, false);
			return;
		}
		UIHelper.ShowYesNo(string.Format(ScriptLocalization.Get(messageId), yellowTan), delegate
		{
			UIHelper.Waiting(true, null);
			PhotonConnectionFactory.Instance.OnUnregisteredFromTournament += this.UnregisteredFromTournament;
			PhotonConnectionFactory.Instance.OnUnregisterFromTournamentFailed += this.UnregisterFromTournamentFailed;
			PhotonConnectionFactory.Instance.UnregisterFromTournament(t.TournamentId);
		}, null, confirmButtonTextLocId, cancelCalled, "NoCaption", null, null, null);
	}

	private void UnregisteredFromTournament()
	{
		this.UnsubscribeUnregisteredFromTournament();
		this.RefreshMyTournaments();
		UIHelper.Waiting(false, null);
	}

	public void UnregFromUgc(UserCompetitionPublic t, string messageId, string confirmButtonTextLocId, Action cancelCalled)
	{
		string yellowTan = UgcConsts.GetYellowTan((!t.IsSponsored || string.IsNullOrEmpty(t.NameCustom)) ? UserCompetitionHelper.GetDefaultName(t) : t.NameCustom);
		if (t.IsStarted)
		{
			cancelCalled();
			UIHelper.ShowCanceledMsg(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("UGC_AlreadyHaveActiveCompCaption"), yellowTan), TournamentCanceledInit.MessageTypes.Warning, null, false);
			return;
		}
		UIHelper.ShowYesNo(string.Format(ScriptLocalization.Get(messageId), yellowTan), delegate
		{
			UIHelper.Waiting(true, null);
			if (UserCompetitionHelper.IsOwnerHost(t))
			{
				PhotonConnectionFactory.Instance.OnRemoveCompetition += this.RemoveOwnerCompetition;
				PhotonConnectionFactory.Instance.OnFailureRemoveCompetition += this.FailureOwnerRemoveCompetition;
				PhotonConnectionFactory.Instance.RemoveCompetition(t.TournamentId, true);
			}
			else
			{
				PhotonConnectionFactory.Instance.OnUnregisterFromCompetition += this.Instance_OnUnregisterFromCompetition;
				PhotonConnectionFactory.Instance.OnFailureUnregisterFromCompetition += this.Instance_OnFailureUnregisterFromCompetition;
				PhotonConnectionFactory.Instance.UnregisterFromCompetition(t.TournamentId);
			}
		}, null, confirmButtonTextLocId, cancelCalled, "NoCaption", null, null, null);
	}

	private void RemoveOwnerCompetition()
	{
		this.UnsubscribeRemoveOwnerCompetition();
		this.Refresh();
		UIHelper.Waiting(false, null);
	}

	private void Instance_OnUnregisterFromCompetition(UserCompetitionPublic competition)
	{
		this.UnsubscribeOnUnregisterFromCompetition();
		this.Refresh();
		UIHelper.Waiting(false, null);
	}

	private void FailureOwnerRemoveCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeRemoveOwnerCompetition();
		UserCompetitionFailureHandler.Fire(failure, null, true);
	}

	private void Instance_OnFailureUnregisterFromCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeOnUnregisterFromCompetition();
		UserCompetitionFailureHandler.Fire(failure, null, true);
	}

	private void UnsubscribeRemoveOwnerCompetition()
	{
		PhotonConnectionFactory.Instance.OnRemoveCompetition -= this.RemoveOwnerCompetition;
		PhotonConnectionFactory.Instance.OnFailureRemoveCompetition -= this.FailureOwnerRemoveCompetition;
	}

	private void UnsubscribeOnUnregisterFromCompetition()
	{
		PhotonConnectionFactory.Instance.OnUnregisterFromCompetition -= this.Instance_OnUnregisterFromCompetition;
		PhotonConnectionFactory.Instance.OnFailureUnregisterFromCompetition -= this.Instance_OnFailureUnregisterFromCompetition;
	}

	private void UnsubscribeUnregisteredFromTournament()
	{
		PhotonConnectionFactory.Instance.OnUnregisteredFromTournament -= this.UnregisteredFromTournament;
		PhotonConnectionFactory.Instance.OnUnregisterFromTournamentFailed -= this.UnregisterFromTournamentFailed;
	}

	private void UnregisterFromTournamentFailed(Failure failure)
	{
		this.UnsubscribeUnregisteredFromTournament();
		Debug.LogError(failure.FullErrorInfo);
	}

	public Action OnRefreshed;

	public Action OnRefreshedTournaments = delegate
	{
	};

	public Action OnTournamentEndedOnCient;

	private readonly FilterForUserCompetitions _registeredFilter = new FilterForUserCompetitions();

	private static List<UserCompetitionPublic> _lastFiltered = new List<UserCompetitionPublic>();

	private List<Tournament> _myTournaments = new List<Tournament>();

	private int? _tournamentIdOnEnd;

	private readonly Guid _requestIdGetUserCompetitions = Guid.NewGuid();
}
