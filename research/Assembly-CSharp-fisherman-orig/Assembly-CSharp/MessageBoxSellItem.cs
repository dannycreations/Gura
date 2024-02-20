using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxSellItem : MonoBehaviour
{
	public void Init(string contentText, string currencyName, string price)
	{
		string text = "\ue62b";
		string text2 = "\ue62c";
		this.Text.text = string.Format("{0}\n {1} {2} ?", contentText, (!(currencyName == "SC")) ? text2 : text, price);
	}

	public GameObject SilverIcon;

	public GameObject GoldIcon;

	public Text SellValueText;

	public TextMeshProUGUI Text;
}
