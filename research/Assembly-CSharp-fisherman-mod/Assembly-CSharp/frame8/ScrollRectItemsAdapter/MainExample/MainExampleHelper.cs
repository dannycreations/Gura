using System;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.MainExample
{
	public class MainExampleHelper : MonoBehaviour
	{
		public static MainExampleHelper Instance { get; private set; }

		private void Start()
		{
			MainExampleHelper.Instance = this;
			DrawerCommandPanel drawer = DrawerCommandPanel.Instance;
			this._Adapters = Object.FindObjectsOfType<ScrollRectItemsAdapterExample>();
			drawer.Init(this._Adapters, true, true, true, false, true);
			BaseParams.UpdateMode updateMode = this._Adapters[0].Parameters.updateMode;
			for (int i = 1; i < this._Adapters.Length; i++)
			{
				if (this._Adapters[i].Parameters.updateMode != updateMode)
				{
					Debug.Log("Different update mode for other adapters.. setting all of them to " + updateMode);
					this._Adapters[i].Parameters.updateMode = updateMode;
				}
			}
			this.AddLoadNonOptimizedExampleButton();
			ButtonWithInputPanel scrollToAndResizeSetting = drawer.AddButtonWithInputPanel("ScrollTo & Resize");
			scrollToAndResizeSetting.button.onClick.AddListener(delegate
			{
				int location = scrollToAndResizeSetting.InputFieldValueAsInt;
				if (location < 0)
				{
					return;
				}
				drawer.RequestSmoothScrollTo(location, delegate
				{
					foreach (ScrollRectItemsAdapterExample scrollRectItemsAdapterExample in <Start>c__AnonStorey._Adapters)
					{
						ClientItemViewsHolder itemViewsHolderIfVisible = scrollRectItemsAdapterExample.GetItemViewsHolderIfVisible(location);
						if (itemViewsHolderIfVisible != null && itemViewsHolderIfVisible.expandCollapseComponent != null)
						{
							itemViewsHolderIfVisible.expandCollapseComponent.OnClicked();
						}
					}
				});
			});
			scrollToAndResizeSetting.transform.SetSiblingIndex(4);
			LabelWithToggles labelWithToggles = drawer.AddLabelWithTogglesPanel("UpdateMode", new string[] { "Default", "OnScroll", "Update" });
			labelWithToggles.subItems[(int)updateMode].toggle.isOn = true;
			labelWithToggles.ToggleChanged += delegate(int idx, bool isOn)
			{
				if (!isOn)
				{
					return;
				}
				drawer.DoForAllAdapters(delegate(ISRIA adapter)
				{
					adapter.BaseParameters.updateMode = (BaseParams.UpdateMode)idx;
				});
			};
		}

		public void OnAdapterInitialized()
		{
			if (++this._InitializedAdapters == this._Adapters.Length)
			{
				DrawerCommandPanel.Instance.RequestChangeItemCountToSpecified();
			}
		}

		private void AddLoadNonOptimizedExampleButton()
		{
			ButtonsPanel buttonsPanel = DrawerCommandPanel.Instance.AddButtonsPanel("Compare to classic ScrollView", string.Empty, string.Empty, string.Empty);
			buttonsPanel.button1.gameObject.AddComponent<LoadSceneOnClick>().sceneName = "non_optimized_example";
			buttonsPanel.button1.image.color = DrawerCommandPanel.Instance.backButtonBehavior.GetComponent<Image>().color;
			Text componentInChildren = DrawerCommandPanel.Instance.backButtonBehavior.GetComponentInChildren<Text>();
			Text componentInChildren2 = buttonsPanel.button1.GetComponentInChildren<Text>();
			componentInChildren2.font = componentInChildren.font;
			componentInChildren2.resizeTextForBestFit = componentInChildren.resizeTextForBestFit;
			componentInChildren2.fontStyle = componentInChildren.fontStyle;
			componentInChildren2.fontSize = componentInChildren.fontSize;
			buttonsPanel.transform.SetAsFirstSibling();
		}

		private ButtonWithInputPanel _ScrollToAndResizeSetting;

		private ScrollRectItemsAdapterExample[] _Adapters;

		private int _InitializedAdapters;
	}
}
