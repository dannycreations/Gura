using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Common.Managers;
using Assets.Scripts.Common.Managers.Helpers;
using I2.Loc;
using InventorySRIA;
using ObjectModel;
using UnityEngine;

public class ManagedHint
{
	public ManagedHint(HintMessage message, int id, HintSystem system)
	{
		this.Message = message;
		this.system = system;
		this.inhintId = id;
		this.parentId = id;
		Debug.Log(string.Format("Created Managed Hint: {0}, inhintID: {1}, parentId: {2}", message.MessageId, this.inhintId, this.parentId));
		this.managedObjects = new Dictionary<int, ManagedHintObject>();
		this.registeredPrefabs = new Dictionary<int, PrefabContainer>();
		this.registeredElementIds = new Dictionary<int, string>();
		this.registeredItemIds = new Dictionary<int, int>();
		this.registeredInstanceIds = new Dictionary<int, Guid>();
		this.registeredCategories = new Dictionary<int, string>();
		this.registeredCategoryHighlights = new Dictionary<int, List<int>>();
		this.spawnedCategoryHintObjects = new Dictionary<Transform, List<int>>();
		this.registeredTextStrings = new Dictionary<int, string>();
		this.registeredSides = new Dictionary<int, HintSide>();
		this.registeredCategoryTransforms = new Dictionary<int, Transform>();
		this.inhintIdsToSpawn = new List<int>();
		this.inhintIdsToIgnoreCanShow = new List<int>();
		this.registeredLineInstanceIds = new List<Guid>();
		this.registeredFilterFunctions = new Dictionary<int, ManagedHint.FilterFunction>();
		this.registeredShowFunctions = new Dictionary<int, ManagedHint.FilterFunction>();
	}

	public HintMessage Message { get; set; }

	private int GenerateInhintId()
	{
		return this.inhintId++;
	}

