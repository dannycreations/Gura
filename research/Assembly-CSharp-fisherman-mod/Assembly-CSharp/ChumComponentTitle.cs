using System;
using Assets.Scripts.UI._2D.Inventory.Mixing;
using UnityEngine;
using UnityEngine.UI;

public class ChumComponentTitle : ChumComponentEmpty
{
	protected override void Awake()
	{
	}

	public void Init(Types type, UiTypes uiType, string title, string upToPrc)
	{
		this._type = type;
		this._uiType = uiType;
		this.Init(title.ToUpper());
		this._upToPrc.text = upToPrc;
	}

	[SerializeField]
	private Text _upToPrc;
}
