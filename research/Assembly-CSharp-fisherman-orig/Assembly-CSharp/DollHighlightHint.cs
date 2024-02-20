using System;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine.UI;

public class DollHighlightHint : ManagedHintObject, IItemUpdated
{
	public bool IsItemUpdate { get; set; }

	private void Awake()
	{
		PhotonConnectionFactory.Instance.OnInventoryUpdated += this.OnInventoryUpdated;
		InitRods.Instance.ChangedActiveRod += this.OnInventoryUpdated;
	}

	private void OnInventoryUpdated()
	{
		this.Init(this._type);
	}

	public void Init(ItemSubTypes type)
	{
		this._type = type;
		this.IsItemUpdate = false;
		string text = "dot-contour-line@1x";
		this.typeAssigned = 2;
		switch (type)
		{
		case ItemSubTypes.All:
			text = "dot-contour-line@1x";
			this.typeAssigned = 2;
			break;
		case ItemSubTypes.Rod:
		case ItemSubTypes.TelescopicRod:
		case ItemSubTypes.MatchRod:
		case ItemSubTypes.SpinningRod:
		case ItemSubTypes.CastingRod:
		case ItemSubTypes.FeederRod:
		case ItemSubTypes.BottomRod:
		case ItemSubTypes.FlyRod:
		case ItemSubTypes.CarpRod:
		case ItemSubTypes.SpodRod:
			if (InitRods.Instance.ActiveRod.Rod.InventoryItem == null)
			{
				text = "PHRod-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Line:
		case ItemSubTypes.MonoLine:
		case ItemSubTypes.BraidLine:
		case ItemSubTypes.FlurLine:
			if (InitRods.Instance.ActiveRod.Line.InventoryItem == null)
			{
				text = "PHLine-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Hook:
		case ItemSubTypes.Bait:
		case ItemSubTypes.Lure:
		case ItemSubTypes.JigBait:
		case ItemSubTypes.FreshBait:
		case ItemSubTypes.CommonBait:
		case ItemSubTypes.BoilBait:
		case ItemSubTypes.InsectsWormBait:
		case ItemSubTypes.Worm:
		case ItemSubTypes.Grub:
		case ItemSubTypes.Shad:
		case ItemSubTypes.Tube:
		case ItemSubTypes.Craw:
			if (InitRods.Instance.ActiveRod.Bait.InventoryItem == null)
			{
				text = "PHBait-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Bobber:
			if (InitRods.Instance.ActiveRod.Tackle.InventoryItem == null)
			{
				text = "PHFloat-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Sinker:
		case ItemSubTypes.CageFeeder:
		case ItemSubTypes.FlatFeeder:
		case ItemSubTypes.PvaFeeder:
			if (InitRods.Instance.ActiveRod.Feeder.InventoryItem == null)
			{
				text = "PHFeeder-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.RodCase:
			if (PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemSubType == ItemSubTypes.RodCase && x.Storage == StoragePlaces.Doll) == null)
			{
				text = "PHRodCase-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.LuresBox:
			if (PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemSubType == ItemSubTypes.LuresBox && x.Storage == StoragePlaces.Doll) == null)
			{
				text = "PHLuresBox-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Waistcoat:
			if (PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemSubType == ItemSubTypes.Waistcoat && x.Storage == StoragePlaces.Doll) == null)
			{
				text = "PHVest-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Hat:
			if (PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemSubType == ItemSubTypes.Hat && x.Storage == StoragePlaces.Doll) == null)
			{
				text = "PHHat-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.SpinReel:
		case ItemSubTypes.LineRunningReel:
		case ItemSubTypes.CastReel:
		case ItemSubTypes.FlyReel:
			if (InitRods.Instance.ActiveRod.Reel.InventoryItem == null)
			{
				text = "PHReel-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.JigHead:
		case ItemSubTypes.SimpleHook:
		case ItemSubTypes.LongHook:
		case ItemSubTypes.BarblessHook:
		case ItemSubTypes.Spoon:
		case ItemSubTypes.Spinner:
		case ItemSubTypes.Spinnerbait:
		case ItemSubTypes.Cranckbait:
		case ItemSubTypes.Popper:
		case ItemSubTypes.Swimbait:
		case ItemSubTypes.Jerkbait:
		case ItemSubTypes.BassJig:
		case ItemSubTypes.Frog:
		case ItemSubTypes.BarblessJigHeads:
		case ItemSubTypes.CommonJigHeads:
		case ItemSubTypes.BarblessSpoons:
		case ItemSubTypes.BarblessSpinners:
		case ItemSubTypes.Walker:
		case ItemSubTypes.CarpHook:
			if (InitRods.Instance.ActiveRod.LureHook.InventoryItem == null)
			{
				text = "PHHook-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Boat:
		case ItemSubTypes.Kayak:
		case ItemSubTypes.Zodiak:
		case ItemSubTypes.MotorBoat:
			if (PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemType == ItemTypes.Boat && x.Storage == StoragePlaces.Doll) == null)
			{
				text = "PHKayak-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Bell:
		case ItemSubTypes.CommonBell:
		case ItemSubTypes.ElectronicBell:
			if (InitRods.Instance.ActiveRod.Bell.InventoryItem == null)
			{
				text = "PHBell2-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Leader:
		case ItemSubTypes.MonoLeader:
		case ItemSubTypes.FlurLeader:
		case ItemSubTypes.BraidLeader:
		case ItemSubTypes.SteelLeader:
		case ItemSubTypes.CarpLeader:
			if (InitRods.Instance.ActiveRod.Leader.InventoryItem == null)
			{
				text = "PHLeash-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Keepnet:
		case ItemSubTypes.Stringer:
			if (PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => (x.ItemSubType == ItemSubTypes.Keepnet || x.ItemSubType == ItemSubTypes.Stringer) && x.Storage == StoragePlaces.Doll) == null)
			{
				text = "PHFishcage-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.RodStand:
			if (PhotonConnectionFactory.Instance.Profile.Inventory.FirstOrDefault((InventoryItem x) => x.ItemSubType == ItemSubTypes.RodStand && x.Storage == StoragePlaces.Doll) == null)
			{
				text = "PHRodHolder-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.Chum:
		case ItemSubTypes.ChumBase:
		case ItemSubTypes.ChumGroundbaits:
		case ItemSubTypes.ChumCarpbaits:
		case ItemSubTypes.ChumMethodMix:
			if (InitRods.Instance.ActiveRod.Chum.InventoryItem == null && InitRods.Instance.ActiveRod.SpodChumAdditional.InventoryItem == null)
			{
				text = "PHFeederMix-shine";
				this.typeAssigned = 1;
			}
			break;
		case ItemSubTypes.SpodFeeder:
			if (InitRods.Instance.ActiveRod.SpodFeeder.InventoryItem == null)
			{
				text = "PHRocket_shine";
				this.typeAssigned = 1;
			}
			break;
		}
		HintSystem.Instance.StartCoroutine(ResourcesHelpers.SetPlaceholderHighlightToImageAsync(text, this.Img, this));
	}

	protected override void Update()
	{
		base.Update();
		if (this.IsItemUpdate)
		{
			this.Img.type = this.typeAssigned;
			this.IsItemUpdate = false;
		}
	}

	protected override void OnDestroy()
	{
		base.StopAllCoroutines();
		PhotonConnectionFactory.Instance.OnInventoryUpdated -= this.OnInventoryUpdated;
		if (InitRods.Instance != null)
		{
			InitRods.Instance.ChangedActiveRod -= this.OnInventoryUpdated;
		}
		base.OnDestroy();
	}

	public Image Img;

	private ItemSubTypes _type;

	private Image.Type typeAssigned = 2;
}
