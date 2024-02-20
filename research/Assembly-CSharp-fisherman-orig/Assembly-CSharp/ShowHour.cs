using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowHour : MonoBehaviour
{
	private void Start()
	{
		TextMeshProUGUI component = base.GetComponent<TextMeshProUGUI>();
		if (component != null)
		{
			component.text = MeasuringSystemManager.GetHourString(new DateTime(2000, 1, 1, this.HourIn24Format, 0, 0));
		}
		else
		{
			base.GetComponent<Text>().text = MeasuringSystemManager.GetHourString(new DateTime(2000, 1, 1, this.HourIn24Format, 0, 0));
		}
	}

	public int HourIn24Format;
}
