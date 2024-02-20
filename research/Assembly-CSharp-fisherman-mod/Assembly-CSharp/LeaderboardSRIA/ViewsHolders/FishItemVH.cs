using System;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.PlayerProfile;
using frame8.Logic.Misc.Other.Extensions;
using LeaderboardSRIA.Models;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LeaderboardSRIA.ViewsHolders
{
	public class FishItemVH : BaseVH
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.Button = this.root.GetComponent<BorderedButton>();
			this.USerShowProfile = this.root.GetComponent<UserShowProfile>();
			this.root.GetComponentAtPath("Avatar/UserAvatar", out this.AvatarIco);
			this.root.GetComponentAtPath("Avatar/UserAvatar", out this.Avatar.Image);
			this.root.GetComponentAtPath("FishImg", out this.FishImage.Image);
			this.root.GetComponentAtPath("Place", out this.Place);
			this.root.GetComponentAtPath("Lvl", out this.Level);
			this.root.GetComponentAtPath("Rank", out this.Rank);
			this.root.GetComponentAtPath("FishName", out this.FishName);
			this.root.GetComponentAtPath("Score", out this.Score);
			this.Score.enableWordWrapping = false;
			this.Score.overflowMode = 0;
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
			FishModel entry = model as FishModel;
			this.Place.text = (entry.id + 1).ToString();
			this.UserName.text = entry.Data.UserName;
			this.Level.text = PlayerProfileHelper.GetPlayerLevelColored(entry.Data);
			this.Rank.text = PlayerProfileHelper.GetPlayerRankColored(entry.Data);
			FishBrief fishBrief = CacheLibrary.MapCache.FishesLight.FirstOrDefault((FishBrief x) => x.FishId == entry.Data.FishId);
			this.FishName.text = entry.Data.FishName;
			this.Score.text = MeasuringSystemManager.FishWeight((float)entry.Data.Weight).ToString("N3");
			this.FishName.text = entry.Data.FishName;
			if (fishBrief != null && fishBrief.ThumbnailBID != null)
			{
				this.FishImage.Load(fishBrief.ThumbnailBID.Value);
			}
			this.Button.onClick.RemoveAllListeners();
			this.Button.onClick.AddListener(delegate
			{
				this.USerShowProfile.RequestById(entry.Data.UserId.ToString());
			});
			this.AvatarIco.overrideSprite = null;
			PlayerProfileHelper.SetAvatarIco(entry.Data.UserId.ToString(), entry.Data.ExternalId, entry.Data.AvatarUrl, this.AvatarIco);
			if (entry.Data.UserId == PhotonConnectionFactory.Instance.Profile.UserId)
			{
				this.Bg.color = FishItemVH.myColor;
			}
			else
			{
				this.Bg.color = FishItemVH.theirColor;
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

		public ResourcesHelpers.AsyncLoadableImage Avatar = new ResourcesHelpers.AsyncLoadableImage();

		public ResourcesHelpers.AsyncLoadableImage FishImage = new ResourcesHelpers.AsyncLoadableImage();

		public TextMeshProUGUI Place;

		public TextMeshProUGUI UserName;

		public TextMeshProUGUI Level;

		public TextMeshProUGUI Rank;

		public TextMeshProUGUI FishName;

		public TextMeshProUGUI Score;

		public BorderedButton Button;

		public UserShowProfile USerShowProfile;
	}
}
