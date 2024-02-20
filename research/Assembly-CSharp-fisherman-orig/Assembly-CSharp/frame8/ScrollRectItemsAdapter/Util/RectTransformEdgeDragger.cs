using System;
using System.Diagnostics;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine;
using UnityEngine.EventSystems;

namespace frame8.ScrollRectItemsAdapter.Util
{
	public class RectTransformEdgeDragger : MonoBehaviour, IDragHandler, IPointerDownHandler, IEventSystemHandler
	{
		[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
		public event Action TargetDragged;

		private void Awake()
		{
			this.rt = base.transform as RectTransform;
		}

		private void Start()
		{
			this.initialPos = this.rt.position;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			this.startDragPos = base.transform.position;
			this.startInset = this.draggedRectTransform.GetInsetFromParentEdge(this.draggedRectTransform.parent as RectTransform, this.draggedEdge);
			if (this.draggedEdge == null || this.draggedEdge == 1)
			{
				this.startSize = this.draggedRectTransform.rect.width;
			}
			else
			{
				this.startSize = this.draggedRectTransform.rect.height;
			}
		}

		public void OnDrag(PointerEventData ped)
		{
			Vector2 vector = ped.position - ped.pressPosition;
			Vector2 vector2 = this.startDragPos;
			float num;
			if (this.draggedEdge == null || this.draggedEdge == 1)
			{
				num = vector.x;
				vector2.x += num;
				float num2 = vector2.x - this.initialPos.x;
				if (Mathf.Abs(num2) > this.maxDistance)
				{
					return;
				}
			}
			else
			{
				num = vector.y;
				vector2.y += num;
				float num2 = vector2.y - this.initialPos.y;
				if (Mathf.Abs(num2) > this.maxDistance)
				{
					return;
				}
			}
			this.rt.position = vector2;
			this.draggedRectTransform.SetInsetAndSizeFromParentEdgeWithCurrentAnchors(this.draggedEdge, this.startInset + num, this.startSize - num);
			if (this.TargetDragged != null)
			{
				this.TargetDragged();
			}
		}

		public RectTransform draggedRectTransform;

		public RectTransform.Edge draggedEdge;

		public float maxDistance = 100f;

		private RectTransform rt;

		private Vector2 startDragPos;

		private Vector2 initialPos;

		private float startInset;

		private float startSize;
	}
}
