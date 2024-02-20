using System;
using UnityEngine;

public class MenuLayout
{
	public MenuLayout(IScreen screen, int itemWidth, int itemFontSize)
	{
		this.owner = screen;
		this.numItems = 0;
		this.width = itemWidth;
		this.fontSize = itemFontSize;
	}

	public IScreen GetOwner()
	{
		return this.owner;
	}

	public void DoLayout()
	{
		this.numItems = this.currCount;
		this.style = new GUIStyle(GUI.skin.GetStyle("Button"));
		this.style.fontSize = this.fontSize;
		this.style.alignment = 4;
		this.styleSelected = new GUIStyle(GUI.skin.GetStyle("Button"));
		this.styleSelected.fontSize = this.fontSize + 8;
		this.styleSelected.alignment = 4;
		this.height = this.style.fontSize + 16;
		this.ySpace = 8;
		this.x = (Screen.width - this.width) / 2;
		this.y = (Screen.height - (this.height + this.ySpace) * this.numItems) / 2;
		this.currCount = 0;
	}

	public void SetSelectedItem(int index)
	{
		if (index < 0)
		{
			this.selectedItemIndex = 0;
		}
		if (index > this.numItems - 1)
		{
			this.selectedItemIndex = this.numItems - 1;
		}
	}

	public void ItemNext()
	{
		if (this.numItems > 0)
		{
			this.selectedItemIndex++;
			if (this.selectedItemIndex >= this.numItems)
			{
				this.selectedItemIndex = 0;
			}
		}
	}

	public void ItemPrev()
	{
		if (this.numItems > 0)
		{
			this.selectedItemIndex--;
			if (this.selectedItemIndex < 0)
			{
				this.selectedItemIndex = this.numItems - 1;
			}
		}
	}

	public void Update()
	{
		this.DoLayout();
		this.HandleInput();
	}

	public void HandleInput()
	{
		this.buttonPressed = false;
		this.backButtonPressed = false;
		float num = 0.3f;
		KeyCode keyCode = 360;
		KeyCode keyCode2 = 362;
		float num2 = Time.timeSinceLevelLoad - MenuLayout.timeOfLastChange;
		bool buttonDown = Input.GetButtonDown("Fire1");
		bool buttonDown2 = Input.GetButtonDown("Fire2");
		if (buttonDown && num2 > num)
		{
			this.buttonPressed = true;
			MenuLayout.timeOfLastChange = Time.timeSinceLevelLoad;
			return;
		}
		if (buttonDown2 && num2 > num)
		{
			this.backButtonPressed = true;
			MenuLayout.timeOfLastChange = Time.timeSinceLevelLoad;
			return;
		}
		float num3 = -Input.GetAxis("Vertical");
		bool flag = num3 > 0.1f || Input.GetKey(keyCode2);
		bool flag2 = num3 < -0.1f || Input.GetKey(keyCode);
		if (flag && num2 > num)
		{
			this.ItemNext();
			MenuLayout.timeOfLastChange = Time.timeSinceLevelLoad;
		}
		if (flag2 && num2 > num)
		{
			this.ItemPrev();
			MenuLayout.timeOfLastChange = Time.timeSinceLevelLoad;
		}
		if (!flag && !flag2 && !buttonDown && !buttonDown2)
		{
			MenuLayout.timeOfLastChange = 0f;
		}
	}

	private bool AddButton(string text, bool enabled = true, bool selected = false)
	{
		GUI.enabled = enabled;
		bool flag = GUI.Button(this.GetRect(), text, (!selected) ? this.style : this.styleSelected);
		this.y += this.height + this.ySpace;
		return flag;
	}

	public bool AddItem(string name, bool enabled = true)
	{
		bool flag = false;
		if (this.numItems > 0)
		{
			if (this.AddButton(name, enabled, this.selectedItemIndex == this.currCount))
			{
				this.selectedItemIndex = this.currCount;
				flag = true;
			}
			else if (this.buttonPressed && enabled && this.selectedItemIndex == this.currCount)
			{
				flag = true;
				this.buttonPressed = false;
			}
		}
		this.currCount++;
		return flag;
	}

	public bool AddBackIndex(string name, bool enabled = true)
	{
		bool flag = false;
		if (this.numItems > 0)
		{
			if (this.AddButton(name, enabled, this.selectedItemIndex == this.currCount))
			{
				this.selectedItemIndex = this.currCount;
				flag = true;
			}
			else if (this.buttonPressed && enabled && this.selectedItemIndex == this.currCount)
			{
				flag = true;
				this.buttonPressed = false;
			}
			else if (this.backButtonPressed && enabled)
			{
				flag = true;
				this.backButtonPressed = false;
			}
		}
		this.currCount++;
		return flag;
	}

	public Rect GetRect()
	{
		return new Rect((float)this.x, (float)this.y, (float)this.width, (float)this.height);
	}

	private int width;

	private int height;

	private int ySpace;

	private int x;

	private int y;

	private GUIStyle style;

	private GUIStyle styleSelected;

	private int selectedItemIndex;

	private bool buttonPressed;

	private bool backButtonPressed;

	private int numItems;

	private int fontSize = 16;

	private static float timeOfLastChange;

	private int currCount;

	private IScreen owner;
}
