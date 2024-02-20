using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NavigationSwitcher : MonoBehaviour
{
	public void Switch()
	{
		if (EventSystem.current == null)
		{
			return;
		}
		GameObject currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
		this.from.enabled = false;
		this.to.enabled = true;
		if (this._selectedObject != null)
		{
			EventSystem.current.SetSelectedGameObject(this._selectedObject);
		}
		if (currentSelectedGameObject != null)
		{
			Selectable[] selectables = this.to.Selectables;
			Selectable selectable = null;
			switch (this.direction)
			{
			case SwitchDirection.Left:
				selectable = this.FindNearestDegree(currentSelectedGameObject.GetComponent<Selectable>(), selectables, new Vector3(0f, -1f, 0f));
				break;
			case SwitchDirection.Right:
				selectable = this.FindNearestDegree(currentSelectedGameObject.GetComponent<Selectable>(), selectables, new Vector3(0f, -1f, 0f));
				break;
			case SwitchDirection.Down:
				selectable = this.FindNearestDegree(currentSelectedGameObject.GetComponent<Selectable>(), selectables, new Vector3(0f, -1f, 0f));
				break;
			case SwitchDirection.Up:
				selectable = this.FindNearestDegree(currentSelectedGameObject.GetComponent<Selectable>(), selectables, new Vector3(0f, -1f, 0f));
				break;
			}
			if (selectable != null)
			{
				EventSystem.current.SetSelectedGameObject(selectable.gameObject);
			}
		}
	}

	private Selectable FindNearestDegree(Selectable findFor, Selectable[] selectables, Vector3 dir)
	{
		dir = dir.normalized;
		Vector3 vector = Quaternion.Inverse(findFor.transform.rotation) * dir;
		Vector3 vector2 = findFor.transform.TransformPoint(this.GetPointOnRectEdge(findFor.transform as RectTransform, vector));
		float num = float.NegativeInfinity;
		Selectable selectable = null;
		foreach (Selectable selectable2 in selectables)
		{
			if (!(selectable2 == findFor) && !(selectable2 == null) && !(selectable2.gameObject.GetComponent<IgnoredSelectable>() != null))
			{
				if (selectable2.IsInteractable() && selectable2.navigation.mode != null && !(selectable2.transform.lossyScale == Vector3.zero) && selectable2.IsInteractable() && selectable2.IsActive())
				{
					RectTransform rectTransform = selectable2.transform as RectTransform;
					Vector3 vector3 = ((!(rectTransform != null)) ? Vector3.zero : rectTransform.rect.center);
					Vector3 vector4 = selectable2.transform.TransformPoint(vector3) - vector2;
					float num2 = Vector3.Dot(dir, vector4);
					if (num2 > 0f)
					{
						float num3 = num2 / vector4.sqrMagnitude;
						if (num3 > num)
						{
							num = num3;
							selectable = selectable2;
						}
					}
				}
			}
		}
		return selectable;
	}

	private Vector3 GetPointOnRectEdge(RectTransform rect, Vector2 dir)
	{
		if (rect == null)
		{
			return Vector3.zero;
		}
		if (dir != Vector2.zero)
		{
			dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
		}
		dir = rect.rect.center + Vector2.Scale(rect.rect.size, dir * 0.5f);
		return dir;
	}

	[SerializeField]
	private UINavigation from;

	[SerializeField]
	private UINavigation to;

	[SerializeField]
	private SwitchDirection direction;

	[SerializeField]
	private GameObject _selectedObject;
}
