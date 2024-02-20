using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.Inventory;
using ObjectModel;
using TMPro;
using UnityEngine;

public class DollInfoEntry : MonoBehaviour
{
	public void Set(InventoryItem item, bool showDescription = false)
	{
		this.Item = item;
		if (item == null)
		{
			base.gameObject.SetActive(false);
			return;
		}
		this.Name.text = item.Name;
		ItemSubTypes itemSubTypes = item.ItemSubType;
		if (itemSubTypes == ItemSubTypes.Chum)
		{
			itemSubTypes = InventoryHelper.GetChumType(item as Chum);
		}
		string text;
		if (!DollInfoEntry.ManuallyOverridenSubTypesIcons.Contains(itemSubTypes) && DollInfoEntry.ItemSubTypesIconMappings.TryGetValue(itemSubTypes, out text))
		{
			this.Icon.text = text;
		}
		else if (DollInfoEntry.ItemTypesIconMappings.TryGetValue(item.ItemType, out text))
		{
			this.Icon.text = text;
		}
		this.Description.text = ((!showDescription) ? string.Empty : InventoryParamsHelper.SplitWithCaptionOutline(InventoryParamsHelper.ParseParamsInfo(item, true)));
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
	}

	public void Set(ItemSubTypes subType, string name, string description = "")
	{
		string text;
		if (DollInfoEntry.ItemSubTypesIconMappings.TryGetValue(subType, out text))
		{
			this.Icon.text = DollInfoEntry.ItemSubTypesIconMappings[subType];
			this.Name.text = name;
			this.Description.text = description;
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
		}
	}

	public DollInfoEntry Set(string icon, string name, string description = "")
	{
		this.Icon.text = icon;
		this.Name.text = name;
		this.Description.text = description;
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(true);
		}
		return this;
	}

	public void SetFirst()
	{
		base.transform.SetAsFirstSibling();
	}

	public static Dictionary<ItemTypes, string> ItemTypesIconMappings = new Dictionary<ItemTypes, string>
	{
		{
			ItemTypes.Rod,
			"\ue653"
		},
		{
			ItemTypes.Reel,
			"\ue654"
		},
		{
			ItemTypes.Line,
			"\ue638"
		},
		{
			ItemTypes.Bobber,
			"\ue635"
		},
		{
			ItemTypes.Hook,
			"\ue634"
		},
		{
			ItemTypes.JigHead,
			"\ue634"
		},
		{
			ItemTypes.JigBait,
			"\ue664"
		},
		{
			ItemTypes.Bait,
			"\ue652"
		},
		{
			ItemTypes.Lure,
			"\ue667"
		},
		{
			ItemTypes.Feeder,
			"\ue70a"
		},
		{
			ItemTypes.Leader,
			"\ue70e"
		},
		{
			ItemTypes.Bell,
			"\ue722"
		},
		{
			ItemTypes.Chum,
			"\ue69f"
		},
		{
			ItemTypes.Sinker,
			"\ue73b"
		},
		{
			ItemTypes.Boat,
			"\ue70d"
		}
	};

	public static Dictionary<ItemSubTypes, string> ItemSubTypesIconMappings = new Dictionary<ItemSubTypes, string>
	{
		{
			ItemSubTypes.FeederRod,
			"\ue69d"
		},
		{
			ItemSubTypes.RodStand,
			"\ue70b"
		},
		{
			ItemSubTypes.Hat,
			"\ue70c"
		},
		{
			ItemSubTypes.RodCase,
			"\ue659"
		},
		{
			ItemSubTypes.Waistcoat,
			"\ue657"
		},
		{
			ItemSubTypes.Boots,
			"\ue639"
		},
		{
			ItemSubTypes.LuresBox,
			"\ue658"
		},
		{
			ItemSubTypes.Keepnet,
			"\ue602"
		},
		{
			ItemSubTypes.Stringer,
			"\ue75c"
		},
		{
			ItemSubTypes.FishNet,
			"\ue602"
		},
		{
			ItemSubTypes.FlatFeeder,
			"\ue74c"
		},
		{
			ItemSubTypes.SpodFeeder,
			"\ue74b"
		},
		{
			ItemSubTypes.PvaFeeder,
			"\ue74d"
		},
		{
			ItemSubTypes.BoilBait,
			"\ue74f"
		},
		{
			ItemSubTypes.InsectsWormBait,
			"\ue641"
		},
		{
			ItemSubTypes.ChumGroundbaits,
			"\ue69f"
		},
		{
			ItemSubTypes.ChumMethodMix,
			"\ue75b"
		},
		{
			ItemSubTypes.ChumCarpbaits,
			"\ue75a"
		},
		{
			ItemSubTypes.Glasses,
			"<size=-2>\ue00f</size>"
		},
		{
			ItemSubTypes.Waggler,
			"\ue00e"
		}
	};

	private static List<ItemSubTypes> ManuallyOverridenSubTypesIcons = new List<ItemSubTypes>
	{
		ItemSubTypes.FeederRod,
		ItemSubTypes.Waggler
	};

	public TextMeshProUGUI Icon;

	public TextMeshProUGUI Name;

	public TextMeshProUGUI Description;

	public InventoryItem Item;
}
