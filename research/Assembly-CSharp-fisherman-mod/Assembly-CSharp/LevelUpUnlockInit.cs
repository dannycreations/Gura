using System;
using System.Collections.Generic;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUnlockInit : MonoBehaviour
{
	public int PondId { get; private set; }

	public void WaterwayUnlocked(Pond pond, string title)
	{
		this.PondId = pond.PondId;
		this.DisableAll();
		this._title.text = title.ToUpper();
		this.previews[0].SetActive(true);
		this.previews[0].GetComponent<Image>().overrideSprite = this._imagePond;
		this.previews[0].transform.Find("Text").gameObject.SetActive(true);
		this.previews[0].transform.Find("Text").GetComponent<Text>().text = WeatherHelper.GetPondWeatherIcon(pond);
		this.previews[0].transform.Find("Name").GetComponent<Text>().text = pond.Name + ", " + pond.State.Name;
	}

	public void InventoryPreviewItemsUnlocked(IEnumerable<InventoryItem> items, string title)
	{
		this.DisableAll();
		this._title.text = title.ToUpper();
		int num = 0;
		foreach (InventoryItem inventoryItem in items)
		{
			if (num == this.previews.Length)
			{
				break;
			}
			this.previews[num].SetActive(true);
			this.previews[num].transform.Find("Text").gameObject.SetActive(false);
			this.previews[num].transform.Find("Name").GetComponent<Text>().text = inventoryItem.Name;
			if (this.imgs.Count <= num)
			{
				this.imgs.Add(new ResourcesHelpers.AsyncLoadableImage());
			}
			this.imgs[num].Image = this.previews[num].GetComponent<Image>();
			this.imgs[num].Load(string.Format("Textures/Inventory/{0}", inventoryItem.ThumbnailBID));
			num++;
		}
	}

	public void InventoryCountableItemsUnlocked(Dictionary<string, int> countable, string title)
	{
		this.DisableAll();
		this._title.text = title.ToUpper();
		int num = 0;
		foreach (KeyValuePair<string, int> keyValuePair in countable)
		{
			this.countablePreviews[num].SetActive(true);
			this.countablePreviews[num].GetComponent<Text>().text = string.Format(keyValuePair.Key, keyValuePair.Value);
			num++;
		}
	}

	private void DisableAll()
	{
		for (int i = 0; i < this.previews.Length; i++)
		{
			this.previews[i].SetActive(false);
		}
		for (int j = 0; j < this.countablePreviews.Length; j++)
		{
			this.countablePreviews[j].SetActive(false);
		}
	}

	[SerializeField]
	private GameObject[] previews;

	[SerializeField]
	private GameObject[] countablePreviews;

	[SerializeField]
	private Text _title;

	[SerializeField]
	private Sprite _imagePond;

	private List<ResourcesHelpers.AsyncLoadableImage> imgs = new List<ResourcesHelpers.AsyncLoadableImage>();
}
