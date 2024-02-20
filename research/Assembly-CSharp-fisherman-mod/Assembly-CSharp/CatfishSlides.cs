using System;

public class CatfishSlides : SlideUnit
{
	public CatfishSlides()
	{
		this.AvailableLevel = 3;
		this.Caption = "CatfishSlidesCaption";
		this.IsActive = true;
		this.Slides.Add(new Slide
		{
			Text = "CatfishSlide1Text",
			ImageName = "Lesson5-(CatFish)-Slide-1"
		});
		this.Slides.Add(new Slide
		{
			Text = "CatfishSlide2Text",
			ImageName = "Lesson5-(CatFish)-Slide-2"
		});
		this.Slides.Add(new Slide
		{
			Text = "CatfishSlide3Text",
			ImageName = "Lesson5-(CatFish)-Slide-3"
		});
		this.Slides.Add(new Slide
		{
			Text = "CatfishSlide4Text",
			ImageName = "Lesson5-(CatFish)-Slide-4"
		});
	}

	public override bool CanRun()
	{
		return PhotonConnectionFactory.Instance.Profile.Level >= 3 && StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId == 102 && StaticUserData.CurrentLocation != null && TutorialSlidesController.FishCounter >= 1;
	}
}
