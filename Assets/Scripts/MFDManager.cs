using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MFDManager : MonoBehaviour  {
	public TabButtonsScript leftTC;
	public TabButtonsScript rightTC;
	public ItemTabManager itemTabLH;
	public ItemTabManager itemTabRH;
	public DataTab dataTabLH;
	public DataTab dataTabRH;
	public bool lastWeaponSideRH;
	public bool lastItemSideRH;
	public bool lastAutomapSideRH;
	public bool lastTargetSideRH;
	public bool lastDataSideRH;
	public bool lastSearchSideRH;
	public bool lastLogSideRH;
	public bool lastLogSecondarySideRH;
	public bool lastMinigameSideRH;
	public enum TabMSG {None,Search,AudioLog,Keypad,Elevator,GridPuzzle,WirePuzzle};
	public static MFDManager a;

	// External to gameObject, assigned in Inspector
	public GameObject searchFX;
	public GameObject searchOriginContainer;

	public void Awake () {
		a = this;
	}

	public void OpenTab(int index, bool overrideToggling,TabMSG type,int intdata1) {
		bool isRH = false;
		switch (index) {
			case 0: isRH = lastWeaponSideRH; break;
			case 1: isRH = lastItemSideRH; break;
			case 2: isRH = lastAutomapSideRH; break;
			case 3: isRH = lastTargetSideRH; break;
			case 4: isRH = lastDataSideRH; break;
		}
		if(!isRH) {
			// RH MFD
			leftTC.TabButtonClickSilent(index,overrideToggling);
			leftTC.SetCurrentAsLast();
			if (type == TabMSG.AudioLog) {
				dataTabLH.Reset();
				dataTabLH.audioLogContainer.SetActive(true);
				dataTabLH.audioLogContainer.GetComponent<LogDataTabContainerManager>().SendLogData(intdata1);
			}

			if (type == TabMSG.Keypad) {
				dataTabLH.Reset();
				dataTabLH.keycodeUIControl.SetActive(true);
			}

			if (type == TabMSG.Elevator) {
				dataTabLH.Reset();
				dataTabLH.elevatorUIControl.SetActive(true);
			}

			if (type == TabMSG.GridPuzzle) {
				dataTabLH.Reset();
				dataTabLH.puzzleGrid.SetActive(true);
			}
		} else {
			// LH MFD
			rightTC.TabButtonClickSilent(index,overrideToggling);
			rightTC.SetCurrentAsLast();
			if (type == TabMSG.AudioLog) {
				dataTabRH.Reset();
				dataTabRH.audioLogContainer.SetActive(true);
				dataTabRH.audioLogContainer.GetComponent<LogDataTabContainerManager>().SendLogData(intdata1);
			}

			if (type == TabMSG.Keypad) {
				dataTabRH.Reset();
				dataTabRH.keycodeUIControl.SetActive(true);
			}

			if (type == TabMSG.Elevator) {
				dataTabRH.Reset();
				dataTabRH.elevatorUIControl.SetActive(true);
			}

			if (type == TabMSG.GridPuzzle) {
				dataTabLH.Reset();
				dataTabLH.puzzleGrid.SetActive(true);
			}
		}
	}

	public void SendSearchToDataTab (string name, int contentCount, int[] resultContents, int[] resultsIndices) {
		// Enable search box scaling effect
		searchOriginContainer.GetComponent<RectTransform>().position = Input.mousePosition;
		searchFX.SetActive(true);

		if (lastSearchSideRH) {
			dataTabRH.Search(name,contentCount,resultContents,resultsIndices);
			searchFX.GetComponent<Animation>().Play();  // TODO: change search FX to move to correct positions
			OpenTab(4,true,TabMSG.Search,0);
		} else {
			dataTabLH.Search(name,contentCount,resultContents,resultsIndices);
			searchFX.GetComponent<Animation>().Play();
			OpenTab(4,true,TabMSG.Search,0);
		}
	}

	public void SendGridPuzzleToDataTab (bool[] states, PuzzleGrid.CellType[] types, PuzzleGrid.GridType gtype, int start, int end, int width, int height, PuzzleGrid.GridColorTheme colors) {
		if (lastDataSideRH) {
			// Send to RH tab
			dataTabRH.GridPuzzle(states,types,gtype,start,end, width, height,colors);
			OpenTab(4,true,TabMSG.GridPuzzle,0);
		} else {
			// Send to LH tab
			dataTabLH.GridPuzzle(states,types,gtype,start,end, width, height,colors);
			OpenTab(4,true,TabMSG.GridPuzzle,0);
		}
	}
}