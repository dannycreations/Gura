using System;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.MainExample
{
	public class ClientItemViewsHolder : BaseItemViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
			Transform child = this.root.GetChild(0);
			child.GetComponentAtPath("AvatarPanel", out this.avatarImage);
			child.GetComponentAtPath("AvatarPanel/StatusText", out this.statusText);
			child.GetComponentAtPath("NameAndLocationPanel/NameText", out this.nameText);
			child.GetComponentAtPath("NameAndLocationPanel/LocationText", out this.locationText);
			RectTransform componentAtPath = child.GetComponentAtPath("FriendsPanel");
			for (int i = 0; i < 5; i++)
			{
				Transform transform = (this.friendsPanels[i] = componentAtPath.GetChild(i));
				this.friendsPanelsCanvasGroups[i] = transform.GetComponent<CanvasGroup>();
				this.friendsAvatarImages[i] = transform.GetComponent<Image>();
			}
			componentAtPath.GetComponentAtPath("FriendsText", out this.friendsText);
			RectTransform componentAtPath2 = this.root.GetComponentAtPath("RatingPanel/Panel");
			componentAtPath2.GetComponentAtPath("Foreground", out this.averageScoreFillImage);
			componentAtPath2.GetComponentAtPath("Text", out this.averageScoreText);
			RectTransform componentAtPath3 = this.root.GetComponentAtPath("RatingBreakdownPanel");
			componentAtPath3.GetComponentAtPath("AvailabilityPanel/Slider", out this.availability01Slider);
			componentAtPath3.GetComponentAtPath("ContractChancePanel/Slider", out this.contractChance01Slider);
			componentAtPath3.GetComponentAtPath("LongTermClientPanel/Slider", out this.longTermClient01Slider);
			this.expandCollapseComponent = this.root.GetComponent<ExpandCollapseOnClick>();
		}

		public void UpdateViews(MyParams p)
		{
			ClientModel clientModel = p.Data[this.ItemIndex];
			this.avatarImage.sprite = p.sampleAvatars[clientModel.avatarImageId];
			this.nameText.text = string.Concat(new object[] { clientModel.clientName, "(#", this.ItemIndex, ")" });
			this.locationText.text = "  " + clientModel.location;
			this.UpdateScores(clientModel);
			this.friendsText.text = clientModel.friendsAvatarIds.Length + ((clientModel.friendsAvatarIds.Length != 1) ? " friends" : " friend");
			if (clientModel.isOnline)
			{
				this.statusText.text = "Online";
				this.statusText.color = Color.green;
			}
			else
			{
				this.statusText.text = "Offline";
				this.statusText.color = Color.white * 0.8f;
			}
			this.UpdateFriendsAvatars(clientModel, p);
			if (this.expandCollapseComponent)
			{
				this.expandCollapseComponent.expanded = clientModel.expanded;
				this.expandCollapseComponent.nonExpandedSize = clientModel.nonExpandedSize;
			}
		}

		private void UpdateScores(ClientModel dataModel)
		{
			Vector3 vector = this.availability01Slider.localScale;
			vector.x = dataModel.availability01;
			this.availability01Slider.localScale = vector;
			vector = this.contractChance01Slider.localScale;
			vector.x = dataModel.contractChance01;
			this.contractChance01Slider.localScale = vector;
			vector = this.longTermClient01Slider.localScale;
			vector.x = dataModel.longTermClient01;
			this.longTermClient01Slider.localScale = vector;
			float averageScore = dataModel.AverageScore01;
			this.averageScoreFillImage.fillAmount = averageScore;
			this.averageScoreText.text = (int)(averageScore * 100f) + "%";
		}

		private void UpdateFriendsAvatars(ClientModel dataModel, MyParams p)
		{
			int i = 0;
			int num = dataModel.friendsAvatarIds.Length;
			int num2 = Mathf.Min(5, num);
			while (i < num2)
			{
				this.friendsAvatarImages[i].sprite = p.sampleAvatarsDownsized[dataModel.friendsAvatarIds[i]];
				this.friendsPanels[i].gameObject.SetActive(true);
				i++;
			}
			while (i < 5)
			{
				this.friendsPanels[i].gameObject.SetActive(false);
				i++;
			}
			if (num > 4)
			{
				this.friendsPanelsCanvasGroups[4].alpha = 0.1f;
			}
			if (num > 3)
			{
				this.friendsPanelsCanvasGroups[3].alpha = 0.4f;
			}
		}

		public Image avatarImage;

		public Image averageScoreFillImage;

		public Text nameText;

		public Text locationText;

		public Text averageScoreText;

		public Text friendsText;

		public RectTransform availability01Slider;

		public RectTransform contractChance01Slider;

		public RectTransform longTermClient01Slider;

		public Text statusText;

		public Transform[] friendsPanels = new Transform[5];

		public CanvasGroup[] friendsPanelsCanvasGroups = new CanvasGroup[5];

		public Image[] friendsAvatarImages = new Image[5];

		public ExpandCollapseOnClick expandCollapseComponent;

		private const int MAX_DISPLAYED_FRIENDS = 5;
	}
}
