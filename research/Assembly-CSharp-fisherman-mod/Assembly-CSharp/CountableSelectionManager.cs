using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class CountableSelectionManager : MonoBehaviour
{
	[field: DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public event EventHandler<EventCountabledArgs> OnAcceptClick;

	private void Start()
	{
		this.ScrollBar.numberOfSteps = (int)this.MaxValue;
		this.OnValueChanged();
	}

	public void IncClick()
	{
		if (this.Value < this.MaxValue)
		{
			this.Value += 1;
		}
		this.OnValueChanged();
	}

	public void DecClick()
	{
		if (this.Value > 1)
		{
			this.Value -= 1;
		}
		this.OnValueChanged();
	}

	public void OnValueChanged()
	{
		if (this.Value > 1)
		{
			this.ScrollBar.value = (float)this.Value * (1f / (float)this.MaxValue);
		}
		else
		{
			this.ScrollBar.value = 0f;
		}
		this.ValueText.text = this.Value.ToString();
	}

	public void OnScrollChanged()
	{
		if ((double)Math.Abs(this.ScrollBar.value) > 0.01)
		{
			this.Value = (short)(this.ScrollBar.value * (float)this.MaxValue);
		}
		else
		{
			this.Value = 1;
		}
		this.ValueText.text = this.Value.ToString();
	}

	public void AcceptClick()
	{
		if (this.OnAcceptClick != null)
		{
			this.OnAcceptClick(this, new EventCountabledArgs
			{
				Value = (int)this.Value
			});
		}
	}

	public short Value = 1;

	public short MaxValue = 10;

	public Text ValueText;

	public Scrollbar ScrollBar;
}
