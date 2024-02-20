using System;
using UnityEngine;
using UnityEngine.UI;

public class DropListCntrlMobile : MonoBehaviour
{
	public void ControllRPM()
	{
		if (this.rpmDropdownList.value == 0)
		{
			this.gasPedalButton.SetActive(true);
			for (int i = 0; i < this.sounds.Length; i++)
			{
				this.sounds[i].GetComponent<MobileSetRPMToSlider>().simulated = true;
			}
			this.gasPedalButton.GetComponent<CarSimulator>().rpm = this.sounds[0].GetComponent<MobileSetRPMToSlider>().rpmSlider.value;
		}
		if (this.rpmDropdownList.value == 1)
		{
			this.gasPedalButton.SetActive(false);
			for (int j = 0; j < this.sounds.Length; j++)
			{
				this.sounds[j].GetComponent<MobileSetRPMToSlider>().simulated = false;
			}
		}
	}

	public Dropdown rpmDropdownList;

	public GameObject gasPedalButton;

	public GameObject[] sounds;
}
