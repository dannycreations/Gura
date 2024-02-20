using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class DebugPlotter
{
	public DebugPlotter(string relativeFilepath, string[] valuesNames, bool autoSave = false)
	{
		this.filepath = relativeFilepath;
		this.orderedValues = new List<DebugPlotter.Value>();
		this.values = new Dictionary<string, DebugPlotter.Value>();
		foreach (string text in valuesNames)
		{
			DebugPlotter.Value value = new DebugPlotter.Value(text, this);
			this.orderedValues.Add(value);
			this.values[text] = value;
		}
		if (autoSave)
		{
			if (DebugPlotter.AutoSaveInstances.ContainsKey(this.filepath))
			{
				Regex regex = new Regex("(.+?)(\\d+)(\\.adv)");
				Match match = regex.Match(this.filepath);
				if (match.Groups.Count == 1)
				{
					Regex regex2 = new Regex("(.+?)(\\.adv)");
					Match match2 = regex2.Match(this.filepath);
					if (match2.Groups.Count == 3)
					{
						this.filepath = match2.Groups[1].Value + "1" + match2.Groups[2].Value;
					}
				}
				else if (match.Groups.Count == 4)
				{
					this.filepath = match.Groups[1].Value + (int.Parse(match.Groups[2].Value) + 1).ToString() + match.Groups[3].Value;
				}
			}
			DebugPlotter.AutoSaveInstances[this.filepath] = this;
		}
	}

	public static void AutoSave()
	{
		if (DebugPlotter.AutoSaveInstances.Count > 0)
		{
			string text = string.Empty;
			foreach (DebugPlotter debugPlotter in DebugPlotter.AutoSaveInstances.Values)
			{
				text = text + debugPlotter.filepath + "\n";
			}
			Debug.LogWarning(string.Concat(new object[]
			{
				"DebugPlotter.AutoSave: Saving ",
				DebugPlotter.AutoSaveInstances.Count,
				" plot tables:\n",
				text
			}));
		}
		foreach (DebugPlotter debugPlotter2 in DebugPlotter.AutoSaveInstances.Values)
		{
			debugPlotter2.Save();
		}
	}

	public string filepath { get; private set; }

	public static DebugPlotter CreateDebugPlotterWithIndexedValues(string relativeFilePath, int valuesCount, bool autoSave = false)
	{
		string[] array = new string[valuesCount];
		for (int i = 0; i < valuesCount; i++)
		{
			array[i] = "value_" + i.ToString();
		}
		return new DebugPlotter(relativeFilePath, array, autoSave);
	}

	public DebugPlotter.Value AddValue(string valuename)
	{
		DebugPlotter.Value value = new DebugPlotter.Value(valuename, this);
		this.orderedValues.Add(value);
		this.values[valuename] = value;
		for (int i = 0; i < this.framesCount; i++)
		{
			value.SkipFrame();
		}
		return value;
	}

	public DebugPlotter.Value GetValue(string valueName)
	{
		if (this.values.ContainsKey(valueName))
		{
			return this.values[valueName];
		}
		return null;
	}

	public DebugPlotter.Value GetValue(int indexedValue)
	{
		return this.GetValue("value_" + indexedValue.ToString());
	}

	public void Save()
	{
	}

	protected void onValueUpdate(DebugPlotter.Value sender)
	{
		if (this.framesCount < sender.Count)
		{
			this.framesCount = sender.Count;
		}
		foreach (DebugPlotter.Value value in this.orderedValues)
		{
			if (value.Count + 2 == sender.Count)
			{
				value.SkipFrame();
			}
		}
	}

	public static Dictionary<string, DebugPlotter> AutoSaveInstances = new Dictionary<string, DebugPlotter>();

	private List<DebugPlotter.Value> orderedValues;

	private Dictionary<string, DebugPlotter.Value> values;

	private int framesCount;

	public class Value
	{
		public Value(string name, DebugPlotter owner)
		{
			this.Name = name;
			this.owner = owner;
			this.data = new List<float>();
		}

		public string Name { get; private set; }

		public void Add(float sample)
		{
		}

		public float Get(int index)
		{
			if (index < this.Count)
			{
				return this.data[index];
			}
			return this.LastSample;
		}

		public void SkipFrame()
		{
			this.data.Add(this.LastSample);
		}

		public float LastSample
		{
			get
			{
				return (this.data.Count <= 0) ? 0f : this.data[this.data.Count - 1];
			}
		}

		public int Count
		{
			get
			{
				return this.data.Count;
			}
		}

		private List<float> data;

		private DebugPlotter owner;
	}
}
