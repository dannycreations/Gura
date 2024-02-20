using System;
using UnityEngine;

public class DisableShopMenu : MonoBehaviour
{
	private void Start()
	{
		if (this.enableYear < 2017 || this.enableYear > 2037 || this.enableMonth < 1 || this.enableMonth > 12 || this.enableDay < 1 || this.enableDay > 30)
		{
			return;
		}
		DateTime dateTime = TimeHelper.UtcTime();
		DateTime dateTime2 = new DateTime(this.enableYear, this.enableMonth, this.enableDay);
		if (this.platform == Application.platform && dateTime < dateTime2)
		{
			base.gameObject.SetActive(false);
		}
	}

	public int enableDay;

	public int enableMonth;

	public int enableYear;

	public RuntimePlatform platform;
}
