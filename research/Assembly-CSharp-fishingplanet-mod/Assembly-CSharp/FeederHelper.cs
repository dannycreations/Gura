using System;
using System.Linq;
using ObjectModel;

public class FeederHelper
{
	public static Chum FindPreparedChumInHand()
	{
		return (from i in PhotonConnectionFactory.Instance.Profile.Inventory.OfType<Chum>()
			where i.Storage == StoragePlaces.Hands
			select i).FirstOrDefault<Chum>();
	}

	public static Chum FindPreparedChumOnDoll()
	{
		return (from i in PhotonConnectionFactory.Instance.Profile.Inventory.OfType<Chum>()
			where i.Storage == StoragePlaces.Doll
			select i).FirstOrDefault<Chum>();
	}

	public static Chum[] FindPreparedChumActiveRodAll()
	{
		Rod rod = RodHelper.FindRodInHands();
		if (rod != null)
		{
			return PhotonConnectionFactory.Instance.Profile.Inventory.OfType<Chum>().Where(delegate(Chum i)
			{
				bool flag2;
				if (i.Storage == StoragePlaces.ParentItem)
				{
					Guid? parentItemInstanceId = i.ParentItemInstanceId;
					bool flag = parentItemInstanceId != null;
					Guid? instanceId = rod.InstanceId;
					flag2 = flag == (instanceId != null) && (parentItemInstanceId == null || parentItemInstanceId.GetValueOrDefault() == instanceId.GetValueOrDefault());
				}
				else
				{
					flag2 = false;
				}
				return flag2;
			}).ToArray<Chum>();
		}
		return new Chum[0];
	}
}
