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
	public class PlayerItemVH : BaseVH
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
			this.root.GetComponentAtPath("Experience", out this.Experience);
			this.root.GetComponentAtPath("FishCount", out this.FishCount);
			this.root.GetComponentAtPath("TrophyCount", out this.TrophyCount);
			this.root.GetComponentAtPath("UserName", out this.UserName);
			this.root.GetComponentAtPath("UniqueCount", out this.UniqueCount);
			this.root.GetComponentAtPath("Bg", out this.Bg);
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(FishModel);
		}

		internal override void UpdateViews(BaseModel model)
		{
			base.UpdateViews(model);
			PlayerModel entry = model as PlayerModel;
			this.Place.text = (entry.id + 1).ToString();
			this.UserName.text = entry.Data.UserName;
			this.Level.text = PlayerProfileHelper.GetPlayerLevelColored(entry.Data);
			this.Rank.text = PlayerProfileHelper.GetPlayerRankColored(entry.Data);
			this.Experience.text = ((!PlayerItemVH.IsWeeklyExp) ? entry.Data.Experience.ToString() : entry.Data.WeeklyExpGain.ToString());
			this.FishCount.text = entry.Data.FishCount.ToString();
			this.TrophyCount.text = entry.Data.TrophyCount.ToString();
			this.UniqueCount.text = entry.Data.UniqueCount.ToString();
			this.Button.onClick.RemoveAllListeners();
			this.Button.onClick.AddListener(delegate
			{
				this.USerShowProfile.RequestById(entry.Data.UserId.ToString());
			});
			this.AvatarIco.overrideSprite = null;
			PlayerProfileHelper.SetAvatarIco(entry.Data.UserId.ToString(), entry.Data.ExternalId, entry.Data.AvatarUrl, this.AvatarIco);
			if (entry.Data.UserId == PhotonConnectionFactory.Instance.Profile.UserId)
			{
				this.Bg.color = PlayerItemVH.myColor;
			}
			else
			{
				this.Bg.color = PlayerItemVH.theirColor;
			}
			if (SettingsManager.InputType == InputModuleManager.InputType.Mouse)
			{
				this.Button.OnPointerExit(new PointerEventData(EventSystem.current));
			}
		}

		private static Color myColor = new Color(1f, 1f, 1f, 0.0235f);

		private static Color theirColor = new Color(0f, 0f, 0f, 0.235f);

		public static bool IsWeeklyExp = false;

		public Image Bg;

		public Image AvatarIco;

		public TextMeshProUGUI Place;

		public TextMeshProUGUI UserName;

		public TextMeshProUGUI Level;

		public TextMeshProUGUI Rank;

		public TextMeshProUGUI Experience;

		public TextMeshProUGUI FishCount;

		public TextMeshProUGUI TrophyCount;

		public TextMeshProUGUI UniqueCount;

		public BorderedButton Button;

		public UserShowProfile USerShowProfile;
	}
}
