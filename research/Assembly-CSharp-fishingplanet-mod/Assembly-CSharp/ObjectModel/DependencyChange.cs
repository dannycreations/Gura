using System;
using System.Collections.Generic;

namespace ObjectModel
{
	public class DependencyChange
	{
		public string Name { get; set; }

		public static IDependencyChange Updated<T>(T oldValue, T newValue)
		{
			return new DependencyChange<T>
			{
				OldValue = oldValue,
				NewValue = newValue
			};
		}

		public static IDependencyChange Updated(int? oldValue, int? newValue)
		{
			return new DependencyChangeInt
			{
				OldValue = ((oldValue == null) ? 0 : oldValue.Value),
				NewValue = ((newValue == null) ? 0 : newValue.Value)
			};
		}

		public static IDependencyChange Updated(int oldValue, int newValue)
		{
			return new DependencyChangeInt
			{
				OldValue = oldValue,
				NewValue = newValue
			};
		}

		public static IDependencyChange Updated(decimal? oldValue, decimal? newValue)
		{
			return new DependencyChangeNumeric
			{
				OldValue = (float)((oldValue == null) ? 0m : oldValue.Value),
				NewValue = (float)((newValue == null) ? 0m : newValue.Value)
			};
		}

		public static IDependencyChange Updated(decimal oldValue, decimal newValue)
		{
			return new DependencyChangeNumeric
			{
				OldValue = (float)oldValue,
				NewValue = (float)newValue
			};
		}

		public static IDependencyChange Updated(float? oldValue, float? newValue)
		{
			return new DependencyChangeNumeric
			{
				OldValue = ((oldValue == null) ? 0f : oldValue.Value),
				NewValue = ((newValue == null) ? 0f : newValue.Value)
			};
		}

		public static IDependencyChange Updated(float oldValue, float newValue)
		{
			return new DependencyChangeNumeric
			{
				OldValue = oldValue,
				NewValue = newValue
			};
		}

		public static IDependencyChange Updated(TimeSpan oldValue, TimeSpan newValue)
		{
			return new DependencyChangeTimeSpan
			{
				OldValue = oldValue,
				NewValue = newValue
			};
		}

		public static IDependencyChangeCollectionItem ItemAdded<T>(T item, string operation = null, IEnumerable<T> collection = null)
		{
			return new DependencyChangeCollectionItem<T>
			{
				Item = item,
				IsAdded = true,
				Operation = (operation ?? "Added"),
				Collection = collection
			};
		}

		public static IDependencyChangeCollectionItem ItemUpdated<T>(T item, T oldItem = default(T), string operation = null, IEnumerable<T> collection = null)
		{
			return new DependencyChangeCollectionItem<T>
			{
				Item = item,
				OldItem = oldItem,
				IsUpdated = true,
				Operation = (operation ?? "Updated"),
				Collection = collection
			};
		}

		public static IDependencyChangeCollectionItem ItemRemoved<T>(T item, string operation = null, IEnumerable<T> collection = null)
		{
			return new DependencyChangeCollectionItem<T>
			{
				Item = item,
				IsRemoved = true,
				Operation = (operation ?? "Removed"),
				Collection = collection
			};
		}

		public static IDependencyChangeCollectionItem ItemAdded<T, E>(E @event, T item, string operation = null, IEnumerable<T> collection = null)
		{
			return new DependencyChangeCollectionItem<T, E>
			{
				Event = @event,
				Item = item,
				IsAdded = true,
				Operation = (operation ?? "Added"),
				Collection = collection
			};
		}

		public static IDependencyChangeCollectionItem ItemUpdated<T, E>(E @event, T item, T oldItem = default(T), string operation = null, IEnumerable<T> collection = null)
		{
			return new DependencyChangeCollectionItem<T, E>
			{
				Event = @event,
				Item = item,
				OldItem = oldItem,
				IsUpdated = true,
				Operation = (operation ?? "Updated"),
				Collection = collection
			};
		}

		public static IDependencyChangeCollectionItem ItemRemoved<T, E>(E @event, T item, string operation = null, IEnumerable<T> collection = null)
		{
			return new DependencyChangeCollectionItem<T, E>
			{
				Event = @event,
				Item = item,
				IsRemoved = true,
				Operation = (operation ?? "Removed"),
				Collection = collection
			};
		}

		public static IDependencyChange UpdatedSlightly(ref Point3 oldValue, Point3 newValue, float distance = 0.35f)
		{
			IDependencyChange dependencyChange = new DependencyChangePoint3
			{
				OldValue = oldValue,
				NewValue = newValue,
				Distance = distance
			};
			if (dependencyChange.IsChanged)
			{
				oldValue = ((newValue == null) ? null : new Point3(newValue));
			}
			return dependencyChange;
		}
	}
}
