using System;
using UnityEngine;
using UnityEngine.UI;

public class IcoCounter : MonoBehaviour
{
	public void UpdateCount(string countText)
	{
		this._countText.text = countText;
	}

	[SerializeField]
	private Text _countText;
}
