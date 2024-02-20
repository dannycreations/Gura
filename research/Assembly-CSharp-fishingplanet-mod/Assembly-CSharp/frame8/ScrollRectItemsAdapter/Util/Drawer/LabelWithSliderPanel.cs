using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util.Drawer
{
	public class LabelWithSliderPanel : MonoBehaviour
	{
		public void Init(string label, string minLabel, string maxLabel)
		{
			this.labelText.text = label;
			this.minLabelText.text = minLabel;
			this.maxLabelText.text = maxLabel;
		}

		internal void Set(float min, float max, float val)
		{
			this.slider.minValue = min;
			this.slider.maxValue = max;
			UnityEvent<float> onValueChanged = this.slider.onValueChanged;
			this.slider.value = val;
			onValueChanged.Invoke(val);
		}

		public Text labelText;

		public Text minLabelText;

		public Text maxLabelText;

		public Slider slider;
	}
}
