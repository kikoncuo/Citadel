﻿using UnityEngine;
#if UNITY_EDITOR 
	using UnityEditor;
#endif
using System.Collections;

public class PlayerHealth : MonoBehaviour {
	//public float health = 211f; //max is 255
	public float radiated = 0f;
	public float resetAfterDeathTime = 0.5f;
	public float timer;
	public static bool playerDead = false;
	public bool mediPatchActive = false;
	public float mediPatchPulseTime = 1f;
	public float mediPatchHealAmount = 10f;
	public bool detoxPatchActive = false;
	public AudioSource PlayerNoise;
	public AudioClip PainSFXClip;
	public AudioClip RadiationClip;
	public GameObject cameraObject;
	public GameObject hardwareShield;
	public GameObject radiationEffect;
	private bool shieldOn = false;
	public bool radiationArea = false;
	private float radiationBleedOffFinished = 0f;
	public float radiationBleedOffTime = 1f;
	public float radiationReductionAmount = 1f;
	public float radiationHealthDamageRatio = 0.2f;
	public GameObject mainPlayerParent;
	public int radiationAmountWarningID = 323;
	public int radiationAreaWarningID = 322;
	public float mediPatchPulseFinished = 0f;
	public int mediPatchPulseCount = 0;
	public bool makingNoise = false;
	[HideInInspector]
	public HealthManager hm;
	public GameObject healingFXFlash;

	private float lastHealth;
	private float painSoundFinished;
	private float radSoundFinished;
	private float radFXFinished;
	private TextWarningsManager twm;

	void Awake () {
		twm = mainPlayerParent.GetComponent<PlayerReferenceManager>().playerTextWarningManager.GetComponent<TextWarningsManager>();
		hm = GetComponent<HealthManager>();
		if (hm == null) Debug.Log("BUG: No HealthManager script found on player!!");
		painSoundFinished = Time.time;
		radSoundFinished = Time.time;
		radFXFinished = Time.time;
		lastHealth = hm.health;
	}

	void Update (){
		if (PlayerNoise.isPlaying)
			makingNoise = true;
		else
			makingNoise = false;

		if (hm.health <= 0f) {
			if (!playerDead) {
				PlayerDying();
			} else {
				PlayerDead();
			}
		}

		if (mediPatchActive) {
			if (mediPatchPulseFinished == 0) mediPatchPulseCount = 0;
			if (mediPatchPulseFinished < Time.time) {
				float timePulse = mediPatchPulseTime;
				hm.health += mediPatchHealAmount; // give health
				//if (hm.health > hm.maxhealth) hm.health = hm.maxhealth; // handled by HealthManager.cs
				timePulse += (mediPatchPulseCount*0.5f);
				mediPatchPulseFinished = Time.time + timePulse;
				mediPatchPulseCount++;
			}
		} else {
			mediPatchPulseFinished = 0;
			mediPatchPulseCount = 0;
		}

		if (detoxPatchActive) {
			radiated = 0f;
		}

		if (radiated > 0) {
			if (radiationArea) twm.SendWarning(("Radiation Area"),0.1f,-2,TextWarningsManager.warningTextColor.white,radiationAreaWarningID);
			twm.SendWarning(("Radiation poisoning "+radiated.ToString()+" LBP"),0.1f,-2,TextWarningsManager.warningTextColor.red,radiationAmountWarningID);
			if (radFXFinished < Time.time) {
				radiationEffect.SetActive(true);
				radFXFinished = Time.time + Random.Range(0.4f,1f);
			}
		}

		if (radiated < 1) {
			radiationArea = false;
		}

		if (radiationBleedOffFinished < Time.time) {
			if (radiated > 0) {
				hm.health -= radiated*radiationHealthDamageRatio*radiationBleedOffTime; // apply health at rate of bleedoff time
				if (!radiationArea) {
					radiated -= radiationReductionAmount;  // bleed off the radiation over time
				} else {
					if (radSoundFinished < Time.time) {
						radSoundFinished = Time.time + Random.Range(0.5f,1.5f);
						PlayerNoise.PlayOneShot(RadiationClip);
					}
				}
				radiationBleedOffFinished = Time.time + radiationBleedOffTime;
			}
		}

		// Did we lose health?
		if (lastHealth > hm.health) {
			if (painSoundFinished < Time.time && !(radSoundFinished < Time.time)) {
				painSoundFinished = Time.time + Random.Range(0.5f,1.5f); // Don't spam pain sounds
				PlayerNoise.PlayOneShot(PainSFXClip);
			}
		}
		lastHealth = hm.health;
	}
	
	void PlayerDying (){
		timer += Time.deltaTime;
		
		if (timer >= resetAfterDeathTime) {
			hm.health = 0f;
			playerDead = true;
		}
	}
	
	void PlayerDead (){
		//gameObject.GetComponent<PlayerMovement>().enabled = false;
		//cameraObject.SetActive(false);
		Cursor.lockState = CursorLockMode.None;
		#if UNITY_EDITOR
		if (Application.isEditor) {
			EditorApplication.isPlaying = false;
			return;
		}
		#endif
		cameraObject.GetComponent<Camera>().enabled = false;
	}
	
	public void TakeDamage (DamageData dd){
		float shieldBlock = 0f;
		if (shieldOn) {
			//shieldBlock = hardwareShield.GetComponent<Shield>().GetShieldBlock();
		}
		dd.armorvalue = shieldBlock;
		dd.defense = 0f;
		float take = Const.a.GetDamageTakeAmount(dd);
		hm.health -= take;
		PlayerNoise.PlayOneShot(PainSFXClip);
		//Debug.Log("Player Health: " + health.ToString());
	}

	public void GiveRadiation (float rad) {
		if (radiated < rad)
			radiated = rad;

		//radiated -= suitReduction;
	}

	public void HealingBed(float amount) {
		hm.HealingBed(amount);
		if (healingFXFlash != null) {
			healingFXFlash.SetActive(true);
		}
	}
}