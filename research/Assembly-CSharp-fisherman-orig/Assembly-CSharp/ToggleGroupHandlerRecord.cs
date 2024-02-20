using System;
using UnityEngine.UI;

[Serializable]
public class ToggleGroupHandlerRecord<T> where T : struct, IConvertible
{
	public T Type;

	public Toggle Toggle;
}
