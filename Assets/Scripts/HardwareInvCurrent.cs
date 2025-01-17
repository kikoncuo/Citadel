using UnityEngine;
using System.Collections;

public class HardwareInvCurrent : MonoBehaviour {
	public int hardwareInvCurrent;
	public int hardwareInvIndex;
	public int[] hardwareInventoryIndices = new int[]{0,1,2,3,4,5,6,7,8,9,10,11,12,13};
	public static HardwareInvCurrent a;
	public bool[] hardwareIsActive;
	public HardwareInventory hwi;
	public HardwareButton[] hardwareButtons;
	
	void Awake() {
		a = this;
        a.hardwareInvCurrent = 0; // Current slot in the general inventory (14 slots)
        a.hardwareInvIndex = 0; // Current index to the item look-up table
	}

	void Update() {
		if (GetInput.a.HardwareCycUp()) {
			hwi.hardwareVersionSetting [hardwareInvCurrent]++;
			if (hwi.hardwareVersionSetting [hardwareInvCurrent] > hwi.hardwareVersion [hardwareInvCurrent])
				hwi.hardwareVersionSetting [hardwareInvCurrent] = hwi.hardwareVersion [hardwareInvCurrent];

			if (hardwareIsActive [hardwareInvCurrent]) {
				hardwareButtons [hardwareInvCurrent].ChangeHardwareVersion (hardwareInvCurrent,hwi.hardwareVersionSetting[hardwareInvCurrent]);
			}
		}

		if (GetInput.a.HardwareCycDown()) {
			hwi.hardwareVersionSetting [hardwareInvCurrent]--;
			if (hwi.hardwareVersionSetting [hardwareInvCurrent] < 1)
				hwi.hardwareVersionSetting [hardwareInvCurrent] = 1;

			if (hardwareIsActive [hardwareInvCurrent]) {
				hardwareButtons [hardwareInvCurrent].ChangeHardwareVersion (hardwareInvCurrent,hwi.hardwareVersionSetting[hardwareInvCurrent]);
			}
		}
	}
}
