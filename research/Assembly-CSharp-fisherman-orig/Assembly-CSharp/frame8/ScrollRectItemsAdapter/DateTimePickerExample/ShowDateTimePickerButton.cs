using System;
using frame8.Logic.Misc.Visual.UI.DateTimePicker;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.DateTimePickerExample
{
	public class ShowDateTimePickerButton : MonoBehaviour
	{
		private void Start()
		{
			Button button = base.GetComponent<Button>();
			if (!button)
			{
				button = base.gameObject.AddComponent<Button>();
			}
			button.onClick.AddListener(delegate
			{
				DateTimePicker8.Show(null);
			});
		}
	}
}
