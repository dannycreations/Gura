using System;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class ShowOnlyOnPortraitOrientation : MonoBehaviour
	{
		private void Update()
		{
			if (Screen.height > Screen.width)
			{
				if (!this.target.activeSelf)
				{
					this.target.SetActive(true);
				}
			}
			else if (this.target.activeSelf)
			{
				this.target.SetActive(false);
			}
		}

		public GameObject target;
	}
}
