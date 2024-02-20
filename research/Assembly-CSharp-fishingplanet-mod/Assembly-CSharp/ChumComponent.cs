using System;
using System.Diagnostics;
using Assets.Scripts.UI._2D.Inventory.Mixing;
using ObjectModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChumComponent : ChumComponentEmpty, IPointerClickHandler, IEventSystemHandler
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ChumIngredient> OnChangePrc = delegate(ChumIngredient ii)
	{
	};

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ChumIngredient> OnDrop2Inventory = delegate(ChumIngredient ii)
	{
	};

	public StoragePlaces Storage
	{
		get
		{
			return this._dragMeDoll.DranNDropType.CurrentActiveStorage;
		}
	}

	public bool OverStorage { get; protected set; }

	public Toggle Tgl
	{
		get
		{
			return this._tgl;
		}
	}

	protected override void Awake()
	{
		base.NormalNameColor = this.Name.color;
	}

	public void Init(Types type, ChumIngredient ii, ToggleGroup tglGroup)
	{
		this._tgl.group = tglGroup;
		this.Init(ii.Name);
		this._ii = ii;
		this.ImageLdbl.OnLoaded.RemoveAllListeners();
		this.ImageLdbl.OnLoaded.AddListener(delegate
		{
			this._dragMeDoll.SetIco(this.Image.overrideSprite);
			this.ImageLdbl.OnLoaded.RemoveAllListeners();
		});
		base.Init(type, UiTypes.Component, (ii.ThumbnailBID == null) ? "0" : ii.ThumbnailBID.ToString());
	}

	public void UpdatePrc(float prc, float weight)
	{
		this._weight.text = string.Format("{0}{1}", MeasuringSystemManager.Kilograms2Grams(weight), MeasuringSystemManager.GramsOzWeightSufix());
		this._prc.text = string.Format("{0}%", prc.ToString("F2").TrimEnd(new char[] { '0' }).TrimEnd(new char[] { '.' })
			.TrimEnd(new char[] { ',' }));
	}

	public void Drop2Inventory()
	{
		this.OnDrop2Inventory(this._ii);
	}

	public void ChangePrc()
	{
		this.OnChangePrc(this._ii);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.clickCount == 2)
		{
			this.OnDrop2Inventory(this._ii);
		}
	}

	public void PointerOnStorage(bool v)
	{
		this.OverStorage = v;
	}

	[SerializeField]
	protected Toggle _tgl;

	[SerializeField]
	private Text _prc;

	[SerializeField]
	private Text _weight;

	[SerializeField]
	private BorderedButton _btn;

	[SerializeField]
	private DragMeDollChumComponent _dragMeDoll;

	[SerializeField]
	private BorderedButton _buttonClear;
}
