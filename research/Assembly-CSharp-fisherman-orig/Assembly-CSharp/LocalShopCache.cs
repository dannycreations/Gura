using System;

public class LocalShopCache : GlobalShopCache
{
	public void EnsureCacheCorrespondsPondId()
	{
		int num = PhotonConnectionFactory.Instance.Profile.MissionsContext.PondId;
		if (this.pondId != num)
		{
			this.Clear();
			this.pondId = num;
		}
	}

	public void GetItemsFromCategory(int[] categoryIds, int pondId)
	{
		base.CheckItemsInCache(categoryIds, delegate(int[] unrequestedCat)
		{
			PhotonConnectionFactory.Instance.GetLocalItemsFromCategory(unrequestedCat, pondId);
		});
	}

	public void Clear()
	{
		this._globalShopChache.Clear();
	}

	private int pondId;
}
