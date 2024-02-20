using System;
using System.Collections.Generic;
using UnityEngine;

namespace frame8.Logic.Misc.Visual.UI.ScrollRectItemsAdapter
{
	public class ItemsDescriptor
	{
		public ItemsDescriptor(float defaultSize)
		{
			this.ReinitializeSizes(ItemCountChangeMode.RESET, 0, -1, new float?(defaultSize));
		}

		public double CumulatedSizeOfAllItems
		{
			get
			{
				return (this.itemsCount != 0) ? this.GetItemSizeCumulative(this.itemsCount - 1, false) : 0.0;
			}
		}

		public float this[int itemIndexInView]
		{
			get
			{
				float num;
				if (this._Sizes.TryGetValue(itemIndexInView, out num))
				{
					return num;
				}
				return this._DefaultSize;
			}
			set
			{
				if (!this._ChangingItemsSizesInProgress)
				{
					throw new UnityException("Call BeginChangingItemsSizes() before");
				}
				if (itemIndexInView != this._IndexInViewOfLastItemThatChangedSizeDuringSizesChange + 1)
				{
					throw new UnityException("Sizes can only be changed for items one by one, one after another(e.g. 3,4,5,6,7..), starting with the one passed to BeginChangingItemsSizes(int)!");
				}
				this.BinaryAddKeyToSortedListIfDoesntExist(itemIndexInView);
				this._CumulatedSizesUntilNowDuringSizesChange += (double)value;
				this._Sizes[itemIndexInView] = value;
				this._SizesCumulative[itemIndexInView] = this._CumulatedSizesUntilNowDuringSizesChange;
				this._IndexInViewOfLastItemThatChangedSizeDuringSizesChange = itemIndexInView;
			}
		}

		public void ReinitializeSizes(ItemCountChangeMode changeMode, int count, int indexIfInsertingOrRemoving = -1, float? newDefaultSize = null)
		{
			if (newDefaultSize != null && newDefaultSize != this._DefaultSize)
			{
				if (changeMode != ItemCountChangeMode.RESET)
				{
					throw new UnityException("Cannot preserve old sizes if the newDefaultSize is different!");
				}
				this._DefaultSize = newDefaultSize.Value;
			}
			if (changeMode == ItemCountChangeMode.RESET)
			{
				this._Sizes.Clear();
				this._SizesCumulative.Clear();
				this._Keys.Clear();
				this.itemsCount = count;
				return;
			}
			if (indexIfInsertingOrRemoving < 0 || indexIfInsertingOrRemoving > this.itemsCount)
			{
				throw new ArgumentOutOfRangeException("indexIfInsertingOrRemoving", indexIfInsertingOrRemoving, "Should be positive and less than itemsCount=" + this.itemsCount);
			}
			int num;
			if (changeMode == ItemCountChangeMode.INSERT)
			{
				this.ShiftSizesKeys(indexIfInsertingOrRemoving, count);
				num = this.itemsCount + count;
			}
			else
			{
				if (count < 0)
				{
					throw new ArgumentOutOfRangeException("count", count, "Cannot be negative!");
				}
				if (indexIfInsertingOrRemoving + count > this.itemsCount)
				{
					throw new ArgumentOutOfRangeException("RemoveItems: index + count", indexIfInsertingOrRemoving + count, "Should be positive and less than- or or equal to itemsCount=" + this.itemsCount);
				}
				count = -count;
				this.ShiftSizesKeys(indexIfInsertingOrRemoving, count);
				num = this.itemsCount + count;
			}
			this.itemsCount = num;
		}

		private void BinaryAddKeyToSortedListIfDoesntExist(int key)
		{
			int num = this._Keys.BinarySearch(key);
			if (num < 0)
			{
				this._Keys.Insert(~num, key);
			}
		}

		private void BinaryRemoveKeyFromSortedList(int key)
		{
			this._Keys.RemoveAt(this._Keys.BinarySearch(key));
		}

