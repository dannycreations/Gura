using System;
using Assets.Scripts.UI._2D.PlayerProfile;
using frame8.Logic.Misc.Other.Extensions;
using FriendsSRIA.Models;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FriendsSRIA.ViewsHolders
{
	public class HeaderFriendGroupVH : BaseVH
	{
		public override void CollectViews()
		{
			base.CollectViews();
			this.root.GetComponentAtPath("HeaderName", out this._groupName);
			this.root.GetComponentAtPath("Nickname", out this._nickname);
			this.root.GetComponentAtPath("Level", out this._level);
			this.root.GetComponentAtPath("Location", out this._location);
			this.root.GetComponentAtPath("Bg", out this._background);
		}

		public override bool CanPresentModelType(Type modelType)
		{
			return modelType == typeof(HeaderFriendGroupModel);
		}

		internal override void UpdateViews(BaseModel model)
		{
			base.UpdateViews(model);
			HeaderFriendGroupModel headerFriendGroupModel = model as HeaderFriendGroupModel;
			this._groupName.text = headerFriendGroupModel.GroupName;
			bool flag = headerFriendGroupModel.type == BaseModel.FriendModelType.Normal;
			this._nickname.text = ((!flag) ? string.Empty : PlayerProfileHelper.UsernameCaption);
			this._level.text = ((!flag) ? string.Empty : ScriptLocalization.Get("LevelButtonPopup"));
			this._location.text = ((!flag) ? string.Empty : ScriptLocalization.Get("LocationCaption"));
			this._background.color = ((headerFriendGroupModel.type != BaseModel.FriendModelType.Request) ? this._usualColor : this._requestsColor);
		}

		private TextMeshProUGUI _groupName;

		private TextMeshProUGUI _nickname;

		private TextMeshProUGUI _level;

		private TextMeshProUGUI _location;

		private Image _background;

		private Color _requestsColor = new Color(0.65882355f, 0.4862745f, 0.21568628f);

		private Color _usualColor = new Color(0.42352942f, 0.43529412f, 0.4509804f);
	}
}
