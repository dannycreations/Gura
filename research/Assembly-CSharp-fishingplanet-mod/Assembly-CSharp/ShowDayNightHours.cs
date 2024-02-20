using System;
using System.Text;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ShowDayNightHours : MonoBehaviour
{
	private void Awake()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int i = 5;
		if (this.IsDay)
		{
			while (i < 22)
			{
				if (i != 5)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(MeasuringSystemManager.GetHourString(new DateTime(2000, 1, 1, i, 0, 0)));
				i += 2;
			}
		}
		if (this.IsNight)
		{
			i = 21;
			for (int j = 0; j < 5; j++)
			{
				if (j != 0)
				{
					stringBuilder.Append(" ");
				}
				stringBuilder.Append(MeasuringSystemManager.GetHourString(new DateTime(2000, 1, 1, i, 0, 0)));
				i += 2;
				if (i >= 24)
				{
					i -= 24;
				}
			}
		}
		base.GetComponent<TextMeshProUGUI>().text = stringBuilder.ToString();
	}

	public bool IsDay;

	public bool IsNight;
}
