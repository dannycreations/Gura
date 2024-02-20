using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorCodedButton : Button
{
	public override void OnPointerEnter(PointerEventData eventData)
	{
		base.OnPointerEnter(eventData);
		if (this.codedObjects != null)
		{
			foreach (GraphicColorAssignment graphicColorAssignment in this.codedObjects)
			{
				if (graphicColorAssignment != null && base.IsHighlighted(eventData) && this.IsInteractable())
				{
					graphicColorAssignment.observedObject.color = graphicColorAssignment.highlightedColor;
				}
			}
		}
	}

	public override void OnPointerExit(PointerEventData eventData)
	{
		base.OnPointerExit(eventData);
		if (this.codedObjects != null)
		{
			foreach (GraphicColorAssignment graphicColorAssignment in this.codedObjects)
			{
				if (graphicColorAssignment != null && !base.IsHighlighted(eventData))
				{
					graphicColorAssignment.observedObject.color = graphicColorAssignment.normalColor;
				}
			}
		}
		this.OnDeselect(new PointerEventData(EventSystem.current));
	}

	public override void OnPointerDown(PointerEventData eventData)
	{
		base.OnPointerDown(eventData);
		if (this.codedObjects != null)
		{
			foreach (GraphicColorAssignment graphicColorAssignment in this.codedObjects)
			{
				if (graphicColorAssignment != null && !base.IsHighlighted(eventData))
				{
					graphicColorAssignment.observedObject.color = graphicColorAssignment.pressedColor;
				}
			}
		}
	}

	public override void OnPointerUp(PointerEventData eventData)
	{
		base.OnPointerUp(eventData);
		if (this.codedObjects != null)
		{
			foreach (GraphicColorAssignment graphicColorAssignment in this.codedObjects)
			{
				if (graphicColorAssignment != null && base.IsHighlighted(eventData) && this.IsInteractable())
				{
					graphicColorAssignment.observedObject.color = graphicColorAssignment.highlightedColor;
				}
			}
		}
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		if (this.codedObjects != null)
		{
			foreach (GraphicColorAssignment graphicColorAssignment in this.codedObjects)
			{
				if (graphicColorAssignment != null && !base.IsHighlighted(eventData))
				{
					graphicColorAssignment.observedObject.color = graphicColorAssignment.normalColor;
				}
			}
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if (this.codedObjects != null)
		{
			foreach (GraphicColorAssignment graphicColorAssignment in this.codedObjects)
			{
				if (graphicColorAssignment != null && base.IsHighlighted(eventData))
				{
					graphicColorAssignment.observedObject.color = graphicColorAssignment.highlightedColor;
				}
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (this.codedObjects != null)
		{
			foreach (GraphicColorAssignment graphicColorAssignment in this.codedObjects)
			{
				if (graphicColorAssignment != null)
				{
					switch (base.currentSelectionState)
					{
					case 0:
						graphicColorAssignment.observedObject.color = graphicColorAssignment.normalColor;
						break;
					case 1:
						graphicColorAssignment.observedObject.color = graphicColorAssignment.highlightedColor;
						break;
					case 2:
						graphicColorAssignment.observedObject.color = graphicColorAssignment.pressedColor;
						break;
					case 3:
						graphicColorAssignment.observedObject.color = graphicColorAssignment.disabedColor;
						break;
					}
				}
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == base.gameObject)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	public void Highlight()
	{
		if (base.currentSelectionState == 1)
		{
			return;
		}
		this.DoStateTransition(1, true);
		if (this.codedObjects != null)
		{
			foreach (GraphicColorAssignment graphicColorAssignment in this.codedObjects)
			{
				if (graphicColorAssignment != null)
				{
					graphicColorAssignment.observedObject.color = graphicColorAssignment.highlightedColor;
				}
			}
		}
	}

	public void Press()
	{
		if (base.currentSelectionState == 2)
		{
			return;
		}
		this.DoStateTransition(2, true);
		if (this.codedObjects != null)
		{
			foreach (GraphicColorAssignment graphicColorAssignment in this.codedObjects)
			{
				if (graphicColorAssignment != null)
				{
					graphicColorAssignment.observedObject.color = graphicColorAssignment.pressedColor;
				}
			}
		}
	}

	public void SetHighlighted(bool state)
	{
		if (state)
		{
			this.Highlight();
		}
		else
		{
			this.Deselect();
		}
	}

	public void SetPressed(bool state)
	{
		if (state)
		{
			this.Press();
		}
		else
		{
			this.Deselect();
		}
	}

	public void Deselect()
	{
		if (base.currentSelectionState == 1)
		{
			return;
		}
		this.DoStateTransition(0, true);
		if (this.codedObjects != null)
		{
			foreach (GraphicColorAssignment graphicColorAssignment in this.codedObjects)
			{
				if (graphicColorAssignment != null)
				{
					graphicColorAssignment.observedObject.color = graphicColorAssignment.normalColor;
				}
			}
		}
	}

	[SerializeField]
	private List<GraphicColorAssignment> codedObjects = new List<GraphicColorAssignment>();
}