	private void SpawnNeededHintObjects()
	{
		float num = Time.time;
		for (int i = 0; i < this.inhintIdsToSpawn.Count; i++)
		{
			int idToSpawn = this.inhintIdsToSpawn[i];
			PrefabContainer prefabContainer;
			if (this.registeredPrefabs.TryGetValue(idToSpawn, out prefabContainer))
			{
				string empty = string.Empty;
				string empty2 = string.Empty;
				string empty3 = string.Empty;
				int num2 = -1;
				Guid instanceId = Guid.Empty;
				ManagedHint.FilterFunction filterFunction = null;
				Transform parentTransform = null;
				string empty4 = string.Empty;
				HintSide side = HintSide.Undefined;
				this.registeredSides.TryGetValue(idToSpawn, out side);
				this.registeredFilterFunctions.TryGetValue(idToSpawn, out filterFunction);
				ManagedHint.FilterFunction filterFunction2;
				this.registeredShowFunctions.TryGetValue(idToSpawn, out filterFunction2);
				ManagedHintSidePair managedHintSidePair = prefabContainer.Data.FirstOrDefault((ManagedHintSidePair x) => x.Side == side) ?? prefabContainer.Data[0];
				bool flag = this.registeredElementIds.TryGetValue(idToSpawn, out empty);
				bool flag2 = this.registeredItemIds.TryGetValue(idToSpawn, out num2);
				bool flag3 = this.registeredInstanceIds.TryGetValue(idToSpawn, out instanceId);
				bool flag4 = this.registeredCategories.TryGetValue(idToSpawn, out empty2);
				bool flag5 = this.registeredCategoryTransforms.TryGetValue(idToSpawn, out parentTransform);
				if (flag && HintSystem.ElementIds.ContainsKey(empty) && HintSystem.ElementIds[empty] != null)
				{
					HintElementId hintElementId = HintSystem.ElementIds[empty];
					if (prefabContainer == this.system.RectOutlineHint || prefabContainer == this.system.RectUnderlineHint)
					{
						if (hintElementId.IsPondPin)
						{
							prefabContainer = this.system.PinOutlineHint;
						}
						else if (hintElementId.IsLocationPin)
						{
							prefabContainer = this.system.LocationOutlineHint;
						}
						else if (hintElementId.IsCirclePin)
						{
							prefabContainer = this.system.CircleOutlineHint;
						}
						else if (hintElementId.IsColorHighlightedTab)
						{
							prefabContainer = this.system.RectUnderlineTextHighlightHint;
						}
					}
					if (side == HintSide.Undefined)
					{
						side = hintElementId.PreferredSide;
						managedHintSidePair = prefabContainer.Data.FirstOrDefault((ManagedHintSidePair x) => x.Side == side) ?? prefabContainer.Data[0];
					}
					ManagedHintObject managedHintObject = Object.Instantiate<ManagedHintObject>(managedHintSidePair.Hint, hintElementId.CachedTransform);
					managedHintObject.SetObserver(this, idToSpawn);
					TooltipHint tooltipHint = managedHintObject as TooltipHint;
					DollHighlightHint dollHighlightHint = managedHintObject as DollHighlightHint;
					if (tooltipHint != null)
					{
						if (this.registeredTextStrings.TryGetValue(idToSpawn, out empty4))
						{
							tooltipHint.Init(empty4, side);
						}
						else
						{
							tooltipHint.Init(this.Message.TooltipFormatted, side);
						}
					}
					if (dollHighlightHint != null)
					{
						if (!string.IsNullOrEmpty(this.Message.Code) && (this.Message.Code == "$MoveItemTypeToEquipment" || this.Message.Code == "$MoveItemTypeToDoll" || this.Message.Code == "$MoveItemToDoll" || this.Message.Code == "$MoveItemToEquipment"))
						{
							dollHighlightHint.Init(ItemSubTypes.All);
						}
						else
						{
							dollHighlightHint.Init(this.Message.ItemSubType);
						}
					}
					managedHintObject.SetFilterFunction(filterFunction, filterFunction2);
					this.managedObjects.Add(idToSpawn, managedHintObject);
					this.inhintIdsToSpawn.RemoveAt(i);
					if (Time.time - num > Time.fixedDeltaTime)
					{
						return;
					}
				}
				if (flag2 && HintSystem.ItemIds.ContainsKey(num2) && HintSystem.ItemIds[num2] != null && HintSystem.ItemIds[num2].Count > 0)
				{
					HintSystem.TransformItemPair transformItemPair = HintSystem.ItemIds[num2].FirstOrDefault((HintSystem.TransformItemPair x) => (this.Message.Length == 0f || (x.item.Length != null && x.item.Length >= (double)this.Message.Length)) && ((this.Message.DisplayStorage == (StoragePlaces)0 && x.item.Storage != StoragePlaces.Doll && x.item.Storage != StoragePlaces.ParentItem && x.item.Storage != StoragePlaces.Shore && x.item.Storage != StoragePlaces.Hands) || x.item.Storage == this.Message.DisplayStorage) && (this.Message.MinThickness == 0f || (x.item is Line && (x.item as Line).Thickness >= this.Message.MinThickness)) && (this.Message.MaxThickness == 0f || (x.item is Line && (x.item as Line).Thickness <= this.Message.MaxThickness)) && (this.Message.MinLoad == 0f || (x.item is Rod && (x.item as Rod).MaxLoad >= this.Message.MinLoad) || (x.item is Reel && (x.item as Reel).MaxLoad >= this.Message.MinLoad) || (x.item is Line && (x.item as Line).MaxLoad >= this.Message.MinLoad)) && (this.Message.MaxLoad == 0f || (x.item is Rod && (x.item as Rod).MaxLoad <= this.Message.MaxLoad) || (x.item is Reel && (x.item as Reel).MaxLoad <= this.Message.MaxLoad) || (x.item is Line && (x.item as Line).MaxLoad <= this.Message.MaxLoad)));
					if (transformItemPair != null)
					{
						this.CheckForLine(transformItemPair.item);
						ManagedHintObject managedHintObject2 = Object.Instantiate<ManagedHintObject>(managedHintSidePair.Hint, transformItemPair.transform);
						managedHintObject2.SetObserver(this, idToSpawn);
						managedHintObject2.SetFilterFunction(filterFunction, filterFunction2);
						this.managedObjects.Add(idToSpawn, managedHintObject2);
						this.inhintIdsToSpawn.RemoveAt(i);
						if (Time.time - num > Time.fixedDeltaTime)
						{
							return;
						}
					}
				}
				if (flag3)
				{
					List<List<HintSystem.TransformItemPair>> list = (from x in HintSystem.ItemIds
						where x.Value.Any((HintSystem.TransformItemPair y) => y.item.InstanceId != null && y.item.InstanceId == instanceId)
						select x.Value).ToList<List<HintSystem.TransformItemPair>>();
					if (list.Count > 0)
					{
						HintSystem.TransformItemPair transformItemPair2 = list[0][0];
						if (transformItemPair2 != null)
						{
							this.CheckForLine(transformItemPair2.item);
							ManagedHintObject managedHintObject3 = Object.Instantiate<ManagedHintObject>(managedHintSidePair.Hint, transformItemPair2.transform);
							managedHintObject3.SetObserver(this, idToSpawn);
							managedHintObject3.SetFilterFunction(filterFunction, filterFunction2);
							this.managedObjects.Add(idToSpawn, managedHintObject3);
							this.inhintIdsToSpawn.RemoveAt(i);
							if (Time.time - num > Time.fixedDeltaTime)
							{
								return;
							}
						}
					}
				}
				if (flag4 && HintSystem.Categories.ContainsKey(empty2))
				{
					if (this.waitForLocalShopCache || this.waitForGlobalShopCache)
					{
						List<InventoryItem> list2 = new List<InventoryItem>();
						if (this.waitForLocalShopCache)
						{
							list2 = CacheLibrary.LocalCacheInstance.GetItemsFromCategoryImmediate(new List<int> { (int)this.Message.ItemSubType });
						}
						else
						{
							list2 = CacheLibrary.GlobalShopCacheInstance.GetItemsFromCategoryImmediate(new List<int> { (int)this.Message.ItemSubType });
						}
						if (list2.Count > 0)
						{
							this.SpawnFilteredShopCategoriesHighlights(list2, filterFunction);
							this.waitForGlobalShopCache = false;
							this.waitForLocalShopCache = false;
							this.inhintIdsToSpawn.RemoveAt(i);
							return;
						}
					}
					int categoryEntryInhintID = idToSpawn;
					HintSystem.TransformItemPair[] array = HintSystem.Categories[empty2].Where((HintSystem.TransformItemPair x) => x.item == null || (x.item != null && x.item.InstanceId != null && (this.Message.Length == 0f || (x.item is Line && (x.item as Line).Length >= (double)this.Message.Length)) && (this.Message.MinThickness == 0f || (x.item is Line && (x.item as Line).Thickness >= this.Message.MinThickness)) && (this.Message.MaxThickness == 0f || (x.item is Line && (x.item as Line).Thickness <= this.Message.MaxThickness)) && (this.Message.MinLoad == 0f || (x.item is Rod && (x.item as Rod).MaxLoad >= this.Message.MinLoad) || (x.item is Reel && (x.item as Reel).MaxLoad >= this.Message.MinLoad) || (x.item is Line && (x.item as Line).MaxLoad >= this.Message.MinLoad)) && (this.Message.MaxLoad == 0f || (x.item is Rod && (x.item as Rod).MaxLoad <= this.Message.MaxLoad) || (x.item is Reel && (x.item as Reel).MaxLoad <= this.Message.MaxLoad) || (x.item is Line && (x.item as Line).MaxLoad <= this.Message.MaxLoad)))).ToArray<HintSystem.TransformItemPair>();
					if (array.Length > 0)
					{
						List<HintSystem.TransformItemPair> listFreeList = new List<HintSystem.TransformItemPair>(array);
						List<HintSystem.TransformItemPair> resultList = new List<HintSystem.TransformItemPair>();
						List<int> uniqueItemIds = new List<int>();
						for (int j = 0; j < array.Length; j++)
						{
							if (array[j].item != null && !uniqueItemIds.Contains(array[j].item.ItemId))
							{
								uniqueItemIds.Add(array[j].item.ItemId);
							}
						}
						if (uniqueItemIds.Count > 0)
						{
							int index;
							for (index = 0; index < uniqueItemIds.Count; index++)
							{
								List<HintSystem.TransformItemPair> list3 = listFreeList.Where((HintSystem.TransformItemPair x) => x.item != null && x.item.ItemId == uniqueItemIds[index]).ToList<HintSystem.TransformItemPair>();
								HintSystem.TransformItemPair transformItemPair3 = null;
								double num3 = 0.0;
								for (int k = 0; k < list3.Count; k++)
								{
									HintSystem.TransformItemPair transformItemPair4 = list3[k];
									if (transformItemPair4.item.Length == null)
									{
										transformItemPair3 = transformItemPair4;
										break;
									}
									if (transformItemPair4.item.Length.Value > num3)
									{
										num3 = transformItemPair4.item.Length.Value;
										transformItemPair3 = transformItemPair4;
									}
								}
								if (transformItemPair3 != null)
								{
									resultList.Add(transformItemPair3);
								}
							}
							List<KeyValuePair<Transform, List<int>>> list4 = this.spawnedCategoryHintObjects.Where((KeyValuePair<Transform, List<int>> x) => listFreeList.Any((HintSystem.TransformItemPair y) => y.transform == x.Key) && x.Value.Contains(categoryEntryInhintID) && !resultList.Any((HintSystem.TransformItemPair z) => z.transform == x.Key)).ToList<KeyValuePair<Transform, List<int>>>();
							using (List<KeyValuePair<Transform, List<int>>>.Enumerator enumerator = list4.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									KeyValuePair<Transform, List<int>> entry = enumerator.Current;
									IEnumerable<KeyValuePair<int, Transform>> enumerable = this.registeredCategoryTransforms.Where((KeyValuePair<int, Transform> x) => x.Value == entry.Key);
									if (enumerable.Any<KeyValuePair<int, Transform>>())
									{
										int key = enumerable.First<KeyValuePair<int, Transform>>().Key;
										if (this.managedObjects.ContainsKey(key) && this.managedObjects[key] != null)
										{
											Object.Destroy(this.managedObjects[key].gameObject);
										}
									}
								}
							}
							array = resultList.Where((HintSystem.TransformItemPair x) => !this.spawnedCategoryHintObjects.ContainsKey(x.transform) || !this.spawnedCategoryHintObjects[x.transform].Contains(categoryEntryInhintID)).ToArray<HintSystem.TransformItemPair>();
						}
						else
						{
							array = listFreeList.Where((HintSystem.TransformItemPair x) => !this.spawnedCategoryHintObjects.ContainsKey(x.transform) || !this.spawnedCategoryHintObjects[x.transform].Contains(categoryEntryInhintID)).ToArray<HintSystem.TransformItemPair>();
						}
					}
					foreach (HintSystem.TransformItemPair transformItemPair5 in array)
					{
						if (transformItemPair5 != null)
						{
							idToSpawn = this.GenerateInhintId();
							this.CheckForLine(transformItemPair5.item);
							this.registeredCategoryTransforms.Add(idToSpawn, transformItemPair5.transform);
							if (!this.spawnedCategoryHintObjects.ContainsKey(transformItemPair5.transform))
							{
								this.spawnedCategoryHintObjects.Add(transformItemPair5.transform, new List<int> { categoryEntryInhintID });
							}
							else if (!this.spawnedCategoryHintObjects[transformItemPair5.transform].Contains(categoryEntryInhintID))
							{
								this.spawnedCategoryHintObjects[transformItemPair5.transform].Add(categoryEntryInhintID);
							}
							this.registeredPrefabs.Add(idToSpawn, prefabContainer);
							ManagedHintObject managedHintObject4 = Object.Instantiate<ManagedHintObject>(managedHintSidePair.Hint, transformItemPair5.transform);
							managedHintObject4.SetObserver(this, idToSpawn);
							managedHintObject4.SetFilterFunction(filterFunction, filterFunction2);
							this.registeredFilterFunctions.Add(idToSpawn, filterFunction);
							this.managedObjects.Add(idToSpawn, managedHintObject4);
							if (!this.registeredCategoryHighlights.ContainsKey(categoryEntryInhintID))
							{
								this.registeredCategoryHighlights.Add(categoryEntryInhintID, new List<int> { idToSpawn });
							}
							else if (!this.registeredCategoryHighlights[categoryEntryInhintID].Contains(idToSpawn))
							{
								this.registeredCategoryHighlights[categoryEntryInhintID].Add(idToSpawn);
							}
						}
					}
					if (Time.time - num > Time.fixedDeltaTime)
					{
						return;
					}
				}
				if (flag5)
				{
					int[] array2 = (from x in this.registeredCategoryHighlights
						where x.Value.Contains(idToSpawn)
						select x into y
						select y.Key).ToArray<int>();
					if (array2.Length > 0)
					{
						int num4 = array2[0];
						string text = this.registeredCategories[num4];
						if (!HintSystem.Categories.ContainsKey(text) || !HintSystem.Categories[text].Any((HintSystem.TransformItemPair x) => x.transform == parentTransform))
						{
							if (this.spawnedCategoryHintObjects.ContainsKey(this.registeredCategoryTransforms[idToSpawn]))
							{
								this.spawnedCategoryHintObjects.Remove(this.registeredCategoryTransforms[idToSpawn]);
							}
							this.registeredCategoryTransforms.Remove(idToSpawn);
						}
						else
						{
							managedHintSidePair = prefabContainer.Data.FirstOrDefault((ManagedHintSidePair x) => x.Side == side) ?? prefabContainer.Data[0];
							ManagedHintObject managedHintObject5 = Object.Instantiate<ManagedHintObject>(managedHintSidePair.Hint, parentTransform);
							managedHintObject5.SetObserver(this, idToSpawn);
							managedHintObject5.SetFilterFunction(filterFunction, filterFunction2);
							this.managedObjects.Add(idToSpawn, managedHintObject5);
						}
						this.inhintIdsToSpawn.RemoveAt(i);
					}
					else
					{
						if (this.spawnedCategoryHintObjects.ContainsKey(this.registeredCategoryTransforms[idToSpawn]))
						{
							this.spawnedCategoryHintObjects.Remove(this.registeredCategoryTransforms[idToSpawn]);
						}
						this.inhintIdsToSpawn.RemoveAt(i);
					}
					if (Time.time - num > Time.fixedDeltaTime)
					{
						return;
					}
				}
				if (!flag && !flag4 && !flag2 && !flag5 && !flag3)
				{
					Debug.LogWarning("Spawning prefab without parent");
					ManagedHintObject managedHintObject6 = Object.Instantiate<ManagedHintObject>(managedHintSidePair.Hint);
					managedHintObject6.SetObserver(this, idToSpawn);
					managedHintObject6.SetFilterFunction(filterFunction, filterFunction2);
					this.managedObjects.Add(idToSpawn, managedHintObject6);
					this.inhintIdsToSpawn.RemoveAt(i);
				}
			}
		}
	}

	private void CheckForLine(InventoryItem item)
	{
		if (item == null)
		{
			return;
		}
		if (item.InstanceId == null)
		{
			Debug.LogWarning("THere's no InstanceId on the LINE: " + item.Name);
		}
		if (item is Line && item.InstanceId != null && !this.registeredLineInstanceIds.Contains(item.InstanceId.Value))
		{
			this.inhintIdsToIgnoreCanShow.Add(this.AddByElementId("LL_Plus", this.system.RectOutlineHint, () => CutLinesController.GetCurrentLineInstanceID() == item.InstanceId && CutLinesController.GetMaxLineLength() > CutLinesController.GetCurrentLineLength(), HintSide.Undefined, null));
			this.inhintIdsToIgnoreCanShow.Add(this.AddByElementId("LL_Minus", this.system.RectOutlineHint, () => CutLinesController.GetCurrentLineInstanceID() == item.InstanceId && CutLinesController.GetMaxLineLength() < CutLinesController.GetCurrentLineLength(), HintSide.Undefined, null));
			this.inhintIdsToIgnoreCanShow.Add(this.AddByElementId("LL_Apply", this.system.RectOutlineHint, () => CutLinesController.GetCurrentLineInstanceID() == item.InstanceId && Mathf.Abs(CutLinesController.GetMaxLineLength() - CutLinesController.GetCurrentLineLength()) < 0.01f, HintSide.Undefined, null));
			this.inhintIdsToIgnoreCanShow.Add(this.AddByElementId("LL_Apply", this.system.ArrowHint, () => CutLinesController.GetCurrentLineInstanceID() == item.InstanceId && Mathf.Abs(CutLinesController.GetMaxLineLength() - CutLinesController.GetCurrentLineLength()) < 0.01f, HintSide.Undefined, null));
			this.registeredLineInstanceIds.Add(item.InstanceId.Value);
		}
	}

	private ManagedHintObject AddPrefab(PrefabContainer prefab, Transform parent = null, ManagedHint.FilterFunction ff = null, HintSide side = HintSide.Undefined, ManagedHint.FilterFunction showFunc = null)
	{
		int num = this.GenerateInhintId();
		this.registeredPrefabs.Add(num, prefab);
		if (side == HintSide.Undefined)
		{
			side = this.Message.Side;
		}
		else
		{
			this.registeredSides.Add(num, side);
		}
		ManagedHintSidePair managedHintSidePair = prefab.Data.FirstOrDefault((ManagedHintSidePair x) => x.Side == side) ?? prefab.Data[0];
		ManagedHintObject managedHintObject = Object.Instantiate<ManagedHintObject>(managedHintSidePair.Hint, parent);
		if (showFunc != null)
		{
			this.registeredShowFunctions.Add(num, showFunc);
		}
		if (ff != null)
		{
			this.registeredFilterFunctions.Add(num, ff);
		}
		managedHintObject.SetFilterFunction(ff, showFunc);
		managedHintObject.SetObserver(this, num);
		this.managedObjects.Add(num, managedHintObject);
		return managedHintObject;
	}

	private int AddByElementId(string elementId, PrefabContainer prefab, ManagedHint.FilterFunction ff = null, HintSide side = HintSide.Undefined, ManagedHint.FilterFunction showFunc = null)
	{
		int num = this.Add(prefab, ff, showFunc, side);
		this.registeredElementIds.Add(num, elementId);
		if (ManagedHint.ElementIdsToIgnoreCanShow.Contains(elementId))
		{
			this.inhintIdsToIgnoreCanShow.Add(num);
		}
		return num;
	}

	private int AddByItemId(int itemId, PrefabContainer prefab, ManagedHint.FilterFunction ff = null, HintSide side = HintSide.Undefined, ManagedHint.FilterFunction showFunc = null)
	{
		int num = this.Add(prefab, ff, showFunc, side);
		this.registeredItemIds.Add(num, itemId);
		return num;
	}

	private int AddByInstanceId(Guid instanceId, PrefabContainer prefab, ManagedHint.FilterFunction ff = null, HintSide side = HintSide.Undefined, ManagedHint.FilterFunction showFunc = null)
	{
		int num = this.Add(prefab, ff, showFunc, side);
		this.registeredInstanceIds.Add(num, instanceId);
		return num;
	}

	private int AddByCategoryType(string category, PrefabContainer prefab, ManagedHint.FilterFunction ff = null, HintSide side = HintSide.Undefined, ManagedHint.FilterFunction showFunc = null)
	{
		int num = this.Add(prefab, ff, showFunc, side);
		this.registeredCategories.Add(num, category);
		return num;
	}

	private int AddTooltip(string elementId, string text = "", ManagedHint.FilterFunction ff = null, HintSide side = HintSide.Undefined, ManagedHint.FilterFunction showFunc = null)
	{
		int num = this.Add(this.system.TooltipHint, ff, showFunc, side);
		this.registeredElementIds.Add(num, elementId);
		if (!string.IsNullOrEmpty(text))
		{
			this.registeredTextStrings.Add(num, text);
		}
		return num;
	}

	private int Add(PrefabContainer prefab, ManagedHint.FilterFunction ff = null, ManagedHint.FilterFunction showFunc = null, HintSide side = HintSide.Undefined)
	{
		int num = this.GenerateInhintId();
		this.registeredPrefabs.Add(num, prefab);
		if (ff != null)
		{
			this.registeredFilterFunctions.Add(num, ff);
		}
		if (showFunc != null)
		{
			this.registeredShowFunctions.Add(num, showFunc);
		}
		if (side != HintSide.Undefined)
		{
			this.registeredSides.Add(num, side);
		}
		this.inhintIdsToSpawn.Add(num);
		return num;
	}

	private void AddTextHint(string titleFormatted, ManagedHint.FilterFunction ff = null)
	{
		TextHint textHint = this.AddPrefab(this.system.TextHint, this.system.TextHintParent.GetParent(), ff, HintSide.Undefined, null) as TextHint;
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		string text;
		if (TextHintParent.TryReplaceControlsInText(titleFormatted, out text, dictionary, false))
		{
			textHint.OriginalText = this.Message.TitleFormatted;
		}
		textHint.Text.text = text;
		textHint.Index = this.Message.OrderIndex;
		textHint.Info = dictionary;
		this.system.TextHintParent.AddTextHint(textHint);
	}

	public void Update()
	{
		MenuPrefabsList menuPrefabsList = MenuHelpers.Instance.MenuPrefabsList;
		if (menuPrefabsList != null && !ManagerScenes.InTransition && !TransferToLocation.IsMoving && !LoadPond.IsMoving && (menuPrefabsList.startFormAS == null || !menuPrefabsList.startFormAS.isActive) && !menuPrefabsList.loadingFormAS.isActive && (menuPrefabsList.travelingFormAS == null || !menuPrefabsList.travelingFormAS.isActive))
		{
			if (!this.inited)
			{
				this.Init();
			}
			else
			{
				this.SpawnNeededHintObjects();
			}
			this.time += Time.deltaTime;
		}
		this.ValidateHint();
	}

	public bool CanShow(int id)
	{
		if (ClientMissionsManager.Instance.CurrentTrackedMission == null || this.Message == null)
		{
			return false;
		}
		MenuPrefabsList menuPrefabsList = MenuHelpers.Instance.MenuPrefabsList;
		if (ClientMissionsManager.Instance.CurrentTrackedMission.MissionId == this.Message.MissionId && !StaticUserData.IS_IN_TUTORIAL && CacheLibrary.AllChachesInited && !menuPrefabsList.IsLoadingOrTransfer())
		{
			if (MessageFactory.InfoMessagesQueue.All((InfoMessage p) => p.MessageType != InfoMessageTypes.MissionAccomplished) && ((!InfoMessageController.Instance.IsActive && !(MessageBoxList.Instance.currentMessage != null) && !CatchedFishInfoHandler.IsPersonalRecordDisplayed()) || this.inhintIdsToIgnoreCanShow.Contains(id)) && (this.Message.ScreenType == GameScreenType.Undefined || this.Message.ScreenType == this.system.ScreenType) && (this.Message.ScreenTab == GameScreenTabType.Undefined || this.Message.ScreenTab == this.system.ScreenTab) && !KeysHandlerAction.HelpShown && this.time * 1000f >= (float)this.Message.ShowAfterMs && (GameFactory.Player == null || GameFactory.Player.State != typeof(PlayerPhotoMode)))
			{
				return ShowHudElements.Instance == null || ShowHudElements.Instance.CurrentState != ShowHudStates.HideAll;
			}
		}
		return false;
	}

	public bool GetManagedObjectDisplayed(int inhintId)
	{
		if (this.managedObjects.ContainsKey(inhintId) && this.managedObjects[inhintId] != null)
		{
			return this.managedObjects[inhintId].gameObject.activeInHierarchy && this.managedObjects[inhintId].Displayed;
		}
		if (this.registeredCategories.ContainsKey(inhintId) && this.registeredCategoryHighlights.ContainsKey(inhintId))
		{
			foreach (int num in this.registeredCategoryHighlights[inhintId])
			{
				if (this.GetManagedObjectDisplayed(num))
				{
					return true;
				}
			}
			return false;
		}
		return false;
	}

	private void ProcessItemId(PrefabContainer prefab)
	{
		if (this.Message.ItemClass == HintItemClass.License)
		{
			this.AddByElementId("Sh_Next", this.system.RectOutlineHint, () => ShopMainPageHandler.Instance.ContentUpdater.FindPagingById(this.Message.ItemId, UpdateContentItems.ItemsTypes.Licensees) > 0, HintSide.Undefined, null);
			this.AddByElementId("Sh_Prev", this.system.RectOutlineHint, () => ShopMainPageHandler.Instance.ContentUpdater.FindPagingById(this.Message.ItemId, UpdateContentItems.ItemsTypes.Licensees) < 0, HintSide.Undefined, null);
			if (this.Message.Count == 0)
			{
				this.AddByElementId("License" + this.Message.ItemId, prefab, null, this.Message.Side, null);
			}
			else
			{
				this.AddByElementId(string.Format("License{0}_{1}", this.Message.ItemId, this.Message.Count), prefab, null, HintSide.Undefined, null);
			}
		}
		else if (this.Message.ItemClass == HintItemClass.InventoryItem)
		{
			if (!string.IsNullOrEmpty(this.Message.InstanceId))
			{
				this.AddByInstanceId(new Guid(this.Message.InstanceId), prefab, null, this.Message.Side, null);
			}
			else
			{
				this.AddByItemId(this.Message.ItemId, prefab, null, this.Message.Side, null);
			}
		}
	}

	private void SpawnHintsForShopByItemType(bool local, bool hasItemId = false)
	{
		GameScreenType shop = ((!local) ? GameScreenType.GlobalShop : GameScreenType.LocalShop);
		this.AddByElementId("DASH_SHOP", this.system.RectUnderlineHint, () => this.system.ScreenType != shop, HintSide.Undefined, null);
		string text = ((!this.addAutoPrefix) ? ScriptLocalization.Get((!local) ? "GotoGlobalShopTooltip" : "GotoLocalShopTooltip") : ("auto_" + ScriptLocalization.Get((!local) ? "GotoGlobalShopTooltip" : "GotoLocalShopTooltip")));
		this.AddTooltip("DASH_SHOP", text, () => this.system.ScreenType != shop, HintSide.Undefined, null);
		if (this.Message.ItemSubType != ItemSubTypes.All)
		{
			Action onGetItemsFunc = delegate
			{
				UpdateContentItems.ItemsTypes itemsTypes = ((this.Message.ItemClass != HintItemClass.License) ? UpdateContentItems.ItemsTypes.InventoryItems : UpdateContentItems.ItemsTypes.Licensees);
				if (hasItemId)
				{
					ShopMainPageHandler.Instance.ContentUpdater.SetPagingById(this.Message.ItemId, itemsTypes);
				}
				else
				{
					List<InventoryItem> list3 = ((!local) ? CacheLibrary.GlobalShopCacheInstance.GetItemsFromCategoryImmediate(new List<int> { (int)this.Message.ItemSubType }) : CacheLibrary.LocalCacheInstance.GetItemsFromCategoryImmediate(new List<int> { (int)this.Message.ItemSubType }));
					List<InventoryItem> list4 = this.FindMaxLevelItems(list3);
					if (list4.Count > 0)
					{
						ShopMainPageHandler.Instance.ContentUpdater.SetPagingById(list4[0].ItemId, itemsTypes);
					}
				}
			};
			ManagedHint.FilterFunction filterFunction = delegate
			{
				if (ShopMainPageHandler.Instance != null)
				{
					ShopMainPageHandler.Instance.ContentUpdater.OnGetItems += onGetItemsFunc;
				}
				return false;
			};
			ManagedHint.FilterFunction filterFunction2 = delegate
			{
				bool flag2 = this.system.ScreenType == shop;
				bool flag3 = ShopMainPageHandler.Instance != null && (!hasItemId || !ShopMainPageHandler.Instance.ContentUpdater.ContainsElement(this.Message.ItemId));
				return flag2 && flag3;
			};
			List<string> list = HintSystem.GenerateShopPathByType(this.Message.ItemSubType);
			bool flag = false;
			foreach (string text2 in list)
			{
				this.AddByElementId(text2, this.system.RectOutlineHint, filterFunction2, HintSide.Undefined, (!flag) ? filterFunction : null);
				this.AddByElementId(text2, this.system.ArrowHint, filterFunction2, HintSide.Undefined, null);
				flag = true;
			}
			if (!hasItemId)
			{
				List<InventoryItem> list2;
				if (local)
				{
					list2 = CacheLibrary.LocalCacheInstance.GetItemsFromCategoryImmediate(new List<int> { (int)this.Message.ItemSubType });
				}
				else
				{
					list2 = CacheLibrary.GlobalShopCacheInstance.GetItemsFromCategoryImmediate(new List<int> { (int)this.Message.ItemSubType });
				}
				if (list2.Count == 0)
				{
					if (local)
					{
						this.waitForLocalShopCache = true;
					}
					else
					{
						this.waitForGlobalShopCache = true;
					}
					this.AddByCategoryType(this.Message.ItemSubType.ToString(), this.system.RectOutlineHint, filterFunction2, HintSide.Undefined, null);
				}
				else
				{
					this.SpawnFilteredShopCategoriesHighlights(list2, filterFunction2);
				}
			}
		}
	}

	private List<InventoryItem> FindMaxLevelItems(List<InventoryItem> filteredItems)
	{
		List<InventoryItem> fi = filteredItems.Where((InventoryItem x) => (this.Message.Length == 0f || (x.Length != null && x.Length >= (double)this.Message.Length)) && (this.Message.DisplayStorage == (StoragePlaces)0 || x.Storage == this.Message.DisplayStorage) && (this.Message.MinThickness == 0f || (x is Line && (x as Line).Thickness >= this.Message.MinThickness)) && (this.Message.MaxThickness == 0f || (x is Line && (x as Line).Thickness <= this.Message.MaxThickness)) && (this.Message.MinLoad == 0f || (x is Rod && (x as Rod).MaxLoad >= this.Message.MinLoad) || (x is Reel && (x as Reel).MaxLoad >= this.Message.MinLoad) || (x is Line && (x as Line).MaxLoad >= this.Message.MinLoad)) && (this.Message.MaxLoad == 0f || (x is Rod && (x as Rod).MaxLoad <= this.Message.MaxLoad) || (x is Reel && (x as Reel).MaxLoad <= this.Message.MaxLoad) || (x is Line && (x as Line).MaxLoad <= this.Message.MaxLoad))).ToList<InventoryItem>();
		int currLevel = PhotonConnectionFactory.Instance.Profile.Level;
		List<InventoryItem> list = new List<InventoryItem>();
		while (list.Count == 0 && currLevel >= 1)
		{
			list = fi.Where(delegate(InventoryItem x)
			{
				bool flag;
				if (x.MinLevel != currLevel || x.PriceSilver == null)
				{
					flag = !fi.Any((InventoryItem y) => y.PriceSilver != null);
				}
				else
				{
					flag = true;
				}
				return flag;
			}).ToList<InventoryItem>();
			currLevel--;
		}
		return list;
	}

	private void SpawnFilteredShopCategoriesHighlights(List<InventoryItem> filteredItems, ManagedHint.FilterFunction ff)
	{
		List<InventoryItem> list = this.FindMaxLevelItems(filteredItems);
		if (list.Count > 0)
		{
			for (int i = 0; i < list.Count; i++)
			{
				this.SpawnHintsForShopItemId(list[i].ItemId, UpdateContentItems.ItemsTypes.InventoryItems, ff, i == 0);
			}
		}
		else
		{
			this.AddByCategoryType(this.Message.ItemSubType.ToString(), this.system.RectOutlineHint, ff, HintSide.Undefined, null);
		}
	}

	private void SpawnHintsForShopItemId(int itemId, UpdateContentItems.ItemsTypes itemType, ManagedHint.FilterFunction ff, bool pagination = true)
	{
		string text;
		switch (itemType)
		{
		case UpdateContentItems.ItemsTypes.Products:
			text = "Product";
			break;
		case UpdateContentItems.ItemsTypes.InventoryItems:
			text = "Shop";
			break;
		case UpdateContentItems.ItemsTypes.Licensees:
			text = "License";
			break;
		default:
			throw new ArgumentOutOfRangeException("itemType", itemType, null);
		}
		this.AddByElementId(text + itemId, this.system.RectOutlineHint, ff, HintSide.Undefined, null);
		this.AddByElementId(text + itemId + "Buy", this.system.ArrowHint, ff, HintSide.Left, null);
		this.AddByElementId(text + itemId + "Buy", this.system.ArrowHint, ff, HintSide.Right, null);
		if (pagination)
		{
			ManagedHint.FilterFunction filterFunction = () => ff() && ShopMainPageHandler.Instance.ContentUpdater.FindPagingById(itemId, itemType) > 0;
			ManagedHint.FilterFunction filterFunction2 = () => ff() && ShopMainPageHandler.Instance.ContentUpdater.FindPagingById(itemId, itemType) < 0;
			this.AddByElementId("Sh_Next", this.system.RectOutlineHint, filterFunction, HintSide.Undefined, null);
			this.AddByElementId("Sh_Next", this.system.ArrowHint, filterFunction, HintSide.Bottom, null);
			this.AddByElementId("Sh_Prev", this.system.RectOutlineHint, filterFunction2, HintSide.Undefined, null);
			this.AddByElementId("Sh_Prev", this.system.ArrowHint, filterFunction2, HintSide.Bottom, null);
		}
	}

	private bool IsInInventoryTab()
	{
		return this.system.ScreenType == GameScreenType.Storage || this.system.ScreenType == GameScreenType.Equipment || this.system.ScreenType == GameScreenType.Fishkeeper || this.system.ScreenType == GameScreenType.ExceedStorage || this.system.ScreenType == GameScreenType.Specials || this.system.ScreenType == GameScreenType.Licenses || this.system.ScreenType == GameScreenType.Presets;
	}

	private int SpawnHintsForEquipByItemId(bool isRod)
	{
		int num;
		if (isRod)
		{
			ManagedHint.FilterFunction filterFunction = delegate
			{
				InitRods instance = InitRods.Instance;
				bool flag = instance != null;
				bool flag2 = flag && instance.ActiveRod.gameObject.activeInHierarchy && instance.ActiveRod.Rod.InventoryItem == null;
				bool flag3 = flag && RodHelper.GetSlotCount() == RodHelper.FindAllUsedRods().Count;
				bool flag4 = flag3 && instance.ActiveRod.Rod.InventoryItem != null && instance.ActiveRod.Rod.InventoryItem.ItemSubType == this.Message.ItemSubType;
				return flag2 || flag4;
			};
			if (!string.IsNullOrEmpty(this.Message.InstanceId))
			{
				num = this.AddByInstanceId(new Guid(this.Message.InstanceId), this.system.RectOutlineHint, filterFunction, HintSide.Undefined, null);
			}
			else
			{
				num = this.AddByItemId(this.Message.ItemId, this.system.RectOutlineHint, filterFunction, HintSide.Undefined, null);
			}
		}
		else
		{
			ManagedHint.FilterFunction filterFunction2 = delegate
			{
				InitRods instance2 = InitRods.Instance;
				bool flag5 = instance2 != null;
				return flag5 && instance2.ActiveRod.gameObject.activeInHierarchy && instance2.ActiveRod.SlotId == this.Message.Slot;
			};
			if (!string.IsNullOrEmpty(this.Message.InstanceId))
			{
				num = this.AddByInstanceId(new Guid(this.Message.InstanceId), this.system.RectOutlineHint, filterFunction2, HintSide.Undefined, null);
			}
			else
			{
				num = this.AddByItemId(this.Message.ItemId, this.system.RectOutlineHint, filterFunction2, HintSide.Undefined, null);
			}
		}
		return num;
	}

	private void SpawnHintsForMoveTypeToEquipment()
	{
		this.HighlightInventoryButton(null);
		this.HighlightInventoryFilter(this.Message.ItemSubType, this.system.RectOutlineHint, () => this.system.ScreenTab != global::InventorySRIA.InventorySRIA.GetMissionTabByItemType(this.Message.ItemSubType), true);
		int categoryHintId = this.AddByCategoryType(this.Message.ItemSubType.ToString(), this.system.RectOutlineHint, null, HintSide.Undefined, null);
		ManagedHint.FilterFunction filterFunction = () => this.system.ScreenType != GameScreenType.Storage && PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemSubType == this.Message.ItemSubType && x.Storage == StoragePlaces.Storage && (this.Message.DisplayStorage == (StoragePlaces)0 || x.Storage == this.Message.DisplayStorage)) != null;
		this.HighlightInventoryTab("Inv_Home", this.system.RectUnderlineHint, filterFunction, false);
		this.HighlightInventoryTab("Inv_Backpack", this.system.DollHighlightHint, () => this.GetManagedObjectDisplayed(categoryHintId), true);
	}

	private void SpawnHintsForMoveTypeToDoll()
	{
		this.HighlightInventoryButton(null);
		int categoryHintId = this.AddByCategoryType(this.Message.ItemSubType.ToString(), this.system.RectOutlineHint, null, HintSide.Undefined, null);
		ManagedHint.FilterFunction filterFunction = () => this.system.ScreenType != GameScreenType.Storage && PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemSubType == this.Message.ItemSubType && x.Storage != StoragePlaces.ParentItem && x.Storage != StoragePlaces.Doll && x.Storage != StoragePlaces.Shore && x.Storage != StoragePlaces.Hands && (this.Message.DisplayStorage == (StoragePlaces)0 || x.Storage == this.Message.DisplayStorage)) != null;
		this.HighlightInventoryTab("Inv_Home", this.system.RectUnderlineHint, filterFunction, false);
		string text = HintSystem.GenerateHighlightSlotIdFromItemType(this.Message.ItemSubType);
		if (!string.IsNullOrEmpty(text))
		{
			this.AddByElementId(text, this.system.DollHighlightHint, () => this.GetManagedObjectDisplayed(categoryHintId), HintSide.Undefined, null);
			this.AddByElementId(text, this.system.ArrowHint, () => this.GetManagedObjectDisplayed(categoryHintId), HintSide.Bottom, null);
		}
	}

	private void SpawnHintsForMoveToDoll()
	{
		this.HighlightInventoryButton(null);
		int hintId = this.AddByItemId(this.Message.ItemId, this.system.RectOutlineHint, null, HintSide.Undefined, null);
		ManagedHint.FilterFunction filterFunction = () => this.system.ScreenType != GameScreenType.Storage && PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemSubType == this.Message.ItemSubType && x.Storage != StoragePlaces.ParentItem && x.Storage != StoragePlaces.Doll && x.Storage != StoragePlaces.Shore && x.Storage != StoragePlaces.Hands && (this.Message.DisplayStorage == (StoragePlaces)0 || x.Storage == this.Message.DisplayStorage)) != null;
		this.HighlightInventoryTab("Inv_Home", this.system.RectUnderlineHint, filterFunction, false);
		string text = HintSystem.GenerateHighlightSlotIdFromItemType(this.Message.ItemSubType);
		if (!string.IsNullOrEmpty(text))
		{
			this.AddByElementId(text, this.system.DollHighlightHint, () => this.GetManagedObjectDisplayed(hintId), HintSide.Undefined, null);
			this.AddByElementId(text, this.system.ArrowHint, () => this.GetManagedObjectDisplayed(hintId), HintSide.Bottom, null);
		}
	}

	private void SpawnHintsForMoveToEquipment()
	{
		this.HighlightInventoryButton(null);
		this.HighlightInventoryFilter(this.Message.ItemSubType, this.system.RectOutlineHint, () => this.system.ScreenTab != global::InventorySRIA.InventorySRIA.GetMissionTabByItemType(this.Message.ItemSubType), true);
		int hintId = this.AddByItemId(this.Message.ItemId, this.system.RectOutlineHint, null, HintSide.Undefined, null);
		ManagedHint.FilterFunction filterFunction = () => this.system.ScreenType != GameScreenType.Storage && PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemId == this.Message.ItemId && x.Storage == StoragePlaces.Storage && (this.Message.DisplayStorage == (StoragePlaces)0 || x.Storage == this.Message.DisplayStorage)) != null;
		this.HighlightInventoryTab("Inv_Home", this.system.RectUnderlineHint, filterFunction, false);
		this.HighlightInventoryTab("Inv_Backpack", this.system.DollHighlightHint, () => this.GetManagedObjectDisplayed(hintId), true);
	}

	private void SpawnHintsForEquipByType(bool isRod, bool hasInstanceId = false, bool hasItemId = false, int elementInhintId = -1)
	{
		this.HighlightInventoryButton(null);
		Guid empty = Guid.Empty;
		if (hasInstanceId)
		{
			empty = new Guid(this.Message.InstanceId);
		}
		Func<StoragePlaces, List<InventoryItem>> getListItemsByStorage = InventoryFilters.GetListItemsByStorage(empty, this.Message, hasInstanceId, hasItemId);
		ManagedHint.FilterFunction hasInBackpack = () => getListItemsByStorage(StoragePlaces.Equipment).Count > 0;
		ManagedHint.FilterFunction hasInStorage = () => getListItemsByStorage(StoragePlaces.Storage).Count > 0;
		ManagedHint.FilterFunction correctSlot = InventoryFilters.CorrectSlot(isRod, this.Message);
		GameScreenType curScreenType = GameScreenType.Undefined;
		ManagedHint.FilterFunction hasInReachableInventory = delegate
		{
			bool flag = (this.system.ScreenType == GameScreenType.Equipment && hasInBackpack()) || (this.system.ScreenType == GameScreenType.Storage && hasInStorage());
			if (flag && curScreenType != this.system.ScreenType && correctSlot())
			{
				curScreenType = this.system.ScreenType;
				List<InventoryItem> list = ((curScreenType != GameScreenType.Equipment) ? getListItemsByStorage(StoragePlaces.Storage) : getListItemsByStorage(StoragePlaces.Equipment));
				MenuPrefabsList menuPrefabsList = MenuHelpers.Instance.MenuPrefabsList;
				global::InventorySRIA.InventorySRIA componentInChildren = menuPrefabsList.inventoryForm.GetComponentInChildren<global::InventorySRIA.InventorySRIA>();
				componentInChildren.Scroll2Item(list.Select((InventoryItem p) => p.ItemId).ToList<int>());
			}
			return flag;
		};
		ManagedHint.FilterFunction categoryHighlight = () => correctSlot() && (!hasItemId || !this.GetManagedObjectDisplayed(elementInhintId)) && hasInReachableInventory();
		ManagedHint.FilterFunction filterFunction = () => categoryHighlight() && this.system.ScreenTab != global::InventorySRIA.InventorySRIA.GetMissionTabByItemType(this.Message.ItemSubType);
		string text = HintSystem.GenerateHighlightSlotIdFromItemType(this.Message.ItemSubType);
		if (!string.IsNullOrEmpty(text))
		{
			ManagedHint.FilterFunction filterFunction2 = () => hasInReachableInventory() && correctSlot();
			this.AddByElementId(text, this.system.DollHighlightHint, filterFunction2, HintSide.Undefined, null);
			this.AddByElementId(text, this.system.ArrowHint, filterFunction2, HintSide.Undefined, null);
		}
		if (this.Message.ItemSubType != ItemSubTypes.All)
		{
			this.HighlightInventoryFilter(this.Message.ItemSubType, this.system.RectOutlineHint, filterFunction, true);
			if (!hasItemId)
			{
				this.AddByCategoryType(this.Message.ItemSubType.ToString(), this.system.RectOutlineHint, categoryHighlight, HintSide.Undefined, null);
			}
		}
		for (int i = 1; i < 8; i++)
		{
			int slot = i;
			this.AddRodSlotHint(slot, this.system.ArrowHint, delegate
			{
				if (!isRod)
				{
					bool flag2 = InitRods.Instance != null && InitRods.Instance.ActiveRod.gameObject.activeInHierarchy && InitRods.Instance.ActiveRod.SlotId == slot;
					return slot == this.Message.Slot && !flag2;
				}
				if (!(InitRods.Instance != null))
				{
					return false;
				}
				bool flag3 = RodHelper.GetSlotCount() == RodHelper.FindAllUsedRods().Count;
				List<Rod> list2 = RodHelper.FindAllUsedRods();
				Rod rod = list2.FirstOrDefault((Rod x) => x != null && x.ItemSubType == this.Message.ItemSubType);
				return (flag3 && rod != null && rod.Slot == slot) || (InitRods.Instance.ActiveRod.Rod.InventoryItem != null && list2.Count >= slot && list2.All((Rod x) => x.Slot != slot));
			});
		}
		this.HighlightInventoryTab("Inv_Backpack", this.system.RectUnderlineTextHighlightHint, () => correctSlot() && ((!hasItemId && !hasInReachableInventory()) || (hasItemId && !this.GetManagedObjectDisplayed(elementInhintId))) && !hasInStorage() && this.system.ScreenType != GameScreenType.Equipment && hasInBackpack(), true);
		this.HighlightInventoryTab("Inv_Home", this.system.RectUnderlineTextHighlightHint, () => correctSlot() && ((!hasItemId && !hasInReachableInventory()) || (hasItemId && !this.GetManagedObjectDisplayed(elementInhintId))) && !hasInBackpack() && this.system.ScreenType != GameScreenType.Storage && hasInStorage(), true);
	}

	private void AddRodSlotHint(int slot, PrefabContainer prefab, ManagedHint.FilterFunction ff)
	{
		this.AddByElementId("PD_Rod_" + slot, prefab, ff, HintSide.Undefined, null);
	}

	private void SpawnGoToPondHints()
	{
		this.HighlightMapButton(false);
		ManagedHint.FilterFunction wrongPondSelected = () => ShowPondInfo.Instance.TravelInit.GetPondElementId() != this.Message.ElementId;
		this.AddByElementId(this.Message.ElementId, this.system.ArrowHint, wrongPondSelected, HintSide.Undefined, null);
		this.AddByElementId(this.Message.ElementId, this.system.PinOutlineHint, wrongPondSelected, HintSide.Undefined, null);
		int count = this.Message.Count;
		if (count > 0)
		{
			this.AddByElementId("TRAVEL_AddDay", this.system.RectOutlineHint, () => this.Message.Count > ShowPondInfo.Instance.TravelInit.ResidenceCost.DaysValue && !wrongPondSelected(), HintSide.Undefined, null);
			this.AddByElementId("TRAVEL_SubstDay", this.system.RectOutlineHint, () => this.Message.Count < ShowPondInfo.Instance.TravelInit.ResidenceCost.DaysValue && !wrongPondSelected(), HintSide.Undefined, null);
			this.AddByElementId("TRAVEL_Go", this.system.ArrowHint, () => this.Message.Count == ShowPondInfo.Instance.TravelInit.ResidenceCost.DaysValue && !wrongPondSelected(), HintSide.Undefined, null);
			this.AddByElementId("TRAVEL_Go", this.system.RectOutlineHint, () => this.Message.Count == ShowPondInfo.Instance.TravelInit.ResidenceCost.DaysValue && !wrongPondSelected(), HintSide.Undefined, null);
		}
		else
		{
			this.AddByElementId("TRAVEL_Go", this.system.ArrowHint, () => !wrongPondSelected(), HintSide.Undefined, null);
			this.AddByElementId("TRAVEL_Go", this.system.RectOutlineHint, () => !wrongPondSelected(), HintSide.Undefined, null);
		}
	}

	private void SpawnGoToLocationHints()
	{
		ManagedHint.FilterFunction filterFunction = () => ShowLocationInfo.Instance.GetCurrentLocation() != this.Message.ElementId;
		this.AddByElementId(this.Message.ElementId, this.system.ArrowHint, filterFunction, HintSide.Undefined, null);
		this.AddByElementId(this.Message.ElementId, this.system.LocationOutlineHint, filterFunction, HintSide.Undefined, null);
		this.HighlightMapButton(true);
	}

	private void HighlightInventoryButton(ManagedHint.FilterFunction logicalAndFilterFunction = null)
	{
		ManagedHint.FilterFunction filterFunction;
		if (logicalAndFilterFunction == null)
		{
			filterFunction = () => !this.IsInInventoryTab();
		}
		else
		{
			filterFunction = () => logicalAndFilterFunction() && !this.IsInInventoryTab();
		}
		this.AddByElementId("DASH_INVENTORY", this.system.RectUnderlineHint, filterFunction, HintSide.Undefined, null);
		string text = ((!this.addAutoPrefix) ? ScriptLocalization.Get("GotoInventoryTooltip") : ("auto_" + ScriptLocalization.Get("GotoInventoryTooltip")));
		this.AddTooltip("DASH_INVENTORY", text, filterFunction, HintSide.Undefined, null);
	}

	private void HighlightInventoryTab(string tabElementId, PrefabContainer prefab, ManagedHint.FilterFunction ff, bool arrow = true)
	{
		int backpackHighlightInhintId = this.AddByElementId(tabElementId, prefab, ff, HintSide.Undefined, null);
		if (arrow)
		{
			this.AddByElementId(tabElementId, this.system.ArrowHint, () => this.GetManagedObjectDisplayed(backpackHighlightInhintId), HintSide.Bottom, null);
		}
	}

	private void HighlightInventoryFilter(ItemSubTypes subType, PrefabContainer prefab, ManagedHint.FilterFunction ff = null, bool arrow = true)
	{
		this.AddByCategoryType(string.Format("tab{0}", subType), prefab, ff, HintSide.Undefined, null);
		if (arrow)
		{
			this.AddByCategoryType(string.Format("tab{0}", subType), this.system.ArrowHint, ff, HintSide.Left, null);
		}
	}

	private void HighlightMapButton(bool local)
	{
		GameScreenType map = ((!local) ? GameScreenType.GlobalMap : GameScreenType.LocalMap);
		this.AddByElementId("DASH_MAP", this.system.RectUnderlineHint, () => this.system.ScreenType != map, HintSide.Undefined, null);
		string text = ((!this.addAutoPrefix) ? ScriptLocalization.Get((!local) ? "GotoGlobalMapTooltip" : "GotoLocalMapTooltip") : ("auto_" + ScriptLocalization.Get((!local) ? "GotoGlobalMapTooltip" : "GotoLocalMapTooltip")));
		this.AddTooltip("DASH_MAP", text, () => this.system.ScreenType != map, HintSide.Undefined, null);
	}

	private void HighlightGoToFishingButton(string exactLocation = "")
	{
		ManagedHint.FilterFunction filterFunction = null;
		if (!string.IsNullOrEmpty(exactLocation))
		{
			filterFunction = () => ShowLocationInfo.Instance.GetCurrentLocation() == exactLocation;
		}
		this.AddByElementId("LM_MAP_GoFishing", this.system.RectOutlineHint, filterFunction, HintSide.Undefined, null);
		this.AddByElementId("LM_MAP_GoFishing", this.system.ArrowHint, filterFunction, HintSide.Undefined, null);
	}

	private void SpawnMoveTimeForwardHints(bool nextDay)
	{
		if (nextDay)
		{
			int nmId = this.AddByElementId("RT_NextMorning", this.system.RectOutlineHint, null, HintSide.Undefined, null);
			this.AddByElementId("RT_NextMorning", this.system.ArrowHint, null, HintSide.Undefined, null);
			int nfId = this.AddByElementId("RT_NightForward", this.system.RectOutlineHint, null, HintSide.Undefined, null);
			this.AddByElementId("RT_NightForward", this.system.ArrowHint, null, HintSide.Undefined, null);
			this.AddByElementId("RT_Skip", this.system.RectOutlineHint, () => !this.GetManagedObjectDisplayed(nmId) && !this.GetManagedObjectDisplayed(nfId), HintSide.Undefined, null);
			this.AddByElementId("RT_Skip", this.system.ArrowHint, () => !this.GetManagedObjectDisplayed(nmId) && !this.GetManagedObjectDisplayed(nfId), HintSide.Undefined, null);
			return;
		}
		this.AddByElementId("RT_Skip", this.system.RectOutlineHint, null, HintSide.Undefined, null);
		this.AddByElementId("RT_Skip", this.system.ArrowHint, null, HintSide.Undefined, null);
		int hours = this.Message.Value;
		if (hours > 0)
		{
			this.AddByElementId("RT_Inc", this.system.RectOutlineHint, () => hours > ShowHudElements.Instance.GetCurrentRewindPanelHoursValue(), HintSide.Undefined, null);
			this.AddByElementId("RT_Dec", this.system.RectOutlineHint, () => hours < ShowHudElements.Instance.GetCurrentRewindPanelHoursValue(), HintSide.Undefined, null);
			this.AddByElementId("RT_Apply", this.system.ArrowHint, () => hours == ShowHudElements.Instance.GetCurrentRewindPanelHoursValue(), HintSide.Undefined, null);
			this.AddByElementId("RT_Apply", this.system.RectOutlineHint, () => hours == ShowHudElements.Instance.GetCurrentRewindPanelHoursValue(), HintSide.Undefined, null);
		}
	}

	public void Init()
	{
		bool flag = false;
		bool flag2 = false;
		ManagedHint.FilterFunction filterFunction = null;
		if (!string.IsNullOrEmpty(this.Message.Code))
		{
			string code = this.Message.Code;
			switch (code)
			{
			case "$gBuyLicenseBasic":
			case "$gBuyLicense":
			case "$gBuyLicense1":
				if (this.Message.ItemId > 0 && this.Message.ItemClass == HintItemClass.License)
				{
					flag = true;
					this.SpawnHintsForShopItemId(this.Message.ItemId, UpdateContentItems.ItemsTypes.Licensees, () => this.system.ScreenType == GameScreenType.LicensesShop, true);
					if (ScreenManager.Instance != null && ShopMainPageHandler.Instance != null && ShopMainPageHandler.Instance.ContentUpdater != null && ScreenManager.Instance.GameScreen == GameScreenType.LicensesShop)
					{
						ShopMainPageHandler.Instance.ContentUpdater.SetPagingById(this.Message.ItemId, UpdateContentItems.ItemsTypes.Licensees);
					}
				}
				break;
			case "$gSelectPond":
			case "$gSelectPondGlobus":
				if (StaticUserData.CurrentPond == null)
				{
					SetPondsOnGlobalMap.GlobeHelp.SetActiveHint(this.Message.ElementId);
				}
				break;
			case "$GotoPond":
			case "$GotoPondDays":
				if (StaticUserData.CurrentPond == null)
				{
					SetPondsOnGlobalMap.GlobeHelp.SetActiveHint(this.Message.ElementId);
				}
				flag = true;
				this.SpawnGoToPondHints();
				filterFunction = () => this.system.ScreenType == GameScreenType.GlobalMap;
				break;
			case "$GotoLocation":
				flag = true;
				this.SpawnGoToLocationHints();
				filterFunction = () => this.system.ScreenType == GameScreenType.LocalMap;
				break;
			case "$GotoFishing":
				flag = true;
				this.HighlightMapButton(true);
				this.HighlightGoToFishingButton(this.Message.ElementId);
				filterFunction = () => this.system.ScreenType == GameScreenType.LocalMap;
				break;
			case "$MoveTimeForwardHours":
				flag = true;
				this.SpawnMoveTimeForwardHints(false);
				filterFunction = () => this.system.ScreenType == GameScreenType.Time;
				break;
			case "$MoveTimeForwardDay":
				flag = true;
				this.SpawnMoveTimeForwardHints(true);
				filterFunction = () => this.system.ScreenType == GameScreenType.Time;
				break;
			case "$GotoHomeBuyGlobalItem":
			case "$GotoHomeItemInStorage":
			case "$GotoHome":
			case "$GotoHomeAndGotoPond":
			case "$GotoHomeItemTypeInStorage":
			case "$GotoHomeBuyGlobalItemType":
				flag = true;
				this.HighlightMapButton(true);
				this.AddByElementId("LM_MAP_Leave", this.system.RectOutlineHint, null, HintSide.Undefined, null);
				filterFunction = () => this.system.ScreenType == GameScreenType.LocalMap;
				break;
			case "$BuyGlobalItem":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == "$BuyGlobalItem").ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				filterFunction = () => this.system.ScreenType == GameScreenType.GlobalShop;
				this.SpawnHintsForShopByItemType(false, true);
				this.SpawnHintsForShopItemId(this.Message.ItemId, UpdateContentItems.ItemsTypes.InventoryItems, filterFunction, true);
				break;
			case "$BuyLocalItem":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				filterFunction = () => this.system.ScreenType == GameScreenType.LocalShop;
				this.SpawnHintsForShopByItemType(true, true);
				this.SpawnHintsForShopItemId(this.Message.ItemId, UpdateContentItems.ItemsTypes.InventoryItems, filterFunction, true);
				break;
			case "$MoveRodOnDoll":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.SpawnHintsForEquipByType(true, !string.IsNullOrEmpty(this.Message.InstanceId), true, this.SpawnHintsForEquipByItemId(true));
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$MoveItemOnRod":
			case "$MoveItemOnRodLength":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.SpawnHintsForEquipByType(false, !string.IsNullOrEmpty(this.Message.InstanceId), true, this.SpawnHintsForEquipByItemId(false));
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$BuyGlobalItemType":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.SpawnHintsForShopByItemType(false, false);
				filterFunction = () => this.system.ScreenType == GameScreenType.GlobalShop;
				break;
			case "$BuyLocalItemType":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.SpawnHintsForShopByItemType(true, false);
				filterFunction = () => this.system.ScreenType == GameScreenType.LocalShop;
				break;
			case "$MoveRodTypeOnDoll":
				flag = true;
				this.SpawnHintsForEquipByType(true, false, false, -1);
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$MoveItemTypeOnRod":
			case "$MoveItemTypeOnRodLength":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.SpawnHintsForEquipByType(false, false, false, -1);
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$MoveItemTypeToEquipment":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.SpawnHintsForMoveTypeToEquipment();
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$MoveItemTypeToDoll":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.SpawnHintsForMoveTypeToDoll();
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$MoveItemToDoll":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.SpawnHintsForMoveToDoll();
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$MoveItemToEquipment":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.SpawnHintsForMoveToEquipment();
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$NoPlaceToBuyNewItemStorageOverloaded":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.HighlightInventoryButton(null);
				this.HighlightInventoryTab("Inv_Home", this.system.RectUnderlineTextHighlightHint, () => this.system.ScreenType != GameScreenType.Storage, true);
				filterFunction = () => this.system.ScreenType == GameScreenType.GlobalShop || this.system.ScreenType == GameScreenType.LocalShop || this.IsInInventoryTab();
				break;
			case "$NoPlaceToBuyNewReelOnPond":
			case "$NoPlaceToBuyNewReelOnGlobal":
			case "$NoPlaceToEquipReelOnGlobal":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.HighlightInventoryButton(null);
				this.HighlightInventoryTab("Inv_Backpack", this.system.RectUnderlineTextHighlightHint, () => this.system.ScreenType != GameScreenType.Equipment, true);
				this.HighlightInventoryFilter(ItemSubTypes.Reel, this.system.RectOutlineHint, () => this.system.ScreenType == GameScreenType.Equipment && this.system.ScreenTab != global::InventorySRIA.InventorySRIA.GetMissionTabByItemType(ItemSubTypes.Reel), true);
				this.AddByElementId("Inv_Restrictions_Reels", this.system.ArrowHint, null, HintSide.Undefined, null);
				this.AddByElementId("Inv_Restrictions_Reels", this.system.RectOutlineHint, null, HintSide.Undefined, null);
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$NoPlaceToBuyNewLineOnPond":
			case "$NoPlaceToBuyNewLineOnGlobal":
			case "$NoPlaceToEquipLineOnGlobal":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.HighlightInventoryButton(null);
				this.HighlightInventoryTab("Inv_Backpack", this.system.RectUnderlineTextHighlightHint, () => this.system.ScreenType != GameScreenType.Equipment, true);
				this.HighlightInventoryFilter(ItemSubTypes.Line, this.system.RectOutlineHint, () => this.system.ScreenType == GameScreenType.Equipment && this.system.ScreenTab != global::InventorySRIA.InventorySRIA.GetMissionTabByItemType(ItemSubTypes.Line), true);
				this.AddByElementId("Inv_Restrictions_Lines", this.system.ArrowHint, null, HintSide.Undefined, null);
				this.AddByElementId("Inv_Restrictions_Lines", this.system.RectOutlineHint, null, HintSide.Undefined, null);
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$NoPlaceToBuyNewChumOnPond":
			case "$NoPlaceToBuyNewChumOnGlobal":
			case "$NoPlaceToEquipChumOnGlobal":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.HighlightInventoryButton(null);
				this.HighlightInventoryTab("Inv_Backpack", this.system.RectUnderlineTextHighlightHint, () => this.system.ScreenType != GameScreenType.Equipment, true);
				this.AddByElementId("Inv_Restrictions_Chums", this.system.ArrowHint, null, HintSide.Undefined, null);
				this.AddByElementId("Inv_Restrictions_Chums", this.system.RectOutlineHint, null, HintSide.Undefined, null);
				this.HighlightInventoryFilter(this.Message.ItemSubType, this.system.RectOutlineHint, () => this.system.ScreenType == GameScreenType.Equipment && this.system.ScreenTab != global::InventorySRIA.InventorySRIA.GetMissionTabByGroupCategory(GroupCategoryType.ChumsAll), true);
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$NoPlaceToBuyNewTackleOnPond":
			case "$NoPlaceToBuyNewTackleOnGlobal":
			case "$NoPlaceToEquipTackleOnGlobal":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.HighlightInventoryButton(null);
				this.HighlightInventoryTab("Inv_Backpack", this.system.RectUnderlineTextHighlightHint, () => this.system.ScreenType != GameScreenType.Equipment, true);
				this.AddByElementId("Inv_Restrictions_Tackles", this.system.ArrowHint, null, HintSide.Undefined, null);
				this.AddByElementId("Inv_Restrictions_Tackles", this.system.RectOutlineHint, null, HintSide.Undefined, null);
				filterFunction = new ManagedHint.FilterFunction(this.IsInInventoryTab);
				break;
			case "$NoPlaceToBuyNewRodOnPond":
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.HighlightMapButton(true);
				this.AddByElementId("LM_MAP_Leave", this.system.RectOutlineHint, null, HintSide.Undefined, null);
				filterFunction = () => this.system.ScreenType == GameScreenType.LocalMap;
				break;
			case "$NoPlaceToBuyNewRodOnGlobal":
			case "$NoPlaceToEquipRodOnGlobal":
			{
				flag = true;
				if (this.system.activeHints.Where((ManagedHint x) => x.Message.Code == this.Message.Code).ToList<ManagedHint>().Count >= 2)
				{
					flag2 = true;
				}
				this.HighlightInventoryButton(null);
				filterFunction = () => this.system.ScreenType == GameScreenType.GlobalShop || this.IsInInventoryTab();
				for (int i = 1; i < 8; i++)
				{
					int num2 = i;
					this.AddRodSlotHint(num2, this.system.ArrowHint, new ManagedHint.FilterFunction(this.IsInInventoryTab));
				}
				break;
			}
			}
		}
		Transform transform = null;
		if (!string.IsNullOrEmpty(this.Message.TitleFormatted))
		{
			string text = ((!flag || !this.addAutoPrefix) ? this.Message.TitleFormatted : ("auto_" + this.Message.TitleFormatted));
			if (flag && flag2 && !string.IsNullOrEmpty(this.Message.DescriptionFormatted))
			{
				text = ((!this.addAutoPrefix) ? this.Message.DescriptionFormatted : ("auto_" + this.Message.DescriptionFormatted));
			}
			this.AddTextHint(text, filterFunction);
		}
		if (!string.IsNullOrEmpty(this.Message.TooltipFormatted))
		{
			string text2 = ((!flag || !this.addAutoPrefix) ? this.Message.TooltipFormatted : ("auto_" + this.Message.TooltipFormatted));
			if (!string.IsNullOrEmpty(this.Message.ElementId))
			{
				this.AddTooltip(this.Message.ElementId, text2, null, this.Message.Side, null);
			}
			else if (this.Message.ItemId > 0)
			{
				this.ProcessItemId(this.system.TooltipHint);
			}
		}
		if (this.Message.ArrowType3D != HintArrowType3D.Undefined)
		{
			ManagedHintObject managedHintObject;
			switch (this.Message.ArrowType3D)
			{
			case HintArrowType3D.Pointer:
				managedHintObject = this.AddPrefab(this.system.ArrowHint3D, null, null, HintSide.Undefined, null);
				break;
			case HintArrowType3D.Ring:
				managedHintObject = this.AddPrefab(this.system.RingHint3DSlim, null, null, HintSide.Undefined, null);
				break;
			case HintArrowType3D.Cross:
				managedHintObject = this.AddPrefab(this.system.CrossHint3D, null, null, HintSide.Undefined, null);
				break;
			default:
				managedHintObject = this.AddPrefab(this.system.RingHint3DNormal, null, null, HintSide.Undefined, null);
				break;
			}
			transform = managedHintObject.transform;
		}
		PrefabContainer prefabByElemId = this.GetPrefabByElemId(this.Message.ElementId);
		if (prefabByElemId == null)
		{
			this.AddPrefab(this.GetPrefabByArrowType(this.Message.ArrowType), this.Message);
			this.AddPrefab(this.GetPrefabByHighlightType(this.Message.HighlightType), this.Message);
		}
		else
		{
			this.AddPrefab(prefabByElemId, this.Message);
		}
		if (this.Message.KeyType != HintKeyType.Undefined)
		{
			HintSystem.KeyPrefabPair keyPrefabPair = this.system.buttonsPrefabs.FirstOrDefault((HintSystem.KeyPrefabPair x) => x.Key == this.Message.KeyType);
			if (keyPrefabPair != null)
			{
				if (!string.IsNullOrEmpty(this.Message.ElementId))
				{
					this.AddByElementId(this.Message.ElementId, keyPrefabPair.Prefab, null, HintSide.Undefined, null);
				}
				else
				{
					this.AddPrefab(keyPrefabPair.Prefab, transform, null, HintSide.Undefined, null);
				}
			}
		}
		if (transform != null)
		{
			if (this.Message.ScreenPosition != null)
			{
				(transform.transform as RectTransform).anchoredPosition = new Vector2(this.Message.ScreenPosition.X, this.Message.ScreenPosition.Y);
			}
			else if (this.Message.ScenePosition != null)
			{
				transform.position = new Vector3(this.Message.ScenePosition.X, this.Message.ScenePosition.Y, this.Message.ScenePosition.Z);
				if (this.Message.Scale != null)
				{
					transform.localScale = new Vector3(this.Message.Scale.X, this.Message.Scale.Y, this.Message.Scale.Z);
				}
				if (this.Message.Rotation != null)
				{
					transform.localRotation = Quaternion.Euler(this.Message.Rotation.X, this.Message.Rotation.Y, this.Message.Rotation.Z);
				}
			}
		}
		this.inited = true;
	}

	public void ElementRemoved(int inhintID)
	{
		if (!this.inited)
		{
			return;
		}
		ManagedHintObject managedHintObject = this.managedObjects[inhintID];
		if ((this.Message.Code == "$gSelectPond" || this.Message.Code == "$gSelectPondGlobus") && StaticUserData.CurrentPond == null)
		{
			SetPondsOnGlobalMap.GlobeHelp.SetActiveHint(null);
		}
		this.managedObjects.Remove(inhintID);
		this.inhintIdsToSpawn.Add(inhintID);
		int[] array = (from x in this.registeredCategoryHighlights
			where x.Value.Contains(inhintID)
			select x into y
			select y.Key).ToArray<int>();
		if (array.Length > 0)
		{
			int num = array[0];
			string text = this.registeredCategories[num];
			this.registeredCategoryHighlights[num].Remove(inhintID);
			this.inhintIdsToSpawn.Remove(inhintID);
			if (this.registeredCategoryTransforms.ContainsKey(inhintID))
			{
				Transform transform = this.registeredCategoryTransforms[inhintID];
				this.registeredCategoryTransforms.Remove(inhintID);
				if (this.spawnedCategoryHintObjects.ContainsKey(transform))
				{
					this.spawnedCategoryHintObjects[transform].Remove(num);
				}
			}
		}
		TextHint textHint = managedHintObject as TextHint;
		if (textHint != null)
		{
			this.system.TextHintParent.RmTextHint(textHint);
		}
	}

	public void Deinit()
	{
		this.inited = false;
		List<int> list = this.managedObjects.Keys.ToList<int>();
		for (int i = 0; i < list.Count; i++)
		{
			int num = list[i];
			ManagedHintObject managedHintObject = this.managedObjects[num];
			TextHint textHint = managedHintObject as TextHint;
			if (textHint != null)
			{
				this.system.TextHintParent.RmTextHint(textHint);
			}
			if (managedHintObject != null)
			{
				Object.Destroy(this.managedObjects[num].gameObject);
			}
			this.managedObjects.Remove(num);
		}
		this.inhintIdsToSpawn.Clear();
		this.inhintIdsToIgnoreCanShow.Clear();
		this.spawnedCategoryHintObjects.Clear();
	}

	public void ValidateHint()
	{
		if (this.Message.ShowDuringMs > 0 && this.time * 1000f >= (float)(this.Message.ShowDuringMs + this.Message.ShowAfterMs))
		{
			this.system.RemoveHint(this.Message);
			return;
		}
	}

	private void AddPrefab(PrefabContainer prefab, HintMessage hintMessage)
	{
		if (prefab != null)
		{
			if (!string.IsNullOrEmpty(hintMessage.ElementId))
			{
				this.AddByElementId(hintMessage.ElementId, prefab, null, HintSide.Undefined, null);
			}
			else if (hintMessage.ItemId > 0)
			{
				this.ProcessItemId(prefab);
			}
		}
	}

	private PrefabContainer GetPrefabByArrowType(HintArrowType arrowType)
	{
		if (arrowType != HintArrowType.Undefined)
		{
			switch (arrowType)
			{
			case HintArrowType.Pointer:
				return this.system.ArrowHint;
			case HintArrowType.LMB:
				return this.system.LMB;
			case HintArrowType.RMB:
				return this.system.RMB;
			case HintArrowType.Bobber:
				return this.system.Bobber;
			case HintArrowType.Hook:
				return this.system.Hook;
			}
			return this.system.ArrowHint;
		}
		return null;
	}

	private PrefabContainer GetPrefabByHighlightType(HintHighlightType highlightType)
	{
		if (highlightType == HintHighlightType.Undefined)
		{
			return null;
		}
		if (highlightType == HintHighlightType.Outline)
		{
			return this.system.RectOutlineHint;
		}
		if (highlightType != HintHighlightType.Underline)
		{
			return this.system.ArrowHint;
		}
		return this.system.RectUnderlineHint;
	}

	private PrefabContainer GetPrefabByElemId(string elementId)
	{
		switch (HintElementsIdMapper.GetHintPrefabByElemId(elementId))
		{
		case HintElementsIdMapper.HintPrefabs.ColorImageParent:
			return this.system.HintColorImageParentPrefab;
		case HintElementsIdMapper.HintPrefabs.ColorTextChildren:
			return this.system.HintColorTextChildrenPrefab;
		case HintElementsIdMapper.HintPrefabs.ColorImageChildren:
			return this.system.HintColorImageChildrenPrefab;
		case HintElementsIdMapper.HintPrefabs.BobberIndicatorBottom:
			return this.system.HintHUDBobberIndicatorBottomPrefab;
		case HintElementsIdMapper.HintPrefabs.BobberIndicatorTop:
			return this.system.HintHUDBobberIndicatorTopPrefab;
		case HintElementsIdMapper.HintPrefabs.BobberIndicatorTimer:
			return this.system.HintHUDBobberIndicatorTimerPrefab;
		case HintElementsIdMapper.HintPrefabs.LineRodReelIndicator:
			return (SettingsManager.FightIndicator != FightIndicator.OneBand) ? this.system.HUDLineRodReelIndicatorThreePrefab : this.system.HUDLineRodReelIndicatorOnePrefab;
		case HintElementsIdMapper.HintPrefabs.FrictionSpeed:
			return this.system.HUDFrictionSpeedPrefab;
		case HintElementsIdMapper.HintPrefabs.Friction:
			return this.system.HUDFrictionPrefab;
		case HintElementsIdMapper.HintPrefabs.CastSimple:
			return this.system.HUDCastSimplePrefab;
		case HintElementsIdMapper.HintPrefabs.CastTarget:
			return this.system.HUDCastTargetPrefab;
		case HintElementsIdMapper.HintPrefabs.Bobber:
			return this.system.HUDBobberPrefab;
		case HintElementsIdMapper.HintPrefabs.Achivements:
			return this.system.AchivementsPrefab;
		case HintElementsIdMapper.HintPrefabs.PondLicensesToggle:
			return this.system.PondLicensesTogglePrefab;
		case HintElementsIdMapper.HintPrefabs.FeederFishingIndicator:
			return this.system.FeederFishingIndicatorPrefab;
		case HintElementsIdMapper.HintPrefabs.BottomFishingIndicator:
			return this.system.BottomFishingIndicatorPrefab;
		default:
			return null;
		}
	}

	private HintSystem system;

	private int inhintId;

	private static List<string> ElementIdsToIgnoreCanShow = new List<string> { "NavMap_ConfirmBuoyLastFish", "MAIN_BoatRent", "MAIN_ExtendStay", "GM_RepairAll", "CW_Take", "CW_Release" };

	private Dictionary<int, ManagedHintObject> managedObjects;

	private Dictionary<int, PrefabContainer> registeredPrefabs;

	private Dictionary<int, string> registeredElementIds;

	private Dictionary<int, Transform> registeredCategoryTransforms;

	private Dictionary<int, int> registeredItemIds;

	private Dictionary<int, Guid> registeredInstanceIds;

	private Dictionary<int, string> registeredCategories;

	private Dictionary<int, List<int>> registeredCategoryHighlights;

	private Dictionary<Transform, List<int>> spawnedCategoryHintObjects;

	private Dictionary<int, string> registeredTextStrings;

	private Dictionary<int, HintSide> registeredSides;

	private Dictionary<int, ManagedHint.FilterFunction> registeredFilterFunctions;

	private Dictionary<int, ManagedHint.FilterFunction> registeredShowFunctions;

	private List<int> inhintIdsToSpawn;

	private List<Guid> registeredLineInstanceIds;

	private List<int> inhintIdsToIgnoreCanShow;

	private bool inited;

	private bool displayed;

	private float time;

	private bool addAutoPrefix;

	private bool waitForLocalShopCache;

	private bool waitForGlobalShopCache;

	public int parentId;

	public delegate bool FilterFunction();
}
