using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;
using UnityEngine.Events;

namespace Assets.Scripts.UI._2D.Tournaments
{
	public class PondCompetitionsManager : IDisposable
	{
		public PondCompetitionsManager(TournamentManager tournamentManager, Action refreshFn)
		{
			this._refreshFn = refreshFn;
			this._tournamentManager = tournamentManager;
		}

		public List<Tournament> Competitions
		{
			get
			{
				List<int> ids = this.AllCompetitions.Select((Tournament p) => p.TournamentId).ToList<int>();
				IEnumerable<Tournament> enumerable = this._tournamentManager.UserGeneratedCompetitions.Where((UserCompetitionPublic p) => !ids.Contains(p.TournamentId)).Select(new Func<UserCompetitionPublic, Tournament>(UserCompetitionHelper.Ugc2Tournament));
				IEnumerable<Tournament> enumerable2 = this.AllCompetitions.Concat(enumerable);
				if (enumerable2.Count<Tournament>() > 1)
				{
					enumerable2 = enumerable2.Where((Tournament p) => p.IsRegistered);
				}
				return enumerable2.ToList<Tournament>();
			}
		}

		public void PushAllTournaments(List<Tournament> tournaments)
		{
			this.AllCompetitions = tournaments;
			this.Print(this.AllCompetitions, "CoMapCtrl:PushAllTournaments");
		}

		public void Start()
		{
			TournamentHelper.OnSeriesUpdated.AddListener(new UnityAction(this.RefreshTournamentSerie));
			PhotonConnectionFactory.Instance.OnUserCompetitionStarted += this.Instance_OnUserCompetitionStarted;
			PhotonConnectionFactory.Instance.OnTournamentStarted += this.OnTournamentStarted;
			PhotonConnectionFactory.Instance.OnTournamentTimeEnded += this.OnTimeEnded;
			PhotonConnectionFactory.Instance.OnTournamentResult += this.OnTournamentResult;
			PhotonConnectionFactory.Instance.OnTournamentCancelled += this.OnTournamentCancelled;
			PhotonConnectionFactory.Instance.OnUserCompetitionCancelled += this.OnUserCompetitionCancelled;
			PhotonConnectionFactory.Instance.OnGettingPlayerTournamentsFailed += this.GettingPlayerTournamentsFailed;
			this._refreshFn();
		}

		public void Stop()
		{
			TournamentHelper.OnSeriesUpdated.RemoveListener(new UnityAction(this.RefreshTournamentSerie));
			PhotonConnectionFactory.Instance.OnUserCompetitionStarted -= this.Instance_OnUserCompetitionStarted;
			PhotonConnectionFactory.Instance.OnTournamentStarted -= this.OnTournamentStarted;
			PhotonConnectionFactory.Instance.OnTournamentTimeEnded -= this.OnTimeEnded;
			PhotonConnectionFactory.Instance.OnTournamentResult -= this.OnTournamentResult;
			PhotonConnectionFactory.Instance.OnTournamentCancelled -= this.OnTournamentCancelled;
			PhotonConnectionFactory.Instance.OnUserCompetitionCancelled -= this.OnUserCompetitionCancelled;
			PhotonConnectionFactory.Instance.OnGettingPlayerTournamentsFailed -= this.GettingPlayerTournamentsFailed;
		}

		public void Dispose()
		{
			this.Stop();
			this._refreshFn = null;
			this._tournamentManager = null;
		}

		private void Instance_OnUserCompetitionStarted(UserCompetitionStartMessage o)
		{
			this._refreshFn();
			LogHelper.Log("CoMapCtrl:OnUserCompetitionStarted");
		}

		private void OnTournamentStarted(TournamentStartInfo info)
		{
			this._refreshFn();
			LogHelper.Log("CoMapCtrl:OnTournamentStarted");
		}

		private void OnUserCompetitionCancelled(UserCompetitionCancellationMessage o)
		{
			this._refreshFn();
			LogHelper.Log("CoMapCtrl:OnUserCompetitionCancelled");
		}

		private void OnTournamentCancelled(TournamentCancelInfo info)
		{
			this._refreshFn();
			LogHelper.Log("CoMapCtrl:OnTournamentCancelled");
		}

		private void OnTournamentResult(TournamentFinalResult info)
		{
			this._refreshFn();
			LogHelper.Log("CoMapCtrl:OnTournamentResult");
		}

		private void OnTimeEnded(EndTournamentTimeResult result)
		{
			this._refreshFn();
			LogHelper.Log("CoMapCtrl:OnTimeEnded");
		}

		private void RefreshTournamentSerie()
		{
			this._refreshFn();
		}

		private void GettingPlayerTournamentsFailed(Failure failure)
		{
			LogHelper.Log("CoMapCtrl:GettingPlayerTournamentsFailed FullErrorInfo:{0}", new object[] { failure.FullErrorInfo });
		}

		private void Print(List<Tournament> ts, string tag)
		{
			string text = "___kocha {0} ts:{1}";
			object[] array = new object[2];
			array[0] = tag;
			array[1] = string.Join(",", ts.Select((Tournament p) => p.TournamentId.ToString()).ToArray<string>());
			LogHelper.Log(text, array);
		}

		private TournamentManager _tournamentManager;

		private Action _refreshFn;

		private List<Tournament> AllCompetitions = new List<Tournament>();
	}
}
