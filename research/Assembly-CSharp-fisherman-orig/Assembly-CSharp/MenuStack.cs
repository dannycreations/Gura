using System;
using System.Collections.Generic;

public class MenuStack
{
	public void SetMenu(MenuLayout menu)
	{
		this.menuStack.Clear();
		this.activeMenu = menu;
		this.activeMenu.GetOwner().OnEnter();
	}

	public MenuLayout GetMenu()
	{
		return this.activeMenu;
	}

	public void PushMenu(MenuLayout menu)
	{
		this.menuStack.Add(this.activeMenu);
		this.activeMenu = menu;
		this.activeMenu.GetOwner().OnEnter();
	}

	public void PopMenu()
	{
		if (this.menuStack.Count > 0)
		{
			this.activeMenu = this.menuStack[this.menuStack.Count - 1];
			this.menuStack.RemoveAt(this.menuStack.Count - 1);
		}
	}

	private MenuLayout activeMenu;

	private List<MenuLayout> menuStack = new List<MenuLayout>();
}
