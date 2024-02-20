using System;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.GridSimulation
{
	public class RowCellView : MonoBehaviour
	{
		public void SetData(Data data)
		{
			this.container.SetActive(data != null);
			if (data != null)
			{
				this.text.text = data.someText;
			}
		}

		public GameObject container;

		public Text text;
	}
}
