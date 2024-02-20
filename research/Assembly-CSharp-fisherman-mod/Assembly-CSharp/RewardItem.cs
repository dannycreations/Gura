using System;
using Assets.Scripts.UI._2D.Helpers;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class RewardItem : MonoBehaviour
{
	public void FillData(InventoryItem item, int count = 1)
	{
		this.Title.text = ((count <= 1) ? item.Name : string.Format("{0} (x{1})", item.Name, count));
		this.thmLdbl.Image = this.Thumbnail;
		this.thmLdbl.Load(string.Format("Textures/Inventory/{0}", item.ThumbnailBID));
	}

	public void FillData(ShopLicense item)
	{
		this.Title.text = item.Name;
		this.thmLdbl.Image = this.Thumbnail;
		this.thmLdbl.Load(string.Format("Textures/Inventory/{0}", item.LogoBID));
	}

	public void FillData(StoreProduct item)
	{
		this.Title.text = item.Name;
		this.thmLdbl.Image = this.Thumbnail;
		this.thmLdbl.Load(string.Format("Textures/Inventory/{0}", item.ImageBID));
	}

	public static void SpawnItem(GameObject itemsRoot, GameObject itemPrefab, InventoryItem i)
	{
		RewardItem.SpawnItem(itemsRoot, itemPrefab).GetComponent<RewardItem>().FillData(i, 1);
	}

	public static void SpawnItem(GameObject itemsRoot, GameObject itemPrefab, ShopLicense i)
	{
		RewardItem.SpawnItem(itemsRoot, itemPrefab).GetComponent<RewardItem>().FillData(i);
	}

	public static void SpawnItem(GameObject itemsRoot, GameObject itemPrefab, StoreProduct i)
	{
		RewardItem.SpawnItem(itemsRoot, itemPrefab).GetComponent<RewardItem>().FillData(i);
	}

	private static GameObject SpawnItem(GameObject itemsRoot, GameObject itemPrefab)
	{
		GameObject gameObject = GUITools.AddChild(itemsRoot, itemPrefab);
		gameObject.SetActive(true);
		return gameObject;
	}

	public Text Title;

	public Image Thumbnail;

	private ResourcesHelpers.AsyncLoadableImage thmLdbl = new ResourcesHelpers.AsyncLoadableImage();
}
