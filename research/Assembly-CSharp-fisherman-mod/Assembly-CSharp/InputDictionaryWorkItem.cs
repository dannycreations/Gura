using System;
using System.Collections.Generic;

public class InputDictionaryWorkItem
{
	public Dictionary<byte, object> Data { get; set; }

	public Func<Dictionary<byte, object>, object> ProcessAction { get; set; }

	public Action<object> ResultAction { get; set; }

	public Action ErrorAction { get; set; }
}
