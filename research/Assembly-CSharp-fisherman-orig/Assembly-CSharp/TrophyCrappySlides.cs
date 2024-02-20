using System;

public class TrophyCrappySlides : SlideUnit
{
	public TrophyCrappySlides()
	{
		this.AvailableLevel = 2;
		this.Caption = "TrophyCrappieSlidesCaption";
		this.IsActive = true;
		this.Slides.Add(new Slide
		{
			Text = "CrappieSlide1Text",
			ImageName = "Lesson4-(TrophyCrappie)-Slide-1"
		});
		this.Slides.Add(new Slide
		{
			Text = "CrappieSlide2Text",
			ImageName = "Lesson4-(TrophyCrappie)-Slide-2"
		});
		this.Slides.Add(new Slide
		{
			Text = "CrappieSlide3Text",
			ImageName = "Lesson4-(TrophyCrappie)-Slide-3"
		});
		this.Slides.Add(new Slide
		{
			Text = "CrappieSlide4Text",
			ImageName = "Lesson4-(TrophyCrappie)-Slide-4"
		});
	}

	public override bool CanRun()
	{
		return StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId == 102 && StaticUserData.CurrentLocation != null && TutorialSlidesController.FishCounter >= 3;
	}
}
