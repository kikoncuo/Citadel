﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VaporizeButton : MonoBehaviour {
	public GeneralInventory playerGenInv;
	public GeneralInvCurrent playerGenCur;
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

	public void OnVaporizeClick() {
		playerGenInv.generalInventoryIndexRef[playerGenCur.generalInvCurrent] = -1;
	}
}
