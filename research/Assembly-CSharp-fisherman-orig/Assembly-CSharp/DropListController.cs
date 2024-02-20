using System;
using UnityEngine;
using UnityEngine.UI;

public class DropListController : MonoBehaviour
{
	public void ControllRPM()
	{
		if (this.rpmDropdownList.value == 0)
		{
			this.gasPedalButton.SetActive(true);
			for (int i = 0; i < this.sounds.Length; i++)
			{
				this.sounds[i].GetComponent<SetRPMToSlider>().simulated = true;
			}
			this.gasPedalButton.GetComponent<CarSimulator>().rpm = this.sounds[0].GetComponent<SetRPMToSlider>().rpmSlider.value;
		}
		if (this.rpmDropdownList.value == 1)
		{
			this.gasPedalButton.SetActive(false);
			for (int j = 0; j < this.sounds.Length; j++)
			{
				this.sounds[j].GetComponent<SetRPMToSlider>().simulated = false;
			}
			if (this.gasPedalPressingCheckbox != null)
			{
				this.gasPedalPressingCheckbox.isOn = true;
			}
		}
	}

	public Dropdown rpmDropdownList;

	public GameObject gasPedalButton;

	public Toggle gasPedalPressingCheckbox;

	public GameObject[] sounds;
}
