using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WindowListCompetitions : WindowList
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<int> OnDetails = delegate(int i)
	{
	};

	protected override void Awake()
	{
		base.Awake();
		this.TitleCaption.text = ScriptLocalization.Get("MySportsEventsCaption").ToUpper();
		this.FormatCaption.text = ScriptLocalization.Get("UGC_FormatTypes").ToUpper();
		this.NameCaption.text = ScriptLocalization.Get("UGC_Name").ToUpper();
		this.PondCaption.text = ScriptLocalization.Get("UGC_Location").ToUpper();
		this.PlayersCaption.text = ScriptLocalization.Get("UGC_AppliedPlayersCountCaption").ToUpper();
		this.StartTimeCaption.text = ScriptLocalization.Get("UGC_StartTimeCaption");
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		this.UnsubscribeGetUserCompetition();
	}

	public void Details()
	{
		if (!this.Alpha.IsHiding)
		{
			this.OnDetails(this.SelectedIndex);
			this.Alpha.HidePanel();
		}
	}

	public void Init(WindowListCompetitions.WindowListContainerCompetitions c)
	{
		this._additionalData = c.Data;
		List<int> list = new List<int>();
		for (int i = 0; i < this._additionalData.Count; i++)
		{
			WindowListCompetitions.WindowListElemCompetitions windowListElemCompetitions = this._additionalData[i];
			if (windowListElemCompetitions.Competition.IsUgc() && windowListElemCompetitions.Ugc == null)
			{
				list.Add(windowListElemCompetitions.Competition.TournamentId);
			}
		}
		if (list.Count > 0)
		{
			PhotonConnectionFactory.Instance.OnGetUserCompetition += this.Instance_OnGetUserCompetition;
			PhotonConnectionFactory.Instance.OnFailureGetUserCompetition += this.Instance_OnFailureGetUserCompetition;
			list.ForEach(delegate(int p)
			{
				PhotonConnectionFactory.Instance.GetUserCompetition(p);
			});
		}
		this.Data = c.Data.Select((WindowListCompetitions.WindowListElemCompetitions p) => p).ToList<WindowList.WindowListElem>();
		if (this._lastSelectedTournamentId != null)
		{
			int num = c.Data.FindIndex((WindowListCompetitions.WindowListElemCompetitions p) => p.Competition.TournamentId == this._lastSelectedTournamentId);
			if (num != -1)
			{
				c.Index = num;
			}
		}
		base.FillView(c);
		for (int j = 0; j < this.Items.Count; j++)
		{
			((WindowListCompetitionsItem)this.Items[j]).Init(c.Data[j].Ugc, c.Data[j].Competition);
		}
		this.UpdateActiveItem(this.SelectedIndex);
	}

	public void ReInit(WindowListCompetitions.WindowListContainerCompetitions data)
	{
		this.UnsubscribeGetUserCompetition();
		base.Clear();
		data.Data.ForEach(delegate(WindowListCompetitions.WindowListElemCompetitions p)
		{
			if (p.Competition.IsUgc() && p.Ugc == null && this._cacheUgc.ContainsKey(p.Competition.TournamentId))
			{
				p.Ugc = this._cacheUgc[p.Competition.TournamentId];
			}
		});
		this.Init(data);
	}

	protected override void OnDblClick()
	{
		this.Details();
	}

	protected override void SetActiveItem(int i)
	{
		base.SetActiveItem(i);
		this.UpdateActiveItem(i);
	}

	private void UpdateActiveItem(int i)
	{
		WindowListCompetitions.WindowListElemCompetitions windowListElemCompetitions = this._additionalData[i];
		this.FormatCaption.gameObject.SetActive(windowListElemCompetitions.Competition.IsUgc());
		this._lastSelectedTournamentId = new int?(windowListElemCompetitions.Competition.TournamentId);
		WindowListCompetitionsItem windowListCompetitionsItem = (WindowListCompetitionsItem)this.Items[i];
		this.OkBtn.interactable = windowListCompetitionsItem.IsStarted && !windowListCompetitionsItem.IsFinished && (UserCompetitionHelper.IsUgcEnabled || !windowListElemCompetitions.Competition.IsUgc());
		this.DetailsBtnValue.text = ScriptLocalization.Get((!windowListElemCompetitions.Competition.IsUgc() || windowListCompetitionsItem.IsStarted) ? "Details" : "TryOpenCaption");
	}

	private string DtFormat(DateTime dt, string locId)
	{
		return string.Format(ScriptLocalization.Get(locId), MeasuringSystemManager.TimeString(dt.ToLocalTime()));
	}

	private void Instance_OnGetUserCompetition(UserCompetitionPublic competition)
	{
		this._cacheUgc[competition.TournamentId] = competition;
		for (int i = 0; i < this._additionalData.Count; i++)
		{
			WindowListCompetitions.WindowListElemCompetitions windowListElemCompetitions = this._additionalData[i];
			if (windowListElemCompetitions.Competition.TournamentId == competition.TournamentId)
			{
				windowListElemCompetitions.Ugc = competition;
				((WindowListCompetitionsItem)this.Items[i]).Init(competition);
				if (i == this.SelectedIndex)
				{
					this.UpdateActiveItem(this.SelectedIndex);
				}
				break;
			}
		}
		if (this._additionalData.All((WindowListCompetitions.WindowListElemCompetitions p) => !p.Competition.IsUgc() || p.Ugc != null))
		{
			this.UnsubscribeGetUserCompetition();
		}
	}

	protected void Instance_OnFailureGetUserCompetition(UserCompetitionFailure failure)
	{
		this.UnsubscribeGetUserCompetition();
		LogHelper.Error("WindowListElemCompetitions:Instance_OnFailureGetUserCompetition {0}", new object[] { failure.FullErrorInfo });
	}

	protected void UnsubscribeGetUserCompetition()
	{
		PhotonConnectionFactory.Instance.OnGetUserCompetition -= this.Instance_OnGetUserCompetition;
		PhotonConnectionFactory.Instance.OnFailureGetUserCompetition -= this.Instance_OnFailureGetUserCompetition;
	}

	[SerializeField]
	protected TextMeshProUGUI TitleCaption;

	[SerializeField]
	protected TextMeshProUGUI FormatCaption;

	[SerializeField]
	protected TextMeshProUGUI NameCaption;

	[SerializeField]
	protected TextMeshProUGUI PondCaption;

	[SerializeField]
	protected TextMeshProUGUI PlayersCaption;

	[SerializeField]
	protected TextMeshProUGUI StartTimeCaption;

	[SerializeField]
	protected Text DetailsBtnValue;

	private List<WindowListCompetitions.WindowListElemCompetitions> _additionalData;

	private int? _lastSelectedTournamentId;

	private Dictionary<int, UserCompetitionPublic> _cacheUgc = new Dictionary<int, UserCompetitionPublic>();

	public class WindowListElemCompetitions : WindowList.WindowListElem
	{
		public UserCompetitionPublic Ugc { get; set; }

		public Tournament Competition { get; set; }
	}

	public class WindowListContainerCompetitions : WindowList.WindowListContainerBase
	{
		public List<WindowListCompetitions.WindowListElemCompetitions> Data { get; set; }
	}
}