		private void ShiftSizesKeys(int startingKey, int amount)
		{
			if (this._Sizes.Count != this._SizesCumulative.Count || this._Sizes.Count != this._Keys.Count)
			{
				throw new InvalidOperationException("The sizes state was corrupted");
			}
			int num = this._Keys.BinarySearch(startingKey);
			if (num < 0)
			{
				num = ~num;
			}
			int i = num;
			double num2 = 0.0;
			if (amount < 0)
			{
				int count = this._Keys.Count;
				int num3 = -amount;
				int num4 = startingKey + num3;
				int num5;
				while (i < this._Keys.Count && (num5 = this._Keys[i]) < num4)
				{
					num2 -= (double)this._Sizes[num5];
					this._Sizes.Remove(num5);
					this._SizesCumulative.Remove(num5);
					this._Keys.RemoveAt(i);
				}
				int num6 = count - this._Keys.Count;
				num2 -= (double)((float)(num3 - num6) * this._DefaultSize);
				while (i < this._Keys.Count)
				{
					num5 = this._Keys[i];
					float num7 = this._Sizes[num5];
					double num8 = this._SizesCumulative[num5];
					this._Sizes.Remove(num5);
					this._SizesCumulative.Remove(num5);
					int num9 = num5 + amount;
					if (num9 < 0)
					{
						Debug.Log("here");
						this._Keys.RemoveAt(i);
					}
					else
					{
						this._Keys[i] = num9;
						this._Sizes[num9] = num7;
						this._SizesCumulative[num9] = num8 + num2;
					}
					i++;
				}
			}
			else
			{
				num2 = (double)((float)amount * this._DefaultSize);
				int num10 = i;
				for (i = this._Keys.Count - 1; i >= num10; i--)
				{
					int num5 = this._Keys[i];
					float num7 = this._Sizes[num5];
					double num8 = this._SizesCumulative[num5];
					this._Sizes.Remove(num5);
					this._SizesCumulative.Remove(num5);
					int num11 = num5 + amount;
					this._Keys[i] = num11;
					this._Sizes[num11] = num7;
					this._SizesCumulative[num11] = num8 + num2;
				}
			}
		}

		public void BeginChangingItemsSizes(int indexInViewOfFirstItemThatWillChangeSize)
		{
			if (this._ChangingItemsSizesInProgress)
			{
				throw new UnityException("Call EndChangingItemsSizes() when done doing it");
			}
			this._ChangingItemsSizesInProgress = true;
			this._IndexInViewOfFirstItemThatChangesSizeDuringSizesChange = indexInViewOfFirstItemThatWillChangeSize;
			this._IndexInViewOfLastItemThatChangedSizeDuringSizesChange = this._IndexInViewOfFirstItemThatChangesSizeDuringSizesChange - 1;
			this._CumulatedSizesUntilNowDuringSizesChange = ((this._IndexInViewOfFirstItemThatChangesSizeDuringSizesChange != 0) ? this.GetItemSizeCumulative(this._IndexInViewOfFirstItemThatChangesSizeDuringSizesChange - 1, true) : 0.0);
		}

		public void EndChangingItemsSizes()
		{
			this._ChangingItemsSizesInProgress = false;
			if (this._IndexInViewOfLastItemThatChangedSizeDuringSizesChange == this._IndexInViewOfFirstItemThatChangesSizeDuringSizesChange - 1)
			{
				return;
			}
			int num = this._Keys.BinarySearch(this._IndexInViewOfLastItemThatChangedSizeDuringSizesChange);
			if (num < 0)
			{
				throw new InvalidOperationException("The sizes state was corrupted");
			}
			double num2 = this._CumulatedSizesUntilNowDuringSizesChange;
			int num3 = this._IndexInViewOfLastItemThatChangedSizeDuringSizesChange;
			for (int i = num + 1; i < this._Keys.Count; i++)
			{
				int num4 = this._Keys[i];
				num2 += (double)((float)(num4 - num3 - 1) * this._DefaultSize + this._Sizes[num4]);
				this._SizesCumulative[num4] = num2;
				num3 = num4;
			}
		}

