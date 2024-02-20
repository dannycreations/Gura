using System;
using System.Linq;
using Assets.Scripts.UI._2D.Inventory;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemComponent : MonoBehaviour
{
	public void Init(InventoryItem item, StoragePlaces places, InitRod activeRod)
	{
		this._activeRod = activeRod;
		Chum chum = item as Chum;
		bool flag = chum != null;
		this._damageIcoChum.gameObject.SetActive(flag);
		this._damageIco.gameObject.SetActive(!flag);
		if (flag)
		{
			this._damageIcoChum.Init(chum);
		}
		else
		{
			this._damageIco.Init(item);
		}
		bool flag2 = item is ChumIngredient;
		this.InventoryItem = item;
		this.Storage = places;
		if (flag || flag2)
		{
			if (flag && chum.IsExpired)
			{
				this.Name.text = string.Format("{0}: {1}", UgcConsts.GetYellowTan(ScriptLocalization.Get("ChumExpiredCaption")), this.InventoryItem.Name);
			}
			else
			{
				this.Name.text = string.Format("{0} : {1} {2}", this.InventoryItem.Name, Math.Round((double)MeasuringSystemManager.FishWeight(this.InventoryItem.Amount), 3, MidpointRounding.AwayFromZero), MeasuringSystemManager.FishWeightSufix());
			}
		}
		else
		{
			this.Name.text = this.InventoryItem.Name;
		}
		this.Description.text = InventoryParamsHelper.ParseParamsInfo(this.InventoryItem, false);
		this.UpdateActivity(activeRod);
		this.Info.group = base.GetComponent<ToggleGroup>();
		string text = InventoryHelper.ItemCountStr(this.InventoryItem);
		this.Count.GetComponent<Text>().text = text;
		this.Count.gameObject.SetActive(!string.IsNullOrEmpty(text));
		bool flag3 = false;
		if (flag2 && ChumMixing.Instance != null)
		{
			flag3 = ChumMixing.Instance.IsIngredientInChum((ChumIngredient)this.InventoryItem);
		}
		this.UpdateMixingState(flag3);
	}

	public void UpdateActivity(InitRod activeRod)
	{
		InventoryItem inventoryItem;
		if (activeRod.Rod != null && activeRod.Rod.InventoryItem != null)
		{
			inventoryItem = activeRod.Rod.InventoryItem;
		}
		else
		{
			inventoryItem = PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => (x.Storage == StoragePlaces.Doll || x.Storage == StoragePlaces.Hands) && x.ItemType == ItemTypes.Rod && x.Slot == activeRod.SlotId);
		}
		this.Blocked.gameObject.SetActive(InventoryHelper.IsBlocked2Equip(inventoryItem, this.InventoryItem, InitRods.Instance.BodyView.gameObject.activeInHierarchy));
	}

	public virtual void Set(InventoryItem inventoryItem, bool shouldCallChangeHandler = true)
	{
		this.InventoryItem = inventoryItem;
		if (this.ChangeHandler != null && shouldCallChangeHandler)
		{
			this.ChangeHandler.OnChange();
		}
	}

	public void ToEquipment()
	{
		PhotonConnectionFactory.Instance.MoveItemOrCombine(this.InventoryItem, null, StoragePlaces.Equipment, true);
	}

	public void ToCar()
	{
		PhotonConnectionFactory.Instance.MoveItemOrCombine(this.InventoryItem, null, StoragePlaces.CarEquipment, true);
	}

	public void DeleteItem()
	{
		PhotonConnectionFactory.Instance.DestroyItem(this.InventoryItem);
	}

	public void ToStorage()
	{
		PhotonConnectionFactory.Instance.MoveItemOrCombine(this.InventoryItem, null, StoragePlaces.Storage, true);
	}

	public void ItemSelected()
	{
		this.OnSelected();
	}

	public void UpdateMixingState(bool isChuming)
	{
		isChuming = isChuming && InitRods.Instance != null && InitRods.Instance.IsChumMixing;
		this.Mixing.gameObject.SetActive(isChuming);
		this.ImageCanvasGroup.alpha = ((!isChuming) ? 1f : 0.24f);
	}

	public void UpdateActivity()
	{
		if (this._activeRod != null)
		{
			this.UpdateActivity(this._activeRod);
		}
	}

	[SerializeField]
	protected CanvasGroup ImageCanvasGroup;

	[SerializeField]
	protected TextMeshProUGUI Mixing;

	[SerializeField]
	protected DamageIconManagerChum _damageIcoChum;

	[SerializeField]
	protected DamageIconManager _damageIco;

	[HideInInspector]
	public InventoryItem InventoryItem;

	[HideInInspector]
	public ChangeHandler ChangeHandler;

	[HideInInspector]
	public StoragePlaces Storage;

	public Text Name;

	public Text Description;

	public Toggle Info;

	public Text Count;

	public Image Blocked;

	private const float MixingImageAlpha = 0.24f;

	private InitRod _activeRod;

	public Action OnSelected = delegate
	{
	};
}
