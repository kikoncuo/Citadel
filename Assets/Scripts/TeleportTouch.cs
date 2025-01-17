﻿using UnityEngine;
using System.Collections;

public class TeleportTouch : MonoBehaviour {
	public Transform targetDestination; // assign in the editor
	public bool playSound;
	public AudioClip SoundFX;
	public GameObject teleportFX;
	public float justUsed = 0f;
	private AudioSource SoundFXSource;

	void Awake () {
		SoundFXSource = GetComponent<AudioSource>();
	}

	void  OnTriggerEnter ( Collider col  ) {
		if ((col.gameObject.tag == "Player") && (col.gameObject.GetComponent<PlayerHealth>().hm.health > 0f) && (justUsed < Time.time)) {
			teleportFX.SetActive(true);
			col.transform.position = targetDestination.position;
			targetDestination.transform.gameObject.GetComponent<TeleportTouch>().justUsed = Time.time + 1.0f;
			if (playSound)
				SoundFXSource.PlayOneShot(SoundFX);
		}
	}
}