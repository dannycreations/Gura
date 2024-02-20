using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Interfaces;
using UnityEngine;

public static class DebugUtility
{
	static DebugUtility()
	{
		UnityDebugSubject.Logger = delegate(string level, string format, object[] args)
		{
			if (level != null)
			{
				if (!(level == "INFO"))
				{
					if (level == "WARN" || level == "ERROR")
					{
						format = "<color=red>" + format + "</color>";
					}
				}
				else
				{
					format = "<color=blue>" + format + "</color>";
				}
			}
			Debug.LogFormat(format, args);
		};
		DebugUtility.Monitored.Add(DebugUtility.Missing);
		DebugUtility.Monitored.Add(DebugUtility.Settings);
		DebugUtility.Monitored.Add(DebugUtility.Steam);
		DebugUtility.Monitored.Add(DebugUtility.Input);
		DebugUtility.Monitored.Add(DebugUtility.Sky);
		DebugUtility.Monitored.Add(DebugUtility.Missions);
		DebugUtility.Monitored.Add(DebugUtility.Inventory);
	}

	public static void SetMonitoredSubjects(string[] subjects)
	{
		DebugUtility.ServerMonitoredSubjects.Clear();
		using (IEnumerator<string> enumerator = (from s in subjects
			select s.Trim() into s
			where !string.IsNullOrEmpty(s)
			select s).GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				string subjectName = enumerator.Current;
				UnityDebugSubject unityDebugSubject = DebugUtility.allSubjects.FirstOrDefault((UnityDebugSubject s) => s.Prefix == subjectName);
				if (unityDebugSubject != null)
				{
					DebugUtility.ServerMonitoredSubjects.Add(unityDebugSubject);
				}
			}
		}
	}

	private static UnityDebugSubject Register(UnityDebugSubject subject)
	{
		subject.SetLogger(delegate(string l, string f, object[] a)
		{
			DebugUtility.UnityLogger(subject, l, f, a);
		});
		DebugUtility.allSubjects.Add(subject);
		return subject;
	}

	private static void UnityLogger(UnityDebugSubject subject, string level, string format, params object[] args)
	{
		if (DebugUtility.ServerMonitoredSubjects.Contains(subject) || DebugUtility.Monitored.Contains(subject))
		{
			UnityDebugSubject.Logger.Invoke(level, format, args);
		}
		if (DebugUtility.ServerMonitoredSubjects.Contains(subject))
		{
			ServerTraceHelper.Log(PhotonConnectionFactory.Instance.UserId, level, (args != null && args.Length != 0) ? string.Format(format, args) : format);
		}
	}

	private static readonly HashSet<UnityDebugSubject> allSubjects = new HashSet<UnityDebugSubject>();

	private static HashSet<UnityDebugSubject> Monitored = new HashSet<UnityDebugSubject>();

	private static HashSet<UnityDebugSubject> ServerMonitoredSubjects = new HashSet<UnityDebugSubject>();

	public static readonly UnityDebugSubject Missing = DebugUtility.Register(new UnityDebugSubject("<color=red>!!!Missing!!!</color>", 0));

	public static readonly UnityDebugSubject Settings = DebugUtility.Register(new UnityDebugSubject("Settings", 0));

	public static readonly UnityDebugSubject Input = DebugUtility.Register(new UnityDebugSubject("Input", 0));

	public static readonly UnityDebugSubject Steam = DebugUtility.Register(new UnityDebugSubject("Steam", 0));

	public static readonly UnityDebugSubject Op = DebugUtility.Register(new UnityDebugSubject("Op", 0));

	public static readonly UnityDebugSubject Connection = DebugUtility.Register(new UnityDebugSubject("Connection", 80));

	public static readonly UnityDebugSubject Travel = DebugUtility.Register(new UnityDebugSubject("Travel", 100));

	public static readonly UnityDebugSubject RoomIdIssue = DebugUtility.Register(new UnityDebugSubject("RoomId Issue", 120));

	public static readonly UnityDebugSubject Sky = DebugUtility.Register(new UnityDebugSubject("Sky", 0));

	public static readonly UnityDebugSubject Missions = DebugUtility.Register(new UnityDebugSubject("Missions", 0));

	public static readonly UnityDebugSubject Inventory = DebugUtility.Register(new UnityDebugSubject("Inventory", 0));
}
