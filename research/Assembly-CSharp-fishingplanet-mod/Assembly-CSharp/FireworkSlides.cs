using System;
using System.Linq;
using Assets.Scripts.Common.Managers.Helpers;
using ObjectModel;

public class FireworkSlides : SlideUnit
{
	public FireworkSlides()
	{
		this.AvailableLevel = 2;
		this.Caption = "FireworkSlidesCaption";
		this.IsActive = true;
		this.Slides.Add(new Slide
		{
			Text = "FireworkSlide1Text",
			ImageName = "fireworks_tutor_1"
		});
		this.Slides.Add(new Slide
		{
			Text = "FireworkSlide2Text",
			ImageName = "fireworks_tutor_2"
		});
		this.Slides.Add(new Slide
		{
			Text = "FireworkSlide3Text",
			ImageName = "fireworks_tutor_3"
		});
	}

	public override bool CanRun()
	{
		if (StaticUserData.CurrentLocation != null && EventsController.CurrentEvent != null)
		{
			if (PhotonConnectionFactory.Instance.Profile.Inventory.Any((InventoryItem x) => x.ItemSubType == ItemSubTypes.Firework) && FireworkSlides._pondHelpers.PondControllerList != null)
			{
				return FireworkSlides._pondHelpers.PondControllerList.Game3DPond.activeSelf;
			}
		}
		return false;
	}

	private static PondHelpers _pondHelpers = new PondHelpers();
}
