using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropMeForwarding : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, IEventSystemHandler
{
	public void OnDrop(PointerEventData eventData)
	{
		if (this.DropMe != null)
		{
			this.DropMe.OnDrop(eventData);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (this.DropMe != null && this.DropMe.DragNDropTypeInst.IsActive)
		{
			this.DropMe.OnPointerEnter(eventData);
		}
		else if (base.GetComponent<ChangeColor>() != null)
		{
			base.GetComponent<ChangeColor>().SetColor(1);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (this.DropMe != null && this.DropMe.DragNDropTypeInst.IsActive)
		{
			this.DropMe.OnPointerExit(eventData);
		}
		else if (base.GetComponent<ChangeColor>() != null)
		{
			base.GetComponent<ChangeColor>().SetColor(0);
		}
	}

	public DropMeStorage DropMe;
}
