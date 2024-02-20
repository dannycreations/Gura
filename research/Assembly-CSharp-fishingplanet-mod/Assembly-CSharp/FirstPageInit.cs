using System;
using Assets.Scripts.UI._2D.Helpers;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class FirstPageInit : MonoBehaviour
{
	private void OnEnable()
	{
		int pondId = StaticUserData.CurrentPond.PondId;
		if (!FirstPageResourcesList.Values.ContainsKey(pondId))
		{
			return;
		}
		FirstPageResource firstPageResource = FirstPageResourcesList.Values[pondId];
		this.Header.Load(firstPageResource.HeaderImage);
		this.Footer.Load(firstPageResource.FooterImage);
		this.Center.Load(firstPageResource.MainImage);
		if (firstPageResource.LeftPanelSet.TypeSet == RequestTypeSet.CategorySet)
		{
			this.LeftButton.CategoryIds = firstPageResource.LeftPanelSet.Ids;
		}
		else
		{
			this.LeftButton.ItemIds = firstPageResource.LeftPanelSet.Ids;
		}
		this.LeftButtonText.text = ScriptLocalization.Get(firstPageResource.LeftPanelSet.Caption);
		if (firstPageResource.RightPanelSet.TypeSet == RequestTypeSet.CategorySet)
		{
			this.RightButton.CategoryIds = firstPageResource.RightPanelSet.Ids;
		}
		else
		{
			this.RightButton.ItemIds = firstPageResource.RightPanelSet.Ids;
		}
		this.RightButtonText.text = ScriptLocalization.Get(firstPageResource.RightPanelSet.Caption);
		if (StaticUserData.CurrentPond.PondId == 2)
		{
			this.LeftButton.GetComponent<Button>().enabled = false;
			this.RightButton.GetComponent<Button>().enabled = false;
		}
	}

	public ResourcesHelpers.AsyncLoadableImage Center;

	public ResourcesHelpers.AsyncLoadableImage Header;

	public ResourcesHelpers.AsyncLoadableImage Footer;

	public LocalShopMenuClick LeftButton;

	public LocalShopMenuClick RightButton;

	public Text LeftButtonText;

	public Text RightButtonText;
}
