using System;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedScrollerDemos.MultipleCellTypesDemo
{
	public class CellViewRow : CellView
	{
		public override void SetData(Data data)
		{
			base.SetData(data);
			this._rowData = data as RowData;
			this.userNameText.text = this._rowData.userName;
			this.userAvatarImage.sprite = Resources.Load<Sprite>(this._rowData.userAvatarSpritePath);
			this.userHighScoreText.text = string.Format("{0:n0}", this._rowData.userHighScore);
		}

		private RowData _rowData;

		public Text userNameText;

		public Image userAvatarImage;

		public Text userHighScoreText;
	}
}
