using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TabNavigation : MonoBehaviour
{
	private void Start()
	{
		this.system = EventSystem.current;
	}

	public void Update()
	{
		if (Input.GetKeyDown(9))
		{
			if (this.system.currentSelectedGameObject == null || this.system.currentSelectedGameObject.GetComponent<Selectable>() == null)
			{
				return;
			}
			Selectable selectable = this.system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
			if (selectable != null)
			{
				InputField component = selectable.GetComponent<InputField>();
				if (component != null)
				{
					component.OnPointerClick(new PointerEventData(this.system));
				}
				this.system.SetSelectedGameObject(selectable.gameObject, new BaseEventData(this.system));
			}
		}
	}

	private EventSystem system;
}
