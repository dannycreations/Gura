using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.MultiplePrefabsExample
{
	[ExecuteInEditMode]
	public class SliderItemBehaviour : MonoBehaviour
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action<float> ValueChanged;

		public float Value
		{
			get
			{
				return this._Slider.value;
			}
			set
			{
				this._Slider.value = value;
			}
		}

		private void Awake()
		{
			this._Value = base.transform.Find("ValueText").GetComponentInChildren<Text>();
			this._Slider = base.GetComponentInChildren<Slider>();
			if (Application.isPlaying)
			{
				this._Slider.onValueChanged.AddListener(new UnityAction<float>(this.OnValueChanged));
				this.OnValueChanged(this._Slider.value);
			}
		}

		private void OnValueChanged(float value)
		{
			this._Value.text = value.ToString("0.00");
			if (this.ValueChanged != null)
			{
				this.ValueChanged(value);
			}
		}

		private Text _Value;

		private Slider _Slider;
	}
}
