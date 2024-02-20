﻿using System;

public class InputByteArrayWorkItem
{
	public byte[] Data { get; set; }

	public Func<byte[], object> ProcessAction { get; set; }

	public Action<object> ResultAction { get; set; }
}
