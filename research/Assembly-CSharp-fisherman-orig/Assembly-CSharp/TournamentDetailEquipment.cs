using System;
using TMPro;
using UnityEngine;

public class TournamentDetailEquipment : MonoBehaviour
{
	public Type EquipmentType { get; private set; }

	public bool IsTitle { get; private set; }

	public string Name
	{
		get
		{
			return this._name.text;
		}
	}

	public bool IsActive
	{
		get
		{
			return base.gameObject.activeSelf;
		}
	}

	public void Init(string name, string description, Type equipmentType, bool isTitle)
	{
		this.Init(name, equipmentType, isTitle);
		this._description.text = description;
	}

	public void Init(string name, Type equipmentType, bool isTitle)
	{
		this._name.text = name;
		this.EquipmentType = equipmentType;
		this.IsTitle = isTitle;
	}

	public void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
	}

	[SerializeField]
	private TextMeshProUGUI _name;

	[SerializeField]
	private TextMeshProUGUI _description;
}
