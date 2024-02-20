using System;

[AttributeUsage(AttributeTargets.Field)]
public class TriggerVariableAttribute : Attribute
{
	public string Name { get; set; }
}
