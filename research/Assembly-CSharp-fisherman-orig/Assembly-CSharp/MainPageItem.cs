using System;

[Serializable]
public abstract class MainPageItem : ActivityStateControlled
{
	public virtual void SetParentActive(bool flag)
	{
	}

	public virtual void SetActive(bool flag)
	{
		base.gameObject.SetActive(flag);
	}
}
