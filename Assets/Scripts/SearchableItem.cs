﻿using UnityEngine;
using System.Collections;

public class SearchableItem : MonoBehaviour {
	[Tooltip("Use lookUp table instead of randomItem#'s for default random item generation, for example for NPCs")]
	public int lookUpIndex = 0; // For randomly generating items
	[Tooltip("The indices referring to the prefab in Const table to have inside this searchable")]
	public int[] contents = {-1,-1,-1,-1};
	[Tooltip("Custom item indices of contents, for referring to specific attributes of content such as log type")]
	public int[] customIndex = {-1,-1,-1,-1};
	[Tooltip("Whether to randomly generate search items based on randomItem# indices")]
	public bool  generateContents = false;
	[Tooltip("Pick index from Const list of potential random item.")]
	public int[] randomItem; // possible item this container could contain if generateContents is true
	[Tooltip("Pick index from Const list of potential random item.")]
	public int[] randomItemCustomIndex; // possible item this container could contain if generateContents is true
	[Tooltip("Name of the searchable item.")]
	public string objectName;
	[Tooltip("Number of slots.")]
	public int numSlots = 4;
	[HideInInspector]
	public bool searchableInUse;
	[HideInInspector]
	public GameObject currentPlayerCapsule;
	private float disconnectDist;

	void Start () {
		disconnectDist = Const.a.frobDistance;
	}

	void Update () {
		if (searchableInUse) {
			if (Vector3.Distance(currentPlayerCapsule.transform.position, gameObject.transform.position) > disconnectDist) {
				searchableInUse = false;
				MFDManager.a.ClearDataTab();
			}
		}
	}
}