using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class MoneyShow : MonoBehaviour
{
	private void Start()
	{
		this._silvers = (int)PhotonConnectionFactory.Instance.Profile.SilverCoins;
		this.SilverCoinsField.text = this._silvers.ToString(CultureInfo.InvariantCulture);
		this._golds = (int)PhotonConnectionFactory.Instance.Profile.GoldCoins;
		this.GoldCoinsField.text = this._golds.ToString(CultureInfo.InvariantCulture);
	}

	internal void Update()
	{
		if (PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && Math.Abs((double)this._silvers - PhotonConnectionFactory.Instance.Profile.SilverCoins) > 0.1)
		{
			this._silvers = (int)PhotonConnectionFactory.Instance.Profile.SilverCoins;
			this.SilverCoinsField.text = this._silvers.ToString(CultureInfo.InvariantCulture);
		}
		if (PhotonConnectionFactory.Instance != null && PhotonConnectionFactory.Instance.Profile != null && this.GoldCoinsField != null && Math.Abs((double)this._golds - PhotonConnectionFactory.Instance.Profile.GoldCoins) > 0.1)
		{
			this._golds = (int)PhotonConnectionFactory.Instance.Profile.GoldCoins;
			this.GoldCoinsField.text = this._golds.ToString(CultureInfo.InvariantCulture);
		}
	}

	private int _silvers;

	private int _golds;

	public Text GoldCoinsField;

	public Text SilverCoinsField;
}
