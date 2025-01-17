﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMeleeDamageCollider : MonoBehaviour {
	private int index;
	private int meleeColliderCounter = 0;
	private float impactMelee = 0f;
	private GameObject ownedBy;
    private AIController ownerAIC;
    private MeshCollider meshCollider;
    private BoxCollider boxCollider;
    private SphereCollider sphereCollider;
    private CapsuleCollider capCollider;

	public void MeleeColliderSetup (int ind, int colCount, float impact, GameObject sourceOwner) {
		index = ind;
		meleeColliderCounter = colCount;
		impactMelee = impact;
		ownedBy = sourceOwner;
        ownerAIC = ownedBy.GetComponent<AIController>();
        meshCollider = GetComponent<MeshCollider>();
        boxCollider = GetComponent<BoxCollider>();
        sphereCollider = GetComponent<SphereCollider>();
        capCollider = GetComponent<CapsuleCollider>();

        if (meshCollider != null) meshCollider.isTrigger = true;
        if (boxCollider != null) boxCollider.isTrigger = true;
        if (sphereCollider != null) sphereCollider.isTrigger = true;
        if (capCollider != null) capCollider.isTrigger = true;
    }

	void OnTriggerEnter (Collider other) {
        if (other.tag == "Player" || other.tag == "NPC") {
            if (ownerAIC.meleeDamageFinished < Time.time) {
                ownerAIC.meleeDamageFinished = Time.time + ownerAIC.timeTillMeleeDamage;
                //Debug.Log("aimelee collider collided!");
                DamageData ddNPC = Const.SetNPCDamageData(index, Const.aiState.Attack1, ownedBy);
                ddNPC.other = gameObject;
                ddNPC.attacknormal = Vector3.Normalize(other.transform.position - transform.position);
                ddNPC.impactVelocity = impactMelee;
                ddNPC.damage = 11f;
                float take = Const.a.GetDamageTakeAmount(ddNPC);
                take = (take / meleeColliderCounter); //split it for multiple tap melee attacks, e.g. double paw swipe
                ddNPC.damage = take;
                other.GetComponent<HealthManager>().TakeDamage(ddNPC);
            }
		}
	}
}
