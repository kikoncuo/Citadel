﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MultiMediaTabManager : MonoBehaviour {
	public GameObject startingSubTab;
	public GameObject secondaryTab1;
	public GameObject secondaryTab2;
	public GameObject emailTab;
	public GameObject dataTab;
	public GameObject headerLabel;

	public void ResetTabs() {
		startingSubTab.SetActive(false);
		secondaryTab1.SetActive(false);
		secondaryTab2.SetActive(false);
		emailTab.SetActive(false);
		dataTab.SetActive(false);
		headerLabel.GetComponent<Text>().text = System.String.Empty;
	}

	public void OpenLogTableContents() {
		ResetTabs();
		startingSubTab.SetActive(true);
		headerLabel.GetComponent<Text>().text = "LOGS";
	}

	public void OpenLogsLevelFolder(int curlevel) {
		ResetTabs();
		secondaryTab1.SetActive(true);
		headerLabel.GetComponent<Text>().text = "Level " + curlevel.ToString() + " Logs";
		secondaryTab1.GetComponent<LogContentsButtonsManager>().currentLevelFolder = curlevel;
		secondaryTab1.GetComponent<LogContentsButtonsManager>().InitializeLogsFromLevelIntoFolder();
	}

	public void OpenLogTextReader() {
		ResetTabs();
		secondaryTab2.SetActive(true);
	}

	public void OpenEmailTableContents() {
		ResetTabs();
		emailTab.SetActive(true);
		headerLabel.GetComponent<Text>().text = "EMAIL";
	}

	public void OpenDataTableContents() {
		ResetTabs();
		dataTab.SetActive(true);
		headerLabel.GetComponent<Text>().text = "DATA";
	}
}
