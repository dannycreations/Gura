using System;

namespace frame8.ScrollRectItemsAdapter.ContentSizeFitterExample
{
	public class ExampleItemModel
	{
		public ExampleItemModel()
		{
			this.HasPendingSizeChange = true;
		}

		public string Title
		{
			get
			{
				return this._Title;
			}
			set
			{
				if (this._Title != value)
				{
					this._Title = value;
					this.HasPendingSizeChange = true;
				}
			}
		}

		public int IconIndex { get; set; }

		public bool HasPendingSizeChange { get; set; }

		private string _Title;
	}
}
