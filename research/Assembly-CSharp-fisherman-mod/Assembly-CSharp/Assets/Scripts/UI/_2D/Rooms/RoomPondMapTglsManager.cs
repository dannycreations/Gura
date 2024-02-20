using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Common;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI._2D.Rooms
{
	public class RoomPondMapTglsManager : IUpdateable
	{
		public RoomPondMapTglsManager(ToggleGroup tGroup, Dictionary<RoomPondMapTglsManager.WindowTypes, RoomPondMapTglsManager.WindowRoomData> roots, GameObject roomElemPrefab, Dictionary<string, bool> roomsDataEnabled, Action<string, string> onSelect, string defaultRoomWindow, UINavigation navigation)
		{
			this._navigation = navigation;
			this._defaultRoomWindow = defaultRoomWindow;
			this._onSelect = onSelect;
			this._roots = roots;
			this._friendsScrollRect = this._roots[RoomPondMapTglsManager.WindowTypes.Friends].RootContent.transform.parent.GetComponent<ScrollRect>();
			foreach (KeyValuePair<string, string> keyValuePair in this._roomsData)
			{
				this.Add(ScriptLocalization.Get(keyValuePair.Value), RoomPondMapTglsManager.WindowTypes.Rooms, tGroup, roomElemPrefab, keyValuePair.Key, roomsDataEnabled.ContainsKey(keyValuePair.Key) && roomsDataEnabled[keyValuePair.Key]);
			}
			for (int i = 0; i < 25; i++)
			{
				this.Add(string.Empty, RoomPondMapTglsManager.WindowTypes.FriendsRooms, tGroup, roomElemPrefab, string.Format("{0}_{1}", RoomPondMapTglsManager.WindowTypes.FriendsRooms, i), false);
				this.Add(string.Empty, RoomPondMapTglsManager.WindowTypes.Friends, tGroup, roomElemPrefab, string.Format("{0}_{1}", RoomPondMapTglsManager.WindowTypes.Friends, i), false);
			}
			this.InitTglEvents();
			this.CorrectPosY();
			this.CheckActivateFriendsRooms();
			this.SelectRoomWindow(this._defaultRoomWindow);
			this.SetLineBold(this._rooms.Values.ToList<List<RoomElementPondMap>>());
		}

		public void Open()
		{
			this.CheckActivateFriendsRooms();
		}

		public void SelectRoomWindow(string roomId)
		{
			LogHelper.Log("___kocha >>> SelectRoomWindow roomId:{0}", new object[] { roomId });
			this._rooms[RoomPondMapTglsManager.WindowTypes.Rooms].ForEach(delegate(RoomElementPondMap p)
			{
				p.SetSelected(p.Id == roomId);
			});
			this.OnSelect();
		}

		public void Update()
		{
			if (this._friendsScrollRect != null && this._friendsScrollRect.verticalNormalizedPosition > 0f)
			{
				if (this._rooms[RoomPondMapTglsManager.WindowTypes.Friends].Count((RoomElementPondMap p) => p.IsVisible) > 4)
				{
					this._friendsScrollRect.verticalNormalizedPosition = this._friendsScrollRect.verticalNormalizedPosition - 0.001f;
				}
			}
		}

		public void Refresh(List<RoomPopulation> rs)
		{
			this._friendsInRooms.Clear();
			List<RoomElementPondMap> list = this._rooms[RoomPondMapTglsManager.WindowTypes.Rooms];
			List<RoomElementPondMap> list2 = this._rooms[RoomPondMapTglsManager.WindowTypes.FriendsRooms];
			RoomElementPondMap roomElementPondMap = list2.FirstOrDefault((RoomElementPondMap p) => p.IsSelected && p.IsVisible);
			List<RoomPopulation> list3 = rs.OrderByDescending((RoomPopulation r) => r.FriendList.Count<string>()).ToList<RoomPopulation>();
			RoomPopulation roomPopulation = list3.FirstOrDefault<RoomPopulation>();
			string text = string.Format("{0} {1}", ScriptLocalization.Get(this._roomsData["top"]), this.GetFriendRoomLoc(1, (roomPopulation == null) ? 0 : roomPopulation.FriendList.Length, (roomPopulation == null) ? 1 : roomPopulation.LanguageId));
			RoomElementPondMap roomElementPondMap2 = list.First((RoomElementPondMap p) => p.Id == "top");
			roomElementPondMap2.SetName(text);
			roomElementPondMap2.SetActiveAndInteractable(list3.Count > 0);
			int num = 0;
			for (int i = 0; i < list2.Count; i++)
			{
				if (i < list3.Count)
				{
					RoomPopulation fr = list3[i];
					list2[i].SetName(this.GetFriendRoomLoc(i + 1, fr.FriendList.Length, fr.LanguageId));
					list2[i].SetActiveAndInteractable(true);
					list2[i].SetId(fr.RoomId);
					if (roomElementPondMap != null && fr.RoomId == roomElementPondMap.Id)
					{
						num = i;
					}
					this._friendsInRooms[fr.RoomId] = (from p in PhotonConnectionFactory.Instance.Profile.Friends
						where fr.FriendList.Contains(p.UserId)
						select p.UserName).ToList<string>();
				}
				else
				{
					list2[i].SetActiveAndInteractable(false);
				}
			}
			if (list3.Count > 0)
			{
				this.UpdateFriends(num);
				if (!this._isSelectTopFriendsRoom)
				{
					this._isSelectTopFriendsRoom = true;
					this.SelectRoomWindow("top");
				}
			}
			else
			{
				this._isSelectTopFriendsRoom = false;
			}
			this.CorrectPosY();
			this._navigation.PreheatSelectables();
			this.CheckActivateFriendsRooms();
			this.SetLineBold(new List<List<RoomElementPondMap>> { list, list2 });
		}

		public void OnMovedToRoom(string roomId)
		{
			if (!string.IsNullOrEmpty(roomId))
			{
				int num = this._rooms[RoomPondMapTglsManager.WindowTypes.FriendsRooms].FindIndex((RoomElementPondMap p) => p.Id == roomId);
				if (num != -1)
				{
					int num2 = this._rooms[RoomPondMapTglsManager.WindowTypes.FriendsRooms].FindIndex((RoomElementPondMap p) => p.IsSelected && p.IsVisible);
					if (num2 != num)
					{
						this.UpdateFriends(num);
					}
					RoomElementPondMap roomElementPondMap = this._rooms[RoomPondMapTglsManager.WindowTypes.Rooms].FirstOrDefault((RoomElementPondMap p) => p.IsSelected && p.IsVisible);
					if (roomElementPondMap != null && roomElementPondMap.Id != "top")
					{
						this.SelectRoomWindow("top");
					}
				}
			}
		}

		private void Add(string name, RoomPondMapTglsManager.WindowTypes wType, ToggleGroup tGroup, GameObject roomElemPrefab, string id, bool isEnabled)
		{
			GameObject gameObject = GUITools.AddChild(this._roots[wType].RootContent, roomElemPrefab);
			RoomElementPondMap component = gameObject.GetComponent<RoomElementPondMap>();
			component.Init(name, tGroup, id, isEnabled);
			this._rooms[wType].Add(component);
		}

		private void InitTglEvents()
		{
			this.Init(this._rooms[RoomPondMapTglsManager.WindowTypes.Rooms], delegate(int roomIdx)
			{
				for (int i = 0; i < this._rooms[RoomPondMapTglsManager.WindowTypes.Rooms].Count; i++)
				{
					this._rooms[RoomPondMapTglsManager.WindowTypes.Rooms][i].SetSelected(roomIdx == i);
				}
				this.CheckActivateFriendsRooms();
			});
			this.Init(this._rooms[RoomPondMapTglsManager.WindowTypes.FriendsRooms], new Action<int>(this.UpdateFriends));
		}

		private void Init(List<RoomElementPondMap> rs, Action<int> onActive)
		{
			for (int i = 0; i < rs.Count; i++)
			{
				int roomIdx = i;
				RoomElementPondMap roomElementPondMap = rs[roomIdx];
				roomElementPondMap.OnActive = (Action<bool>)Delegate.Combine(roomElementPondMap.OnActive, new Action<bool>(delegate(bool b)
				{
					if (b)
					{
						onActive(roomIdx);
						this.OnSelect();
					}
				}));
			}
		}

		private void OnSelect()
		{
			RoomElementPondMap roomElementPondMap = this._rooms[RoomPondMapTglsManager.WindowTypes.Rooms].FirstOrDefault((RoomElementPondMap p) => p.IsSelected && p.IsVisible);
			if (roomElementPondMap != null)
			{
				string text = roomElementPondMap.Id;
				string text2 = roomElementPondMap.Name;
				if (text == "top")
				{
					RoomElementPondMap roomElementPondMap2 = this._rooms[RoomPondMapTglsManager.WindowTypes.FriendsRooms].First((RoomElementPondMap p) => p.IsSelected && p.IsVisible);
					text2 = roomElementPondMap2.Name;
					text = roomElementPondMap2.Id;
				}
				this._onSelect((!(text == "random")) ? text : null, text2);
			}
		}

		private void UpdateFriends(int roomIdx)
		{
			List<RoomElementPondMap> list = this._rooms[RoomPondMapTglsManager.WindowTypes.FriendsRooms];
			List<RoomElementPondMap> list2 = this._rooms[RoomPondMapTglsManager.WindowTypes.Friends];
			this._friendsScrollRect.verticalNormalizedPosition = 1f;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].SetSelected(roomIdx == i);
			}
			RoomElementPondMap roomElementPondMap = list.FirstOrDefault((RoomElementPondMap p) => p.IsSelected && p.IsVisible);
			if (roomElementPondMap == null || !this._friendsInRooms.ContainsKey(roomElementPondMap.Id))
			{
				LogHelper.Error("___kocha UpdateFriends roomIdx:{0} curFriendRoom is null:{1} roomId:{2} _friendsInRooms:{3}", new object[]
				{
					roomIdx,
					roomElementPondMap == null,
					(!(roomElementPondMap != null)) ? "-1" : roomElementPondMap.Id,
					string.Join(",", this._friendsInRooms.Keys.ToArray<string>())
				});
			}
			else
			{
				List<string> list3 = this._friendsInRooms[roomElementPondMap.Id];
				for (int j = 0; j < list2.Count; j++)
				{
					RoomElementPondMap roomElementPondMap2 = list2[j];
					if (j < list3.Count)
					{
						roomElementPondMap2.SetName(list3[j]);
						roomElementPondMap2.SetActive(true);
					}
					else
					{
						roomElementPondMap2.SetActive(false);
					}
				}
			}
			this.SetLineBold(new List<List<RoomElementPondMap>> { list2 });
		}

		private void SetLineBold(List<List<RoomElementPondMap>> rooms)
		{
			rooms.ForEach(delegate(List<RoomElementPondMap> r)
			{
				if (r.Count((RoomElementPondMap p) => p.IsVisible) > 4)
				{
					r.ForEach(delegate(RoomElementPondMap p)
					{
						p.SetLineBold(false);
					});
				}
				else
				{
					int num = r.FindLastIndex((RoomElementPondMap p) => p.IsVisible);
					for (int i = 0; i < r.Count; i++)
					{
						r[i].SetLineBold(i == num);
					}
				}
			});
		}

		private string GetFriendRoomLoc(int roomIdx, int friendsCount, int languageId)
		{
			return string.Format("{0}{1} [{2}] [{3}]", new object[]
			{
				ScriptLocalization.Get("RoomCaption"),
				roomIdx,
				UgcConsts.GetWinner(friendsCount.ToString()),
				ChangeLanguage.GetCustomLanguagesShort((CustomLanguages)(languageId - 1))
			});
		}

		private void CheckActivateFriendsRooms()
		{
			List<RoomElementPondMap> list = this._rooms[RoomPondMapTglsManager.WindowTypes.Rooms];
			List<RoomElementPondMap> list2 = this._rooms[RoomPondMapTglsManager.WindowTypes.FriendsRooms];
			RoomElementPondMap roomElementPondMap = list.First((RoomElementPondMap p) => p.Id == "top");
			bool flag = roomElementPondMap.IsSelected && roomElementPondMap.IsVisible;
			this._roots[RoomPondMapTglsManager.WindowTypes.FriendsRooms].MainGo.SetActive(flag);
			this._roots[RoomPondMapTglsManager.WindowTypes.Friends].MainGo.SetActive(flag);
			RoomElementPondMap selected = ((!(UINavigation.CurrentSelectedGo != null)) ? null : UINavigation.CurrentSelectedGo.GetComponent<RoomElementPondMap>());
			if (roomElementPondMap.IsSelected && !roomElementPondMap.IsVisible)
			{
				roomElementPondMap.SetSelected(false);
				roomElementPondMap.SetOn(false);
				this.SelectRoomWindow(this._defaultRoomWindow);
				if (selected != null && (selected.Id == roomElementPondMap.Id || list2.Any((RoomElementPondMap p) => p.Id == selected.Id)))
				{
					this.SetSelectedGameObject(list);
					return;
				}
			}
			if (selected != null)
			{
				RoomElementPondMap roomElementPondMap2 = list2.FirstOrDefault((RoomElementPondMap p) => p.Id == selected.Id && !p.IsVisible);
				if (roomElementPondMap2 != null)
				{
					roomElementPondMap2.SetOn(false);
					this.SetSelectedGameObject(list2);
				}
			}
		}

		private void SetSelectedGameObject(List<RoomElementPondMap> r)
		{
			RoomElementPondMap roomElementPondMap = r.FirstOrDefault((RoomElementPondMap p) => p.IsSelected && p.IsVisible);
			if (roomElementPondMap != null)
			{
				UINavigation.SetSelectedGameObject(roomElementPondMap.gameObject);
				roomElementPondMap.SetOn(true);
			}
		}

		private void CorrectPosY()
		{
			int num = this._rooms[RoomPondMapTglsManager.WindowTypes.Rooms].Count((RoomElementPondMap p) => p.IsVisible);
			RectTransform component = this._roots[RoomPondMapTglsManager.WindowTypes.Rooms].MainGo.GetComponent<RectTransform>();
			component.anchoredPosition = new Vector2(component.anchoredPosition.x, 44.45001f);
			for (int i = 4 - num; i > 0; i--)
			{
				component.anchoredPosition = new Vector2(component.anchoredPosition.x, component.anchoredPosition.y - 75f);
			}
		}

		private readonly Dictionary<RoomPondMapTglsManager.WindowTypes, List<RoomElementPondMap>> _rooms = new Dictionary<RoomPondMapTglsManager.WindowTypes, List<RoomElementPondMap>>
		{
			{
				RoomPondMapTglsManager.WindowTypes.Rooms,
				new List<RoomElementPondMap>()
			},
			{
				RoomPondMapTglsManager.WindowTypes.FriendsRooms,
				new List<RoomElementPondMap>()
			},
			{
				RoomPondMapTglsManager.WindowTypes.Friends,
				new List<RoomElementPondMap>()
			}
		};

		private Dictionary<RoomPondMapTglsManager.WindowTypes, RoomPondMapTglsManager.WindowRoomData> _roots;

		private readonly Dictionary<string, List<string>> _friendsInRooms = new Dictionary<string, List<string>>();

		private const int MaxElems = 4;

		private const int MaxFriendsRooms = 25;

		private const float FriendsScrollSpeed = 0.001f;

		private const float RoomsFourElemsPosY = 44.45001f;

		private const float ElemHeight = 75f;

		private readonly Dictionary<string, string> _roomsData = new Dictionary<string, string>
		{
			{ "top", "JoinFriends" },
			{ "random", "RandomRoomCaption" },
			{ "friends", "GotoFriendsRoom" },
			{ "private", "GotoPrivateRoom" }
		};

		private UINavigation _navigation;

		private ScrollRect _friendsScrollRect;

		private Action<string, string> _onSelect;

		private string _defaultRoomWindow = "private";

		private bool _isSelectTopFriendsRoom;

		public enum WindowTypes : byte
		{
			Rooms,
			FriendsRooms,
			Friends
		}

		public class WindowRoomData
		{
			public GameObject RootContent { get; set; }

			public GameObject MainGo { get; set; }
		}
	}
}
