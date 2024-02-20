using System;

[AttributeUsage(AttributeTargets.Class)]
public class TriggerNameAttribute : Attribute
{
	public string Name { get; set; }
}
