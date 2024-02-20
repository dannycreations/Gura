using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.PlayerProfile;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ConcreteTopPlayerCompetition : ConcreteTop
{
	private void Awake()
	{
		this._texts = new List<Text> { this.Number, this.Name, this.Level, this.Played, this.Win, this.Rating };
		this._hlg = base.GetComponent<HorizontalLayoutGroup>();
		this._vlgProgress = this.Progress.transform.parent.GetComponent<VerticalLayoutGroup>();
		this._hlg.enabled = false;
		this._vlgProgress.enabled = false;
	}

	public void InitItem(TopTournamentPlayers player)
	{
		if (this._vlgProgress != null)
		{
			this._vlgProgress.enabled = true;
		}
		if (player != null)
		{
			bool flag = player.UserId == PhotonConnectionFactory.Instance.Profile.UserId;
			PointerActionHandler component = this.Name.GetComponent<PointerActionHandler>();
			if (component != null)
			{
				component.enabled = !flag;
			}
			this.UpdateTextsColor(flag);
			this.Number.text = player.CompetitionRank.ToString();
			this.Name.text = player.UserName;
			this.Level.text = PlayerProfileHelper.GetPlayerLevelRank(player);
			this.Played.text = player.Played.ToString();
			this.Win.text = player.Won.ToString();
			this.Rating.text = player.Rating.ToString();
			if (player.CompetitionRankChange != 0)
			{
				this.ProgressIcon.gameObject.SetActive(true);
				this.Progress.gameObject.SetActive(true);
				if (player.CompetitionRankChange > 0)
				{
					this.ProgressIcon.color = Color.green;
					this.ProgressIcon.text = "\ue682";
					this.Progress.text = "+" + player.CompetitionRankChange;
				}
				if (player.CompetitionRankChange < 0)
				{
					this.ProgressIcon.color = Color.red;
					this.ProgressIcon.text = "\ue683";
					this.Progress.text = player.CompetitionRankChange.ToString();
				}
			}
			else
			{
				this.Progress.gameObject.SetActive(false);
				this.ProgressIcon.color = Color.white;
				this.ProgressIcon.text = "\ue684";
				this.Progress.text = string.Empty;
			}
			this.UserId = player.UserId.ToString();
		}
		else
		{
			this.Number.text = "-";
			this.Name.text = "-";
			this.Level.text = "-";
			this.Played.text = "-";
			this.Win.text = "-";
			this.Rating.text = "-";
			this.Progress.text = string.Empty;
			this.ProgressIcon.text = string.Empty;
			this.UserId = "0";
		}
		base.Invoke("DisableHLG", 0.1f);
	}

	private void DisableHLG()
	{
		if (this._vlgProgress != null)
		{
			this._vlgProgress.enabled = false;
		}
	}

	private void UpdateTextsColor(bool isOwner)
	{
		if (this._texts != null)
		{
			this._texts.ForEach(delegate(Text p)
			{
				p.color = ((!isOwner) ? this._otherPlayerColor : this._selfPlayerColor);
			});
		}
	}

	public Text Number;

	public Text Name;

	public Text Level;

	public Text Played;

	public Text Win;

	public Text Rating;

	public Text ProgressIcon;

	public Text Progress;

	private HorizontalLayoutGroup _hlg;

	private VerticalLayoutGroup _vlgProgress;

	private readonly Color _selfPlayerColor = new Color(1f, 0.84705883f, 0f, 1f);

	private readonly Color _otherPlayerColor = Color.white;

	private List<Text> _texts;
}
