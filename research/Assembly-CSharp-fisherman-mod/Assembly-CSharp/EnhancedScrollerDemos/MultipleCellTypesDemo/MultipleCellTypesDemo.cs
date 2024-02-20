using System;
using EnhancedUI;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace EnhancedScrollerDemos.MultipleCellTypesDemo
{
	public class MultipleCellTypesDemo : MonoBehaviour, IEnhancedScrollerDelegate
	{
		private void Start()
		{
			this.scroller.Delegate = this;
			this.LoadData();
		}

		private void LoadData()
		{
			this._data = new SmallList<Data>();
			this._data.Add(new HeaderData
			{
				category = "Platinum Players"
			});
			this._data.Add(new RowData
			{
				userName = "John Smith",
				userAvatarSpritePath = this.resourcePath + "/avatar_male",
				userHighScore = 21323199UL
			});
			this._data.Add(new RowData
			{
				userName = "Jane Doe",
				userAvatarSpritePath = this.resourcePath + "/avatar_female",
				userHighScore = 20793219UL
			});
			this._data.Add(new RowData
			{
				userName = "Julie Prost",
				userAvatarSpritePath = this.resourcePath + "/avatar_female",
				userHighScore = 19932132UL
			});
			this._data.Add(new FooterData());
			this._data.Add(new HeaderData
			{
				category = "Gold Players"
			});
			this._data.Add(new RowData
			{
				userName = "Jim Bob",
				userAvatarSpritePath = this.resourcePath + "/avatar_male",
				userHighScore = 1002132UL
			});
			this._data.Add(new RowData
			{
				userName = "Susan Anthony",
				userAvatarSpritePath = this.resourcePath + "/avatar_female",
				userHighScore = 991234UL
			});
			this._data.Add(new FooterData());
			this._data.Add(new HeaderData
			{
				category = "Silver Players"
			});
			this._data.Add(new RowData
			{
				userName = "Gary Richards",
				userAvatarSpritePath = this.resourcePath + "/avatar_male",
				userHighScore = 905723UL
			});
			this._data.Add(new RowData
			{
				userName = "John Doe",
				userAvatarSpritePath = this.resourcePath + "/avatar_male",
				userHighScore = 702318UL
			});
			this._data.Add(new RowData
			{
				userName = "Lisa Ford",
				userAvatarSpritePath = this.resourcePath + "/avatar_female",
				userHighScore = 697767UL
			});
			this._data.Add(new RowData
			{
				userName = "Jacob Morris",
				userAvatarSpritePath = this.resourcePath + "/avatar_male",
				userHighScore = 409393UL
			});
			this._data.Add(new RowData
			{
				userName = "Carolyn Shephard",
				userAvatarSpritePath = this.resourcePath + "/avatar_female",
				userHighScore = 104352UL
			});
			this._data.Add(new RowData
			{
				userName = "Guy Wilson",
				userAvatarSpritePath = this.resourcePath + "/avatar_male",
				userHighScore = 88321UL
			});
			this._data.Add(new RowData
			{
				userName = "Jackie Jones",
				userAvatarSpritePath = this.resourcePath + "/avatar_female",
				userHighScore = 20826UL
			});
			this._data.Add(new RowData
			{
				userName = "Sally Brewer",
				userAvatarSpritePath = this.resourcePath + "/avatar_female",
				userHighScore = 17389UL
			});
			this._data.Add(new RowData
			{
				userName = "Joe West",
				userAvatarSpritePath = this.resourcePath + "/avatar_male",
				userHighScore = 2918UL
			});
			this._data.Add(new FooterData());
			this.scroller.ReloadData(0f);
		}

		public int GetNumberOfCells(EnhancedScroller scroller)
		{
			return this._data.Count;
		}

		public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
		{
			if (this._data[dataIndex] is HeaderData)
			{
				return 70f;
			}
			if (this._data[dataIndex] is RowData)
			{
				return 100f;
			}
			return 90f;
		}

		public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
		{
			CellView cellView;
			if (this._data[dataIndex] is HeaderData)
			{
				cellView = scroller.GetCellView(this.headerCellViewPrefab) as CellViewHeader;
				cellView.name = "[Header] " + (this._data[dataIndex] as HeaderData).category;
			}
			else if (this._data[dataIndex] is RowData)
			{
				cellView = scroller.GetCellView(this.rowCellViewPrefab) as CellViewRow;
				cellView.name = "[Row] " + (this._data[dataIndex] as RowData).userName;
			}
			else
			{
				cellView = scroller.GetCellView(this.footerCellViewPrefab) as CellViewFooter;
				cellView.name = "[Footer]";
			}
			cellView.SetData(this._data[dataIndex]);
			return cellView;
		}

		private SmallList<Data> _data;

		public EnhancedScroller scroller;

		public EnhancedScrollerCellView headerCellViewPrefab;

		public EnhancedScrollerCellView rowCellViewPrefab;

		public EnhancedScrollerCellView footerCellViewPrefab;

		public string resourcePath;
	}
}
