using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ApplyButton : MonoBehaviour {
	public PatchInventory playerPatchInv;
	public PatchCurrent playerPatchCur;
	public PatchButtonsManager playerPatchButtons;
	public GameObject playerCamera;
	public Image ico;
	public Text ict;

	public void PtrEnter () {
		GUIState.a.PtrHandler(true,true,GUIState.ButtonType.Generic,gameObject);
		playerCamera.GetComponent<MouseLookScript>().currentButton = gameObject;
	}

	public void PtrExit () {
		GUIState.a.PtrHandler(false,false,GUIState.ButtonType.None,null);
	}

	public void OnApplyClick() {
		playerPatchButtons.patchButtons[playerPatchCur.patchCurrent].GetComponent<PatchButton>().DoubleClick();
	}
}
