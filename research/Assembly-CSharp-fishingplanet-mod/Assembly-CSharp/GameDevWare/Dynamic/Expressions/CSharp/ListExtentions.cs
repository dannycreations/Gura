using System;
using System.Collections.Generic;
using GameDevWare.Dynamic.Expressions.Properties;

namespace GameDevWare.Dynamic.Expressions.CSharp
{
	internal static class ListExtentions
	{
		public static T Dequeue<T>(this List<T> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			if (list.Count == 0)
			{
				throw new ArgumentException(Resources.EXCEPTION_LIST_LISTISEMPTY, "list");
			}
			T t = list[0];
			list.RemoveAt(0);
			return t;
		}

		public static T Pop<T>(this List<T> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException("list");
			}
			if (list.Count == 0)
			{
				throw new ArgumentException(Resources.EXCEPTION_LIST_LISTISEMPTY, "list");
			}
			T t = list[list.Count - 1];
			list.RemoveAt(list.Count - 1);
			return t;
		}
	}
}
