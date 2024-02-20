using System;
using System.Collections.Generic;

public static class PondsLicenses
{
	public static readonly Dictionary<PondsLicenses.US, string> LicensesesLocalization = new Dictionary<PondsLicenses.US, string>
	{
		{
			PondsLicenses.US.Alaska,
			"AlaskaState"
		},
		{
			PondsLicenses.US.Alberta,
			"AlbertaMenu"
		},
		{
			PondsLicenses.US.California,
			"CaliforniaMenu"
		},
		{
			PondsLicenses.US.Colorado,
			"ColoradoMenu"
		},
		{
			PondsLicenses.US.Florida,
			"FloridaMenu"
		},
		{
			PondsLicenses.US.Louisiana,
			"LouisianaMenu"
		},
		{
			PondsLicenses.US.Michigan,
			"MichiganMenu"
		},
		{
			PondsLicenses.US.Missouri,
			"MissouryMenu"
		},
		{
			PondsLicenses.US.NewYork,
			"NewYorkMenu"
		},
		{
			PondsLicenses.US.NorthCarolina,
			"NorthCarolinaMenu"
		},
		{
			PondsLicenses.US.Oregon,
			"OregonMenu"
		},
		{
			PondsLicenses.US.Texas,
			"TexasState"
		}
	};

	public enum US : byte
	{
		Unknown,
		Alaska = 108,
		California = 102,
		Michigan = 107,
		Louisiana = 116,
		Alberta = 113,
		Florida = 105,
		Oregon = 100,
		NorthCarolina = 1,
		Colorado = 106,
		NewYork = 110,
		Missouri = 103,
		Texas = 109
	}
}
