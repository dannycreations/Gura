using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ObjectModel;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChumResult : NameBehaviour, IDisposable
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action<ChumResult.Events, ChumResult> OnAction = delegate(ChumResult.Events ev, ChumResult chum)
	{
	};

	public Chum ChumObj { get; protected set; }

	public bool IsNew { get; protected set; }

	public ChumResult.Events State
	{
		get
		{
			return this._state;
		}
	}

	public Toggle Tgl
	{
		get
		{
			return this._tgl;
		}
	}

	protected virtual void Awake()
	{
		this._objectsList = this._objects.ToList<GameObject>();
		this._textsList = this._texts.ToList<Text>();
		this._btnSelect.onClick.AddListener(new UnityAction(this.Select));
		this._btnDelete.onClick.AddListener(new UnityAction(this.Delete));
	}

	public virtual void Init(Chum chum, bool isNew, ToggleGroup tglGroup, ChumResult.Events state = ChumResult.Events.None)
	{
		this._tgl.group = tglGroup;
		this.IsNew = isNew;
		this.ChumObj = chum;
		this._textsList.ForEach(delegate(Text p)
		{
			p.text = ChumMixing.ChumNameCorrection(chum.Name);
		});
		this._btnDelete.interactable = !isNew;
		this._btnDelete.gameObject.SetActive(!isNew);
		this._icoDelete.SetActive(!isNew);
		this.SetState(state);
	}

	public void SetState(ChumResult.Events ev)
	{
		this._state = ev;
		for (int i = 0; i < this._objectsList.Count; i++)
		{
			this._objectsList[i].SetActive(i == (int)ev);
		}
	}

	protected virtual void Select()
	{
		this.OnAction(ChumResult.Events.Select, this);
	}

	protected virtual void Delete()
	{
		this.OnAction(ChumResult.Events.Delete, this);
	}

	public void Dispose()
	{
		Object.Destroy(base.gameObject);
	}

	[SerializeField]
	protected Toggle _tgl;

	[SerializeField]
	protected BorderedButton _btnDelete;

	[SerializeField]
	protected BorderedButton _btnSelect;

	[SerializeField]
	protected GameObject[] _objects;

	[SerializeField]
	protected Text[] _texts;

	[SerializeField]
	protected GameObject _icoDelete;

	private List<GameObject> _objectsList;

	private List<Text> _textsList;

	private ChumResult.Events _state;

	public enum Events : byte
	{
		None,
		Select,
		Delete
	}
}
