using System;

[Serializable]
public class TutorialSlide
{
	public int SequenceNumber;

	public SlideTextBlock PrimaryText = new SlideTextBlock();

	public SlideImageBlock PrimaryImage = new SlideImageBlock();

	public SlideTextBlock SecondaryText = new SlideTextBlock();

	public SlideImageBlock SecondaryImage = new SlideImageBlock();
}
