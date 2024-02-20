using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Inventory.Mixing;
using I2.Loc;
using InventorySRIA;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChumMixing : ActivityStateControlled
{
	public static ChumMixing Instance { get; private set; }

	private Dictionary<Types, int> GetEmptyInt
	{
		get
		{
			return new Dictionary<Types, int>
			{
				{
					Types.Base,
					0
				},
				{
					Types.Aroma,
					0
				},
				{
					Types.Particle,
					0
				}
			};
		}
	}

	private void Awake()
	{
		ChumMixing.Instance = this;
		this._slotsForLevel = this.GetEmptyInt;
		this._nextLevelUnlock = this.GetEmptyInt;
		this._emptiesList = this._empties.ToList<ChumMixing.IngredientData>();
		this._uiTypePrefabsList = this._uiTypePrefabs.ToList<ChumMixing.UiTypePrefabs>();
		this._buttonRename.onClick.AddListener(new UnityAction(this.OnRename));
		this._buttonRenameIco.onClick.AddListener(new UnityAction(this.OnRename));
		this._buttonClear.onClick.AddListener(new UnityAction(this.Clear));
		this._newChumIco.gameObject.SetActive(false);
		this._dropMe.OnAction += this._dropMe_OnAction;
		this._tgl.GetComponent<ToggleStateChanges>().OnSelected += this.ChumMixing_OnSelected;
		this._navigations = new UINavigation[]
		{
			this._tglGroupResult.GetComponent<UINavigation>(),
			this._tglGroupMix.GetComponent<UINavigation>()
		};
	}

	protected override void Start()
	{
		base.Start();
		this.UpdateLevelDependencies();
		for (int i = 0; i < ChumMixConst.Ingredients.Count; i++)
		{
			Types type = ChumMixConst.Ingredients[i];
			this.AddChumIngredient<ChumComponentTitle>(UiTypes.Title).Init(type, UiTypes.Title, ScriptLocalization.Get(ChumMixConst.Titles[type]), this.GetUpToPrcText(type));
			if (type == Types.Water)
			{
				this.AddChumIngredient<ChumComponentWater>(UiTypes.Water).Init(type, UiTypes.Water, 0f);
			}
			else
			{
				this.AddChumIngredient<ChumComponentEmpty>(UiTypes.Empty).Init(type, UiTypes.Empty, this._emptiesList.Find((ChumMixing.IngredientData p) => p.Type == type).Sp);
			}
		}
		this.InitRecipes();
		this.InitProduct();
		this.UpdateChumInfo();
		InfoMessageController.Instance.OnActivate += this.Instance_OnActivate;
		PhotonConnectionFactory.Instance.OnProductDelivered += this.Instance_OnProductDelivered;
		PhotonConnectionFactory.Instance.OnProductBought += this.Instance_OnProductBought;
		PhotonConnectionFactory.Instance.OnBuyProductFailed += this.Instance_OnBuyProductFailed;
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.InventoryUpdated;
		PhotonConnectionFactory.Instance.OnChumRecipeRemove += this.Instance_OnChumRecipeRemove;
		PhotonConnectionFactory.Instance.OnChumRecipeRemoveFailure += this.Instance_OnChumRecipeRemoveFailure;
		PhotonConnectionFactory.Instance.OnChumRecipeSaveNew += this.Instance_OnChumRecipeSaveNew;
		PhotonConnectionFactory.Instance.OnChumRecipeSaveNewFailure += this.Instance_OnChumRecipeSaveNewFailure;
		if (InventoryInit.Instance != null && InventoryInit.Instance.SRIA != null)
		{
			InventoryInit.Instance.SRIA.OnInventoryTabSwitched.AddListener(new UnityAction(this.UpdateButtonsInteractability));
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		PhotonConnectionFactory.Instance.OnProductDelivered -= this.Instance_OnProductDelivered;
		PhotonConnectionFactory.Instance.OnProductBought -= this.Instance_OnProductBought;
		PhotonConnectionFactory.Instance.OnBuyProductFailed -= this.Instance_OnBuyProductFailed;
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.InventoryUpdated;
		PhotonConnectionFactory.Instance.OnChumRecipeRemove -= this.Instance_OnChumRecipeRemove;
		PhotonConnectionFactory.Instance.OnChumRecipeRemoveFailure -= this.Instance_OnChumRecipeRemoveFailure;
		PhotonConnectionFactory.Instance.OnChumRecipeSaveNew -= this.Instance_OnChumRecipeSaveNew;
		PhotonConnectionFactory.Instance.OnChumRecipeSaveNewFailure -= this.Instance_OnChumRecipeSaveNewFailure;
		if (InventoryInit.Instance != null && InventoryInit.Instance.SRIA != null)
		{
			InventoryInit.Instance.SRIA.OnInventoryTabSwitched.RemoveListener(new UnityAction(this.UpdateButtonsInteractability));
		}
	}

	protected override void SetHelp()
	{
		Chum curChum = this.CurChum;
		if (curChum != null && curChum.Ingredients != null)
		{
			curChum.Ingredients.ForEach(new Action<ChumIngredient>(this.UpdateSria));
		}
	}

	protected override void HideHelp()
	{
		Chum curChum = this.CurChum;
		if (curChum != null && curChum.Ingredients != null)
		{
			curChum.Ingredients.ForEach(new Action<ChumIngredient>(this.UpdateSria));
		}
	}

	public void Add(InventoryItem ii)
	{
		if (this.IsBlockedBySnowballs(ii))
		{
			return;
		}
		if (this.IsDifferentBases(ii))
		{
			this.ShowErrorCantMixDifferentBases();
			return;
		}
		if (this.IsParticleBlocked(ii))
		{
			this.ShowErrorParticlesBlocked();
			return;
		}
		ChumIngredient chumIngredient = ii as ChumIngredient;
		Types typeByClass = ChumMixing.GetTypeByClass(chumIngredient);
		if (this.IsBlocked(ii))
		{
			if (this._nextLevelUnlock[typeByClass] > 0)
			{
				UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("UnlockedAtLevelCaption"), this._nextLevelUnlock[typeByClass]), true, null, false);
			}
			return;
		}
		Chum curChum = this.CurChum;
		float num = (float)this.ChumWeight;
		if (typeByClass != Types.Base && (num <= 0f || !ChumChecker.IsOk(Types.Base, ChumMixConst.PrcAdd[Types.Base], curChum, -1)))
		{
			this.ChumMixErrMsgBasesMin();
			return;
		}
		ChumIngredient chumIngredient2 = ((curChum.Ingredients == null) ? null : curChum.Ingredients.FirstOrDefault((ChumIngredient p) => p.ItemId == ii.ItemId));
		float num2;
		if (chumIngredient2 != null)
		{
			Guid? instanceId = chumIngredient2.InstanceId;
			bool flag = instanceId != null;
			Guid? instanceId2 = ii.InstanceId;
			if (flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault()))
			{
				num2 = chumIngredient2.Amount;
				goto IL_192;
			}
		}
		num2 = 0f;
		IL_192:
		float num3 = num2;
		float? num4 = null;
		if (typeByClass != Types.Base)
		{
			float num5 = ChumMixConst.PrcAdd[typeByClass];
			if (!ChumChecker.IsOk(typeByClass, num5, curChum, ii.ItemId))
			{
				this.ShowChumMixErrMsgIngredientMax(typeByClass);
				return;
			}
			double prc = ChumChecker.GetPrc(typeByClass, curChum, ii.ItemId);
			num5 -= (float)prc;
			float num6 = 1000f * (num - num3);
			float num7 = num5 / 100f;
			float num8 = 1f - num7;
			double num9 = (double)(num7 * num6 / num8);
			num4 = new float?((float)Math.Floor(num9));
			float? num10 = num4;
			num4 = ((num10 == null) ? null : new float?(num10.GetValueOrDefault() / 1000f));
			if (num9 < 1.0)
			{
				this.ShowChumMixErrMsgIngredientMax(typeByClass);
				return;
			}
			float value = num4.Value;
			double? weight = ii.Weight;
			num4 = new float?(Mathf.Min(value, (float)((weight == null) ? 0.0 : weight.Value)));
		}
		CutChumComponent cutChumComponent = this.InitMessageBox<CutChumComponent>(MessageBoxList.Instance.CutChumComponentPrefab);
		float num11 = (float)this.ChumWeight;
		if (chumIngredient2 != null)
		{
			Guid? instanceId3 = chumIngredient2.InstanceId;
			bool flag2 = instanceId3 != null;
			Guid? instanceId4 = ii.InstanceId;
			if (flag2 != (instanceId4 != null) || (instanceId3 != null && instanceId3.GetValueOrDefault() != instanceId4.GetValueOrDefault()))
			{
				num11 -= chumIngredient2.Amount;
			}
		}
		cutChumComponent.Init(chumIngredient, num11, num3, num4);
		cutChumComponent.OnAccepted += this.CutCtrl_OnAccepted;
	}

	public bool IsParticleBlocked(InventoryItem ii)
	{
		if (ii is ChumParticle)
		{
			IChumComponent @base = this.Base0;
			if (@base != null)
			{
				return @base.Ingredient.ItemSubType != ItemSubTypes.ChumGroundbaits;
			}
		}
		return false;
	}

	public bool IsDifferentBases(InventoryItem ii)
	{
		if (ii is ChumBase)
		{
			IChumComponent @base = this.Base0;
			if (@base != null)
			{
				return @base.Ingredient.ItemSubType != ii.ItemSubType;
			}
		}
		return false;
	}

	public bool IsIngredientInChum(ChumIngredient ingredient)
	{
		Chum curChum = this.CurChum;
		return curChum != null && curChum.Ingredients != null && curChum.Ingredients.Any(delegate(ChumIngredient p)
		{
			bool flag2;
			if (p.ItemId == ingredient.ItemId)
			{
				Guid? instanceId = p.InstanceId;
				bool flag = instanceId != null;
				Guid? instanceId2 = ingredient.InstanceId;
				flag2 = flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault());
			}
			else
			{
				flag2 = false;
			}
			return flag2;
		});
	}

	public IChumComponent Base0
	{
		get
		{
			return this._ingredients.FirstOrDefault((IChumComponent p) => p.Type == Types.Base && p.UiType == UiTypes.Component);
		}
	}

	public bool IsBlockedBySnowballs(InventoryItem ii)
	{
		IChumComponent @base = this.Base0;
		return @base != null && ((@base.Ingredient != null && @base.Ingredient.SpecialItem == InventorySpecialItem.Snow && ii.SpecialItem != InventorySpecialItem.Snow) || (@base.Ingredient.SpecialItem != InventorySpecialItem.Snow && ii.SpecialItem == InventorySpecialItem.Snow));
	}

	private void ShowChumMixErrMsgIngredientMax(Types type)
	{
		UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("ChumMixErrMsgIngredientMax"), ScriptLocalization.Get(ChumMixConst.Titles[type]), string.Format("{0}%", ChumMixConst.PrcAdd[type])), true, null, false);
	}

	private void ChumMixErrMsgBasesMin()
	{
		UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), string.Format(ScriptLocalization.Get("ChumMixErrMsgBasesMin"), string.Format("{0}%", ChumMixConst.PrcAdd[Types.Base])), true, null, false);
	}

	private void ShowErrorCantMixDifferentBases()
	{
		UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("ShowErrorCantMixDifferentBases"), true, null, false);
	}

	private void ShowErrorParticlesBlocked()
	{
		UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("ShowErrorParticlesBlocked"), true, null, false);
	}

	private void CutCtrl_OnAccepted(float weight, ChumIngredient ii)
	{
		this.SetWeight(weight, ii, true, true);
	}

	private void SetWeight(float weight, ChumIngredient ii, bool isUpdateChumInfo, bool isChekBase = false)
	{
		Chum curChum = this.CurChum;
		IChumComponent chumComponent = this._ingredients.FirstOrDefault((IChumComponent p) => p.UiType == UiTypes.Component && p.Ingredient.ItemId == ii.ItemId);
		if (chumComponent != null)
		{
			if (weight <= 0f)
			{
				Guid? instanceId = chumComponent.Ingredient.InstanceId;
				bool flag = instanceId != null;
				Guid? instanceId2 = ii.InstanceId;
				if (flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault()))
				{
					this.RemoveChumComponent(chumComponent, true);
				}
			}
			else
			{
				Guid? instanceId3 = chumComponent.Ingredient.InstanceId;
				bool flag2 = instanceId3 != null;
				Guid? instanceId4 = ii.InstanceId;
				if (flag2 != (instanceId4 != null) || (instanceId3 != null && instanceId3.GetValueOrDefault() != instanceId4.GetValueOrDefault()))
				{
					this.RemoveChumComponent(chumComponent, true);
					this.UpdateSria(chumComponent.Ingredient);
					this.SetWeight(weight, ii, isUpdateChumInfo, isChekBase);
				}
				else
				{
					Inventory.AddChumIngredient(curChum, ii, (double)MeasuringSystemManager.Grams2Kilograms(weight));
				}
			}
			if (isChekBase && ii is ChumBase)
			{
				this.CheckBase();
			}
		}
		else if (weight > 0f)
		{
			Inventory.AddChumIngredient(curChum, ii, (double)MeasuringSystemManager.Grams2Kilograms(weight));
			chumComponent = this.AddChumIngredient<ChumComponent>(UiTypes.Component);
			ChumComponent chumComponent2 = (ChumComponent)chumComponent;
			chumComponent2.Init(ChumMixing.GetTypeByClass(ii), ii, this._tglGroupMix);
			chumComponent2.OnChangePrc += this.Ingredient_OnChangePrc;
			chumComponent2.OnDrop2Inventory += this.ChumComp_OnDrop2Inventory;
			chumComponent2.Tgl.GetComponent<ToggleStateChanges>().OnSelected += this.ChumMixing_OnSelected;
			this.SetNavigationX(chumComponent2.Tgl, this._recipes[0].Tgl, null);
			DragMeDollChumComponent component = chumComponent2.GetComponent<DragMeDollChumComponent>();
			component.DranNDropType = this._dropMe.DragNDropTypeInst;
		}
		if (isUpdateChumInfo)
		{
			this.UpdateChumInfo();
		}
		this.UpdateSria(ii);
	}

	private void UpdateSria(ChumIngredient ii)
	{
		MenuPrefabsList menuPrefabsList = MenuHelpers.Instance.MenuPrefabsList;
		if (menuPrefabsList != null)
		{
			global::InventorySRIA.InventorySRIA componentInChildren = menuPrefabsList.inventoryForm.GetComponentInChildren<global::InventorySRIA.InventorySRIA>();
			if (componentInChildren != null)
			{
				componentInChildren.UpdateActivityChumIngredients();
				componentInChildren.SetChumIngredientMixing(this.IsIngredientInChum(ii), ii);
			}
		}
	}

	private void RemoveChumComponent(IChumComponent ingredient, bool removeFromChum = false)
	{
		if (removeFromChum)
		{
			Inventory.RemoveChumIngredient(this.CurChum, ingredient.Ingredient);
		}
		this._ingredients.Remove(ingredient);
		ingredient.Dispose();
	}

	private void SetNavigationX(Toggle tgl, Selectable l, Selectable r)
	{
		Navigation navigation = default(Navigation);
		navigation.mode = 4;
		navigation.selectOnUp = tgl.navigation.selectOnUp;
		navigation.selectOnDown = tgl.navigation.selectOnDown;
		navigation.selectOnLeft = l;
		navigation.selectOnRight = r;
		tgl.navigation = navigation;
	}

	private void ChumMixing_OnSelected(Toggle tgl, bool isSelected)
	{
		if (isSelected)
		{
			ChumResult chumResult = this._recipes.FirstOrDefault((ChumResult p) => p.Tgl == tgl);
			bool flag = chumResult != null;
			if (flag)
			{
				this.UpdateRecipesSelected(chumResult);
			}
			this._navigations[0].enabled = flag;
			this._navigations[1].enabled = !flag;
			List<Toggle> list = this._tglGroupMix.ActiveToggles().ToList<Toggle>();
			list.AddRange(this._tglGroupResult.ActiveToggles());
			list.ForEach(delegate(Toggle t)
			{
				if (t != tgl)
				{
					t.isOn = false;
					t.GetComponent<ToggleStateChanges>().IsActive(false);
				}
			});
			if (tgl != this._tgl && this._tgl.isOn)
			{
				this._tgl.isOn = false;
				this._tgl.GetComponent<ToggleStateChanges>().IsActive(false);
			}
		}
		else
		{
			tgl.GetComponent<ToggleStateChanges>().IsActive(false);
		}
		tgl.GetComponent<HotkeyPressRedirect>().enabled = isSelected;
	}

	private void CheckBase()
	{
		double chumWeight = this.ChumWeight;
		if (chumWeight <= 0.0 || !ChumChecker.IsOk(Types.Base, ChumMixConst.PrcAdd[Types.Base], this.CurChum, -1))
		{
			this._ingredients.Where((IChumComponent p) => p.UiType == UiTypes.Component && (p.Type == Types.Aroma || p.Type == Types.Particle)).ToList<IChumComponent>().ForEach(delegate(IChumComponent p)
			{
				this.SetWeight(0f, p.Ingredient, false, false);
			});
		}
	}

	private void ChumComp_OnDrop2Inventory(ChumIngredient ii)
	{
		this.CutCtrl_OnAccepted(0f, ii);
	}

	private void Ingredient_OnChangePrc(ChumIngredient ii)
	{
		this.Add(ii);
	}

	private static Types GetTypeByClass(ChumIngredient ii)
	{
		return (!(ii is ChumBase)) ? ((!(ii is ChumAroma)) ? Types.Particle : Types.Aroma) : Types.Base;
	}

	private void UpdatePrcIngredients()
	{
		float num = (float)this.ChumWeight;
		if (num <= 0f)
		{
			return;
		}
		this.CurChum.Ingredients.ForEach(delegate(ChumIngredient p)
		{
			IChumComponent chumComponent = this._ingredients.FirstOrDefault((IChumComponent i) => i.UiType == UiTypes.Component && i.Ingredient.ItemId == p.ItemId);
			if (chumComponent != null)
			{
				((ChumComponent)chumComponent).UpdatePrc(p.Percentage, p.Amount);
			}
		});
		this.SortIngredients();
	}

	private void SortIngredients()
	{
		for (int i = 0; i < ChumMixConst.Ingredients.Count; i++)
		{
			Types type = ChumMixConst.Ingredients[i];
			int index = this._ingredients.Find((IChumComponent p) => p.UiType == UiTypes.Title && p.Type == type).GetSiblingIndex();
			this._ingredients.ForEach(delegate(IChumComponent p)
			{
				if (p.Type == type && p.UiType == UiTypes.Component)
				{
					p.SetSiblingIndex(index + 1);
				}
			});
		}
	}

	public void OnMix()
	{
		Chum curChum = this.CurChum;
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		curChum.UpdateIngredients(true);
		if (profile.Inventory.CanMixChum(curChum))
		{
			this.InitMessageBox<ChumMixProgress>(MessageBoxList.Instance.ChumMixingProgressPrefab).Init(curChum, this._transferItemFunc);
		}
		else if (profile.Inventory.LastVerificationError == "Can't mix chum - should be on pond")
		{
			UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("OnPondFailedMesssage"), true, null, false);
		}
		else
		{
			UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), profile.Inventory.LastVerificationError, true, null, false);
		}
	}

	private void OnRename()
	{
		Chum curChum = this.CurChum;
		if (!PhotonConnectionFactory.Instance.Profile.Inventory.CanSaveNewChumRecipe(curChum))
		{
			if (this.ChumWeight <= 0.0 || !ChumChecker.IsOk(Types.Base, ChumMixConst.PrcAdd[Types.Base], this.CurChum, -1))
			{
				this.ChumMixErrMsgBasesMin();
			}
			return;
		}
		ChumRename chumRename = this.InitMessageBox<ChumRename>(MessageBoxList.Instance.RenameChumPrefab);
		chumRename.Init(curChum.Name);
		chumRename.OnRenamed += this.CutCtrl_OnRenamed;
		chumRename.Open();
	}

	private void CutCtrl_OnRenamed(string chumName)
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (string.IsNullOrEmpty(chumName.Trim()))
		{
			UIHelper.ShowMessage(ScriptLocalization.Get("MessageCaption"), ScriptLocalization.Get("ChumMixErrMsgCantSaveChumEmptyName"), true, null, false);
			return;
		}
		Chum curChum = this.CurChum;
		profile.Inventory.RenameChumRecipe(curChum, chumName);
		PhotonConnectionFactory.Instance.ChumRecipeSaveNew(curChum);
	}

	private double ChumWeight
	{
		get
		{
			Chum curChum = this.CurChum;
			double num;
			if (curChum == null || curChum.Ingredients == null)
			{
				num = 0.0;
			}
			else
			{
				num = curChum.Ingredients.SumDouble(delegate(ChumIngredient p)
				{
					double? weight = p.Weight;
					return (weight == null) ? 0.0 : weight.Value;
				});
			}
			return num;
		}
	}

	private void UpdateChumInfo()
	{
		Chum curChum = this.CurChum;
		bool flag = curChum.Ingredients != null && curChum.Ingredients.Count > 0;
		this._emptyIco.gameObject.SetActive(!flag);
		this._newChumIco.gameObject.SetActive(flag);
		this._newChIcoLdbl.Image = this._newChumIco;
		if (flag)
		{
			ChumBase heaviestChumBase = curChum.HeaviestChumBase;
			this._newChIcoLdbl.Load(string.Format("Textures/Inventory/{0}", (heaviestChumBase.DollThumbnailBID == null) ? "0" : heaviestChumBase.DollThumbnailBID.ToString()));
		}
		this.UpdateWater();
		this.UpdatePrcIngredients();
		curChum.UpdateIngredients(true);
		this.UpdateMixBtnInteractable(curChum);
		this.UpdateLevelLocks();
		this.CheckInInventory();
		this.UpdateActiveAllTypes();
	}

	private void UpdateMixBtnInteractable(Chum chum)
	{
		bool flag = PhotonConnectionFactory.Instance.Profile.Inventory.CanMixChum(chum);
		this._buttonMix.interactable = (flag || PhotonConnectionFactory.Instance.Profile.Inventory.LastVerificationError == "Can't mix chum - should be on pond") && InventoryInit.Instance.SRIA.gameObject.activeInHierarchy;
	}

	private void UpdateButtonsInteractability()
	{
		bool activeInHierarchy = InventoryInit.Instance.SRIA.gameObject.activeInHierarchy;
		this._buttonRename.interactable = activeInHierarchy;
		this._buttonRenameIco.interactable = activeInHierarchy;
		this._buttonClear.interactable = activeInHierarchy;
		this.UpdateMixBtnInteractable(this.CurChum);
		this.InitProduct();
	}

	private void InventoryUpdated()
	{
		this.CheckInInventory();
		this.UpdateMixBtnInteractable(this.CurChum);
		this.CurChum.Ingredients.ForEach(new Action<ChumIngredient>(this.UpdateSria));
	}

	private void CheckInInventory()
	{
		List<ChumIngredient> inventory = (from p in PhotonConnectionFactory.Instance.Profile.Inventory.OfType<ChumIngredient>()
			where p.Storage == StoragePlaces.Equipment
			select p).ToList<ChumIngredient>();
		this._ingredients.ForEach(delegate(IChumComponent p)
		{
			if (p.UiType == UiTypes.Component && p.Type != Types.Water)
			{
				IEnumerable<ChumIngredient> enumerable = inventory.Where(delegate(ChumIngredient i)
				{
					if (i.ItemId == p.Ingredient.ItemId && i.ItemSubType == p.Ingredient.ItemSubType)
					{
						Guid? instanceId = i.InstanceId;
						bool flag = instanceId != null;
						Guid? instanceId2 = p.Ingredient.InstanceId;
						if (flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault()))
						{
							return i.Amount >= p.Ingredient.Amount;
						}
					}
					return false;
				});
				p.HasInInventory(enumerable.Any<ChumIngredient>());
			}
		});
	}

	private void UpdateWater()
	{
		double chumWeight = this.ChumWeight;
		double num = ((!this.IsNeededWater(this.Base0)) ? 0.0 : Math.Round(chumWeight / 2.0, 2, MidpointRounding.AwayFromZero));
		double num2 = chumWeight + num;
		this._newChumWeight.text = string.Format("{0} {1}", MeasuringSystemManager.Kilograms2Grams((float)num2), MeasuringSystemManager.GramsOzWeightSufix());
		ChumComponentWater chumComponentWater = (ChumComponentWater)this._ingredients.Find((IChumComponent p) => p.UiType == UiTypes.Water);
		chumComponentWater.UpdateValue((float)num);
	}

	private void _dropMe_OnAction(InventoryItem ii, DropMeChum.States state)
	{
		ChumIngredient chumIngredient = ii as ChumIngredient;
		if (chumIngredient != null)
		{
			Types type = ChumMixing.GetTypeByClass(chumIngredient);
			ChumComponentEmpty chumComponentEmpty = this._ingredients.Find((IChumComponent p) => p.Type == type && p.UiType == UiTypes.Empty) as ChumComponentEmpty;
			if (chumComponentEmpty != null)
			{
				bool flag = state == DropMeChum.States.PointerEntered;
				chumComponentEmpty.SetDrag(flag);
				if (flag)
				{
					int siblingIndex = chumComponentEmpty.GetSiblingIndex();
					float num = (float)siblingIndex / (float)this._chumMixContainer.transform.childCount;
					if (this._chumMixScrollbar.direction == 2)
					{
						num = 1f - num;
					}
					base.StartCoroutine(this.Scroll(this._chumMixScrollbar, num));
				}
			}
			if (state == DropMeChum.States.Droped)
			{
				this.Add(ii);
			}
		}
	}

	protected IEnumerator Scroll(Scrollbar sb, float v)
	{
		yield return new WaitForEndOfFrame();
		sb.value = v;
		yield break;
	}

	private T AddChumIngredient<T>(UiTypes uiType) where T : IChumComponent
	{
		GameObject gameObject = GUITools.AddChild(this._chumMixContainer.gameObject, this._uiTypePrefabsList.Find((ChumMixing.UiTypePrefabs p) => p.Type == uiType).Prefab);
		ChumMixing.InitPos(gameObject);
		T component = gameObject.GetComponent<T>();
		this._ingredients.Add(component);
		return component;
	}

	private void InitRecipes()
	{
		PhotonConnectionFactory.Instance.Profile.ChumRecipes.ForEach(delegate(Chum recipe)
		{
			this.AddRecipe(recipe, false);
		});
		this.InitSlots();
	}

	private void ClearCurrentChum()
	{
		List<IChumComponent> list = this._ingredients.Where((IChumComponent p) => p.UiType == UiTypes.Component).ToList<IChumComponent>();
		for (int i = 0; i < list.Count; i++)
		{
			this.RemoveChumComponent(list[i], false);
		}
		list.ForEach(delegate(IChumComponent p)
		{
			this.UpdateSria(p.Ingredient);
		});
	}

	private void SetCurrentChum(ChumResult chumRes)
	{
		this.ClearCurrentChum();
		if (chumRes.ChumObj.Ingredients != null)
		{
			for (int i = 0; i < chumRes.ChumObj.Ingredients.Count; i++)
			{
				ChumIngredient chumIngredient = chumRes.ChumObj.Ingredients[i];
				this.SetWeight(MeasuringSystemManager.Kilograms2Grams(chumIngredient.Amount), chumIngredient, false, true);
				this.UpdateSria(chumIngredient);
			}
		}
		this._newChumName.text = ChumMixing.ChumNameCorrection(chumRes.ChumObj.Name);
		this.UpdateChumInfo();
	}

	private void Instance_OnChumRecipeSaveNew(Chum chum)
	{
		this._recipes.Find((ChumResult p) => p.State == ChumResult.Events.Select).Init(chum, false, this._tglGroupResult, ChumResult.Events.Select);
		this._newChumName.text = ChumMixing.ChumNameCorrection(chum.Name);
		GameFactory.Message.ShowMessage(ScriptLocalization.Get("RecipeSaveOkCaption"), null, 3f, false);
		this.CurChum.Ingredients.ForEach(new Action<ChumIngredient>(this.UpdateSria));
	}

	public static string ChumNameCorrection(string chumName)
	{
		return (chumName.Length < 16) ? chumName : (chumName.Substring(0, 12) + "...");
	}

	private void AddRecipe(Chum chum, bool isNew)
	{
		GameObject gameObject = GUITools.AddChild(this._chumResultContainer.gameObject, this._chumResultPrefab);
		ChumMixing.InitPos(gameObject);
		ChumResult component = gameObject.GetComponent<ChumResult>();
		component.name += this.jjj++;
		component.Init(chum, isNew, this._tglGroupResult, ChumResult.Events.None);
		component.OnAction += this.RecipeNew_OnAction;
		component.Tgl.GetComponent<ToggleStateChanges>().OnSelected += this.ChumMixing_OnSelected;
		this.SetNavigationX(component.Tgl, null, this._tgl);
		this._recipes.Add(component);
		if (!this._isTglNavigationInited)
		{
			this._isTglNavigationInited = true;
			this.SetNavigationX(this._tgl, this._recipes[0].Tgl, null);
			this._navigations[1].SetBindingSelectable(UINavigation.Bindings.Left, this._recipes[0].Tgl);
		}
	}

	private void RecipeNew_OnAction(ChumResult.Events ev, ChumResult o)
	{
		if (ev != ChumResult.Events.Select)
		{
			if (ev == ChumResult.Events.Delete)
			{
				PhotonConnectionFactory.Instance.ChumRecipeRemove(o.ChumObj);
			}
		}
		else
		{
			this.UpdateRecipesSelected(o);
		}
	}

	private void UpdateRecipesSelected(ChumResult o)
	{
		ChumResult chumResult = this._recipes.FirstOrDefault((ChumResult p) => p.State == ChumResult.Events.Select);
		if (chumResult != null)
		{
			if (o != null && o.Equals(chumResult))
			{
				return;
			}
			chumResult.SetState(ChumResult.Events.None);
		}
		o = o ?? this._recipes[0];
		o.SetState(ChumResult.Events.Select);
		this.SetCurrentChum(o);
	}

	private void Instance_OnChumRecipeRemove(Chum chum)
	{
		ChumResult chumResult = this._recipes.FirstOrDefault(delegate(ChumResult p)
		{
			Guid? instanceId = p.ChumObj.InstanceId;
			bool flag = instanceId != null;
			Guid? instanceId2 = chum.InstanceId;
			return flag == (instanceId2 != null) && (instanceId == null || instanceId.GetValueOrDefault() == instanceId2.GetValueOrDefault());
		});
		if (chumResult != null)
		{
			chumResult.Init(this.CreateEmptyRecipe(), true, this._tglGroupResult, chumResult.State);
			if (chumResult.State == ChumResult.Events.Select)
			{
				this.SetCurrentChum(chumResult);
			}
		}
	}

	private void Instance_OnChumRecipeRename(Chum chum)
	{
	}

	private void Instance_OnChumRecipeRemoveFailure(Failure failure)
	{
		Debug.LogError(failure.ErrorMessage);
	}

	private void Instance_OnChumRecipeSaveNewFailure(Failure failure)
	{
		Debug.LogError(failure.ErrorMessage);
	}

	private void Instance_OnChumRecipeRenameFailure(Failure failure)
	{
		Debug.LogError(failure.ErrorMessage);
	}

	private string GetUpToPrcText(Types type)
	{
		if (!ChumMixConst.PrcAdd.ContainsKey(type) || ChumMixConst.PrcAdd[type] <= 0f || type == Types.Base)
		{
			return string.Empty;
		}
		return string.Format(ScriptLocalization.Get("UpToCaption"), ChumMixConst.PrcAdd[type]) + "%";
	}

	private void Clear()
	{
		Inventory.ClearChumIngredients(this.CurChum);
		this.ClearCurrentChum();
		this.UpdateChumInfo();
	}

	private void Instance_OnActivate(InfoMessageTypes type, bool isActive)
	{
		if (type == InfoMessageTypes.LevelUp && !isActive)
		{
			this.UpdateLevelDependencies();
			this.UpdateLevelLocks();
		}
	}

	private void UpdateLevelDependencies()
	{
		List<Types> list = this._slotsForLevel.Keys.ToList<Types>();
		for (int i = 0; i < list.Count; i++)
		{
			this._nextLevelUnlock[list[i]] = ChumMixConst.SlotsLevelsFuncs[list[i]](PhotonConnectionFactory.Instance.Profile.Level);
		}
		int num;
		int num2;
		int num3;
		Inventory.GetChumSlotsForLevel(PhotonConnectionFactory.Instance.Profile.Level, out num, out num2, out num3);
		this._slotsForLevel[Types.Base] = num;
		this._slotsForLevel[Types.Aroma] = num2;
		this._slotsForLevel[Types.Particle] = num3;
	}

	private void UpdateLevelLocks()
	{
		Dictionary<Types, bool> blockedSlots = new Dictionary<Types, bool>();
		Dictionary<Types, int> componentsCount = this.ComponentsCount();
		List<Types> list = this._slotsForLevel.Keys.ToList<Types>();
		List<IChumComponent> list2 = this._ingredients.Where((IChumComponent p) => componentsCount.ContainsKey(p.Type)).ToList<IChumComponent>();
		for (int i = 0; i < list.Count; i++)
		{
			blockedSlots[list[i]] = componentsCount[list[i]] >= this._slotsForLevel[list[i]];
		}
		IChumComponent @base = this.Base0;
		List<Types> blockedSnowballsSlots = new List<Types>();
		if (@base != null && @base.Ingredient != null && @base.Ingredient.SpecialItem == InventorySpecialItem.Snow)
		{
			blockedSnowballsSlots.Add(Types.Aroma);
			blockedSnowballsSlots.Add(Types.Particle);
		}
		list2.ForEach(delegate(IChumComponent p)
		{
			if (this._nextLevelUnlock[p.Type] > 0)
			{
				bool flag = componentsCount[p.Type] == 0;
				if (p.UiType == UiTypes.Empty)
				{
					ChumComponentEmpty chumComponentEmpty = (ChumComponentEmpty)p;
					chumComponentEmpty.SetBlock(blockedSlots[p.Type], flag);
					chumComponentEmpty.SetEnable(!blockedSlots[p.Type] && !blockedSnowballsSlots.Contains(p.Type));
					if (blockedSlots[p.Type])
					{
						chumComponentEmpty.SetBlockText(string.Format(ScriptLocalization.Get("UnlockedAtLevelCaption"), this._nextLevelUnlock[p.Type]));
					}
				}
				else if (p.UiType == UiTypes.Title)
				{
					((ChumComponentEmpty)p).SetEnable((!blockedSlots[p.Type] || !flag) && !blockedSnowballsSlots.Contains(p.Type));
				}
			}
			else if (p.UiType == UiTypes.Empty && blockedSlots[p.Type])
			{
				this._ingredients.Remove(p);
				p.Dispose();
			}
		});
		list.Where((Types t) => !blockedSlots[t] && !this._ingredients.Any((IChumComponent p) => p.Type == t && p.UiType == UiTypes.Empty)).ToList<Types>().ForEach(delegate(Types t)
		{
			ChumComponentEmpty chumComponentEmpty2 = this.AddChumIngredient<ChumComponentEmpty>(UiTypes.Empty);
			chumComponentEmpty2.Init(t, UiTypes.Empty, this._emptiesList.Find((ChumMixing.IngredientData p) => p.Type == t).Sp);
			int siblingIndex = (from p in this._ingredients
				where p.Type == t && (p.UiType == UiTypes.Component || p.UiType == UiTypes.Title)
				orderby p.GetSiblingIndex() descending
				select p).ToList<IChumComponent>()[0].GetSiblingIndex();
			chumComponentEmpty2.SetSiblingIndex(siblingIndex + 1);
		});
		bool baseOk = this.ChumWeight > 0.0 && ChumChecker.IsOk(Types.Base, ChumMixConst.PrcAdd[Types.Base], this.CurChum, -1);
		this._ingredients.ForEach(delegate(IChumComponent p)
		{
			if ((p.Type == Types.Aroma || p.Type == Types.Particle) && (p.UiType == UiTypes.Empty || p.UiType == UiTypes.Title))
			{
				bool flag2 = !blockedSlots[p.Type];
				bool flag3 = componentsCount[p.Type] == 0;
				if (p.UiType == UiTypes.Title)
				{
					flag2 = flag2 || !flag3;
				}
				if (flag2)
				{
					p.SetEnable(baseOk && !blockedSnowballsSlots.Contains(p.Type));
				}
			}
		});
	}

	private Dictionary<Types, int> ComponentsCount()
	{
		Dictionary<Types, int> componentsCount = this.GetEmptyInt;
		this._ingredients.Where((IChumComponent p) => componentsCount.ContainsKey(p.Type)).ToList<IChumComponent>().ForEach(delegate(IChumComponent p)
		{
			if (p.UiType == UiTypes.Component)
			{
				Dictionary<Types, int> componentsCount2;
				Types type;
				(componentsCount2 = componentsCount)[type = p.Type] = componentsCount2[type] + 1;
			}
		});
		return componentsCount;
	}

	private void UpdateActiveAllTypes()
	{
		IChumComponent @base = this.Base0;
		this.SetActiveType(Types.Particle, @base == null || @base.Ingredient.ItemSubType == ItemSubTypes.ChumGroundbaits);
		this.SetActiveType(Types.Water, this.IsNeededWater(@base));
	}

	private bool IsNeededWater(IChumComponent c)
	{
		return c != null && (c.Ingredient.ItemSubType == ItemSubTypes.ChumGroundbaits || c.Ingredient.ItemSubType == ItemSubTypes.ChumMethodMix);
	}

	private void SetActiveType(Types type, bool flag)
	{
		this._ingredients.Where((IChumComponent p) => p.Type == type).ToList<IChumComponent>().ForEach(delegate(IChumComponent p)
		{
			p.SetActive(flag);
		});
	}

	private bool IsBlocked(InventoryItem ii)
	{
		Types type = ChumMixing.GetTypeByClass(ii as ChumIngredient);
		return this._ingredients.Count((IChumComponent p) => p.Type == type && p.UiType == UiTypes.Component && p.Ingredient.ItemId != ii.ItemId) >= this._slotsForLevel[type];
	}

	private Chum CurChum
	{
		get
		{
			ChumResult chumResult = this._recipes.FirstOrDefault((ChumResult p) => p.State == ChumResult.Events.Select);
			return (!(chumResult != null)) ? null : chumResult.ChumObj;
		}
	}

	private void InitProduct()
	{
		this._productChumRecipe = CacheLibrary.ProductCache.Products.FirstOrDefault((StoreProduct x) => x.TypeId == 8);
		if (this._productChumRecipe != null && InventoryInit.Instance.SRIA.gameObject.activeInHierarchy)
		{
			string currencyIcon = MeasuringSystemManager.GetCurrencyIcon(this._productChumRecipe.ProductCurrency);
			float num = (float)this._productChumRecipe.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow);
			this._buttonAddSlot.transform.Find("LabelMoney").GetComponent<Text>().text = string.Format("{0}{1}", currencyIcon, num);
			this._buttonAddSlot.interactable = true;
		}
		else
		{
			this._buttonAddSlot.interactable = false;
			if (this._productChumRecipe == null)
			{
				Debug.LogWarning("ChumMixing:InitSlots - can't found product ChumRecipe");
			}
		}
	}

	private void InitSlots()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		int currentChumRecipesCapacity = profile.Inventory.CurrentChumRecipesCapacity;
		this.Instance_OnProductBought(new ProfileProduct
		{
			TypeId = 8
		}, currentChumRecipesCapacity - profile.ChumRecipes.Count);
		this.UpdateRecipesSelected(null);
	}

	public void AddSlot()
	{
		this._messageBox = GUITools.AddChild(InfoMessageController.Instance.gameObject, MessageBoxList.Instance.MessageBoxWithCurrencyPrefab);
		ChumMixing.InitPos(this._messageBox);
		this._messageBox.GetComponent<MessageBoxSellItem>().Init(ScriptLocalization.Get("RodPresetBuySlotConfirm"), this._productChumRecipe.ProductCurrency, ((float)this._productChumRecipe.GetPrice(PhotonConnectionFactory.Instance.ServerUtcNow)).ToString(CultureInfo.InvariantCulture));
		EventConfirmAction component = this._messageBox.GetComponent<EventConfirmAction>();
		component.ConfirmActionCalled += delegate(object sender, EventArgs e)
		{
			this._buyClick.BuyProduct(this._productChumRecipe);
			if (this._messageBox != null)
			{
				this._messageBox.GetComponent<MessageBox>().Close();
			}
		};
		component.CancelActionCalled += delegate(object sender, EventArgs e)
		{
			if (this._messageBox != null)
			{
				this._messageBox.GetComponent<MessageBox>().Close();
			}
		};
		this._messageBox.GetComponent<MessageBox>().Init(string.Empty, ScriptLocalization.Get("YesCaption"), ScriptLocalization.Get("NoCaption"), true);
	}

	private void Instance_OnBuyProductFailed(Failure failure)
	{
	}

	private void Instance_OnProductDelivered(ProfileProduct product, int count, bool announce)
	{
		if (product.ChumRecipesExt != null)
		{
			Profile profile = PhotonConnectionFactory.Instance.Profile;
			int num = profile.Inventory.CurrentChumRecipesCapacity - this._recipes.Count;
			if (num > 0)
			{
				for (int i = 0; i < num; i++)
				{
					this.AddRecipe(this.CreateEmptyRecipe(), true);
				}
			}
		}
	}

	private void Instance_OnProductBought(ProfileProduct product, int count)
	{
		if (product.TypeId == 8)
		{
			for (int i = 0; i < count; i++)
			{
				this.AddRecipe(this.CreateEmptyRecipe(), true);
			}
		}
	}

	private Chum CreateEmptyRecipe()
	{
		Chum chum = new Chum();
		PhotonConnectionFactory.Instance.Profile.Inventory.RenameChumRecipe(chum, string.Format("{0}", ScriptLocalization.Get("EmptyRecipeCaption")));
		return chum;
	}

	private T InitMessageBox<T>(GameObject prefab)
	{
		this._messageBox = GUITools.AddChild(MessageBoxList.Instance.gameObject, prefab);
		ChumMixing.InitPos(this._messageBox);
		return this._messageBox.GetComponent<T>();
	}

	private static void InitPos(GameObject o)
	{
		RectTransform component = o.GetComponent<RectTransform>();
		component.anchoredPosition = Vector3.zero;
		component.sizeDelta = Vector2.zero;
	}

	[SerializeField]
	private ToggleGroup _tglGroupResult;

	[SerializeField]
	private ToggleGroup _tglGroupMix;

	[SerializeField]
	private Toggle _tgl;

	[SerializeField]
	private BuyClick _buyClick;

	[SerializeField]
	private DropMeChum _dropMe;

	[SerializeField]
	private Scrollbar _chumMixScrollbar;

	[SerializeField]
	private GameObject _chumResultPrefab;

	[SerializeField]
	private Transform _chumResultContainer;

	[SerializeField]
	private Transform _chumMixContainer;

	[Space(8f)]
	[SerializeField]
	private BorderedButton _buttonMix;

	[SerializeField]
	private BorderedButton _buttonRename;

	[SerializeField]
	private BorderedButton _buttonRenameIco;

	[SerializeField]
	private BorderedButton _buttonClear;

	[SerializeField]
	private BorderedButton _buttonAddSlot;

	[Space(5f)]
	[SerializeField]
	private Text _newChumName;

	[SerializeField]
	private Text _newChumWeight;

	[SerializeField]
	private Image _newChumIco;

	private ResourcesHelpers.AsyncLoadableImage _newChIcoLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	private Image _emptyIco;

	[SerializeField]
	private ChumMixing.IngredientData[] _empties;

	private List<ChumMixing.IngredientData> _emptiesList;

	[SerializeField]
	private ChumMixing.UiTypePrefabs[] _uiTypePrefabs;

	private List<ChumMixing.UiTypePrefabs> _uiTypePrefabsList;

	private GameObject _messageBox;

	private Func<InventoryItem, InventoryItem, bool> _transferItemFunc;

	private List<IChumComponent> _ingredients = new List<IChumComponent>();

	private List<ChumResult> _recipes = new List<ChumResult>();

	private Dictionary<Types, int> _slotsForLevel;

	private Dictionary<Types, int> _nextLevelUnlock;

	private StoreProduct _productChumRecipe;

	private bool _isTglNavigationInited;

	private UINavigation[] _navigations;

	private int jjj;

	[Serializable]
	public class IngredientData
	{
		public Types Type;

		public Sprite Sp;
	}

	[Serializable]
	public class UiTypePrefabs
	{
		public UiTypes Type;

		public GameObject Prefab;
	}
}
