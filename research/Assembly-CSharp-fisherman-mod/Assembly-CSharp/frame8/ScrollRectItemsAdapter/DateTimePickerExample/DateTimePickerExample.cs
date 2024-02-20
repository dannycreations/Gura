using System;
using frame8.Logic.Misc.Visual.UI.DateTimePicker;
using frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter;
using frame8.ScrollRectItemsAdapter.Util.Drawer;
using UnityEngine;
using UnityEngine.Events;

namespace frame8.ScrollRectItemsAdapter.DateTimePickerExample
{
	public class DateTimePickerExample : MonoBehaviour
	{
		private void Start()
		{
			DrawerCommandPanel.Instance.Init(new ISRIA[0], false, false, false, false, false);
			DrawerCommandPanel.Instance.AddButtonsPanel("Show another", string.Empty, string.Empty, string.Empty).button1.onClick.AddListener(new UnityAction(this.Show));
			DrawerCommandPanel.Instance.simulateLowEndDeviceSetting.gameObject.SetActive(false);
			DrawerCommandPanel.Instance.galleryEffectSetting.gameObject.SetActive(false);
			this.Show();
		}

		public void Show()
		{
			DateTimePicker8.Show(null);
		}
	}
}
