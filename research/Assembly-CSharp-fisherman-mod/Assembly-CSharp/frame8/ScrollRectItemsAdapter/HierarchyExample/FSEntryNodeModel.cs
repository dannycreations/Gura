using System;
using System.Collections.Generic;

namespace frame8.ScrollRectItemsAdapter.HierarchyExample
{
	public class FSEntryNodeModel
	{
		public bool IsDirectory
		{
			get
			{
				return this.children != null;
			}
		}

		public List<FSEntryNodeModel> GetFlattenedHierarchyAndExpandAll()
		{
			List<FSEntryNodeModel> list = new List<FSEntryNodeModel>();
			for (int i = 0; i < this.children.Length; i++)
			{
				FSEntryNodeModel fsentryNodeModel = this.children[i];
				list.Add(fsentryNodeModel);
				fsentryNodeModel.expanded = true;
				if (fsentryNodeModel.IsDirectory)
				{
					list.AddRange(fsentryNodeModel.GetFlattenedHierarchyAndExpandAll());
				}
			}
			return list;
		}

		public FSEntryNodeModel[] children;

		public int depth;

		public string title;

		public bool expanded;
	}
}
