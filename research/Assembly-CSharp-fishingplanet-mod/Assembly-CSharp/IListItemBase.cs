using System;

public interface IListItemBase
{
	bool IsActive { get; }

	event Action OnSelect;

	event Action OnOk;

	void SetActive(bool flag);

	void SetToggle(bool flag);
}
