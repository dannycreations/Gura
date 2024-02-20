using System;
using System.Diagnostics;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TournamentListItemHandler : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<TournamentEventArgs> IsSelectedItem;

	public void Init(Tournament tournament, bool isOdd = false)
	{
		this._currenTournament = tournament;
		this.Name.text = this._currenTournament.Name;
		if (this._currenTournament.IsActive)
		{
			this.Status.text = ScriptLocalization.Get("RunningStatusText");
		}
		else if (this._currenTournament.IsEnded)
		{
			this.Status.text = ScriptLocalization.Get("FinishedStatusText");
		}
		else if (this._currenTournament.IsCanceled)
		{
			this.Status.text = ScriptLocalization.Get("CanceledStatusText");
		}
		else
		{
			this.Status.text = ScriptLocalization.Get("PlannedStatusText");
		}
		base.GetComponent<Image>().enabled = !isOdd;
	}

	public void IsSelected()
	{
		if (this.Toggle.isOn && this.IsSelectedItem != null)
		{
			this.IsSelectedItem(this, new TournamentEventArgs
			{
				Tournament = this._currenTournament
			});
		}
	}

	public Text Name;

	public Toggle Toggle;

	public Text Status;

	private Tournament _currenTournament;
}
