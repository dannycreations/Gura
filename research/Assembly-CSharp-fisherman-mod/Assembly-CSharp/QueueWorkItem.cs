using System;

public class QueueWorkItem
{
	public object Data { get; set; }

	public Action<object> ResultAction { get; set; }
}
