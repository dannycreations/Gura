using System;
using Assets.Scripts.Common.Managers.Helpers;

public class TravelSlides : SlideUnit
{
	public TravelSlides()
	{
		this.AvailableLevel = 2;
		this.Caption = "TravelsSlidesCaption";
		this.IsActive = true;
		this.Slides.Add(new Slide
		{
			Text = "TravelSlide1Text",
			ImageName = "TutorialTravelSlide1ImageName",
			IsLocalized = true
		});
		this.Slides.Add(new Slide
		{
			Text = "TravelSlide2Text",
			ImageName = "Lesson3-(Travel)-Slide-2"
		});
		this.Slides.Add(new Slide
		{
			Text = "TravelSlide3Text",
			ImageName = "Lesson3-(Travel)-Slide-3"
		});
		this.Slides.Add(new Slide
		{
			Text = "TravelSlide4Text",
			ImageName = "Lesson3-(Travel)-Slide-4"
		});
	}

	public override bool CanRun()
	{
		if (TravelSlides._helpers.MenuPrefabsList != null && TravelSlides._helpers.MenuPrefabsList.globalMapForm != null)
		{
			if (TravelSlides._helpers.MenuPrefabsList.globalMapFormAS.isActive && !this.wasActive)
			{
				TravelSlides.Count++;
			}
			this.wasActive = TravelSlides._helpers.MenuPrefabsList.globalMapFormAS.isActive;
		}
		return StaticUserData.CurrentPond == null && TravelSlides.Count > 1 && TravelSlides._helpers.MenuPrefabsList != null && TravelSlides._helpers.MenuPrefabsList.globalMapForm != null && TravelSlides._helpers.MenuPrefabsList.globalMapFormAS.CanRun && (TravelSlides._pondHelpers == null || TravelSlides._pondHelpers.PondControllerList == null) && InfoMessageController.Instance.currentMessage == null;
	}

	private static MenuHelpers _helpers = new MenuHelpers();

	private static PondHelpers _pondHelpers = new PondHelpers();

	public static int Count = 0;

	private bool wasActive;
}
