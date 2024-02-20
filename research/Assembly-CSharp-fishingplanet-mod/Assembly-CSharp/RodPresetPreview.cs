using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Inventory;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class RodPresetPreview : MonoBehaviour
{
	internal void Awake()
	{
		this.sizeSetters = base.GetComponentsInChildren<ContentPreferredSizeSetter>();
	}

	private void RebuildLayout()
	{
		if (this.sizeSetters != null)
		{
			foreach (ContentPreferredSizeSetter contentPreferredSizeSetter in this.sizeSetters)
			{
				contentPreferredSizeSetter.Refresh();
			}
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(base.transform as RectTransform);
		if (this.layout != null)
		{
			this.layout.enabled = false;
			this.layout.enabled = true;
		}
	}

	internal void OnEnable()
	{
		this.Reset(false);
	}

	internal void OnDisable()
	{
		this.Reset(false);
	}

	private void ProcessImg(RodPresetPreview.ImageWithLayout img, Sprite sprite, Color color)
	{
		img.LoadableImage.Image.overrideSprite = sprite;
		img.LoadableImage.Image.color = color;
		img.Layout.ignoreLayout = color == Color.clear;
	}

	private void ProcessImg(RodPresetPreview.ImageWithLayout img, Color color, string path)
	{
		img.LoadableImage.Load(path);
		img.LoadableImage.Image.color = color;
		img.Layout.ignoreLayout = color == Color.clear;
	}

	public void Reset(bool clear = false)
	{
		this.ProcessImg(this.Rod, null, (!clear) ? this.PlaceholderColor : Color.clear);
		this.ProcessImg(this.Reel, null, (!clear) ? this.PlaceholderColor : Color.clear);
		this.ProcessImg(this.Line, null, (!clear) ? this.PlaceholderColor : Color.clear);
		this.ProcessImg(this.Hooks, null, (!clear) ? this.PlaceholderColor : Color.clear);
		this.ProcessImg(this.Lure, null, (!clear) ? this.PlaceholderColor : Color.clear);
		this.ProcessImg(this.Tackle, null, Color.clear);
		this.ProcessImg(this.Leader, null, Color.clear);
		this.ProcessImg(this.Feeder, null, Color.clear);
		this.ProcessImg(this.Bell, null, Color.clear);
		this.ProcessImg(this.SpodFeeder, null, Color.clear);
		this.ProcessImg(this.CarpSinker, null, Color.clear);
		this.ProcessImg(this.SpinningSinker, null, Color.clear);
		this.IsClear = true;
		this.RebuildLayout();
	}

	private void SetThumbnail(RodPresetPreview.ImageWithLayout img, InventoryItem item, bool isAbscent)
	{
		if (item == null)
		{
			this.ProcessImg(img, null, this.PlaceholderColor);
			return;
		}
		if (item.DollThumbnailBID != null)
		{
			this.ProcessImg(img, (!isAbscent) ? this.AvailableColor : this.AbscentColor, string.Format("Textures/Inventory/{0}", item.DollThumbnailBID));
		}
		else
		{
			this.ProcessImg(img, ResourcesHelpers.GetTransparentSprite(), (!isAbscent) ? this.AvailableColor : this.AbscentColor);
		}
	}

	private void VerifyLeaderHiddenIfPredatorFishOff(List<ItemTypes> types, RodTemplate template)
	{
		if (!PhotonConnectionFactory.Instance.IsPredatorFishOn)
		{
			bool flag = types.Contains(ItemTypes.Leader);
			if (flag && !template.IsBottomFishingTemplate() && !template.IsSinkerRig())
			{
				types.Remove(ItemTypes.Leader);
			}
		}
	}

	public void Show(RodSetup setup)
	{
		if (setup == null)
		{
			return;
		}
		RodTemplate rodTemplate = setup.RodTemplate;
		List<ItemTypes> list = new List<ItemTypes>(RodTemplates.GetTypesForTemplate(rodTemplate));
		list.Add(ItemTypes.Rod);
		this.VerifyLeaderHiddenIfPredatorFishOff(list, rodTemplate);
		if (setup.Items == null)
		{
			string text = string.Format("Rod setup[{0}]: Items list empty, check json.", setup.Name);
			Debug.LogError(text);
			PhotonConnectionFactory.Instance.PinError(text, "RodPresetPreview.Show");
			return;
		}
		if (setup.Items.Count > list.Count && rodTemplate != RodTemplate.UnEquiped)
		{
			string text2 = string.Format("Rod setup[{0}]: items count is more than amount of types, that should be present, check json.", setup.Name);
			Debug.LogError(text2);
			PhotonConnectionFactory.Instance.PinError(text2, "RodPresetPreview.Show");
		}
		this.SetThumbnail(this.Feeder, null, false);
		this.Reset(true);
		if (list != null)
		{
			using (List<ItemTypes>.Enumerator enumerator = list.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					RodPresetPreview.<Show>c__AnonStorey0 <Show>c__AnonStorey = new RodPresetPreview.<Show>c__AnonStorey0();
					<Show>c__AnonStorey.type = enumerator.Current;
					InventoryItem item = setup.Items.FirstOrDefault((InventoryItem x) => x.ItemType == <Show>c__AnonStorey.type);
					bool flag = item == null || PhotonConnectionFactory.Instance.Profile.Inventory.All((InventoryItem x) => x.ItemId != item.ItemId);
					switch (<Show>c__AnonStorey.type)
					{
					case ItemTypes.Rod:
						this.SetThumbnail(this.Rod, item, flag);
						break;
					case ItemTypes.Reel:
						this.SetThumbnail(this.Reel, item, flag);
						break;
					default:
						switch (<Show>c__AnonStorey.type)
						{
						case ItemTypes.Bell:
							this.SetThumbnail(this.Bell, item, flag);
							break;
						default:
							if (<Show>c__AnonStorey.type == ItemTypes.JigHead)
							{
								this.SetThumbnail(this.Hooks, item, flag);
							}
							break;
						case ItemTypes.Leader:
							this.SetThumbnail(this.Leader, item, flag);
							break;
						}
						break;
					case ItemTypes.Line:
						this.SetThumbnail(this.Line, item, flag);
						break;
					case ItemTypes.Hook:
						this.SetThumbnail(this.Hooks, item, flag);
						break;
					case ItemTypes.Bobber:
						this.SetThumbnail(this.Tackle, item, flag);
						break;
					case ItemTypes.Sinker:
					case ItemTypes.Feeder:
						if (item != null)
						{
							if (item.ItemSubType == ItemSubTypes.SpodFeeder)
							{
								this.SetThumbnail(this.SpodFeeder, item, flag);
							}
							else if (setup.RodTemplate == RodTemplate.PVACarp && item.ItemSubType == ItemSubTypes.Sinker)
							{
								this.SetThumbnail(this.CarpSinker, item, flag);
							}
							else
							{
								this.SetThumbnail(this.Feeder, item, flag);
							}
						}
						break;
					case ItemTypes.Bait:
						this.SetThumbnail(this.Lure, item, flag);
						break;
					case ItemTypes.Lure:
						this.SetThumbnail(this.Hooks, item, flag);
						break;
					case ItemTypes.JigBait:
						this.SetThumbnail(this.Lure, item, flag);
						break;
					}
				}
			}
		}
		if (this.TackleContent != null)
		{
			InitRod activeRod = this.TackleContent.ActiveRod;
			if (setup.Items.Any((InventoryItem i) => PhotonConnectionFactory.Instance.Profile.Inventory.All((InventoryItem x) => x.ItemId != i.ItemId)))
			{
				GameFactory.Message.KillLastMessage();
				GameFactory.Message.ShowMessage(string.Format(ScriptLocalization.Get("RodPresetNoItemTooltip"), setup.Name), base.transform.root.gameObject, 2f, false);
			}
		}
		this.IsClear = false;
		this.RebuildLayout();
	}

	public void CopyFromActiveRod()
	{
		this.Reset(false);
		InitRod activeRod = this.TackleContent.ActiveRod;
		if (activeRod != null && activeRod.Rod != null && activeRod.Rod.InventoryItem != null)
		{
			RodTemplate closestTemplate = InventoryHelper.GetClosestTemplate(activeRod.Rod.InventoryItem as Rod);
			List<ItemTypes> list = new List<ItemTypes> { ItemTypes.Rod };
			ItemTypes[] typesForTemplate = RodTemplates.GetTypesForTemplate(closestTemplate);
			if (typesForTemplate != null)
			{
				list.AddRange(typesForTemplate);
			}
			this.VerifyLeaderHiddenIfPredatorFishOff(list, closestTemplate);
			bool flag = activeRod.Rod.InventoryItem != null && activeRod.Rod.InventoryItem.ItemSubType == ItemSubTypes.FeederRod;
			bool flag2 = activeRod.Rod.InventoryItem != null && activeRod.Rod.InventoryItem.ItemSubType == ItemSubTypes.BottomRod;
			bool flag3 = activeRod.Rod.InventoryItem != null && activeRod.Rod.InventoryItem.ItemSubType == ItemSubTypes.CarpRod;
			bool flag4 = activeRod.Rod.InventoryItem != null && activeRod.Rod.InventoryItem.ItemSubType == ItemSubTypes.SpodRod;
			bool flag5 = closestTemplate.IsSinkerRig();
			this.ProcessImg(this.Rod, activeRod.Rod.InventoryImage.overrideSprite, this.AvailableColor);
			this.ProcessImg(this.Reel, (activeRod.Reel.InventoryItem == null) ? null : activeRod.Reel.InventoryImage.overrideSprite, (activeRod.Reel.InventoryItem == null) ? this.PlaceholderColor : this.AvailableColor);
			this.ProcessImg(this.Line, (activeRod.Line.InventoryItem == null) ? null : activeRod.Line.InventoryImage.overrideSprite, (activeRod.Line.InventoryItem == null) ? this.PlaceholderColor : this.AvailableColor);
			this.ProcessImg(this.Hooks, (activeRod.LureHook.InventoryItem == null) ? null : activeRod.LureHook.InventoryImage.overrideSprite, (activeRod.LureHook.InventoryItem == null) ? (flag4 ? Color.clear : this.PlaceholderColor) : this.AvailableColor);
			this.ProcessImg(this.Lure, (activeRod.Bait.InventoryItem == null) ? null : activeRod.Bait.InventoryImage.overrideSprite, (activeRod.Bait.InventoryItem == null) ? ((flag4 || closestTemplate == RodTemplate.Lure) ? Color.clear : this.PlaceholderColor) : this.AvailableColor);
			this.ProcessImg(this.Bell, (activeRod.Bell.InventoryItem == null) ? null : activeRod.Bell.InventoryImage.overrideSprite, (activeRod.Bell.InventoryItem == null) ? ((!list.Contains(ItemTypes.Bell) && !flag && !flag2) ? Color.clear : this.PlaceholderColor) : this.AvailableColor);
			this.ProcessImg(this.Feeder, (activeRod.Feeder.InventoryItem == null) ? null : activeRod.Feeder.InventoryImage.overrideSprite, (activeRod.Feeder.InventoryItem == null) ? (((!list.Contains(ItemTypes.Feeder) && !flag && !flag2) || flag4) ? Color.clear : this.PlaceholderColor) : this.AvailableColor);
			this.ProcessImg(this.Leader, (activeRod.Leader.InventoryItem == null) ? null : activeRod.Leader.InventoryImage.overrideSprite, (activeRod.Leader.InventoryItem == null) ? ((!list.Contains(ItemTypes.Leader) && !flag && !flag2) ? Color.clear : this.PlaceholderColor) : this.AvailableColor);
			this.ProcessImg(this.Tackle, (activeRod.Tackle.InventoryItem == null) ? null : activeRod.Tackle.InventoryImage.overrideSprite, (activeRod.Tackle.InventoryItem == null) ? ((!list.Contains(ItemTypes.Bobber)) ? Color.clear : this.PlaceholderColor) : this.AvailableColor);
			this.ProcessImg(this.SpodFeeder, (activeRod.SpodFeeder.InventoryItem == null) ? null : activeRod.SpodFeeder.InventoryImage.overrideSprite, (activeRod.SpodFeeder.InventoryItem == null) ? ((!list.Contains(ItemTypes.Feeder) || !flag4) ? Color.clear : this.PlaceholderColor) : this.AvailableColor);
			this.ProcessImg(this.CarpSinker, (activeRod.PVASinker.InventoryItem == null) ? null : activeRod.PVASinker.InventoryImage.overrideSprite, (activeRod.PVASinker.InventoryItem == null) ? ((!list.Contains(ItemTypes.Sinker) || !flag3) ? Color.clear : this.PlaceholderColor) : this.AvailableColor);
			this.ProcessImg(this.SpinningSinker, (activeRod.SpinningSinker.InventoryItem == null) ? null : activeRod.SpinningSinker.InventoryImage.overrideSprite, (activeRod.SpinningSinker.InventoryItem == null) ? ((!list.Contains(ItemTypes.Sinker) || !flag5) ? Color.clear : this.PlaceholderColor) : this.AvailableColor);
		}
		this.IsClear = false;
		this.RebuildLayout();
	}

	public InitRods TackleContent;

	public GameObject Content;

	public Color PlaceholderColor;

	public Color AvailableColor;

	public Color AbscentColor;

	public RodPresetPreview.ImageWithLayout Rod;

	public RodPresetPreview.ImageWithLayout Reel;

	public RodPresetPreview.ImageWithLayout Line;

	public RodPresetPreview.ImageWithLayout Tackle;

	public RodPresetPreview.ImageWithLayout Hooks;

	public RodPresetPreview.ImageWithLayout Lure;

	public RodPresetPreview.ImageWithLayout Bell;

	public RodPresetPreview.ImageWithLayout Feeder;

	public RodPresetPreview.ImageWithLayout Leader;

	public RodPresetPreview.ImageWithLayout SpodFeeder;

	public RodPresetPreview.ImageWithLayout CarpSinker;

	public RodPresetPreview.ImageWithLayout SpinningSinker;

	private List<ResourcesHelpers.AsyncLoadableImage> ImgsList = new List<ResourcesHelpers.AsyncLoadableImage>();

	private ContentPreferredSizeSetter[] sizeSetters;

	public VerticalLayoutGroup layout;

	public bool IsClear;

	[Serializable]
	public class ImageWithLayout
	{
		public ResourcesHelpers.AsyncLoadableImage LoadableImage;

		public LayoutElement Layout;
	}
}
