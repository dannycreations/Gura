using System;

namespace Assets.Scripts.UI._2D.Tournaments.UGC
{
	public class UgcMenuStateManager
	{
		public UgcMenuStateManager(MenuPrefabsList menuPrefabsList)
		{
			this._menuPrefabsList = menuPrefabsList;
		}

		public UgcMenuStateManager.UgcStates UgcState { get; private set; }

		public void SetState(UgcMenuStateManager.UgcStates state)
		{
			this.UgcState = state;
		}

		public void SetActiveFormSport(UgcMenuStateManager.UgcStates state, bool flag, bool immediate = false)
		{
			this.SetState(state);
			ActivityState activityState = this._menuPrefabsList.SportAS;
			if (state != UgcMenuStateManager.UgcStates.Room)
			{
				if (state == UgcMenuStateManager.UgcStates.Create)
				{
					activityState = this._menuPrefabsList.CreateTournamentFormAS;
				}
			}
			else
			{
				activityState = this._menuPrefabsList.UgcRoomFormAS;
			}
			this.SetActiveAs(activityState, flag, immediate);
		}

		public CreateTournamentPageHandler CreateTournamentCtrl
		{
			get
			{
				return (!(this._menuPrefabsList.CreateTournamentFormAS != null)) ? null : this._menuPrefabsList.CreateTournamentFormAS.GetComponent<CreateTournamentPageHandler>();
			}
		}

		public RoomUserCompetition RoomUserCompetitionCtrl
		{
			get
			{
				return (!(this._menuPrefabsList.UgcRoomFormAS != null)) ? null : this._menuPrefabsList.UgcRoomFormAS.GetComponent<RoomUserCompetition>();
			}
		}

		private void SetActiveAs(ActivityState f, bool flag, bool immediate = false)
		{
			if (f != null)
			{
				if (flag)
				{
					this._menuPrefabsList.currentActiveForm = f.gameObject;
					f.Show(immediate);
				}
				else
				{
					f.Hide(immediate);
				}
			}
		}

		private readonly MenuPrefabsList _menuPrefabsList;

		public enum UgcStates : byte
		{
			Sport,
			Create,
			Room
		}
	}
}
