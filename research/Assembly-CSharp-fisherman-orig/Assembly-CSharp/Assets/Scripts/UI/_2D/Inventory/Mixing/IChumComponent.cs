using System;
using ObjectModel;

namespace Assets.Scripts.UI._2D.Inventory.Mixing
{
	public interface IChumComponent : IDisposable
	{
		void SetActive(bool flag);

		void HasInInventory(bool v);

		void SetEnable(bool flag);

		void SetSiblingIndex(int i);

		int GetSiblingIndex();

		ChumIngredient Ingredient { get; }

		Types Type { get; }

		UiTypes UiType { get; }
	}
}
