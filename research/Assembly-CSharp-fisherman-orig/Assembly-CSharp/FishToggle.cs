using System;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class FishToggle : MonoBehaviour
{
	public Toggle Toggle
	{
		get
		{
			return this._toggle;
		}
	}

	public void Init(Fish fish, string types)
	{
		this._fish = fish;
		FishCategory fishCategory = CacheLibrary.MapCache.GetFishCategory(fish.CategoryId);
		string text = ((fishCategory != null) ? fishCategory.Name.ToUpper() : string.Empty);
		if (types.Contains("#B9B9B9FF"))
		{
			text = string.Format("<color=\"#B9B9B9FF\">{0}</color>", text);
		}
		this._name.text = text;
		this._types.text = types;
		this._imageLoadable.Image = this._image;
		this._imageLoadable.Load((fish.ThumbnailBID == null) ? string.Empty : string.Format("Textures/Inventory/{0}", fish.ThumbnailBID));
	}

	[SerializeField]
	private Text _name;

	[SerializeField]
	private Text _types;

	[SerializeField]
	private Image _image;

	private ResourcesHelpers.AsyncLoadableImage _imageLoadable = new ResourcesHelpers.AsyncLoadableImage();

	[SerializeField]
	private Toggle _toggle;

	private Fish _fish;
}
