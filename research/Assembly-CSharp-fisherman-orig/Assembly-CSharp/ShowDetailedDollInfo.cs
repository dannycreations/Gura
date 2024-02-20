using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ShowDetailedDollInfo : MonoBehaviour
{
	internal void Awake()
	{
		this.sr = base.GetComponent<ScrollRect>();
		this._entries = new List<DollInfoEntry>(this.Parent.GetComponentsInChildren<DollInfoEntry>());
		if (this.ActiveRod != null)
		{
			this.ActiveRod.OutfitOrRodOrChumMixingSwitched += this.Refresh;
		}
		if (this.ActiveRod != null && !this.ActiveRod.IsOtherPlayer)
		{
			ShowDetailedDollInfo.Instance = this;
			PhotonConnectionFactory.Instance.OnInventoryUpdated += this.Refresh;
			PhotonConnectionFactory.Instance.OnActiveQuiverTipSet += this.Refresh;
			PhotonConnectionFactory.Instance.OnSetActiveQuiverTipFailed += this.OnFailure;
		}
	}

	private void OnFailure(Failure failure)
	{
		Debug.Log(failure.ErrorMessage);
		this.Refresh();
	}

	internal void OnDestroy()
	{
		if (this.ActiveRod != null)
		{
			this.ActiveRod.OutfitOrRodOrChumMixingSwitched -= this.Refresh;
		}
		if (this.ActiveRod != null && !this.ActiveRod.IsOtherPlayer)
		{
			PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.Refresh;
			PhotonConnectionFactory.Instance.OnActiveQuiverTipSet -= this.Refresh;
			PhotonConnectionFactory.Instance.OnSetActiveQuiverTipFailed -= this.OnFailure;
		}
	}

	public void Clear()
	{
		foreach (DollInfoEntry dollInfoEntry in this._entries)
		{
			dollInfoEntry.gameObject.SetActive(false);
		}
		this.free.Clear();
		this.free.AddRange(this._entries);
		this.lastHighlighted = null;
		this.rod = null;
		this.reel = null;
	}

	public void OnEnable()
	{
		this.Refresh();
	}

	public DollInfoEntry GetNext()
	{
		DollInfoEntry dollInfoEntry;
		if (this.free.Count == 0)
		{
			dollInfoEntry = Object.Instantiate<DollInfoEntry>(this.Prefab, this.Parent.transform);
			this._entries.Add(dollInfoEntry);
		}
		else
		{
			dollInfoEntry = this.free[0];
			this.free.RemoveAt(0);
		}
		dollInfoEntry.transform.SetAsLastSibling();
		return dollInfoEntry;
	}

	public void Refresh()
	{
		base.Invoke("PerformRefresh", 0.1f);
	}

	private void PerformRefresh()
	{
		this.Clear();
		InitRod activeRod = this.ActiveRod.ActiveRod;
		if (activeRod != null && activeRod.Rod != null && activeRod.Rod.gameObject.activeInHierarchy)
		{
			this.Set(activeRod.Bell);
			this.Set(activeRod.Rod);
			this.Set(activeRod.Reel);
			this.Set(activeRod.Line);
			this.Set(activeRod.PVASinker);
			this.Set(activeRod.Feeder);
			this.Set(activeRod.SpodFeeder);
			this.Set(activeRod.Chum);
			this.Set(activeRod.SpodChumAdditional);
			this.Set(activeRod.SpinningSinker);
			this.Set(activeRod.Leader);
			this.Set(activeRod.Tackle);
			this.Set(activeRod.LureHook);
			this.Set(activeRod.Bait);
		}
		else if (this.ActiveRod.BodyToggle.Toggle.isOn)
		{
			this.Set(this.ActiveOutfit.Hat);
			this.Set(this.ActiveOutfit.RodCase);
			this.Set(this.ActiveOutfit.Coat);
			this.Set(this.ActiveOutfit.TackleBox);
			this.Set(this.ActiveOutfit.FishKeepnet);
			this.Set(this.ActiveOutfit.RodStand);
			this.Set(this.ActiveOutfit.Kayak);
			this.Set(this.ActiveOutfit.Chum);
		}
		this.LastLine.transform.SetAsLastSibling();
		this.LastLine.gameObject.SetActive(this.free.Count != this._entries.Count);
	}

	private void Set(InventoryItemComponent iic)
	{
		if (iic != null && iic.InventoryItem != null)
		{
			FeederRod feederRod = iic.InventoryItem as FeederRod;
			if (feederRod != null && feederRod.QuiverId != null)
			{
				QuiverTip quiverTip = new List<QuiverTip>(feederRod.QuiverTips).FirstOrDefault((QuiverTip x) => x.ItemId == feederRod.QuiverId);
				if (quiverTip != null)
				{
					if (MeasuringSystemManager.CurrentMeasuringSystem != MeasuringSystem.Imperial)
					{
						this.GetNext().Set(feederRod.ItemSubType, string.Format("{0} {1} {2} ( {3} {4} )", new object[]
						{
							ScriptLocalization.Get("QuiverTipLabel"),
							Mathf.Round(10f * MeasuringSystemManager.Kilograms2Oz(quiverTip.Test)) / 10f,
							"Oz",
							Mathf.Round(10f * MeasuringSystemManager.Kilograms2Grams(quiverTip.Test)) / 10f,
							MeasuringSystemManager.GramsOzWeightSufix()
						}), string.Empty);
					}
					else
					{
						this.GetNext().Set(feederRod.ItemSubType, string.Format("{0} {1} {2}", ScriptLocalization.Get("QuiverTipLabel"), Mathf.Round(10f * MeasuringSystemManager.Kilograms2Oz(quiverTip.Test)) / 10f, "Oz"), string.Empty);
					}
				}
			}
			DollInfoEntry next = this.GetNext();
			bool flag = false;
			if (iic.InventoryItem.ItemType == ItemTypes.Rod)
			{
				this.rod = next;
				flag = true;
			}
			if (iic.InventoryItem.ItemType == ItemTypes.Reel)
			{
				this.reel = next;
				flag = true;
			}
			next.Set(iic.InventoryItem, flag);
		}
	}

	public void Highlight(InventoryItemComponent iic, bool highlight)
	{
		DollInfoEntry dollInfoEntry = this._entries.FirstOrDefault((DollInfoEntry x) => x.Item == iic.InventoryItem && x.Item != null);
		if (dollInfoEntry != null)
		{
			if (this.lastHighlighted != null && this.lastHighlighted != dollInfoEntry && !highlight)
			{
				this.lastHighlighted.Set(this.lastHighlighted.Item, false);
			}
			this.lastHighlighted = ((!highlight) ? null : dollInfoEntry);
			dollInfoEntry.Set(iic.InventoryItem, highlight);
			if (!highlight)
			{
				this.PerformRefresh();
			}
			else
			{
				if (this.rod != null && this.rod != dollInfoEntry)
				{
					this.rod.Set(this.rod.Item, false);
				}
				if (this.reel != null && this.reel != dollInfoEntry)
				{
					this.reel.Set(this.reel.Item, false);
				}
			}
		}
	}

	public DollInfoEntry Prefab;

	public GameObject Parent;

	public GameObject LastLine;

	private List<DollInfoEntry> _entries;

	private List<DollInfoEntry> free = new List<DollInfoEntry>();

	public InitRods ActiveRod;

	public InitOutfit ActiveOutfit;

	public static ShowDetailedDollInfo Instance;

	private ScrollRect sr;

	private DollInfoEntry lastHighlighted;

	private DollInfoEntry rod;

	private DollInfoEntry reel;

	public class TypeIconBinding
	{
		public string icon;

		public ItemSubTypes type;
	}
}
