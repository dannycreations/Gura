using System;
using System.Collections;
using EnhancedUI.EnhancedScroller;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.RemoteResourcesDemo
{
	public class CellView : EnhancedScrollerCellView
	{
		public void SetData(Data data)
		{
			base.StartCoroutine(this.LoadRemoteImage(data));
		}

		public IEnumerator LoadRemoteImage(Data data)
		{
			string path = data.imageUrl;
			WWW www = new WWW(path);
			yield return www;
			this.cellImage.sprite = Sprite.Create(www.texture, new Rect(0f, 0f, data.imageDimensions.x, data.imageDimensions.y), new Vector2(0f, 0f), data.imageDimensions.x);
			yield break;
		}

		public void ClearImage()
		{
			this.cellImage.sprite = this.defaultSprite;
		}

		public Image cellImage;

		public Sprite defaultSprite;
	}
}
