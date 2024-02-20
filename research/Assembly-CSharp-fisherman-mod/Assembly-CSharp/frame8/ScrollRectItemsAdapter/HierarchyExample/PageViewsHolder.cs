using System;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.HierarchyExample
{
	public class PageViewsHolder : BaseItemViewsHolder
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this._RootLayoutGroup = this.root.GetComponent<HorizontalLayoutGroup>();
			this._PanelRT = this.root.GetChild(0) as RectTransform;
			this._PanelRT.GetComponentAtPath("TitleText", out this.titleText);
			this._PanelRT.GetComponentAtPath("FoldOutButton", out this.foldoutButton);
			this._PanelRT.GetComponentAtPath("DirectoryIconImage", out this._DirectoryIconImage);
			this._PanelRT.GetComponentAtPath("FileIconImage", out this._FileIconImage);
			this.foldoutButton.transform.GetComponentAtPath("FoldOutArrowImage", out this.foldoutArrowImage);
		}

		public override void MarkForRebuild()
		{
			LayoutRebuilder.MarkLayoutForRebuild(this._PanelRT);
			LayoutRebuilder.MarkLayoutForRebuild(this.root);
			base.MarkForRebuild();
		}

		public void UpdateViews(FSEntryNodeModel model)
		{
			this.titleText.text = model.title;
			bool isDirectory = model.IsDirectory;
			this.foldoutButton.interactable = isDirectory;
			this._DirectoryIconImage.gameObject.SetActive(isDirectory);
			this._FileIconImage.gameObject.SetActive(!isDirectory);
			this.foldoutArrowImage.gameObject.SetActive(isDirectory);
			if (isDirectory)
			{
				this.foldoutArrowImage.rectTransform.localRotation = Quaternion.Euler(0f, 0f, (float)((!model.expanded) ? 0 : (-90)));
			}
			this._RootLayoutGroup.padding.left = 25 * model.depth;
		}

		public Text titleText;

		public Image foldoutArrowImage;

		public Button foldoutButton;

		private Image _FileIconImage;

		private Image _DirectoryIconImage;

		private RectTransform _PanelRT;

		private HorizontalLayoutGroup _RootLayoutGroup;
	}
}
