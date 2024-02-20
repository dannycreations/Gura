using System;
using Assets.Scripts.Common.Managers.Helpers;

public class InventorySlides : SlideUnit
{
	public InventorySlides()
	{
		this.AvailableLevel = 2;
		this.Caption = "InventorySlidesCaption";
		this.IsActive = true;
		this.Slides.Add(new Slide
		{
			Text = "InventorySlide1Text",
			ImageName = "Lesson1-(Inventory)-Slide-1"
		});
		this.Slides.Add(new Slide
		{
			Text = "InventorySlide2Text",
			ImageName = "Lesson1-(Inventory)-Slide-2"
		});
		this.Slides.Add(new Slide
		{
			Text = "InventorySlide3Text",
			ImageName = "Tutorial1Slide1ImageName",
			IsLocalized = true
		});
		this.Slides.Add(new Slide
		{
			Text = "InventorySlide4Text",
			ImageName = "Lesson1-(Inventory)-Slide-4"
		});
	}

	public override bool CanRun()
	{
		return StaticUserData.CurrentPond != null && InventorySlides._helpers.MenuPrefabsList != null && InventorySlides._helpers.MenuPrefabsList.shopForm != null && InventorySlides._helpers.MenuPrefabsList.shopFormAS.isActive;
	}

	private static MenuHelpers _helpers = new MenuHelpers();
}
