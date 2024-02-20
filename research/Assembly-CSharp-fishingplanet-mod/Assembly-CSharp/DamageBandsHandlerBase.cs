using System;
using ObjectModel;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DamageBandsHandlerBase : MonoBehaviour
{
	public bool IsHintActive { get; set; }

	protected virtual void Init()
	{
		if (this.DurabilityImages == null)
		{
			this.Items = new InventoryItem[]
			{
				GameFactory.Player.Rod.AssembledRod.Rod,
				GameFactory.Player.Rod.AssembledRod.Reel,
				GameFactory.Player.Rod.AssembledRod.Line
			};
			this.DurabilityImages = new Image[] { this.RodDurability, this.ReelDurability, this.LineDurability };
			this.DamageTexts = new TextMeshProUGUI[] { this.RodDamageText, this.ReelDamageText, this.LineDamageText };
			this._durabilities = new sbyte[this.Items.Length];
			for (int i = 0; i < this._durabilities.Length; i++)
			{
				this._durabilities[0] = -1;
			}
		}
		if (!this.Items[0].Equals(GameFactory.Player.Rod.AssembledRod.Rod))
		{
			this.Items[0] = GameFactory.Player.Rod.AssembledRod.Rod;
		}
		if (!this.Items[1].Equals(GameFactory.Player.Rod.AssembledRod.Reel))
		{
			this.Items[1] = GameFactory.Player.Rod.AssembledRod.Reel;
		}
		if (!this.Items[2].Equals(GameFactory.Player.Rod.AssembledRod.Line))
		{
			this.Items[2] = GameFactory.Player.Rod.AssembledRod.Line;
		}
	}

	protected virtual void Refresh(InventoryItem[] items, Image[] durabilityImages, TextMeshProUGUI[] damageTexts, CanvasGroup[] cgs, RullerTackleType[] rullerTypes, float? minAlpha)
	{
		for (int i = 0; i < items.Length; i++)
		{
			InventoryItem inventoryItem = items[i];
			if (inventoryItem.MaxDurability != null)
			{
				float num = (float)inventoryItem.Durability / (float)inventoryItem.MaxDurability.Value * 100f;
				if (num < 33f)
				{
					durabilityImages[i].color = this.LowDurabilityColor;
				}
				else
				{
					durabilityImages[i].color = ((num > 66f) ? this.HighDurabilityColor : this.NormalDurabilityColor);
				}
				durabilityImages[i].fillAmount = num / 100f;
				float num2 = (float)inventoryItem.Durability / (float)inventoryItem.MaxDurability.Value;
				sbyte b = (sbyte)(num2 * 100f);
				if ((int)this._durabilities[i] != (int)b)
				{
					this._durabilities[i] = b;
					damageTexts[i].text = string.Format("{0}%", b);
				}
			}
			if (minAlpha != null)
			{
				this.UpdateCg(GameFactory.Player.Reel.ForceRuller == rullerTypes[i], cgs[i], minAlpha.Value);
			}
		}
	}

	protected virtual void UpdateCg(bool isMin, CanvasGroup cg, float minAlpha)
	{
		if (isMin)
		{
			cg.alpha = Mathf.Min(1f, cg.alpha + Time.deltaTime * 4f);
		}
		else
		{
			cg.alpha = Mathf.Max(minAlpha, cg.alpha - Time.deltaTime * 4f);
		}
	}

	protected virtual bool IsEnabled
	{
		get
		{
			return GameFactory.Player.Rod != null && GameFactory.Player.Rod.AssembledRod != null && GameFactory.Player.Rod.AssembledRod.Rod != null && GameFactory.Player.Rod.AssembledRod.Reel != null && GameFactory.Player.Rod.AssembledRod.Line != null;
		}
	}

	public Image RodDurability;

	public Image ReelDurability;

	public Image LineDurability;

	public Color HighDurabilityColor;

	public Color NormalDurabilityColor;

	public Color LowDurabilityColor;

	public TextMeshProUGUI RodDamageText;

	public TextMeshProUGUI ReelDamageText;

	public TextMeshProUGUI LineDamageText;

	protected InventoryItem[] Items;

	protected RullerTackleType[] RullerTypes = new RullerTackleType[]
	{
		RullerTackleType.Rod,
		RullerTackleType.Reel,
		RullerTackleType.Line
	};

	protected Image[] DurabilityImages;

	protected TextMeshProUGUI[] DamageTexts;

	protected const float LowDurability = 33f;

	protected const float HighDurability = 66f;

	private sbyte[] _durabilities;
}
