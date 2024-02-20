using System;
using Assets.Scripts.Common.Managers.Helpers;

public class ShopSlides : SlideUnit
{
	public ShopSlides()
	{
		this.AvailableLevel = 2;
		this.Caption = "ShopSlidesCaption";
		this.IsActive = true;
		this.Slides.Add(new Slide
		{
			Text = "ShopSlide1Text",
			ImageName = "Lesson2-(Store)-Slide-1"
		});
		this.Slides.Add(new Slide
		{
			Text = "ShopSlide2Text",
			ImageName = "Lesson2-(Store)-Slide-2"
		});
		this.Slides.Add(new Slide
		{
			Text = "ShopSlide3Text",
			ImageName = "Lesson2-(Store)-Slide-3"
		});
		this.Slides.Add(new Slide
		{
			Text = "ShopSlide4Text",
			ImageName = "Lesson2-(Store)-Slide-4"
		});
	}

	public override bool CanRun()
	{
		return StaticUserData.CurrentPond == null && ShopSlides._helpers.MenuPrefabsList != null && ShopSlides._helpers.MenuPrefabsList.shopForm != null && ShopSlides._helpers.MenuPrefabsList.shopFormAS.isActive && (ShopSlides._pondHelpers == null || ShopSlides._pondHelpers.PondControllerList == null);
	}

	private static MenuHelpers _helpers = new MenuHelpers();

	private static PondHelpers _pondHelpers = new PondHelpers();
}
