using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class HardwareInvButton : MonoBehaviour {
    public int HardwareInvButtonIndex;
    public int useableItemIndex;
	public HardwareInventory hardwareInventory;
	public MFDManager mfdManager;
	public Text butText;

    void HardwareInvClick() {
		HardwareInvCurrent.a.hardwareInvCurrent = HardwareInvButtonIndex;  //Set current
		mfdManager.SendInfoToItemTab(useableItemIndex);
    }
		
    void Start() {
        GetComponent<Button>().onClick.AddListener(() => { HardwareInvClick(); });
		butText.text = Const.a.useableItemsNameText [useableItemIndex];
    }

    //void Update() {
		//useableItemIndex = HardwareInventory.a.hardwareInventoryIndexRef[HardwareInvButtonIndex];
		//butText.text = Const.a.useableItemsNameText [useableItemIndex];
    //}
}
