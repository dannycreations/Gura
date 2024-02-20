using System;
using UnityEngine.UI;

public interface IWindowListItem : IListItemBase
{
	int RadioId { get; set; }

	void Init(string t, ToggleGroup tg, bool interactable, int radioId);

	void Remove();

	void UpdateText(string t);

	int GetSiblingIndex();
}
