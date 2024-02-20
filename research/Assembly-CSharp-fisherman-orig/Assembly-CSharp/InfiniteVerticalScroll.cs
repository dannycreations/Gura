using System;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteVerticalScroll : InfiniteScroll
{
	protected override void OnEnable()
	{
		base.OnEnable();
		base.content.localPosition = new Vector3(0f, 0f, 0f);
	}

	protected void FixedUpdate()
	{
		base.content.localPosition = new Vector3(0f, base.content.localPosition.y + 1f, 0f);
	}

	protected override float GetSize(RectTransform item)
	{
		return item.GetComponent<LayoutElement>().minHeight + base.content.GetComponent<VerticalLayoutGroup>().spacing;
	}

	protected override float GetDimension(Vector2 vector)
	{
		return vector.y;
	}

	protected override Vector2 GetVector(float value)
	{
		return new Vector2(0f, value);
	}

	protected override float GetPos(RectTransform item)
	{
		return item.localPosition.y + base.content.localPosition.y;
	}

	protected override int OneOrMinusOne()
	{
		return 1;
	}
}
