using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class DamageIconManager : MonoBehaviour
{
	private void Start()
	{
		this.OnEnable();
		this.GetIco();
	}

	private void Update()
	{
		this._timer += Time.deltaTime;
		if (this._timer >= 0.5f)
		{
			this._timer = 0f;
			if (this._ii == null || Math.Abs(this._oldDurability - (float)this._ii.Durability) > 0.001f)
			{
				this.SetDamage();
			}
		}
	}

	public void Init(InventoryItem ii)
	{
		this._ii = ii;
		this.Refresh();
	}

	public void Refresh()
	{
		this.SetDamage();
	}

	private void OnEnable()
	{
		if (this._ii != null)
		{
			this._oldDurability = (float)this._ii.Durability;
			this.SetDamage();
		}
	}

	private void SetDamage()
	{
		if (this.Icon == null)
		{
			return;
		}
		if (this._ii == null || this._ii.MaxDurability == null || this._ii.MaxDurability.Value == 0 || this._ii.IsUnwearable)
		{
			this.Icon.SetActive(false);
			return;
		}
		this._oldDurability = (float)this._ii.Durability;
		this.Icon.SetActive(true);
		float num = (float)this._ii.Durability / (float)this._ii.MaxDurability.Value;
		this.DurabilityInProcent = (int)(num * 100f);
		this.Durability.fillAmount = num;
		if (num < 0.33f)
		{
			this.Durability.color = ColorManager.DurabilityLowColor;
		}
		else
		{
			this.Durability.color = ((num > 0.66f) ? ColorManager.DurabilityHighColor : ColorManager.DurabilityNormalColor);
		}
		if (this.GetIco())
		{
			this._icon.color = (((float)this._ii.Durability > 0f) ? this._normalColor : this._damagedColor);
		}
	}

	private bool GetIco()
	{
		if (this._icon == null)
		{
			Transform transform = this.Icon.transform.Find("Icon");
			if (transform == null)
			{
				return false;
			}
			this._icon = transform.GetComponent<Image>();
			this._normalColor = new Color(this._icon.color.r, this._icon.color.g, this._icon.color.b, 0.3f);
			this._normalColor = this._icon.color;
		}
		return true;
	}

	public GameObject Icon;

	public Image Durability;

	[HideInInspector]
	public int DurabilityInProcent;

	private float _oldDurability;

	private Image _icon;

	private const float TimerMax = 0.5f;

	private const float DamagedAlpha = 0.3f;

	private float _timer;

	private InventoryItem _ii;

	private Color _normalColor;

	private Color _damagedColor;
}
