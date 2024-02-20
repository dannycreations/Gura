using System;
using System.Collections.Generic;

[Serializable]
public class TutorialSlideBlock
{
	public void SetUpTriggers()
	{
		for (int i = 0; i < this.TriggerTypes.Count; i++)
		{
			if (TutorialTrigger.Activators.ContainsKey(this.TriggerTypes[i]))
			{
				this.Triggers.Add(TutorialTrigger.Activators[this.TriggerTypes[i]]());
				this.Triggers[this.Triggers.Count - 1].AcceptArguments(this.TriggerArguments[i].Array);
			}
		}
	}

	public int ID;

	public SlideCategories Category;

	public string Name;

	public List<TutorialSlide> Slides = new List<TutorialSlide>();

	public List<string> TriggerTypes = new List<string>();

	public List<TutorialTrigger> Triggers = new List<TutorialTrigger>();

	public List<StringListWrapper> TriggerArguments = new List<StringListWrapper>();
}
