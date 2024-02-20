using System;
using System.Collections.Generic;
using System.Linq;
using I2.Loc;
using ObjectModel;

public class LicencesFilterPonds : LicencesFilter
{
	internal override void Init()
	{
		base.Init();
		if (StaticUserData.CurrentPond != null || this.States == null)
		{
			return;
		}
		CategoryFilter categoryFilter = new CategoryFilter
		{
			CategoryName = ScriptLocalization.Get("StateCaption"),
			Filters = new List<ISelectionFilterBase>()
		};
		List<State> list = this.States;
		list.ForEach(delegate(State p)
		{
			p.Name = p.Name.TrimStart(new char[0]).TrimEnd(new char[0]).ToUpper();
		});
		list = list.OrderBy((State p) => p.Name).ToList<State>();
		for (int i = 0; i < list.Count; i++)
		{
			State state = list[i];
			if (CacheLibrary.MapCache.CachedPonds.Where((Pond p) => p.State.StateId == state.StateId).All(new Func<Pond, bool>(PondHelper.PondPaidLocked)))
			{
				LogHelper.Warning("LicencesFilterPonds:Init - skipped State with all paid Ponds StateId:{0} Name:{1}", new object[] { state.StateId, state.Name });
			}
			else
			{
				categoryFilter.Filters.Add(new SingleSelectionFilter
				{
					Caption = state.Name,
					FilterFieldName = "StateId",
					FilterFieldType = typeof(int),
					Value = state.StateId
				});
			}
		}
		this.FilterCategories.Add((short)this.FilterCategories.Count, categoryFilter);
	}

	public void SetStates(List<State> states)
	{
		this.States = states;
	}

	protected List<State> States;
}
