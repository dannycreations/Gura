using System;
using System.Diagnostics;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.Drawer
{
	public class DrawerCommandPanel : MonoBehaviour
	{
		public static DrawerCommandPanel Instance { get; private set; }

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<int> ItemCountChangeRequested;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> AddItemRequested;

		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<bool> RemoveItemRequested;

		private void Awake()
		{
			DrawerCommandPanel.Instance = this;
			base.transform.GetComponentAtPath("SettingsScrollView/SettingsPanel", out this.settingsPanel);
			base.transform.GetComponentAtPath("TitlePanel/LabelText", out this.titleText);
			base.transform.GetComponentAtPath("NavPanelParent/BackPanel/Button", out this.backButtonBehavior);
			base.transform.GetComponentAtPath("NavPanelParent/NextPanel/Button", out this.nextButtonBehavior);
			this.settingsPanel.GetComponentAtPath("ScrollToPanel", out this.scrollToPanel);
			this.settingsPanel.GetComponentAtPath("SetCountPanel", out this.setCountPanel);
			this.SetSceneName();
			Transform transform = base.transform;
			while (!this._ScrollRect && (transform = transform.parent))
			{
				this._ScrollRect = transform.GetComponent<ScrollRect>();
			}
		}

		private void Start()
		{
			if (!this.skipAutoDraw)
			{
				this.Draw(100f);
			}
		}

		private void Update()
		{
			if ((float)Screen.width != this._LastScreenWidth && (float)Screen.height != this._LastScreenHeight)
			{
				if (Screen.width < Screen.height)
				{
					this.Draw(-100f);
				}
				this._LastScreenWidth = (float)Screen.width;
				this._LastScreenHeight = (float)Screen.height;
			}
			if (this.contentGravityPanel)
			{
				bool flag = false;
				foreach (ISRIA isria in this._Adapters)
				{
					if (isria.ContentVirtualSizeToViewportRatio < 1.0)
					{
						flag = true;
						break;
					}
				}
				this.contentGravityPanel.Interactable = flag;
			}
		}

		private void OnDestroy()
		{
			if (this.simulateLowEndDeviceSetting)
			{
				this.simulateLowEndDeviceSetting.toggle.isOn = false;
			}
			DrawerCommandPanel.Instance = null;
		}

		public void Init(ISRIA adapter, bool addGravityCommand = true, bool addItemEdgeFreezeCommand = true, bool addContentEdgeFreezeCommand = true, bool addServerDelaySetting = true, bool addOneItemAddRemovePanel = true)
		{
			this.Init(new ISRIA[] { adapter }, addGravityCommand, addItemEdgeFreezeCommand, addContentEdgeFreezeCommand, addServerDelaySetting, addOneItemAddRemovePanel);
		}

		public void Init(ISRIA[] adapters, bool addGravityCommand = true, bool addItemEdgeFreezeCommand = true, bool addContentEdgeFreezeCommand = true, bool addServerDelaySetting = true, bool addOneItemAddRemovePanel = true)
		{
			this._Adapters = adapters;
			this.scrollToPanel.mainSubPanel.button.onClick.AddListener(new UnityAction(this.RequestSmoothScrollToSpecified));
			this.setCountPanel.button.onClick.AddListener(new UnityAction(this.RequestChangeItemCountToSpecifiedIgnoringServerDelay));
			if (addGravityCommand)
			{
				this.contentGravityPanel = this.AddLabelWithTogglesPanel("Gravity when content smaller than viewport", new string[] { "Start", "Center", "End" });
				this.contentGravityPanel.ToggleChanged += delegate(int toggleIdx, bool isOn)
				{
					if (!isOn)
					{
						return;
					}
					this.DoForAllAdapters(delegate(ISRIA adapter)
					{
						if (adapter.Initialized)
						{
							adapter.BaseParameters.contentGravity = toggleIdx + BaseParams.ContentGravity.START;
							adapter.BaseParameters.UpdateContentPivotFromGravityType();
							adapter.SetVirtualAbstractNormalizedScrollPosition(1.0, true);
						}
					});
				};
			}
			if (addItemEdgeFreezeCommand)
			{
				this.freezeItemEndEdgeToggle = this.AddLabelWithTogglePanel("Freeze item end edge when expanding/collapsing").toggle;
			}
			if (addContentEdgeFreezeCommand)
			{
				this.freezeContentEndEdgeToggle = this.AddLabelWithTogglePanel("Freeze content end edge when adding items (ex. for chat)").toggle;
			}
			if (addServerDelaySetting)
			{
				this.serverDelaySetting = this.AddLabelWithInputPanel("Server simulated delay:");
				this.serverDelaySetting.inputField.text = 1 + string.Empty;
				this.serverDelaySetting.inputField.keyboardType = 4;
				this.serverDelaySetting.inputField.characterLimit = 1;
			}
			if (addOneItemAddRemovePanel)
			{
				this.addRemoveOnePanel = this.AddButtonsPanel("+1 tail", "+1 head", "-1 tail", "-1 head");
				this.addRemoveOnePanel.transform.SetSiblingIndex(1);
				this.addRemoveOnePanel.button1.onClick.AddListener(delegate
				{
					if (this.AddItemRequested != null)
					{
						this.AddItemRequested(true);
					}
				});
				this.addRemoveOnePanel.button2.onClick.AddListener(delegate
				{
					if (this.AddItemRequested != null)
					{
						this.AddItemRequested(false);
					}
				});
				this.addRemoveOnePanel.button3.onClick.AddListener(delegate
				{
					if (this.RemoveItemRequested != null)
					{
						this.RemoveItemRequested(true);
					}
				});
				this.addRemoveOnePanel.button4.onClick.AddListener(delegate
				{
					if (this.RemoveItemRequested != null)
					{
						this.RemoveItemRequested(false);
					}
				});
			}
			this.galleryEffectSetting = this.AddLabelWithSliderPanel("Gallery effect", "None", "Max");
			this.galleryEffectSetting.slider.onValueChanged.AddListener(delegate(float v)
			{
				this.DoForAllAdapters(delegate(ISRIA adapter)
				{
					adapter.BaseParameters.galleryEffectAmount = v;
				});
			});
			this.galleryEffectSetting.Set(0f, 1f, 0.1f);
			int vSyncCountBefore = QualitySettings.vSyncCount;
			int targetFrameRateBefore = Application.targetFrameRate;
			this.simulateLowEndDeviceSetting = this.AddLabelWithTogglePanel("Simulate low-end device");
			this.simulateLowEndDeviceSetting.transform.SetAsLastSibling();
			this.simulateLowEndDeviceSetting.toggle.onValueChanged.AddListener(delegate(bool isOn)
			{
				if (isOn)
				{
					vSyncCountBefore = QualitySettings.vSyncCount;
					targetFrameRateBefore = Application.targetFrameRate;
					QualitySettings.vSyncCount = 0;
					Application.targetFrameRate = 20;
				}
				else
				{
					if (QualitySettings.vSyncCount == 0)
					{
						QualitySettings.vSyncCount = vSyncCountBefore;
					}
					if (Application.targetFrameRate == 20)
					{
						Application.targetFrameRate = targetFrameRateBefore;
					}
				}
			});
		}

		public ButtonWithInputPanel AddButtonWithInputPanel(string label = "")
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.buttonWithInputPrefab.gameObject);
			gameObject.transform.SetParent(this.settingsPanel, false);
			ButtonWithInputPanel component = gameObject.GetComponent<ButtonWithInputPanel>();
			component.button.transform.GetComponentAtPath("Text").text = label;
			return component;
		}

		public LabelWithInputPanel AddLabelWithInputPanel(string label = "")
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.labelWithInputPrefab.gameObject);
			gameObject.transform.SetParent(this.settingsPanel, false);
			LabelWithInputPanel component = gameObject.GetComponent<LabelWithInputPanel>();
			component.labelText.text = label;
			return component;
		}

		public ButtonsPanel AddButtonsPanel(string label1 = "", string label2 = "", string label3 = "", string label4 = "")
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.buttonsPrefab.gameObject);
			gameObject.transform.SetParent(this.settingsPanel, false);
			ButtonsPanel component = gameObject.GetComponent<ButtonsPanel>();
			component.Init(label1, label2, label3, label4);
			return component;
		}

		public LabelWithToggle AddLabelWithTogglePanel(string label = "")
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.labelWithTogglePrefab.gameObject);
			gameObject.transform.SetParent(this.settingsPanel, false);
			LabelWithToggle component = gameObject.GetComponent<LabelWithToggle>();
			component.Init(label);
			return component;
		}

		public LabelWithToggles AddLabelWithTogglesPanel(string mainLabel, params string[] subItemLabels)
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.labelWithTogglesPrefab.gameObject);
			gameObject.transform.SetParent(this.settingsPanel, false);
			LabelWithToggles component = gameObject.GetComponent<LabelWithToggles>();
			component.Init(mainLabel, subItemLabels);
			return component;
		}

		public LabelWithSliderPanel AddLabelWithSliderPanel(string label = "", string minLabel = "", string maxLabel = "")
		{
			GameObject gameObject = Object.Instantiate<GameObject>(this.labelWithSliderPrefab.gameObject);
			gameObject.transform.SetParent(this.settingsPanel, false);
			LabelWithSliderPanel component = gameObject.GetComponent<LabelWithSliderPanel>();
			component.Init(label, minLabel, maxLabel);
			return component;
		}

		public void RequestChangeItemCountToSpecified()
		{
			int inputFieldValueAsInt = this.setCountPanel.InputFieldValueAsInt;
			if (this.ItemCountChangeRequested != null)
			{
				this.ItemCountChangeRequested(inputFieldValueAsInt);
			}
		}

		public void RequestChangeItemCountToSpecifiedIgnoringServerDelay()
		{
			bool flag = this.serverDelaySetting != null;
			string text = ((!flag) ? null : this.serverDelaySetting.inputField.text);
			if (flag)
			{
				this.serverDelaySetting.inputField.text = "0";
			}
			this.RequestChangeItemCountToSpecified();
			if (flag)
			{
				this.serverDelaySetting.inputField.text = text;
			}
		}

		public void RequestSmoothScrollToSpecified()
		{
			this.RequestSmoothScrollTo(this.scrollToPanel.mainSubPanel.InputFieldValueAsInt, null);
		}

		public void RequestSmoothScrollTo(int index, Action onDone)
		{
			if (index < 0 || index + 1 > this._Adapters[0].GetItemsCount())
			{
				return;
			}
			int numDone = 0;
			float dur = this.scrollToPanel.durationAdvPanel.InputFieldValueAsFloat;
			dur = Mathf.Clamp(dur, 0.01f, 9f);
			this.DoForAllAdapters(delegate(ISRIA adapter)
			{
				adapter.SmoothScrollTo(index, dur, Mathf.Clamp01(this.scrollToPanel.gravityAdvPanel.InputFieldValueAsFloat), Mathf.Clamp01(this.scrollToPanel.itemPivotAdvPanel.InputFieldValueAsFloat), delegate(float progress)
				{
					this.scrollToPanel.durationAdvPanel.inputField.text = (dur * progress).ToString("#0.##");
					if (progress == 1f && ++numDone == this._Adapters.Length && onDone != null)
					{
						onDone();
					}
					return true;
				}, true);
			});
		}

		public void DoForAllAdapters(Action<ISRIA> action)
		{
			for (int i = 0; i < this._Adapters.Length; i++)
			{
				action(this._Adapters[i]);
			}
		}

		private void Draw(float horSpeed)
		{
			PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
			pointerEventData.scrollDelta = new Vector2(horSpeed, 0f);
			this._ScrollRect.OnScroll(pointerEventData);
			this._ScrollRect.velocity = pointerEventData.scrollDelta * 7f;
		}

		private void SetSceneName()
		{
			string text = SceneManager.GetActiveScene().name;
			text = text.Replace("_", " ");
			if (text == "example")
			{
				text = "main demo";
			}
			else
			{
				text = text.Replace("example", string.Empty);
			}
			text = char.ToUpper(text[0]) + text.Substring(1);
			this.titleText.text = text;
		}

		public ButtonWithInputPanel buttonWithInputPrefab;

		public LabelWithInputPanel labelWithInputPrefab;

		public LabelWithToggle labelWithTogglePrefab;

		public LabelWithToggles labelWithTogglesPrefab;

		public LabelWithSliderPanel labelWithSliderPrefab;

		public ButtonsPanel buttonsPrefab;

		public bool skipAutoDraw;

		[NonSerialized]
		public Text titleText;

		[NonSerialized]
		public LoadSceneOnClick backButtonBehavior;

		[NonSerialized]
		public LoadSceneOnClick nextButtonBehavior;

		[NonSerialized]
		public LabelWithToggles contentGravityPanel;

		[NonSerialized]
		public ButtonsPanel addRemoveOnePanel;

		[NonSerialized]
		public ButtonWithInputPanel setCountPanel;

		[NonSerialized]
		public ScrollToPanel scrollToPanel;

		[NonSerialized]
		public RectTransform settingsPanel;

		[NonSerialized]
		public Toggle freezeItemEndEdgeToggle;

		[NonSerialized]
		public Toggle freezeContentEndEdgeToggle;

		[NonSerialized]
		public LabelWithInputPanel serverDelaySetting;

		[NonSerialized]
		public LabelWithToggle simulateLowEndDeviceSetting;

		[NonSerialized]
		public LabelWithSliderPanel galleryEffectSetting;

		private const int TARGET_FRAMERATE_FOR_SIMULATING_SLOW_DEVICES = 20;

		private ScrollRect _ScrollRect;

		private ISRIA[] _Adapters;

		private float _LastScreenWidth;

		private float _LastScreenHeight;
	}
}
