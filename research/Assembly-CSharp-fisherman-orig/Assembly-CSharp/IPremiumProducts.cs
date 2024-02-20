using System;
using System.Collections.Generic;
using ObjectModel;

public interface IPremiumProducts
{
	void Init(List<StoreProduct> products);

	void SetActive(bool flag);
}
