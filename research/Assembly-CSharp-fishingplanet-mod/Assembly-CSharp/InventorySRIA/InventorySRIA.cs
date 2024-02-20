using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using I2.Loc;
using InventorySRIA.Models;
using InventorySRIA.ViewsHolders;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace InventorySRIA
{
	public class InventorySRIA : SRIA<MyParams, BaseVH>, ShowDetailedInfo.ISizeChangesHandler
	{
		public static List<string> GetCategories(GroupCategoryType type)
		{
			return InventorySRIA._groupItems[type].Select((ItemSubTypes x) => x.ToString()).ToList<string>();
		}

		public static List<string> GetParentCategories(GroupCategoryType type)
		{
			if (StaticUserData.AllCategories == null)
			{
				return new List<string>();
			}
			return StaticUserData.AllCategories.Where((InventoryCategory x) => x.ItemSubType != null && InventorySRIA._groupItems[type].Contains(x.ItemSubType.Value) && x.ParentCategory != null).Select(delegate(InventoryCategory x)
			{
				ItemSubTypes? itemSubType = x.ParentCategory.ItemSubType;
				return ((itemSubType == null) ? ((ItemSubTypes)x.ParentCategory.ItemType) : itemSubType.Value).ToString();
			}).ToList<string>();
		}

		private void OnInventoryUpdated()
		{
			this.BuildModels();
		}

		private void SendGameScreenChange()
		{
			GameScreenType gameScreenType = GameScreenType.Equipment;
			StoragePlaces places = this._Params.places;
			if (places != StoragePlaces.Storage)
			{
				if (places == StoragePlaces.Equipment)
				{
					gameScreenType = GameScreenType.Equipment;
				}
			}
			else if (this._Params.isExceedInventory)
			{
				gameScreenType = GameScreenType.ExceedStorage;
			}
			else
			{
				gameScreenType = GameScreenType.Storage;
			}
			GameScreenTabType currentInventoryTab = this.GetCurrentInventoryTab();
			MonoBehaviour.print(currentInventoryTab);
			UIStatsCollector.ChangeGameScreen(gameScreenType, currentInventoryTab, null, null, null, null, null);
		}

		public static GameScreenTabType GetMissionTabByItemType(ItemSubTypes itemType)
		{
			return InventorySRIA.GetMissionTabByGroupCategory(InventorySRIA._groupItems.FirstOrDefault((KeyValuePair<GroupCategoryType, HashSet<ItemSubTypes>> x) => x.Value.Contains(itemType)).Key);
		}

		public static GameScreenTabType GetMissionTabByGroupCategory(GroupCategoryType cat)
		{
			switch (cat)
			{
			case GroupCategoryType.RodAndReels:
				return GameScreenTabType.RodsReels;
			case GroupCategoryType.Lines:
				return GameScreenTabType.Lines;
			case GroupCategoryType.TerminalTacles:
				return GameScreenTabType.BobbersHooks;
			case GroupCategoryType.Baits:
				return GameScreenTabType.Baits;
			case GroupCategoryType.JigWithBaits:
				return GameScreenTabType.JigsHeadBaits;
			case GroupCategoryType.HardBaits:
				return GameScreenTabType.HardBaits;
			case GroupCategoryType.BassJigs:
				return GameScreenTabType.BassJigs;
			case GroupCategoryType.SpoonAndSpinners:
				return GameScreenTabType.SpoonSpinners;
			case GroupCategoryType.ChumsAll:
			case GroupCategoryType.ChumBases:
			case GroupCategoryType.ChumAromas:
			case GroupCategoryType.ChumParticles:
			case GroupCategoryType.Chums:
				return GameScreenTabType.Chums;
			case GroupCategoryType.OutfitAndTools:
				return GameScreenTabType.OutfitTools;
			case GroupCategoryType.Misc:
				return GameScreenTabType.Misc;
			case GroupCategoryType.UnderwaterItem:
				return GameScreenTabType.Trash;
			default:
				return GameScreenTabType.All;
			}
		}

		public GameScreenTabType GetCurrentInventoryTab()
		{
			if (this._Params.supportedGroups.Length == 1 || (this._Params.supportedGroups.Length > 0 && InventorySRIA.ChumMixingGroups.Contains(this._Params.supportedGroups[0])))
			{
				return InventorySRIA.GetMissionTabByGroupCategory(this._Params.supportedGroups[0].type);
			}
			return GameScreenTabType.All;
		}

		public void SetStorage(StorageTypes type)
		{
			this._currentStorage = type;
			if (!this.GroupsFoldMap.ContainsKey(type))
			{
				this.GroupsFoldMap.Add(type, new Dictionary<GroupCategoryType, bool>());
				foreach (GroupCategoryType groupCategoryType in InventorySRIA._groupItems.Keys)
				{
					this.GroupsFoldMap[type][groupCategoryType] = true;
				}
			}
			if (type != StorageTypes.Equipment)
			{
				if (type != StorageTypes.Storage)
				{
					if (type == StorageTypes.Excess)
					{
						this.Equipment.enabled = false;
						this.Storage.enabled = true;
						this._Params.DropMeStorage = this.Storage;
						this._Params.places = this.Storage.storage;
						this._Params.isExceedInventory = true;
						this.ExpandContent.SetActive(true);
						this.EquipmentContent.SetActive(false);
						this.ExcessContent.SetActive(true);
					}
				}
				else
				{
					this.Equipment.enabled = false;
					this.Storage.enabled = true;
					this._Params.DropMeStorage = this.Storage;
					this._Params.places = this.Storage.storage;
					this._Params.isExceedInventory = false;
					this.ExpandContent.SetActive(true);
					this.EquipmentContent.SetActive(false);
					this.ExcessContent.SetActive(false);
				}
			}
			else
			{
				this.Equipment.enabled = true;
				this.Storage.enabled = false;
				this._Params.DropMeStorage = this.Equipment;
				this._Params.places = this.Equipment.storage;
				this._Params.isExceedInventory = false;
				this.ExpandContent.SetActive(false);
				this.EquipmentContent.SetActive(true);
				this.ExcessContent.SetActive(false);
			}
			if (!this.FilterSelector.activeSelf)
			{
				this.FilterSelector.SetActive(true);
			}
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
				this.RefreshFiltersAndBuildModels();
				if (this.OnInventoryTabSwitched != null)
				{
					this.OnInventoryTabSwitched.Invoke();
				}
				return;
			}
			this.RefreshFiltersAndBuildModels();
		}

		public void SetEquipment(bool isOn)
		{
			if (!isOn)
			{
				return;
			}
			this.SetStorage(StorageTypes.Equipment);
			InitStorages.Instance.ActiveStorage.Setup(StoragePlaces.Equipment);
			this.SendGameScreenChange();
			if (this.OnInventoryTabSwitched != null)
			{
				this.OnInventoryTabSwitched.Invoke();
			}
		}

		public void SetStorage(bool isOn)
		{
			if (!isOn)
			{
				return;
			}
			this.SetStorage(StorageTypes.Storage);
			InitStorages.Instance.ActiveStorage.Setup(StoragePlaces.Storage);
			this.SendGameScreenChange();
			if (this.OnInventoryTabSwitched != null)
			{
				this.OnInventoryTabSwitched.Invoke();
			}
		}

		public void SetExcess(bool isOn)
		{
			if (!isOn)
			{
				return;
			}
			this.SetStorage(StorageTypes.Excess);
			this.SendGameScreenChange();
			if (this.OnInventoryTabSwitched != null)
			{
				this.OnInventoryTabSwitched.Invoke();
			}
		}

		public void SetSpecials(bool isOn)
		{
			if (isOn)
			{
				this.FilterSelector.SetActive(false);
				base.gameObject.SetActive(false);
				if (this.OnInventoryTabSwitched != null)
				{
					this.OnInventoryTabSwitched.Invoke();
				}
			}
		}

		public void SetTemplates(bool isOn)
		{
			if (isOn)
			{
				this.FilterSelector.SetActive(false);
				base.gameObject.SetActive(false);
				if (this.OnInventoryTabSwitched != null)
				{
					this.OnInventoryTabSwitched.Invoke();
				}
			}
		}

		public void SetLicenses(bool isOn)
		{
			if (isOn)
			{
				this.FilterSelector.SetActive(false);
				base.gameObject.SetActive(false);
				if (this.OnInventoryTabSwitched != null)
				{
					this.OnInventoryTabSwitched.Invoke();
				}
			}
		}

		public void SetKeepnet(bool isOn)
		{
			if (isOn)
			{
				this.FilterSelector.SetActive(false);
				base.gameObject.SetActive(false);
				if (this.OnInventoryTabSwitched != null)
				{
					this.OnInventoryTabSwitched.Invoke();
				}
			}
		}

		public void SetCategory(GroupCategoryType cat)
		{
			GroupCategorySetter[] componentsInChildren = this.FilterSelector.GetComponentsInChildren<GroupCategorySetter>();
			GroupCategorySetter groupCategorySetter = componentsInChildren.FirstOrDefault((GroupCategorySetter x) => x.SupportedGroup == cat);
			if (groupCategorySetter != null)
			{
				groupCategorySetter.GetComponent<Toggle>().isOn = true;
			}
		}

		public void SetSupportedGroups(GroupCategoryType type)
		{
			GroupCategoryIcon[] array;
			if (type == GroupCategoryType.All)
			{
				array = this.StandartSupportedGroups.Where((GroupCategoryIcon x) => x.type != type).ToArray<GroupCategoryIcon>();
			}
			else if (type == GroupCategoryType.ChumsAll)
			{
				array = InventorySRIA.ChumMixingGroups.Where((GroupCategoryIcon x) => x.type != type).ToArray<GroupCategoryIcon>();
			}
			else
			{
				array = this.AccumulatedSupportedGroups.Where((GroupCategoryIcon x) => x.type == type).ToArray<GroupCategoryIcon>();
			}
			if (this._Params.supportedGroups.SequenceEqual(array) && this._lastDisplayedStorage == this._currentStorage)
			{
				return;
			}
			this.ScrollTo(0, 0f, 0f);
			this._Params.supportedGroups = array;
			this.needUpdateGST = true;
			this.BuildModels();
			this.UiNavigation.SetPaused(false);
			if (!base.Initialized)
			{
				return;
			}
			base.StopAllCoroutines();
			this.AssignSelectFirstItem();
		}

		private void AssignSelectFirstItem()
		{
			if (base.gameObject.activeInHierarchy && this._Params.data.Count > 0)
			{
				BaseModel baseModel = this._Params.data.FirstOrDefault((BaseModel x) => x.CachedType == typeof(ExpandableInventoryItemModel));
				this._lastIndex = ((baseModel == null) ? 0 : this._Params.data.IndexOf(baseModel));
			}
		}

		public void UpdateActivityChumIngredients()
		{
			List<BaseModel> data = base.Parameters.data;
			for (int i = 0; i < data.Count; i++)
			{
				ExpandableInventoryItemVH expandableInventoryItemVH = base.GetItemViewsHolder(i) as ExpandableInventoryItemVH;
				if (expandableInventoryItemVH != null && expandableInventoryItemVH.IiComponent.InventoryItem is ChumIngredient)
				{
					expandableInventoryItemVH.IiComponent.UpdateActivity();
				}
			}
		}

		public void SetChumIngredientMixing(bool isMixing, ChumIngredient ii)
		{
			int num = base.Parameters.data.FindIndex(delegate(BaseModel p)
			{
				ExpandableInventoryItemModel expandableInventoryItemModel = p as ExpandableInventoryItemModel;
				bool flag2;
				if (expandableInventoryItemModel != null && expandableInventoryItemModel.item.ItemId == ii.ItemId)
				{
					Guid? instanceId = expandableInventoryItemModel.item.InstanceId;
					bool flag = instanceId != null;
					Guid? instanceId2 = ii.InstanceId;
					flag2 = flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault());
				}
				else
				{
					flag2 = false;
				}
				return flag2;
			});
			if (num != -1)
			{
				ExpandableInventoryItemVH expandableInventoryItemVH = base.GetItemViewsHolder(num) as ExpandableInventoryItemVH;
				if (expandableInventoryItemVH != null)
				{
					expandableInventoryItemVH.IiComponent.UpdateMixingState(isMixing);
				}
			}
		}

		public void Scroll2Item(List<int> ids)
		{
			int num = -1;
			List<BaseModel> data = base.Parameters.data;
			for (int i = 0; i < data.Count; i++)
			{
				ExpandableInventoryItemModel expandableInventoryItemModel = data[i] as ExpandableInventoryItemModel;
				if (expandableInventoryItemModel != null)
				{
					if (ids.Contains(expandableInventoryItemModel.item.ItemId))
					{
						num = ((num != -1) ? Math.Min(i, num) : i);
					}
					if (num != -1 && ids.Count == 1)
					{
						break;
					}
				}
			}
			if (num != -1 && num < this._Params.data.Count && !StaticUserData.IS_IN_TUTORIAL)
			{
				this._lastIndex = num;
				this.SmoothScrollTo(num, 0.1f, 0f, 0f, null, false);
				base.StartCoroutine(this.SetElementSelected(this._lastIndex, 0.15f));
			}
		}

		protected override void Start()
		{
			this._cg = ActivityState.GetParentActivityState(base.transform).CanvasGroup;
			base.Start();
			this.SetEquipment(true);
			this.AssignSelectFirstItem();
		}

		protected override void Awake()
		{
			base.Awake();
			if (this.UiNavigation == null)
			{
				this.UiNavigation = base.GetComponent<UINavigation>();
			}
			UINavigation uiNavigation = this.UiNavigation;
			uiNavigation.OnBottomReached = (Action<Selectable>)Delegate.Combine(uiNavigation.OnBottomReached, new Action<Selectable>(this.OnBottomReached));
			UINavigation uiNavigation2 = this.UiNavigation;
			uiNavigation2.OnTopReached = (Action<Selectable>)Delegate.Combine(uiNavigation2.OnTopReached, new Action<Selectable>(this.OnTopReached));
			this.AccumulatedSupportedGroups.AddRange(this.StandartSupportedGroups);
			this.AccumulatedSupportedGroups.AddRange(InventorySRIA.ChumMixingGroups);
			this.filters = new List<GroupCategorySetter>(this.FilterSelector.GetComponentsInChildren<GroupCategorySetter>());
			if (this.FilterSelectorToggleGroup == null)
			{
				this.FilterSelectorToggleGroup = this.FilterSelector.GetComponent<ToggleGroup>();
			}
		}

		public void SetFilters(bool mixing)
		{
			List<GroupCategoryIcon> list = ((!mixing) ? this.StandartSupportedGroups : InventorySRIA.ChumMixingGroups);
			int i = 0;
			while (i < list.Count)
			{
				GroupCategorySetter groupCategorySetter;
				if (i < this.filters.Count)
				{
					groupCategorySetter = this.filters[i];
				}
				else
				{
					groupCategorySetter = Object.Instantiate<GroupCategorySetter>(this.filters[0], this.FilterSelector.transform);
					this.filters.Add(groupCategorySetter);
				}
				GroupCategoryIcon groupCategoryIcon = list[i++];
				groupCategorySetter.Init(groupCategoryIcon, this.FilterSelectorToggleGroup);
				groupCategorySetter.gameObject.SetActive(true);
			}
			while (i < this.filters.Count)
			{
				this.filters[i++].gameObject.SetActive(false);
			}
			this.FilterSelectorToggleGroup.allowSwitchOff = false;
		}

		private void RefreshFiltersAndBuildModels()
		{
			if (!base.Initialized)
			{
				return;
			}
			bool isOn = InitRods.Instance.ChumMixToggle.Toggle.isOn;
			GroupCategorySetter groupCategorySetter = this.filters.FirstOrDefault((GroupCategorySetter x) => x.gameObject.activeInHierarchy && x.Toggle.isOn);
			this.SetFilters(isOn);
			this.FilterSelectorToggleGroup.allowSwitchOff = true;
			this.FilterSelectorToggleGroup.SetAllTogglesOff();
			if (groupCategorySetter == null || !groupCategorySetter.gameObject.activeInHierarchy)
			{
				this.filters[0].Toggle.isOn = true;
			}
			else
			{
				groupCategorySetter.Toggle.isOn = true;
			}
			this.FilterSelectorToggleGroup.allowSwitchOff = false;
			this.Blink();
		}

		public void SubscribeAndRefresh()
		{
			PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
			this._Params.Rods.OutfitOrRodOrChumMixingSwitched += this.RefreshFiltersAndBuildModels;
			this.subscribed = true;
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.needUpdateGST = true;
			this.BuildModels();
			if (!base.Initialized)
			{
				return;
			}
		}

		public void Unsubscribe()
		{
			PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
			this._Params.Rods.OutfitOrRodOrChumMixingSwitched -= this.RefreshFiltersAndBuildModels;
			this.subscribed = false;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			this.OnInventoryTabSwitched.RemoveAllListeners();
			UINavigation uiNavigation = this.UiNavigation;
			uiNavigation.OnBottomReached = (Action<Selectable>)Delegate.Remove(uiNavigation.OnBottomReached, new Action<Selectable>(this.OnBottomReached));
			UINavigation uiNavigation2 = this.UiNavigation;
			uiNavigation2.OnTopReached = (Action<Selectable>)Delegate.Remove(uiNavigation2.OnTopReached, new Action<Selectable>(this.OnTopReached));
			if (this.subscribed)
			{
				this.Unsubscribe();
			}
		}

		protected override void Update()
		{
			if (this._cg != null && !this._cg.interactable)
			{
				return;
			}
			if (this.needRebuild && base.Initialized)
			{
				this.needRebuild = false;
				this.Blink();
			}
			base.Update();
		}

		private void OnTopReached(Selectable curr)
		{
			if (!base.gameObject.activeInHierarchy || (this._cg != null && !this._cg.interactable))
			{
				return;
			}
			BaseVH itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(0);
			if (curr != null && !EventSystem.current.alreadySelecting && curr.transform.IsChildOf(base.transform) && ((itemViewsHolderIfVisible != null && itemViewsHolderIfVisible.root.gameObject == curr.gameObject) || !curr.gameObject.activeInHierarchy))
			{
				this.UiNavigation.SetPaused(true);
				EventSystem.current.SetSelectedGameObject(null);
				base.StopAllCoroutines();
				int num = this._Params.data.Count - 1;
				this.ScrollTo(num, 1f, 1f);
				if (this.ExpandContent.activeInHierarchy)
				{
					EventSystem.current.SetSelectedGameObject(this.ExpandButton.gameObject);
					this.UiNavigation.SetPaused(false);
					this.UiNavigation.UpdateFromEventSystemSelected();
				}
				else
				{
					base.StartCoroutine(this.SetElementSelected(num, 0.06f));
				}
			}
		}

		private void OnBottomReached(Selectable curr)
		{
			if (!base.gameObject.activeInHierarchy || (this._cg != null && !this._cg.interactable) || (this.ExpandButton.gameObject.activeInHierarchy && curr != this.ExpandButton))
			{
				return;
			}
			if (curr != null && !EventSystem.current.alreadySelecting && curr.transform.IsChildOf(base.transform))
			{
				int num = this._Params.data.Count - 1;
				BaseVH itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(num);
				if ((itemViewsHolderIfVisible != null && itemViewsHolderIfVisible.root.gameObject == curr.gameObject) || curr.gameObject == this.ExpandButton.gameObject || !curr.gameObject.activeInHierarchy)
				{
					this.UiNavigation.SetPaused(true);
					EventSystem.current.SetSelectedGameObject(null);
					base.StopAllCoroutines();
					this.ScrollTo(0, 0f, 0f);
					base.StartCoroutine(this.SetElementSelected(0, 0.06f));
					this.AssignSelectFirstItem();
				}
			}
		}

		protected override BaseVH CreateViewsHolder(int itemIndex)
		{
			Type cachedType = this._Params.data[itemIndex].CachedType;
			if (cachedType == typeof(HeaderGroupModel))
			{
				HeaderGroupVH headerGroupVH = new HeaderGroupVH();
				headerGroupVH.Init(this._Params.headerGroupPrefab, itemIndex, true, true);
				headerGroupVH.ExpandPanel.OnExpanded.RemoveAllListeners();
				headerGroupVH.ExpandPanel.OnExpanded.AddListener(delegate(bool a)
				{
					this.OnGroupExpandRequested(this._Params.data[itemIndex] as HeaderGroupModel, a, itemIndex + 1);
				});
				this.UiNavigation.ForceUpdate();
				return headerGroupVH;
			}
			if (cachedType == typeof(ExpandableInventoryItemModel))
			{
				ExpandableInventoryItemVH instance = new ExpandableInventoryItemVH();
				instance.Init(this._Params.inventoryItemPrefab, itemIndex, true, true);
				instance.IiComponent.OnSelected = delegate
				{
					this._lastIndex = instance.ItemIndex;
				};
				instance.sdi.sizeChangesHandler = this;
				this.UiNavigation.ForceUpdate();
				return instance;
			}
			throw new InvalidOperationException("Unrecognized model type: " + cachedType.Name);
		}

		protected override void UpdateViewsHolder(BaseVH newOrRecycled)
		{
			BaseModel model = this._Params.data[newOrRecycled.ItemIndex];
			newOrRecycled.UpdateViews(model);
			if (model.CachedType == typeof(HeaderGroupModel))
			{
				HeaderGroupVH headerGroupVH = newOrRecycled as HeaderGroupVH;
				headerGroupVH.ExpandPanel.OnExpanded.RemoveAllListeners();
				headerGroupVH.ExpandPanel.OnExpanded.AddListener(delegate(bool a)
				{
					this.OnGroupExpandRequested(model as HeaderGroupModel, a, newOrRecycled.ItemIndex + 1);
				});
			}
			else if (model.CachedType == typeof(ExpandableInventoryItemModel))
			{
				ExpandableInventoryItemVH vh = newOrRecycled as ExpandableInventoryItemVH;
				vh.IiComponent.OnSelected = delegate
				{
					this._lastIndex = vh.ItemIndex;
				};
			}
			this.UiNavigation.ForceUpdate();
		}

		protected override bool IsRecyclable(BaseVH potentiallyRecyclable, int indexOfItemThatWillBecomeVisible, float heightOfItemThatWillBecomeVisible)
		{
			return EventSystem.current != null && potentiallyRecyclable != null && potentiallyRecyclable.root != null && EventSystem.current.currentSelectedGameObject != potentiallyRecyclable.root.gameObject && this._Params.data != null && indexOfItemThatWillBecomeVisible < this._Params.data.Count && potentiallyRecyclable.CanPresentModelType(this._Params.data[indexOfItemThatWillBecomeVisible].CachedType);
		}

		bool ShowDetailedInfo.ISizeChangesHandler.HandleSizeChangeRequest(RectTransform rt, float newRequestedSize)
		{
			BaseVH itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(rt);
			if (itemViewsHolderIfVisible != null)
			{
				base.RequestChangeItemSizeAndUpdateLayout(itemViewsHolderIfVisible, newRequestedSize, false, true);
				return true;
			}
			return false;
		}

		public void OnExpandedStateChanged(RectTransform rt, bool expanded)
		{
			BaseVH itemViewsHolderIfVisible = base.GetItemViewsHolderIfVisible(rt);
			if (itemViewsHolderIfVisible != null)
			{
				ExpandableInventoryItemModel expandableInventoryItemModel = this._Params.data[itemViewsHolderIfVisible.ItemIndex] as ExpandableInventoryItemModel;
				if (expandableInventoryItemModel == null)
				{
					throw new UnityException(string.Concat(new object[]
					{
						"MultiplePrefabsExample.MyScrollRectAdapter.OnExpandedStateChanged: item model at index ",
						itemViewsHolderIfVisible.ItemIndex,
						" is not of type ",
						typeof(ExpandableInventoryItemModel).Name,
						", as expected by the views holder having this itemIndex. Happy debugging :)"
					}));
				}
				expandableInventoryItemModel.expanded = expanded;
			}
		}

		private void OnGroupExpandRequested(HeaderGroupModel header, bool expand, int itemIndex)
		{
			if (header.Opened != expand)
			{
				header.Opened = expand;
				if (!this.GroupsFoldMap.ContainsKey(this._currentStorage))
				{
					this.GroupsFoldMap.Add(this._currentStorage, new Dictionary<GroupCategoryType, bool>());
				}
				if (!this.GroupsFoldMap[this._currentStorage].ContainsKey(header.GCT))
				{
					this.GroupsFoldMap[this._currentStorage].Add(header.GCT, expand);
				}
				else
				{
					this.GroupsFoldMap[this._currentStorage][header.GCT] = expand;
				}
				if (header.Opened)
				{
					List<BaseModel> list = new List<BaseModel>();
					for (int i = 0; i < header.Items.Length; i++)
					{
						list.Add(new ExpandableInventoryItemModel
						{
							item = header.Items[i],
							ActiveRod = this._Params.Rods.ActiveRod,
							Storage = this._Params.DropMeStorage,
							Places = this._Params.places,
							TypeObjectOfDragItem = this._Params.TypeObjectOfDragItem
						});
					}
					this._Params.data.InsertRange(itemIndex, list);
					this.InsertItems(itemIndex, list.Count, false, false);
				}
				else
				{
					this._Params.data.RemoveRange(itemIndex, header.Items.Length);
					this.RemoveItems(itemIndex, header.Items.Length, false, false);
				}
				base.StartCoroutine(this.SetElementSelected(Mathf.Max(0, itemIndex - 1), 0.1f));
			}
		}

		private IEnumerator SetElementSelected(int itemIndex, float time = 0.1f)
		{
			yield return new WaitForSeconds(time);
			this.UiNavigation.SetPaused(false);
			BaseVH vh = base.GetItemViewsHolderIfVisible(itemIndex);
			if (vh != null)
			{
				EventSystem.current.SetSelectedGameObject(vh.root.gameObject);
			}
			else
			{
				this.UiNavigation.SetFirstActive();
			}
			this.UiNavigation.UpdateFromEventSystemSelected();
			yield break;
		}

		private void ShowAndReset()
		{
			this.ContentFader.OnHide.RemoveListener(new UnityAction(this.ShowAndReset));
			this.ResetItems(this._Params.data.Count, false, false);
			this._lastDisplayedStorage = this._currentStorage;
			this.ContentFader.ShowPanel();
			this.blinking = false;
			if (this._Params.data.Count <= this._lastIndex)
			{
				this._lastIndex = Mathf.Max(this._Params.data.Count - 1, 0);
			}
			if (!StaticUserData.IS_IN_TUTORIAL)
			{
				this.ScrollTo(this._lastIndex, 0.5f, 0.5f);
				if (base.gameObject.activeInHierarchy)
				{
					base.StartCoroutine(this.SetElementSelected(this._lastIndex, 0.1f));
				}
			}
		}

		private void ShowImmediate()
		{
			this.ContentFader.OnHide.RemoveListener(new UnityAction(this.ShowAndReset));
			base.StopAllCoroutines();
			this.ResetItems(this._Params.data.Count, false, false);
			this._lastDisplayedStorage = this._currentStorage;
			this.ContentFader.FastShowPanel();
			this.blinking = false;
			if (this._Params.data.Count <= this._lastIndex)
			{
				this._lastIndex = Mathf.Max(this._Params.data.Count - 1, 0);
			}
			if (!StaticUserData.IS_IN_TUTORIAL)
			{
				this.ScrollTo(this._lastIndex, 0.5f, 0.5f);
				if (base.gameObject.activeInHierarchy)
				{
					base.StartCoroutine(this.SetElementSelected(this._lastIndex, 0.1f));
				}
			}
		}

		private void Blink()
		{
			if (!this.blinking)
			{
				this.blinking = true;
				this.ContentFader.OnHide.AddListener(new UnityAction(this.ShowAndReset));
				this.ContentFader.HidePanel();
			}
			else
			{
				this.ShowImmediate();
			}
		}

		public void BuildModels()
		{
			base.StopAllCoroutines();
			List<BaseModel> list = new List<BaseModel>();
			for (int i = 0; i < this._Params.supportedGroups.Length; i++)
			{
				GroupCategoryType type = this._Params.supportedGroups[i].type;
				ItemSubTypes[] supportedSubTypes = InventorySRIA._groupItems[type].ToArray<ItemSubTypes>();
				if (PhotonConnectionFactory.Instance == null || PhotonConnectionFactory.Instance.Profile == null)
				{
					break;
				}
				List<InventoryItem> list2;
				if (this._Params.isExceedInventory)
				{
					list2 = PhotonConnectionFactory.Instance.Profile.Inventory.StorageExceededInventory.Where(delegate(InventoryItem x)
					{
						bool? isHidden = x.IsHidden;
						bool? flag2 = ((isHidden == null) ? null : new bool?(!isHidden.Value));
						return (flag2 == null) ? (x.Storage == this._Params.places && supportedSubTypes.Contains(x.ItemSubType)) : flag2.Value;
					}).ToList<InventoryItem>();
				}
				else if (this._Params.places == StoragePlaces.Storage)
				{
					list2 = PhotonConnectionFactory.Instance.Profile.Inventory.StorageInventory.Where(delegate(InventoryItem x)
					{
						bool? isHidden2 = x.IsHidden;
						bool? flag3 = ((isHidden2 == null) ? null : new bool?(!isHidden2.Value));
						return (flag3 == null) ? (x.Storage == this._Params.places && supportedSubTypes.Contains(x.ItemSubType)) : flag3.Value;
					}).ToList<InventoryItem>();
				}
				else
				{
					list2 = PhotonConnectionFactory.Instance.Profile.Inventory.Where(delegate(InventoryItem x)
					{
						bool? isHidden3 = x.IsHidden;
						bool? flag4 = ((isHidden3 == null) ? null : new bool?(!isHidden3.Value));
						return (flag4 == null) ? (x.Storage == this._Params.places && supportedSubTypes.Contains(x.ItemSubType)) : flag4.Value;
					}).ToList<InventoryItem>();
				}
				list2.Sort(delegate(InventoryItem a, InventoryItem b)
				{
					int num = a.ItemType.CompareTo(b.ItemType);
					if (num == 0)
					{
						num = a.ItemSubType.CompareTo(b.ItemSubType);
						if (num == 0)
						{
							num = string.Compare(a.Name, b.Name, StringComparison.InvariantCulture);
						}
					}
					return num;
				});
				bool flag = !this.GroupsFoldMap.ContainsKey(this._currentStorage) || !this.GroupsFoldMap[this._currentStorage].ContainsKey(this._Params.supportedGroups[i].type) || this.GroupsFoldMap[this._currentStorage][this._Params.supportedGroups[i].type];
				BaseModel baseModel = new HeaderGroupModel
				{
					GroupName = ScriptLocalization.Get(this._Params.supportedGroups[i].localizationTerm).ToUpper(),
					GroupIcon = this._Params.supportedGroups[i].icon,
					GCT = this._Params.supportedGroups[i].type,
					Opened = flag,
					Items = list2.ToArray()
				};
				if (list2.Count > 0)
				{
					list.Add(baseModel);
				}
				if ((baseModel as HeaderGroupModel).Opened)
				{
					for (int j = 0; j < list2.Count; j++)
					{
						list.Add(new ExpandableInventoryItemModel
						{
							item = list2[j],
							ActiveRod = this._Params.Rods.ActiveRod,
							Storage = this._Params.DropMeStorage,
							Places = this._Params.places,
							TypeObjectOfDragItem = this._Params.TypeObjectOfDragItem
						});
					}
				}
			}
			this._Params.data.Clear();
			this._Params.data.AddRange(list);
			if (!base.Initialized)
			{
				this.needRebuild = true;
				return;
			}
			this.Blink();
			if (this.needUpdateGST)
			{
				this.needUpdateGST = false;
				this.SendGameScreenChange();
			}
		}

		protected override void CollectItemsSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving, ItemsDescriptor itemsDesc)
		{
			base.CollectItemsSizes(changeMode, count, indexIfInsertingOrRemoving, itemsDesc);
			if (changeMode == ItemCountChangeMode.REMOVE || count == 0)
			{
				return;
			}
			int num;
			if (changeMode == ItemCountChangeMode.RESET)
			{
				num = 0;
			}
			else
			{
				num = indexIfInsertingOrRemoving;
			}
			int num2 = num + count;
			itemsDesc.BeginChangingItemsSizes(num);
			for (int i = num; i < num2; i++)
			{
				Type cachedType = this._Params.data[i].CachedType;
				if (cachedType == typeof(HeaderGroupModel))
				{
					itemsDesc[i] = 40f;
				}
				else if (cachedType == typeof(ExpandableInventoryItemModel))
				{
					ExpandableInventoryItemModel expandableInventoryItemModel = this._Params.data[i] as ExpandableInventoryItemModel;
					float num3 = 80f;
					if (expandableInventoryItemModel != null)
					{
						num3 = ((!expandableInventoryItemModel.expanded) ? expandableInventoryItemModel.nonExpandedSize : expandableInventoryItemModel.ExpandedSize);
					}
					itemsDesc[i] = num3;
				}
			}
			itemsDesc.EndChangingItemsSizes();
		}

		private List<GroupCategoryIcon> AccumulatedSupportedGroups = new List<GroupCategoryIcon>();

		private readonly List<GroupCategoryIcon> StandartSupportedGroups = new List<GroupCategoryIcon>
		{
			new GroupCategoryIcon
			{
				type = GroupCategoryType.All,
				icon = "\ue65f",
				localizationTerm = "ChatAllCaption",
				hintElementId = "Inv_F_All"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.RodAndReels,
				icon = "\ue653",
				localizationTerm = "RodsAndReelsUpperCaption",
				hintElementId = "Inv_F_RodsReels"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.Lines,
				icon = "\ue638",
				localizationTerm = "LinesUpperCaption",
				hintElementId = "Inv_F_Lines"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.TerminalTacles,
				icon = "\ue73c",
				localizationTerm = "TerminalTacklUpperCaption",
				hintElementId = "Inv_F_TTackles"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.Baits,
				icon = "\ue668",
				localizationTerm = "BaitsCaption",
				hintElementId = "Inv_F_Baits"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.JigWithBaits,
				icon = "\ue664",
				localizationTerm = "JigHeadsAndJigBaitsCaption",
				hintElementId = "Inv_F_Jigs"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.HardBaits,
				icon = "\ue666",
				localizationTerm = "HardBaitsCaption",
				hintElementId = "Inv_F_Hardbait"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.BassJigs,
				icon = "\ue665",
				localizationTerm = "BassJigCaption",
				hintElementId = "Inv_F_BassJigs"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.SpoonAndSpinners,
				icon = "\ue667",
				localizationTerm = "SpoonAndSpinnerCaption",
				hintElementId = "Inv_F_SpoonSpinners"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.ChumsAll,
				icon = "\ue69e",
				localizationTerm = "ChumCaption",
				hintElementId = "Inv_F_Chums"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.OutfitAndTools,
				icon = "\ue60a",
				localizationTerm = "OutfitsAndToolsUpperCaption",
				hintElementId = "Inv_F_OutfitTools"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.Misc,
				icon = "\ue63b",
				localizationTerm = "MiscUpperCaption",
				hintElementId = "Inv_F_Misc"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.UnderwaterItem,
				icon = "\ue776",
				localizationTerm = "UnderwaterItemsCaption",
				hintElementId = "Inv_F_UnderwaterItems"
			}
		};

		private static readonly List<GroupCategoryIcon> ChumMixingGroups = new List<GroupCategoryIcon>
		{
			new GroupCategoryIcon
			{
				type = GroupCategoryType.ChumsAll,
				icon = "\ue69e",
				localizationTerm = "ChumCaption",
				hintElementId = "Inv_F_Chums"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.Chums,
				icon = "\ue69f",
				localizationTerm = "ChumMixingResult",
				hintElementId = "Inv_F_ChumMixes"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.ChumBases,
				icon = "\ue72d",
				localizationTerm = "ChumBasesCaption",
				hintElementId = "Inv_F_ChumBases"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.ChumParticles,
				icon = "\ue71e",
				localizationTerm = "ChumParticlesCaption",
				hintElementId = "Inv_F_ChumParticles"
			},
			new GroupCategoryIcon
			{
				type = GroupCategoryType.ChumAromas,
				icon = "\ue71d",
				localizationTerm = "ChumAromasCaption",
				hintElementId = "Inv_F_ChumAromas"
			}
		};

		private static Dictionary<GroupCategoryType, HashSet<ItemSubTypes>> _groupItems = new Dictionary<GroupCategoryType, HashSet<ItemSubTypes>>
		{
			{
				GroupCategoryType.RodAndReels,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.Rod,
					ItemSubTypes.Reel,
					ItemSubTypes.TelescopicRod,
					ItemSubTypes.MatchRod,
					ItemSubTypes.SpinningRod,
					ItemSubTypes.CastingRod,
					ItemSubTypes.FeederRod,
					ItemSubTypes.BottomRod,
					ItemSubTypes.CarpRod,
					ItemSubTypes.SpodRod,
					ItemSubTypes.FlyRod,
					ItemSubTypes.SpinReel,
					ItemSubTypes.LineRunningReel,
					ItemSubTypes.CastReel,
					ItemSubTypes.FlyReel
				}
			},
			{
				GroupCategoryType.TerminalTacles,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.TerminalTackle,
					ItemSubTypes.Hook,
					ItemSubTypes.CarpHook,
					ItemSubTypes.Bobber,
					ItemSubTypes.Waggler,
					ItemSubTypes.Slider,
					ItemSubTypes.Sinker,
					ItemSubTypes.CageFeeder,
					ItemSubTypes.FlatFeeder,
					ItemSubTypes.PvaFeeder,
					ItemSubTypes.SpodFeeder,
					ItemSubTypes.CommonBell,
					ItemSubTypes.Bell,
					ItemSubTypes.ElectronicBell,
					ItemSubTypes.SimpleHook,
					ItemSubTypes.LongHook,
					ItemSubTypes.BarblessHook,
					ItemSubTypes.SpinningSinker,
					ItemSubTypes.DropSinker,
					ItemSubTypes.OffsetHook,
					ItemSubTypes.CarolinaRig,
					ItemSubTypes.TexasRig,
					ItemSubTypes.ThreewayRig
				}
			},
			{
				GroupCategoryType.Lines,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.Line,
					ItemSubTypes.MonoLine,
					ItemSubTypes.BraidLine,
					ItemSubTypes.FlurLine,
					ItemSubTypes.Leader,
					ItemSubTypes.BraidLeader,
					ItemSubTypes.MonoLeader,
					ItemSubTypes.SteelLeader,
					ItemSubTypes.CarpLeader,
					ItemSubTypes.FlurLeader,
					ItemSubTypes.TitaniumLeader
				}
			},
			{
				GroupCategoryType.OutfitAndTools,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.Outfit,
					ItemSubTypes.Tool,
					ItemSubTypes.Misc,
					ItemSubTypes.RodCase,
					ItemSubTypes.LuresBox,
					ItemSubTypes.Waistcoat,
					ItemSubTypes.Boots,
					ItemSubTypes.Hat,
					ItemSubTypes.Clothing,
					ItemSubTypes.Belt,
					ItemSubTypes.RescueVest,
					ItemSubTypes.Glasses,
					ItemSubTypes.Gloves,
					ItemSubTypes.Talisman,
					ItemSubTypes.SounderBattery,
					ItemSubTypes.Keepnet,
					ItemSubTypes.Stringer,
					ItemSubTypes.FishNet,
					ItemSubTypes.RodStand,
					ItemSubTypes.EchoSounder,
					ItemSubTypes.Tent,
					ItemSubTypes.Chair,
					ItemSubTypes.Umbrella,
					ItemSubTypes.Cooker,
					ItemSubTypes.Radio,
					ItemSubTypes.Repellent,
					ItemSubTypes.Food,
					ItemSubTypes.Firework
				}
			},
			{
				GroupCategoryType.JigWithBaits,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.JigBait,
					ItemSubTypes.JigHead,
					ItemSubTypes.CommonJigHeads,
					ItemSubTypes.BarblessJigHeads,
					ItemSubTypes.Worm,
					ItemSubTypes.Grub,
					ItemSubTypes.Shad,
					ItemSubTypes.Tube,
					ItemSubTypes.Craw,
					ItemSubTypes.Slug
				}
			},
			{
				GroupCategoryType.HardBaits,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.Cranckbait,
					ItemSubTypes.Jerkbait,
					ItemSubTypes.Minnow,
					ItemSubTypes.Popper,
					ItemSubTypes.Frog,
					ItemSubTypes.Walker,
					ItemSubTypes.Swimbait
				}
			},
			{
				GroupCategoryType.BassJigs,
				new HashSet<ItemSubTypes> { ItemSubTypes.BassJig }
			},
			{
				GroupCategoryType.SpoonAndSpinners,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.Spoon,
					ItemSubTypes.Spinner,
					ItemSubTypes.Spinnerbait,
					ItemSubTypes.BuzzBait,
					ItemSubTypes.BarblessSpinners,
					ItemSubTypes.BarblessSpoons,
					ItemSubTypes.Tail
				}
			},
			{
				GroupCategoryType.Baits,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.Bait,
					ItemSubTypes.CommonBait,
					ItemSubTypes.BoilBait,
					ItemSubTypes.InsectsWormBait,
					ItemSubTypes.FreshBait
				}
			},
			{
				GroupCategoryType.ChumsAll,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.Chum,
					ItemSubTypes.ChumGroundbaits,
					ItemSubTypes.ChumCarpbaits,
					ItemSubTypes.ChumMethodMix,
					ItemSubTypes.ChumAroma,
					ItemSubTypes.ChumParticle,
					ItemSubTypes.Attractant
				}
			},
			{
				GroupCategoryType.Misc,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.Kayak,
					ItemSubTypes.Zodiak,
					ItemSubTypes.RubberDinghy,
					ItemSubTypes.FlatBottomedBoat,
					ItemSubTypes.MotorBoat,
					ItemSubTypes.BassBoat,
					ItemSubTypes.DeckBoat,
					ItemSubTypes.FishingYacht
				}
			},
			{
				GroupCategoryType.UnderwaterItem,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.UnderwaterItem,
					ItemSubTypes.Weed,
					ItemSubTypes.ShoreWeed,
					ItemSubTypes.Sticks,
					ItemSubTypes.Cork,
					ItemSubTypes.Shell,
					ItemSubTypes.Creature,
					ItemSubTypes.Loot,
					ItemSubTypes.Trash
				}
			},
			{
				GroupCategoryType.ChumBases,
				new HashSet<ItemSubTypes>
				{
					ItemSubTypes.ChumGroundbaits,
					ItemSubTypes.ChumCarpbaits,
					ItemSubTypes.ChumMethodMix
				}
			},
			{
				GroupCategoryType.ChumParticles,
				new HashSet<ItemSubTypes> { ItemSubTypes.ChumParticle }
			},
			{
				GroupCategoryType.ChumAromas,
				new HashSet<ItemSubTypes> { ItemSubTypes.ChumAroma }
			},
			{
				GroupCategoryType.Chums,
				new HashSet<ItemSubTypes> { ItemSubTypes.Chum }
			}
		};

		public DropMeStorage Equipment;

		public DropMeStorage Storage;

		public Selectable ExpandButton;

		public GameObject ExpandContent;

		public GameObject EquipmentContent;

		public GameObject ExcessContent;

		public AlphaFade ContentFader;

		public GameObject FilterSelector;

		public UINavigation UiNavigation;

		public UnityEvent OnInventoryTabSwitched;

		private readonly Dictionary<StorageTypes, Dictionary<GroupCategoryType, bool>> GroupsFoldMap = new Dictionary<StorageTypes, Dictionary<GroupCategoryType, bool>>();

		private bool needUpdateGST = true;

		private bool needRebuild;

		private StorageTypes _currentStorage;

		private StorageTypes _lastDisplayedStorage;

		private CanvasGroup _cg;

		private List<GroupCategorySetter> filters = new List<GroupCategorySetter>();

		[SerializeField]
		private ToggleGroup FilterSelectorToggleGroup;

		private bool subscribed;

		private int _lastIndex;

		private bool blinking;
	}
}