		public int GetItemRealIndexFromViewIndex(int indexInView)
		{
			return (this.realIndexOfFirstItemInView + indexInView) % this.itemsCount;
		}

		public int GetItemViewIndexFromRealIndex(int realIndex)
		{
			return (realIndex - this.realIndexOfFirstItemInView + this.itemsCount) % this.itemsCount;
		}

		public double GetItemSizeCumulative(int itemIndexInView, bool allowInferringFromNeighborAfter = true)
		{
			if (this._Keys.Count > 0)
			{
				double num;
				if (this._SizesCumulative.TryGetValue(itemIndexInView, out num))
				{
					return num;
				}
				int num2 = this._Keys.BinarySearch(itemIndexInView);
				if (num2 >= 0)
				{
					throw new InvalidOperationException("The sizes state was corrupted. key not in _SizesCumulative, but present in _Keys");
				}
				num2 = ~num2;
				int num3 = num2 - 1;
				if (num2 < this._Keys.Count && allowInferringFromNeighborAfter)
				{
					int num4 = this._Keys[num2];
					int num5 = num4 - itemIndexInView;
					if (num3 < 0 || num5 < itemIndexInView - this._Keys[num3])
					{
						return this._SizesCumulative[num4] - (double)(this[num4] + (float)(num5 - 1) * this._DefaultSize);
					}
				}
				if (num3 >= 0)
				{
					int num6 = this._Keys[num3];
					return this._SizesCumulative[num6] + (double)((float)(itemIndexInView - num6) * this._DefaultSize);
				}
			}
			return (double)((float)(itemIndexInView + 1) * this._DefaultSize);
		}

		public void RotateItemsSizesOnScrollViewLooped(int newValueOf_RealIndexOfFirstItemInView)
		{
			int num = this.realIndexOfFirstItemInView;
			this.realIndexOfFirstItemInView = newValueOf_RealIndexOfFirstItemInView;
			int num2 = num - this.realIndexOfFirstItemInView;
			int count = this._Keys.Count;
			if (num2 == 0 && count == 0)
			{
				return;
			}
			if (num2 < 0)
			{
				num2 += this.itemsCount;
			}
			int[] array = this._Keys.ToArray();
			Dictionary<int, float> sizes = this._Sizes;
			this._Keys.Clear();
			this._Sizes = new Dictionary<int, float>(count);
			this._SizesCumulative.Clear();
			this._SizesCumulative = new Dictionary<int, double>(count);
			double num3 = 0.0;
			int num4 = -1;
			for (int i = 0; i < count; i++)
			{
				int num5 = array[i];
				int num6 = (num5 + num2) % this.itemsCount;
				this.BinaryAddKeyToSortedListIfDoesntExist(num6);
				float num7 = sizes[num5];
				this._Sizes[num6] = num7;
				int num8 = num6 - num4 - 1;
				num3 += (double)((float)num8 * this._DefaultSize);
				num3 += (double)num7;
				this._SizesCumulative[num6] = num3;
				num4 = num6;
			}
		}

		public float itemsConstantTransversalSize;

		public int itemsCount;

		public double cumulatedSizesOfAllItemsPlusSpacing;

		public int realIndexOfFirstItemInView;

		public int maxVisibleItemsSeenSinceLastScrollViewSizeChange;

		public int destroyedItemsSinceLastScrollViewSizeChange;

		private List<int> _Keys = new List<int>();

		private Dictionary<int, float> _Sizes = new Dictionary<int, float>();

		private Dictionary<int, double> _SizesCumulative = new Dictionary<int, double>();

		private float _DefaultSize;

		private bool _ChangingItemsSizesInProgress;

		private int _IndexInViewOfFirstItemThatChangesSizeDuringSizesChange;

		private int _IndexInViewOfLastItemThatChangedSizeDuringSizesChange = -1;

		private double _CumulatedSizesUntilNowDuringSizesChange;
	}
}
