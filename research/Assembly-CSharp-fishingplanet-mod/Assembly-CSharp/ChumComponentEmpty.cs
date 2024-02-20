using System;
using Assets.Scripts.UI._2D.Helpers;
using Assets.Scripts.UI._2D.Inventory.Mixing;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class ChumComponentEmpty : NameBehaviour, IChumComponent, IDisposable
{
	public ChumIngredient Ingredient
	{
		get
		{
			return this._ii;
		}
	}

	public Types Type
	{
		get
		{
			return this._type;
		}
	}

	public UiTypes UiType
	{
		get
		{
			return this._uiType;
		}
	}

	public bool IsEnabled
	{
		get
		{
			return this.Cg.alpha >= 1f;
		}
	}

	protected Color BeginDragColor { get; set; }

	protected Color NormalColor { get; set; }

	protected Color NormalNameColor { get; set; }

	protected virtual void Awake()
	{
		this.NormalNameColor = this.Name.color;
		this.BeginDragColor = new Color(this.ImageDrop.color.r, this.ImageDrop.color.g, this.ImageDrop.color.b, 0.3f);
		this.NormalColor = this.ImageDrop.color;
		this.BlockY0 = this.Block.anchoredPosition.y;
		this.BlockImageHeight0 = this.BlockImage.rect.height;
	}

	public void Dispose()
	{
		Object.Destroy(base.gameObject);
	}

	public void Init(Types type, UiTypes uiType, string path)
	{
		this._type = type;
		this._uiType = uiType;
		this.ImageLdbl.Image = this.Image;
		this.ImageLdbl.Load(string.Format("Textures/Inventory/{0}", path));
	}

	public void Init(Types type, UiTypes uiType, Sprite sp)
	{
		this._type = type;
		this._uiType = uiType;
		this.Image.overrideSprite = sp;
	}

	public int GetSiblingIndex()
	{
		return base.transform.GetSiblingIndex();
	}

	public void SetSiblingIndex(int i)
	{
		base.transform.SetSiblingIndex(i);
	}

	public void SetEnable(bool flag)
	{
		if (this.Cg != null)
		{
			this.Cg.alpha = ((!flag) ? 0.05f : 1f);
		}
	}

	public void SetDrag(bool flag)
	{
		if (this.ImageDrop != null)
		{
			this.ImageDrop.color = ((!flag) ? this.NormalColor : this.BeginDragColor);
		}
	}

	public void SetBlock(bool flag, bool isFullBlock)
	{
		if (this.Block == null || this.BlockImage == null)
		{
			return;
		}
		this.Block.gameObject.SetActive(flag);
		this.Block.anchoredPosition = new Vector2(this.Block.anchoredPosition.x, (!isFullBlock) ? this.BlockY0 : 34f);
		this.BlockImage.sizeDelta = new Vector2(this.BlockImage.rect.width, (!isFullBlock) ? this.BlockImageHeight0 : 124f);
	}

	public void SetBlockText(string t)
	{
		this.BlockText.text = t;
	}

	public void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
	}

	public void HasInInventory(bool v)
	{
		this.Name.color = ((!v) ? Color.red : this.NormalNameColor);
	}

	[SerializeField]
	protected Image Image;

	protected ResourcesHelpers.AsyncLoadableImage ImageLdbl = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	protected CanvasGroup Cg;

	[SerializeField]
	protected Image ImageDrop;

	[SerializeField]
	protected RectTransform Block;

	[SerializeField]
	protected RectTransform BlockImage;

	[SerializeField]
	protected Text BlockText;

	protected ChumIngredient _ii;

	protected Types _type;

	protected UiTypes _uiType = UiTypes.Empty;

	protected const float BlockY = 34f;

	protected const float BlockImageHeight = 124f;

	protected const float BlockAlpha = 0.05f;

	protected float BlockY0;

	protected float BlockImageHeight0;
}
