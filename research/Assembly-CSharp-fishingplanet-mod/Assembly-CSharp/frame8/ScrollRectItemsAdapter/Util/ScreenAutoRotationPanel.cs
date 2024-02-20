using System;
using UnityEngine;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class ScreenAutoRotationPanel : MonoBehaviour
	{
		private void Awake()
		{
			Screen.autorotateToPortraitUpsideDown = false;
			Screen.autorotateToPortrait = this.allowPortrait;
			if (this.allowPortrait)
			{
				Screen.orientation = 5;
			}
			else
			{
				Screen.orientation = 3;
				base.gameObject.SetActive(false);
			}
		}

		public bool allowPortrait;
	}
}
