using System;
using UnityEngine;

namespace EnhancedUI.EnhancedScroller
{
	public class EnhancedScrollerCellView : MonoBehaviour
	{
		public virtual void RefreshCellView()
		{
		}

		public string cellIdentifier;

		[NonSerialized]
		public int cellIndex;

		[NonSerialized]
		public int dataIndex;

		[NonSerialized]
		public bool active;
	}
}
