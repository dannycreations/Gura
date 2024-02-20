using System;
using UnityEngine;
using UnityEngine.UI;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class SimpleImageColorBouncer : MonoBehaviour
	{
		private void Start()
		{
			this._Graphic = base.GetComponent<Graphic>();
		}

		private void Update()
		{
			this._Graphic.color = Color.Lerp(this.a, this.b, (Mathf.Sin(Time.time / this._PeriodInSeconds) + 1f) / 2f);
		}

		public Color a;

		public Color b;

		[Range(0.001f, 10f)]
		[SerializeField]
		private float _PeriodInSeconds = 0.3f;

		private Graphic _Graphic;
	}
}
