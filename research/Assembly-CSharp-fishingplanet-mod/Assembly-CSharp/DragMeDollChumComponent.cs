using System;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class DragMeDollChumComponent : DragMe
{
	protected override StoragePlaces GetCurrentActiveStorage()
	{
		return StoragePlaces.Storage;
	}

	protected override void SetDraggingIcon(Image image)
	{
		image.overrideSprite = this._sp;
	}

	public void SetIco(Sprite overrideSprite)
	{
		this._sp = overrideSprite;
	}

	private Sprite _sp;
}
