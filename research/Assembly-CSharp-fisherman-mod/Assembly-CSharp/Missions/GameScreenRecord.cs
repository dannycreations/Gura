using System;
using ObjectModel;

namespace Missions
{
	public class GameScreenRecord
	{
		public GameScreenRecord(GameScreenType gameScreen, GameScreenTabType gameScreenTab = GameScreenTabType.Undefined, int? categoryId = null, int? itemId = null, int[] childCategoryIds = null, string categoryElementId = null, string[] categoryElementsPath = null)
		{
			this._gameScreen = gameScreen;
			this._gameScreenTab = gameScreenTab;
			this._categoryId = categoryId;
			this._itemId = itemId;
			this._childCategoryIds = childCategoryIds;
			this._categoryElementId = categoryElementId;
			this._categoryElementsPath = categoryElementsPath;
		}

		public GameScreenType GameScreen
		{
			get
			{
				return this._gameScreen;
			}
		}

		public GameScreenTabType GameScreenTab
		{
			get
			{
				return this._gameScreenTab;
			}
		}

		public int? CategoryId
		{
			get
			{
				return this._categoryId;
			}
		}

		public int? ItemId
		{
			get
			{
				return this._itemId;
			}
		}

		public int[] ChildCategoryIds
		{
			get
			{
				return this._childCategoryIds;
			}
		}

		public string CategoryElementId
		{
			get
			{
				return this._categoryElementId;
			}
		}

		public string[] CategoryElementsPath
		{
			get
			{
				return this._categoryElementsPath;
			}
		}

		private GameScreenType _gameScreen;

		private GameScreenTabType _gameScreenTab;

		private int? _categoryId;

		private int? _itemId;

		private int[] _childCategoryIds;

		private string _categoryElementId;

		private string[] _categoryElementsPath;
	}
}
