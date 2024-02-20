using System;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.ChatExample
{
	public class ChatMessageViewsHolder : BaseItemViewsHolder
	{
		public ContentSizeFitter contentSizeFitter { get; private set; }

		public float PopupAnimationStartTime { get; private set; }

		public bool IsPopupAnimationActive
		{
			get
			{
				return this._IsAnimating;
			}
			set
			{
				if (value)
				{
					Vector3 localScale = this.messageContentPanelImage.transform.localScale;
					localScale.x = 0f;
					this.messageContentPanelImage.transform.localScale = localScale;
					this.PopupAnimationStartTime = Time.time;
				}
				else
				{
					this.messageContentPanelImage.transform.localScale = Vector3.one;
				}
				this._IsAnimating = value;
			}
		}

		public override void CollectViews()
		{
			base.CollectViews();
			this._RootLayoutGroup = this.root.GetComponent<VerticalLayoutGroup>();
			this.paddingAtIconSide = this._RootLayoutGroup.padding.right;
			this.paddingAtOtherSide = this._RootLayoutGroup.padding.left;
			this.contentSizeFitter = this.root.GetComponent<ContentSizeFitter>();
			this.contentSizeFitter.enabled = false;
			this.root.GetComponentAtPath("MessageContentPanel", out this._MessageContentLayoutGroup);
			this.messageContentPanelImage = this._MessageContentLayoutGroup.GetComponent<Image>();
			this.messageContentPanelImage.transform.GetComponentAtPath("Image", out this.image);
			this.messageContentPanelImage.transform.GetComponentAtPath("TimeText", out this.timeText);
			this.messageContentPanelImage.transform.GetComponentAtPath("Text", out this.text);
			this.root.GetComponentAtPath("LeftIconImage", out this.leftIcon);
			this.root.GetComponentAtPath("RightIconImage", out this.rightIcon);
			this.colorAtInit = this.messageContentPanelImage.color;
		}

		public override void MarkForRebuild()
		{
			base.MarkForRebuild();
			if (this.contentSizeFitter)
			{
				this.contentSizeFitter.enabled = true;
			}
		}

		public void UpdateFromModel(ChatMessageModel model, MyParams parameters)
		{
			this.timeText.text = model.TimestampAsDateTime.ToString("HH:mm");
			string text = string.Concat(new object[] { "[#", this.ItemIndex, "] ", model.Text });
			if (this.text.text != text)
			{
				this.text.text = text;
			}
			this.leftIcon.gameObject.SetActive(!model.IsMine);
			this.rightIcon.gameObject.SetActive(model.IsMine);
			if (model.ImageIndex < 0)
			{
				this.image.gameObject.SetActive(false);
			}
			else
			{
				this.image.gameObject.SetActive(true);
				this.image.sprite = parameters.availableChatImages[model.ImageIndex];
			}
			if (model.IsMine)
			{
				this.messageContentPanelImage.rectTransform.pivot = new Vector2(1.4f, 0.5f);
				this.messageContentPanelImage.color = new Color(0.75f, 1f, 1f, this.colorAtInit.a);
				LayoutGroup rootLayoutGroup = this._RootLayoutGroup;
				TextAnchor textAnchor = 5;
				this.text.alignment = textAnchor;
				textAnchor = textAnchor;
				this._MessageContentLayoutGroup.childAlignment = textAnchor;
				rootLayoutGroup.childAlignment = textAnchor;
				this._RootLayoutGroup.padding.right = this.paddingAtIconSide;
				this._RootLayoutGroup.padding.left = this.paddingAtOtherSide;
			}
			else
			{
				this.messageContentPanelImage.rectTransform.pivot = new Vector2(-0.4f, 0.5f);
				this.messageContentPanelImage.color = this.colorAtInit;
				LayoutGroup rootLayoutGroup2 = this._RootLayoutGroup;
				TextAnchor textAnchor = 3;
				this.text.alignment = textAnchor;
				textAnchor = textAnchor;
				this._MessageContentLayoutGroup.childAlignment = textAnchor;
				rootLayoutGroup2.childAlignment = textAnchor;
				this._RootLayoutGroup.padding.right = this.paddingAtOtherSide;
				this._RootLayoutGroup.padding.left = this.paddingAtIconSide;
			}
		}

		internal void UpdatePopupAnimation()
		{
			float num = Time.time - this.PopupAnimationStartTime;
			float num2;
			if (num > 0.2f)
			{
				num2 = 1f;
			}
			else
			{
				num2 = Mathf.Sin(num / 0.2f * 3.1415927f / 2f);
			}
			Vector3 localScale = this.messageContentPanelImage.transform.localScale;
			localScale.x = num2;
			this.messageContentPanelImage.transform.localScale = localScale;
			if (num2 == 1f)
			{
				this.IsPopupAnimationActive = false;
			}
		}

		public Text timeText;

		public Text text;

		public Image leftIcon;

		public Image rightIcon;

		public Image image;

		public Image messageContentPanelImage;

		private const float POPUP_ANIMATION_TIME = 0.2f;

		private bool _IsAnimating;

		private VerticalLayoutGroup _RootLayoutGroup;

		private VerticalLayoutGroup _MessageContentLayoutGroup;

		private int paddingAtIconSide;

		private int paddingAtOtherSide;

		private Color colorAtInit;
	}
}
