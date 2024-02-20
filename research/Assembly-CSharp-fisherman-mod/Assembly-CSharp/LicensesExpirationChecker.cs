using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI._2D.Tournaments.UGC;
using I2.Loc;
using ObjectModel;
using UnityEngine;

public class LicensesExpirationChecker : MonoBehaviour
{
	public static LicensesExpirationChecker Instance
	{
		get
		{
			return LicensesExpirationChecker._instance;
		}
	}

	private void Awake()
	{
		if (LicensesExpirationChecker._instance != null)
		{
			Object.Destroy(LicensesExpirationChecker._instance.gameObject);
		}
		LicensesExpirationChecker._instance = this;
		Object.DontDestroyOnLoad(this);
	}

	private void ShowMessage(bool isExpired, PlayerLicense license)
	{
		GameFactory.Message.ShowMessage(string.Format(ScriptLocalization.Get((!isExpired) ? "LicenseAboutToExpire" : "LicenseExpired"), UgcConsts.GetYellowTan(license.Name)), base.transform.root.gameObject, 4f, true);
	}

	private void Update()
	{
		Profile profile = PhotonConnectionFactory.Instance.Profile;
		if (profile == null || profile.Licenses == null || ManagerScenes.InTransition)
		{
			return;
		}
		this._currentTime += Time.deltaTime;
		if (this._currentTime < 4f)
		{
			return;
		}
		this._currentTime = 0f;
		if (!this._inited)
		{
			IEnumerable<string> enumerable = from l in profile.Licenses
				where l.CanExpire && l.End < TimeHelper.UtcTime()
				select l.Name;
			this._expiredLicenses.UnionWith(enumerable);
			this._aboutToExpireLicenses.UnionWith(enumerable);
			this._inited = true;
			return;
		}
		for (int i = 0; i < profile.Licenses.Count; i++)
		{
			PlayerLicense playerLicense = profile.Licenses[i];
			if (playerLicense.CanExpire)
			{
				bool flag = playerLicense.End <= TimeHelper.UtcTime() && !this._expiredLicenses.Contains(playerLicense.Name);
				bool flag2 = playerLicense.End <= TimeHelper.UtcTime().AddMinutes(5.0) && !this._aboutToExpireLicenses.Contains(playerLicense.Name);
				if (flag)
				{
					this._expiredLicenses.Add(playerLicense.Name);
				}
				if (flag2)
				{
					this._aboutToExpireLicenses.Add(playerLicense.Name);
				}
				if (flag || flag2)
				{
					if (flag && this.OnLicenseLost != null)
					{
						this.OnLicenseLost(playerLicense);
					}
					if (GameFactory.Message != null)
					{
						if (GameFactory.Message.IsDisplaying)
						{
							if (flag)
							{
								GameFactory.Message.KillLastMessage();
								this.ShowMessage(flag, playerLicense);
							}
							else
							{
								this._aboutToExpireLicenses.Remove(playerLicense.Name);
							}
						}
						else
						{
							this.ShowMessage(flag, playerLicense);
						}
						break;
					}
				}
				if (playerLicense.End > TimeHelper.UtcTime() && this._expiredLicenses.Contains(playerLicense.Name))
				{
					this._aboutToExpireLicenses.Remove(playerLicense.Name);
					this._expiredLicenses.Remove(playerLicense.Name);
				}
			}
		}
	}

	private HashSet<string> _aboutToExpireLicenses = new HashSet<string>();

	private HashSet<string> _expiredLicenses = new HashSet<string>();

	private bool _inited;

	private const int NotifyBeforeMinutes = 5;

	private const float MessageLifeTime = 4f;

	private float _currentTime = 5f;

	private static LicensesExpirationChecker _instance;

	public Action<PlayerLicense> OnLicenseAcquired;

	public Action<PlayerLicense> OnLicenseLost;
}
