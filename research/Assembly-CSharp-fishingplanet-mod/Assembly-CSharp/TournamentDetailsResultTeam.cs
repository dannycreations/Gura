using System;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TournamentDetailsResultTeam : MonoBehaviour
{
	public void Init(string team, int? place, UserCompetitionPublic ugc, string score1, string score2, bool isDraw)
	{
		string text = UgcConsts.GetTeamLoc(team);
		int num = ((!isDraw) ? ((place == null || place.Value <= 0) ? 1 : (place.Value - 1)) : 0);
		this._cup.overrideSprite = this._cups[(!isDraw) ? num : 1];
		this._ico.color = UgcConsts.GetTeamColor(team);
		if (num == 0 && !isDraw)
		{
			text = string.Format("<b>{0}</b>", UgcConsts.GetWinner(text));
			double num2 = UserCompetitionHelper.PrizePoolWithComission(ugc);
		}
		else
		{
			this._cup.color = new Color(0.13333334f, 0.13333334f, 0.13333334f);
		}
		int num3 = ugc.Players.Count((UserCompetitionPlayer p) => p.Team.ToUpper() == team.ToUpper());
		this._info.text = string.Format("{0} <color=#F7F7F766>({1})</color>", text, num3);
	}

	[SerializeField]
	private TextMeshProUGUI _info;

	[SerializeField]
	private TextMeshProUGUI _rewards;

	[SerializeField]
	private Image _ico;

	[SerializeField]
	private Image _cup;

	[SerializeField]
	private Sprite[] _cups;
}
