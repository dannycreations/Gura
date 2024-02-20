using System;

public class BassOnPopperSlides : SlideUnit
{
	public BassOnPopperSlides()
	{
		this.AvailableLevel = 4;
		this.Caption = "BassOnPopperSlidesCaption";
		this.IsActive = true;
		this.Slides.Add(new Slide
		{
			Text = "BassOnFloatSlide1Text",
			ImageName = "Lesson6-(Bass)-Slide-1"
		});
		this.Slides.Add(new Slide
		{
			Text = "BassOnFloatSlide2Text",
			ImageName = "Lesson6-(Bass)-Slide-2"
		});
		this.Slides.Add(new Slide
		{
			Text = "BassOnFloatSlide3Text",
			ImageName = "Lesson6-(Bass)-Slide-3"
		});
		this.Slides.Add(new Slide
		{
			Text = "BassOnFloatSlide4Text",
			ImageName = "Lesson6-(Bass)-Slide-4"
		});
		this.Slides.Add(new Slide
		{
			Text = "BassOnFloatSlide5Text",
			ImageName = "Lesson6-(Bass)-Slide-5"
		});
	}

	public override bool CanRun()
	{
		return PhotonConnectionFactory.Instance.Profile.Level >= 4 && StaticUserData.CurrentPond != null && StaticUserData.CurrentPond.PondId == 102 && StaticUserData.CurrentLocation != null && TutorialSlidesController.FishCounter >= 2;
	}
}
