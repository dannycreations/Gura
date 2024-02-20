using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TransitionRegion
{
	public TransitionRegion(Selectable[] Selectables, TransitionRegion.ContentNavigation Navigation, Selectable root = null, bool rememberLast = false)
	{
		if (Selectables != null)
		{
			this.selectables = Selectables;
			this.contentNavigation = Navigation;
			root = this.selectableRoot;
			this.rememberLast = rememberLast;
			this.SortSelectables();
		}
	}

	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event Action OnClose;

	public void SetNavigationType(TransitionRegion.ContentNavigation nav)
	{
		this.contentNavigation = nav;
	}

	public void UpdateContent(Selectable[] newSelectables)
	{
		this.selectables = newSelectables;
		this.SortSelectables();
	}

	public void Close()
	{
		if (this.OnClose != null)
		{
			this.OnClose();
		}
	}

	public GameObject GetFirstGameObject(bool lastSelectedPrioritized = true)
	{
		if (this.rememberByPosition)
		{
			float num = float.PositiveInfinity;
			int num2 = -1;
			for (int i = 0; i < this.selectables.Length; i++)
			{
				if (this.selectables[i] != null && this.selectables[i].gameObject.GetComponent<IgnoredSelectable>() == null && this.selectables[i].transform.lossyScale != Vector3.zero && this.selectables[i].IsInteractable() && this.selectables[i].enabled && this.selectables[i].gameObject.activeInHierarchy)
				{
					float sqrMagnitude = (this.selectables[i].transform.position - this.lastPosition).sqrMagnitude;
					if (sqrMagnitude < num)
					{
						num = sqrMagnitude;
						num2 = i;
					}
				}
			}
			return (num2 < 0) ? null : this.selectables[num2].gameObject;
		}
		if (lastSelectedPrioritized && this.previousSelectable != null && this.previousSelectable.transform.lossyScale != Vector3.zero && this.previousSelectable.IsInteractable() && this.previousSelectable.IsActive())
		{
			return this.previousSelectable.gameObject;
		}
		for (int j = this.selectables.Length - 1; j >= 0; j--)
		{
			if (this.selectables[j] != null && this.selectables[j].gameObject.GetComponent<IgnoredSelectable>() == null && this.selectables[j].transform.lossyScale != Vector3.zero && this.selectables[j].IsInteractable() && this.selectables[j].enabled && this.selectables[j].gameObject.activeInHierarchy)
			{
				return this.selectables[j].gameObject;
			}
		}
		return null;
	}

	public void ForceUpdate(Selectable currentSelectable)
	{
		if (!object.ReferenceEquals(currentSelectable, null))
		{
			this.previousSelectable = currentSelectable;
			if (this.contentNavigation == TransitionRegion.ContentNavigation.None || this.selectables.Length < 2)
			{
				return;
			}
			if (this.contentNavigation == TransitionRegion.ContentNavigation.Horizontal)
			{
				this.SetUpHorizontal();
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.Vertical)
			{
				this.SetUpVertical();
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.HorizontalAndVertical)
			{
				this.SetUpHorizontal();
				this.SetUpVertical();
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.Nearset)
			{
				this.SetUpHorizontalStrict(false);
				this.SetUpVerticalStrict(false);
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.NearestHorizontalNonCircled)
			{
				this.SetUpHorizontalStrictNonCircled();
				this.SetUpVerticalStrict(false);
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.NonStrict)
			{
				this.SetUpHorizontalStrict(false);
				this.SetUpVerticalNonStrict();
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.TrueNonStrict)
			{
				this.SetUpHorizontal();
				this.SetUpVerticalNonStrict();
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.HorizontalHeightBased)
			{
				this.SetUpHorizontalHeightBased();
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.VerticalWidthBased)
			{
				this.SetUpHorizontal();
				this.SetUpVerticalStrict(false);
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.VerticalSRIA)
			{
				this.SetUpHorizontalStrict(true);
				this.SetUpVerticalStrict(true);
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.WidthAndHeightBased)
			{
				this.SetUpSimpleHorizontal();
				this.SetUpVerticalWidthStrict();
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.CustomVertical)
			{
				this.SetUpVerticalStrict(false);
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.CustomHorizontal)
			{
				this.SetUpHorizontalStrict(false);
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.HorizontalVerticalStrict)
			{
				this.SetUpHorizontalHeightStrict();
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.VerticalCustomWidthBased)
			{
				this.SetUpVerticalWidthStrict();
			}
		}
	}

	public void ForceUpdateWithAxis(Selectable currentSelectable, Vector2 direction)
	{
		if (!object.ReferenceEquals(currentSelectable, null))
		{
			this.previousSelectable = currentSelectable;
			if (this.contentNavigation == TransitionRegion.ContentNavigation.SmallestDegree)
			{
				this.SetUpSmallestDegree(direction);
			}
			else if (this.contentNavigation == TransitionRegion.ContentNavigation.SmallestDegreeCircled)
			{
				this.SetUpWithDirection(direction, false);
			}
		}
	}

	public void Update(Selectable currentSelectable)
	{
		if (object.ReferenceEquals(currentSelectable, null) && !object.ReferenceEquals(currentSelectable, this.previousSelectable))
		{
			this.previousSelectable = currentSelectable;
		}
		if (currentSelectable == null || currentSelectable.transform.lossyScale == Vector3.zero || !currentSelectable.IsInteractable() || !currentSelectable.IsActive())
		{
			if (this.previousSelectable == null)
			{
				GameObject firstGameObject = this.GetFirstGameObject(true);
				if (firstGameObject != null)
				{
					currentSelectable = firstGameObject.GetComponent<Selectable>();
					EventSystem.current.SetSelectedGameObject(firstGameObject);
				}
			}
			else
			{
				currentSelectable = this.FindNearest(this.previousSelectable);
				if (currentSelectable != null)
				{
					EventSystem.current.SetSelectedGameObject(currentSelectable.gameObject);
				}
			}
		}
	}

	public void RememberLast(Selectable last, bool byPosition = false)
	{
		this.previousSelectable = last;
		this.rememberByPosition = byPosition;
		if (byPosition)
		{
			this.lastPosition = this.previousSelectable.transform.position;
		}
	}

	private void SortSelectables()
	{
		Vector3[] corners = new Vector3[4];
		Vector2 vect1 = Vector2.one;
		Vector2 vect2 = Vector2.one;
		Vector2 upCorner = Vector2.one;
		Array.Sort<Selectable>(this.selectables, delegate(Selectable item1, Selectable item2)
		{
			if (item1 != null && item2 != null)
			{
				(item1.transform as RectTransform).GetWorldCorners(corners);
				vect1.x = corners[0].x;
				vect1.y = corners[2].y;
				(item2.transform as RectTransform).GetWorldCorners(corners);
				vect2.x = corners[0].x;
				vect2.y = corners[2].y;
				upCorner.x = ((vect1.x <= vect2.x) ? vect1.x : vect2.x);
				upCorner.y = ((vect1.y <= vect2.y) ? vect2.y : vect1.y);
				float sqrMagnitude = (upCorner - vect1).sqrMagnitude;
				return (upCorner - vect2).sqrMagnitude.CompareTo(sqrMagnitude);
			}
			if (item1 == item2)
			{
				return 0;
			}
			if (item1 == null)
			{
				return 1;
			}
			if (item1 == item2)
			{
				return 0;
			}
			if (item1 == null)
			{
				return 1;
			}
			return -1;
		});
	}

	private void SetUpHorizontal()
	{
		Navigation navigation = this.previousSelectable.navigation;
		navigation.mode = 4;
		navigation.selectOnRight = this.FindSelectable(this.previousSelectable.transform.rotation * Vector3.right, this.previousSelectable, this.selectables);
		navigation.selectOnLeft = this.FindSelectable(this.previousSelectable.transform.rotation * Vector3.left, this.previousSelectable, this.selectables);
		this.previousSelectable.navigation = navigation;
	}

	private void SetUpVertical()
	{
		Navigation navigation = this.previousSelectable.navigation;
		navigation.mode = 4;
		navigation.selectOnUp = this.FindSelectable(this.previousSelectable.transform.rotation * Vector3.up, this.previousSelectable, this.selectables);
		navigation.selectOnDown = this.FindSelectable(this.previousSelectable.transform.rotation * Vector3.down, this.previousSelectable, this.selectables);
		this.previousSelectable.navigation = navigation;
	}

	private void SetUpSmallestDegree(Vector2 direction)
	{
		Navigation navigation = this.previousSelectable.navigation;
		navigation.mode = 4;
		Selectable selectable = this.FindNearestDegree(this.previousSelectable, this.previousSelectable.transform.rotation * direction);
		navigation.selectOnUp = selectable;
		navigation.selectOnDown = selectable;
		navigation.selectOnLeft = selectable;
		navigation.selectOnRight = selectable;
		this.previousSelectable.navigation = navigation;
	}

	private void SetUpWithDirection(Vector2 dir, bool ignoreLast = false)
	{
		dir = dir.normalized;
		RectTransform component = this.previousSelectable.GetComponent<RectTransform>();
		Vector3[] corners = new Vector3[4];
		component.GetWorldCorners(corners);
		Vector2 center1 = this.FormRect(corners).center;
		float extentsPrecisionFactor = Mathf.Max(component.rect.width, component.rect.height);
		extentsPrecisionFactor *= extentsPrecisionFactor;
		float extentsMax = extentsPrecisionFactor * 4f;
		List<Selectable> list = new List<Selectable>();
		List<Selectable> list2 = new List<Selectable>();
		foreach (Selectable selectable in this.selectables)
		{
			if (!(selectable == this.previousSelectable) && !(selectable == null) && !(selectable.gameObject.GetComponent<IgnoredSelectable>() != null))
			{
				if (selectable.IsInteractable() && !(selectable.transform.lossyScale == Vector3.zero) && selectable.gameObject.activeInHierarchy)
				{
					selectable.GetComponent<RectTransform>().GetWorldCorners(corners);
					Vector2 center = this.FormRect(corners).center;
					float num = Vector2.Dot(dir, (center - center1).normalized);
					if (num > 0.5f)
					{
						list.Add(selectable);
					}
					else if (num < -0.5f)
					{
						list2.Add(selectable);
					}
				}
			}
		}
		list.Sort(delegate(Selectable x, Selectable y)
		{
			x.GetComponent<RectTransform>().GetWorldCorners(corners);
			Vector2 center3 = this.FormRect(corners).center;
			y.GetComponent<RectTransform>().GetWorldCorners(corners);
			Vector2 center4 = this.FormRect(corners).center;
			float num5 = Vector2.Dot(dir, (center3 - center1).normalized);
			float num6 = Vector2.Dot(dir, (center4 - center1).normalized);
			float num7 = Vector2.SqrMagnitude(center3 - center1);
			float num8 = Vector2.SqrMagnitude(center4 - center1);
			float num9 = Mathf.Max(0f, Mathf.Min(1f, (num7 - extentsPrecisionFactor) / (extentsMax - extentsPrecisionFactor)));
			float num10 = Mathf.Max(0f, Mathf.Min(1f, (num8 - extentsPrecisionFactor) / (extentsMax - extentsPrecisionFactor)));
			num5 *= 1f - num9;
			num6 *= 1f - num10;
			return ((1f - num5) * num7).CompareTo((1f - num6) * num8);
		});
		list2.Sort(delegate(Selectable x, Selectable y)
		{
			x.GetComponent<RectTransform>().GetWorldCorners(corners);
			Vector2 center5 = this.FormRect(corners).center;
			y.GetComponent<RectTransform>().GetWorldCorners(corners);
			Vector2 center6 = this.FormRect(corners).center;
			float num11 = Vector2.Dot(-dir, (center5 - center1).normalized);
			float num12 = Vector2.Dot(-dir, (center6 - center1).normalized);
			return ((1f - num11) * Vector2.SqrMagnitude(center5 - center1)).CompareTo((1f - num12) * Vector2.SqrMagnitude(center6 - center1));
		});
		MonoBehaviour.print("-------------------------------------------");
		MonoBehaviour.print("From:");
		MonoBehaviour.print(component.parent.name + "/" + component.name);
		MonoBehaviour.print("To:");
		foreach (Selectable selectable2 in list)
		{
			selectable2.GetComponent<RectTransform>().GetWorldCorners(corners);
			Vector2 center2 = this.FormRect(corners).center;
			float num2 = Vector2.Dot(dir, (center2 - center1).normalized);
			float num3 = Vector2.SqrMagnitude(center2 - center1);
			float num4 = Mathf.Max(0f, Mathf.Min(1f, (num3 - extentsPrecisionFactor) / (extentsMax - extentsPrecisionFactor)));
			MonoBehaviour.print(selectable2.transform.parent.name + "/" + selectable2.transform.name);
			MonoBehaviour.print(string.Format("dot: {0}\n sqr distance:{1}\n precision loss factor:{2}\nvalue{3}", new object[]
			{
				num2,
				num3,
				1f - num4,
				(1f - num2 * (1f - num4)) * num3
			}));
		}
		Selectable selectable3 = ((list.Count <= 0) ? null : list[0]);
		object obj = ((list2.Count <= 0) ? null : list2[0]);
		Selectable selectable4 = ((list2.Count <= 0) ? null : list2[list2.Count - 1]);
		object obj2 = ((list.Count <= 0) ? null : list[list.Count - 1]);
		Navigation navigation = this.previousSelectable.navigation;
		navigation.mode = 4;
		navigation.selectOnRight = selectable3 ?? ((!ignoreLast) ? selectable4 : null);
		navigation.selectOnLeft = navigation.selectOnRight;
		navigation.selectOnUp = navigation.selectOnRight;
		navigation.selectOnDown = navigation.selectOnRight;
		this.previousSelectable.navigation = navigation;
	}

	private void SetUpNearest()
	{
		Selectable selectable = this.previousSelectable;
		Selectable selectable2 = null;
		float num = float.PositiveInfinity;
		RectTransform rectTransform = selectable.GetComponent<RectTransform>();
		Rect rect;
		rect..ctor(rectTransform.position.x - rectTransform.rect.size.x / 2f, rectTransform.position.y - rectTransform.rect.size.y / 2f, rectTransform.rect.size.x, rectTransform.rect.size.y);
		foreach (Selectable selectable3 in this.selectables)
		{
			if (!(selectable3 == null) && !(selectable3.gameObject.GetComponent<IgnoredSelectable>() != null) && !object.ReferenceEquals(selectable, selectable3) && !(selectable3.transform.lossyScale == Vector3.zero) && selectable3.IsInteractable() && selectable3.IsActive())
			{
				rectTransform = selectable3.GetComponent<RectTransform>();
				Rect rect2;
				rect2..ctor(rectTransform.position.x - rectTransform.rect.size.x / 2f, rectTransform.position.y - rectTransform.rect.size.y / 2f, rectTransform.rect.size.x, rectTransform.rect.size.y);
				float num2 = ((rect2.xMin >= rect.xMax) ? rect.xMax : rect2.xMin);
				float xMin = rect2.xMin;
				float num3 = rect2.yMax;
				float num4;
				if (rect.yMax > rect2.yMin)
				{
					num4 = rect.yMax;
					num3 = rect2.yMin;
				}
				else
				{
					num4 = ((rect.yMin >= rect2.yMax) ? num3 : rect.yMin);
				}
				Vector3 vector;
				vector..ctor(num2 - xMin, num4 - num3);
				float sqrMagnitude = vector.sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					selectable2 = selectable3;
				}
			}
		}
		if (selectable2 != null)
		{
			Navigation navigation = selectable.navigation;
			navigation.mode = 4;
			navigation.selectOnRight = selectable2;
			selectable.navigation = navigation;
		}
	}

	private Selectable FindNearestDegree(Selectable finFor, Vector2 dir)
	{
		dir = dir.normalized;
		Vector3 vector = Quaternion.Inverse(finFor.transform.rotation) * dir;
		Vector3 vector2 = finFor.transform.TransformPoint(this.GetPointOnRectEdge(finFor.transform as RectTransform, vector));
		float num = float.NegativeInfinity;
		Selectable selectable = null;
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Selectable selectable2 = this.selectables[i];
			if (!(selectable2 == finFor) && !(selectable2 == null) && !(selectable2.gameObject.GetComponent<IgnoredSelectable>() != null))
			{
				if (selectable2.IsInteractable() && !(selectable2.transform.lossyScale == Vector3.zero) && selectable2.IsActive())
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

	private Selectable FindNearest(Selectable finFor)
	{
		Selectable selectable = null;
		float num = float.PositiveInfinity;
		RectTransform rectTransform = finFor.GetComponent<RectTransform>();
		Rect rect;
		rect..ctor(rectTransform.position.x - rectTransform.rect.size.x / 2f, rectTransform.position.y - rectTransform.rect.size.y / 2f, rectTransform.rect.size.x, rectTransform.rect.size.y);
		foreach (Selectable selectable2 in this.selectables)
		{
			if (!(selectable2 == null) && !(selectable2.gameObject.GetComponent<IgnoredSelectable>() != null) && !object.ReferenceEquals(finFor, selectable2) && !(selectable2.transform.lossyScale == Vector3.zero) && selectable2.IsInteractable() && selectable2.IsActive())
			{
				rectTransform = selectable2.GetComponent<RectTransform>();
				Rect rect2;
				rect2..ctor(rectTransform.position.x - rectTransform.rect.size.x / 2f, rectTransform.position.y - rectTransform.rect.size.y / 2f, rectTransform.rect.size.x, rectTransform.rect.size.y);
				float num2 = ((rect2.xMin >= rect.xMax) ? rect.xMax : rect2.xMin);
				float xMin = rect2.xMin;
				float num3 = rect2.yMax;
				float num4;
				if (rect.yMax > rect2.yMin)
				{
					num4 = rect.yMax;
					num3 = rect2.yMin;
				}
				else
				{
					num4 = ((rect.yMin >= rect2.yMax) ? num3 : rect.yMin);
				}
				Vector3 vector;
				vector..ctor(num2 - xMin, num4 - num3);
				float sqrMagnitude = vector.sqrMagnitude;
				if (sqrMagnitude < num)
				{
					num = sqrMagnitude;
					selectable = selectable2;
				}
			}
		}
		return selectable;
	}

	private void SetUpVerticalStrict(bool ignoreLast = false)
	{
		Selectable selectable = null;
		Selectable selectable2 = null;
		Selectable selectable3 = null;
		Selectable selectable4 = null;
		Selectable selectable5 = this.previousSelectable;
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		float num3 = float.NegativeInfinity;
		float num4 = float.NegativeInfinity;
		RectTransform rectTransform = selectable5.GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Rect rect = this.FormRect(array);
		Vector2 center = rect.center;
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Selectable selectable6 = this.selectables[i];
			if (!(selectable6 == null) && !(selectable6.gameObject.GetComponent<IgnoredSelectable>() != null) && !object.ReferenceEquals(selectable5, selectable6) && !(selectable6.transform.lossyScale == Vector3.zero) && selectable6.IsInteractable() && selectable6.IsActive())
			{
				rectTransform = selectable6.GetComponent<RectTransform>();
				rectTransform.GetWorldCorners(array);
				Rect rect2 = this.FormRect(array);
				Vector2 center2 = rect2.center;
				if ((rect2.xMax >= rect.xMax && rect2.xMin <= rect.xMax) || (rect2.xMax >= rect.xMin && rect2.xMin <= rect.xMin) || (rect.xMax >= rect2.xMin && rect.xMin <= rect2.xMin))
				{
					float num5 = center2.y - center.y;
					if (num5 >= 0f)
					{
						num5 = Mathf.Min(Mathf.Abs(rect2.yMin - rect.yMax), num5);
						if (num5 <= num)
						{
							num = num5;
							selectable = selectable6;
						}
						if (num5 >= num3)
						{
							num3 = num5;
							selectable3 = selectable6;
						}
					}
					else
					{
						num5 = Mathf.Min(Mathf.Abs(num5), Mathf.Abs(rect.yMin - rect2.yMax));
						if (num5 <= num2)
						{
							num2 = num5;
							selectable2 = selectable6;
						}
						if (num5 >= num4)
						{
							num4 = num5;
							selectable4 = selectable6;
						}
					}
				}
			}
		}
		Navigation navigation = selectable5.navigation;
		navigation.mode = 4;
		navigation.selectOnDown = selectable2 ?? ((!ignoreLast) ? selectable3 : null);
		navigation.selectOnUp = selectable ?? ((!ignoreLast) ? selectable4 : null);
		selectable5.navigation = navigation;
	}

	private void SetUpVerticalNonStrict()
	{
		Selectable selectable = null;
		Selectable selectable2 = null;
		Selectable selectable3 = this.previousSelectable;
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		RectTransform rectTransform = selectable3.GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Rect rect = this.FormRect(array);
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Selectable selectable4 = this.selectables[i];
			if (!(selectable4 == null) && !(selectable4.gameObject.GetComponent<IgnoredSelectable>() != null) && !object.ReferenceEquals(selectable3, selectable4) && !(selectable4.transform.lossyScale == Vector3.zero) && selectable4.IsInteractable() && selectable4.IsActive())
			{
				rectTransform = selectable4.GetComponent<RectTransform>();
				rectTransform.GetWorldCorners(array);
				Rect rect2 = this.FormRect(array);
				if (rect2.yMin - rect.yMax >= -2f && (rect2.center - rect.center).sqrMagnitude <= num)
				{
					num = (rect2.center - rect.center).sqrMagnitude;
					selectable = selectable4;
				}
				if (rect.yMin - rect2.yMax >= -2f && (rect.center - rect2.center).sqrMagnitude <= num2)
				{
					num2 = (rect.center - rect2.center).sqrMagnitude;
					selectable2 = selectable4;
				}
			}
		}
		Navigation navigation = selectable3.navigation;
		navigation.mode = 4;
		navigation.selectOnDown = selectable2;
		navigation.selectOnUp = selectable;
		selectable3.navigation = navigation;
	}

	private void SetUpHorizontalStrict(bool ignoreLast = false)
	{
		Selectable selectable = null;
		Selectable selectable2 = null;
		Selectable selectable3 = null;
		Selectable selectable4 = null;
		Selectable selectable5 = this.previousSelectable;
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		float num3 = float.NegativeInfinity;
		float num4 = float.NegativeInfinity;
		RectTransform rectTransform = selectable5.GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Rect rect = this.FormRect(array);
		Vector2 center = rect.center;
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Selectable selectable6 = this.selectables[i];
			if (!(selectable6 == null) && !(selectable6.gameObject.GetComponent<IgnoredSelectable>() != null) && !object.ReferenceEquals(selectable5, selectable6) && !(selectable6.transform.lossyScale == Vector3.zero) && selectable6.IsInteractable() && selectable6.IsActive())
			{
				rectTransform = selectable6.GetComponent<RectTransform>();
				rectTransform.GetWorldCorners(array);
				Rect rect2 = this.FormRect(array);
				Vector2 center2 = rect2.center;
				if ((rect2.yMax >= rect.yMax && rect2.yMin <= rect.yMax) || (rect2.yMax >= rect.yMin && rect2.yMin <= rect.yMin) || (rect.yMax >= rect2.yMin && rect.yMin <= rect2.yMin))
				{
					float num5 = center2.x - center.x;
					if (num5 > 0f)
					{
						num5 = Mathf.Min(num5, Mathf.Abs(rect2.xMin - rect.xMax));
						if (num5 <= num)
						{
							num = num5;
							selectable = selectable6;
						}
						if (num5 > num3)
						{
							num3 = num5;
							selectable4 = selectable6;
						}
					}
					else
					{
						num5 = Mathf.Min(Mathf.Abs(rect.xMin - rect2.xMax), Mathf.Abs(num5));
						if (num5 <= num2)
						{
							num2 = num5;
							selectable2 = selectable6;
						}
						if (num5 > num4)
						{
							num4 = num5;
							selectable3 = selectable6;
						}
					}
				}
			}
		}
		Navigation navigation = selectable5.navigation;
		navigation.mode = 4;
		navigation.selectOnRight = selectable ?? ((!ignoreLast) ? selectable3 : null);
		navigation.selectOnLeft = selectable2 ?? ((!ignoreLast) ? selectable4 : null);
		selectable5.navigation = navigation;
	}

	private void SetUpHorizontalStrictNonCircled()
	{
		Selectable selectable = null;
		Selectable selectable2 = null;
		Selectable selectable3 = this.previousSelectable;
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		RectTransform rectTransform = selectable3.GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Rect rect = this.FormRect(array);
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Selectable selectable4 = this.selectables[i];
			if (!(selectable4 == null) && !(selectable4.gameObject.GetComponent<IgnoredSelectable>() != null) && !object.ReferenceEquals(selectable3, selectable4) && !(selectable4.transform.lossyScale == Vector3.zero) && selectable4.IsInteractable() && selectable4.IsActive())
			{
				rectTransform = selectable4.GetComponent<RectTransform>();
				rectTransform.GetWorldCorners(array);
				Rect rect2 = this.FormRect(array);
				if ((rect2.yMax >= rect.yMax && rect2.yMin <= rect.yMax) || (rect2.yMax >= rect.yMin && rect2.yMin <= rect.yMin) || (rect.yMax >= rect2.yMin && rect.yMin <= rect2.yMin))
				{
					if (rect2.xMin - rect.xMax >= -2f && Mathf.Abs(rect2.xMin - rect.xMax) <= num)
					{
						num = Mathf.Abs(rect2.xMin - rect.xMax);
						selectable = selectable4;
					}
					if (rect.xMin - rect2.xMax >= -2f && Mathf.Abs(rect.xMin - rect2.xMax) <= num2)
					{
						num2 = Mathf.Abs(rect.xMin - rect2.xMax);
						selectable2 = selectable4;
					}
				}
			}
		}
		Navigation navigation = selectable3.navigation;
		navigation.mode = 4;
		navigation.selectOnRight = selectable;
		navigation.selectOnLeft = selectable2;
		selectable3.navigation = navigation;
	}

	private void SetUpHorizontalHeightBased()
	{
		Selectable selectable = this.previousSelectable;
		Selectable selectable2 = null;
		Selectable selectable3 = null;
		Selectable selectable4 = null;
		Selectable selectable5 = null;
		float num = float.PositiveInfinity;
		float num2 = float.NegativeInfinity;
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		RectTransform rectTransform = selectable.GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Rect rect = this.FormRect(array);
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Selectable selectable6 = this.selectables[i];
			if (!(selectable6 == null) && !(selectable6.gameObject.GetComponent<IgnoredSelectable>() != null) && !object.ReferenceEquals(selectable, selectable6) && !(selectable6.transform.lossyScale == Vector3.zero) && selectable6.IsInteractable() && selectable6.IsActive())
			{
				rectTransform = selectable6.GetComponent<RectTransform>();
				rectTransform.GetWorldCorners(array);
				Rect rect2 = this.FormRect(array);
				if (rect.yMax > rect2.yMax || (rect.xMax - rect2.xMin <= 4f && rect.yMax >= rect2.yMax))
				{
					if (Mathf.Abs(rect.yMax - rect2.yMax) <= num)
					{
						num = Mathf.Abs(rect.yMax - rect2.yMax);
						selectable2 = selectable6;
					}
					if (Mathf.Abs(rect.yMax - rect2.yMax) > num2)
					{
						num2 = Mathf.Abs(rect.yMax - rect2.yMax);
						selectable5 = selectable6;
					}
				}
				if (rect.yMax < rect2.yMax || (rect2.xMax - rect.xMin <= 4f && rect.yMax <= rect2.yMax))
				{
					if (Mathf.Abs(rect.yMax - rect2.yMax) < num3)
					{
						num3 = Mathf.Abs(rect.yMax - rect2.yMax);
						selectable3 = selectable6;
					}
					if (Mathf.Abs(rect.yMax - rect2.yMax) >= num4)
					{
						num4 = Mathf.Abs(rect.yMax - rect2.yMax);
						selectable4 = selectable6;
					}
				}
			}
		}
		Navigation navigation = selectable.navigation;
		navigation.mode = 4;
		navigation.selectOnRight = (selectable2 ?? selectable4) ?? this.selectables[0];
		navigation.selectOnLeft = (selectable3 ?? selectable5) ?? this.selectables[0];
		selectable.navigation = navigation;
	}

	private void SetUpSimpleHorizontal()
	{
		Selectable selectable = null;
		Selectable selectable2 = null;
		Selectable selectable3 = null;
		Selectable selectable4 = null;
		Selectable selectable5 = this.previousSelectable;
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		float num3 = float.NegativeInfinity;
		float num4 = float.NegativeInfinity;
		RectTransform rectTransform = selectable5.GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Rect rect = this.FormRect(array);
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Selectable selectable6 = this.selectables[i];
			if (!(selectable6 == null) && !(selectable6.gameObject.GetComponent<IgnoredSelectable>() != null) && !object.ReferenceEquals(selectable5, selectable6) && !(selectable6.transform.lossyScale == Vector3.zero) && selectable6.IsInteractable() && selectable6.IsActive())
			{
				rectTransform = selectable6.GetComponent<RectTransform>();
				rectTransform.GetWorldCorners(array);
				Rect rect2 = this.FormRect(array);
				if (Mathf.Abs(rect2.center.y - rect.center.y) <= 10f)
				{
					if (rect2.xMin - rect.xMax >= -2f)
					{
						if (Mathf.Abs(rect2.xMin - rect.xMax) <= num)
						{
							num = Mathf.Abs(rect2.xMin - rect.xMax);
							selectable = selectable6;
						}
						if (Mathf.Abs(rect2.xMax - rect.xMin) > num3)
						{
							num3 = Mathf.Abs(rect2.xMax - rect.xMin);
							selectable4 = selectable6;
						}
					}
					if (rect.xMin - rect2.xMax >= -2f)
					{
						if (Mathf.Abs(rect.xMin - rect2.xMax) <= num2)
						{
							num2 = Mathf.Abs(rect.xMin - rect2.xMax);
							selectable2 = selectable6;
						}
						if (Mathf.Abs(rect.xMax - rect2.xMin) > num4)
						{
							num4 = Mathf.Abs(rect.xMax - rect2.xMin);
							selectable3 = selectable6;
						}
					}
				}
			}
		}
		Navigation navigation = selectable5.navigation;
		navigation.mode = 4;
		navigation.selectOnRight = selectable ?? selectable3;
		navigation.selectOnLeft = selectable2 ?? selectable4;
		selectable5.navigation = navigation;
	}

	private void SetUpHorizontalHeightStrict()
	{
		Selectable selectable = this.previousSelectable;
		Selectable selectable2 = null;
		Selectable selectable3 = null;
		Rect rect = default(Rect);
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		float num3 = float.PositiveInfinity;
		float num4 = float.PositiveInfinity;
		RectTransform rectTransform = selectable.GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Rect rect2 = this.FormRect(array);
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Selectable selectable4 = this.selectables[i];
			if (!(selectable4 == null) && !(selectable4.gameObject.GetComponent<IgnoredSelectable>() != null) && !object.ReferenceEquals(selectable, selectable4) && !(selectable4.transform.lossyScale == Vector3.zero) && selectable4.IsInteractable() && selectable4.IsActive())
			{
				rectTransform = selectable4.GetComponent<RectTransform>();
				rectTransform.GetWorldCorners(array);
				Rect rect3 = this.FormRect(array);
				if (rect3.xMin - rect2.xMax >= 4f && rect3.xMin - rect2.xMax <= num2 && rect3.yMax >= rect2.yMax)
				{
					if (rect3.xMin - rect2.xMax - num2 > -1f && rect3.xMin - rect2.xMax - num2 < 1f && Mathf.Abs(rect3.center.y - rect2.center.y) < Mathf.Abs(num - rect2.center.y))
					{
						num2 = rect3.xMin - rect2.xMax;
						selectable2 = selectable4;
						num = rect3.center.y;
					}
					if (selectable2 != null)
					{
						if (Mathf.Abs(rect.yMax - rect2.yMax) >= Mathf.Abs(rect3.yMax - rect2.yMax))
						{
							rect = rect3;
							selectable2 = selectable4;
							num2 = rect3.xMin - rect2.xMax;
							num = rect3.center.y;
						}
					}
					else
					{
						rect = rect3;
						selectable2 = selectable4;
						num2 = rect3.xMin - rect2.xMax;
						num = rect3.center.y;
					}
				}
				if (rect2.xMin - rect3.xMax >= 4f && rect2.xMin - rect3.xMax <= num4 && rect3.yMax >= rect2.yMax)
				{
					if (rect2.xMin - rect3.xMax - num4 > -1f && rect2.xMin - rect3.xMax - num4 < 1f)
					{
						if (Mathf.Abs(rect3.center.y - rect2.center.y) < Mathf.Abs(num3 - rect2.center.y))
						{
							num4 = rect2.xMin - rect3.xMax;
							selectable3 = selectable4;
							num3 = rect3.center.y;
						}
					}
					else
					{
						num4 = rect2.xMin - rect3.xMax;
						selectable3 = selectable4;
						num3 = rect3.center.y;
					}
				}
			}
		}
		Navigation navigation = selectable.navigation;
		navigation.mode = 4;
		navigation.selectOnRight = selectable2;
		navigation.selectOnLeft = selectable3;
		selectable.navigation = navigation;
	}

	private void SetUpVerticalWidthStrict()
	{
		Selectable selectable = null;
		Selectable selectable2 = null;
		Selectable selectable3 = null;
		Selectable selectable4 = null;
		Selectable selectable5 = this.previousSelectable;
		float num = float.PositiveInfinity;
		float num2 = float.PositiveInfinity;
		float num3 = float.NegativeInfinity;
		float num4 = float.NegativeInfinity;
		float num5 = float.PositiveInfinity;
		float num6 = float.PositiveInfinity;
		float num7 = float.PositiveInfinity;
		float num8 = float.PositiveInfinity;
		RectTransform rectTransform = selectable5.GetComponent<RectTransform>();
		Vector3[] array = new Vector3[4];
		rectTransform.GetWorldCorners(array);
		Rect rect = this.FormRect(array);
		for (int i = 0; i < this.selectables.Length; i++)
		{
			Selectable selectable6 = this.selectables[i];
			if (!(selectable6 == null) && !(selectable6.gameObject.GetComponent<IgnoredSelectable>() != null) && !object.ReferenceEquals(selectable5, selectable6) && !(selectable6.transform.lossyScale == Vector3.zero) && selectable6.IsInteractable() && selectable6.IsActive())
			{
				rectTransform = selectable6.GetComponent<RectTransform>();
				rectTransform.GetWorldCorners(array);
				Rect rect2 = this.FormRect(array);
				if (rect2.yMin - rect.yMax >= -2f)
				{
					if (Mathf.Abs(rect2.yMin - rect.yMax) <= num && (selectable == null || Mathf.Abs(rect2.xMin - rect.xMin) <= num5))
					{
						num = Mathf.Abs(rect2.yMin - rect.yMax);
						num5 = Mathf.Abs(rect2.xMin - rect.xMin);
						selectable = selectable6;
					}
					if (Mathf.Abs(rect2.yMax - rect.yMin) + 0.01f >= num3 && (selectable3 == null || Mathf.Abs(rect2.xMin - rect.xMin) <= num6))
					{
						num3 = Mathf.Abs(rect2.yMax - rect.yMin);
						num6 = Mathf.Abs(rect2.xMin - rect.xMin);
						selectable3 = selectable6;
					}
				}
				if (rect.yMin - rect2.yMax >= -2f)
				{
					if (Mathf.Abs(rect.yMin - rect2.yMax) <= num2 && (selectable2 == null || Mathf.Abs(rect2.xMin - rect.xMin) <= num7))
					{
						num2 = Mathf.Abs(rect.yMin - rect2.yMax);
						num7 = Mathf.Abs(rect2.xMin - rect.xMin);
						selectable2 = selectable6;
					}
					if (Mathf.Abs(rect.yMax - rect2.yMin) >= num4 && (selectable4 == null || Mathf.Abs(rect2.xMin - rect.xMin) <= num8))
					{
						num4 = Mathf.Abs(rect.yMax - rect2.yMin);
						num8 = Mathf.Abs(rect2.xMin - rect.xMin);
						selectable4 = selectable6;
					}
				}
			}
		}
		Navigation navigation = selectable5.navigation;
		navigation.mode = 4;
		navigation.selectOnDown = selectable2 ?? selectable3;
		navigation.selectOnUp = selectable ?? selectable4;
		selectable5.navigation = navigation;
	}

	private Selectable FindSelectable(Vector3 dir, Selectable findFor, Selectable[] selectables)
	{
		dir = dir.normalized;
		Vector3 vector = Quaternion.Inverse(findFor.transform.rotation) * dir;
		Vector3 vector2 = findFor.transform.TransformPoint(this.GetPointOnRectEdge(findFor.transform as RectTransform, vector));
		float num = float.PositiveInfinity;
		Selectable selectable = null;
		foreach (Selectable selectable2 in selectables)
		{
			if (!(selectable2 == this.selectableRoot) && !(selectable2 == null) && !(selectable2.gameObject.GetComponent<IgnoredSelectable>() != null) && !(selectable2 == findFor))
			{
				if (selectable2.IsInteractable() && !(selectable2.transform.lossyScale == Vector3.zero) && selectable2.IsActive())
				{
					RectTransform rectTransform = selectable2.transform as RectTransform;
					Vector3 vector3 = ((!(rectTransform != null)) ? Vector3.zero : rectTransform.rect.center);
					Vector3 vector4 = selectable2.transform.TransformPoint(vector3);
					Vector3 vector5 = vector4 - vector2;
					float num2 = Vector3.Dot(dir, vector5);
					if (num2 > 0f)
					{
						if (vector5.magnitude < num)
						{
							selectable = selectable2;
							num = vector5.magnitude;
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

	private Rect FormRect(Vector3[] corners)
	{
		return new Rect(corners[0].x, corners[0].y, (corners[2] - corners[0]).x, (corners[1] - corners[0]).y);
	}

	private Transform regionTransform;

	private Selectable previousSelectable;

	private Selectable selectableRoot;

	private Selectable[] selectables;

	private TransitionRegion.ContentNavigation contentNavigation;

	private bool rememberByPosition;

	private bool rememberLast;

	private Vector3 lastPosition;

	public enum ContentNavigation
	{
		Horizontal,
		Vertical,
		HorizontalAndVertical,
		Nearset,
		HorizontalHeightBased,
		VerticalWidthBased,
		WidthAndHeightBased,
		CustomVertical,
		CustomHorizontal,
		None,
		SmallestDegree,
		NonStrict,
		HorizontalVerticalStrict,
		NearestHorizontalNonCircled,
		VerticalCustomWidthBased,
		VerticalSRIA,
		SmallestDegreeCircled,
		TrueNonStrict
	}
}
