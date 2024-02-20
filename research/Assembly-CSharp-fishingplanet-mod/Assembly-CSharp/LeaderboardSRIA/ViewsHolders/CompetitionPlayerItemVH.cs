using System;
using Assets.Scripts.UI._2D.PlayerProfile;
using frame8.Logic.Misc.Other.Extensions;
using LeaderboardSRIA.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LeaderboardSRIA.ViewsHolders
{
	public class CompetitionPlayerItemVH : BaseVH
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.Button = this.root.GetComponent<BorderedButton>();
			this.USerShowProfile = this.root.GetComponent<UserShowProfile>();
			this.root.GetComponentAtPath("Avatar/UserAvatar", out this.AvatarIco);
			this.root.GetComponentAtPath("Place", out this.Place);
			this.root.GetComponentAtPath("Lvl", out this.Level);
			this.root.GetComponentAtPath("Rank", out this.Rank);
			this.root.GetComponentAtPath("Progress/Performance", out this.Performance);
			this.root.GetComponentAtPath("Progress/Value", out this.PerformanceValue);
			this.root.GetComponentAtPath("GamesPlayed", out this.GamesPlayed);
			this.root.GetComponentAtPath("VictoriesCount", out this.VictoriesCount);
			this.root.GetComponentAtPath("Rating", out this.Rating);
			this.root.GetComponentAtPath("UserName", out this.UserName);
			this.root.GetComponentAtPath("Bg", out this.Bg);
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(FishModel);
		}

		internal override void UpdateViews(BaseModel model)
		{
			base.UpdateViews(model);
			CompetitionPlayerModel entry = model as CompetitionPlayerModel;
			if (entry.Data == null)
			{
				this.Place.text = "-";
				this.UserName.text = "-";
				this.Level.text = "-";
				this.Rank.text = "-";
				this.Rating.text = "-";
				this.VictoriesCount.text = "-";
				this.GamesPlayed.text = "-";
				this.PerformanceValue.gameObject.SetActive(false);
				this.Performance.color = Color.white;
				this.Performance.text = "-";
				this.PerformanceValue.text = string.Empty;
				this.Button.onClick.RemoveAllListeners();
				this.AvatarIco.overrideSprite = null;
				this.AvatarIco.transform.parent.gameObject.SetActive(false);
				this.Bg.color = CompetitionPlayerItemVH.theirColor;
				return;
			}
			this.Place.text = entry.Data.CompetitionRank.ToString();
			this.UserName.text = entry.Data.UserName;
			this.Level.text = PlayerProfileHelper.GetPlayerLevelColored(entry.Data);
			this.Rank.text = PlayerProfileHelper.GetPlayerRankColored(entry.Data);
			this.Rating.text = entry.Data.Rating.ToString();
			this.VictoriesCount.text = entry.Data.Won.ToString();
			this.GamesPlayed.text = entry.Data.Played.ToString();
			if (entry.Data.CompetitionRankChange != 0)
			{
				this.Performance.gameObject.SetActive(true);
				this.PerformanceValue.gameObject.SetActive(true);
				if (entry.Data.CompetitionRankChange > 0)
				{
					this.Performance.color = Color.green;
					this.Performance.text = "\ue682";
					this.PerformanceValue.text = "+" + entry.Data.CompetitionRankChange;
				}
				if (entry.Data.CompetitionRankChange < 0)
				{
					this.Performance.color = Color.red;
					this.Performance.text = "\ue683";
					this.PerformanceValue.text = entry.Data.CompetitionRankChange.ToString();
				}
			}
			else
			{
				this.PerformanceValue.gameObject.SetActive(false);
				this.Performance.color = Color.white;
				this.Performance.text = "\ue684";
				this.PerformanceValue.text = string.Empty;
			}
			this.Button.onClick.RemoveAllListeners();
			this.Button.onClick.AddListener(delegate
			{
				this.USerShowProfile.RequestById(entry.Data.UserId.ToString());
			});
			if (!this.AvatarIco.transform.parent.gameObject.activeSelf)
			{
				this.AvatarIco.transform.parent.gameObject.SetActive(true);
			}
			this.AvatarIco.overrideSprite = null;
			PlayerProfileHelper.SetAvatarIco(entry.Data.UserId.ToString(), entry.Data.ExternalId, entry.Data.AvatarUrl, this.AvatarIco);
			if (entry.Data.UserId == PhotonConnectionFactory.Instance.Profile.UserId)
			{
				this.Bg.color = CompetitionPlayerItemVH.myColor;
			}
			else
			{
				this.Bg.color = CompetitionPlayerItemVH.theirColor;
			}
			if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
			{
				this.Button.OnPointerExit(new PointerEventData(EventSystem.current));
			}
		}

		private static Color myColor = new Color(1f, 1f, 1f, 0.0235f);

		private static Color theirColor = new Color(0f, 0f, 0f, 0.235f);

		public Image Bg;

		public Image AvatarIco;

		public TextMeshProUGUI Place;

		public TextMeshProUGUI Performance;

		public TextMeshProUGUI PerformanceValue;

		public TextMeshProUGUI UserName;

		public TextMeshProUGUI Level;

		public TextMeshProUGUI Rank;

		public TextMeshProUGUI GamesPlayed;

		public TextMeshProUGUI VictoriesCount;

		public TextMeshProUGUI Rating;

		public BorderedButton Button;

		public UserShowProfile USerShowProfile;
	}
}
