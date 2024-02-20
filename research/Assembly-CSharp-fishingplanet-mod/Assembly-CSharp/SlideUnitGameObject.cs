using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SlideUnitGameObject : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<ConcreteSlideEventArgs> OnClickSelected;

	public void OnSelected()
	{
		if (this.OnClickSelected != null)
		{
			this.OnClickSelected(this, new ConcreteSlideEventArgs
			{
				ConcreteSlideBlock = this.ConcreteSlideBlock
			});
		}
	}

	private void Start()
	{
		base.GetComponent<Toggle>().onValueChanged.AddListener(new UnityAction<bool>(this.OnChanged));
	}

	private void OnChanged(bool value)
	{
		if (value)
		{
			this.OnSelected();
		}
	}

	public TutorialSlideBlock ConcreteSlideBlock;
}
