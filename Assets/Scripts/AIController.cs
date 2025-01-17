﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIController : MonoBehaviour {
	public int index = 0; // NPC reference index for looking up constants in tables in Const.cs
	public Const.aiState currentState;
    public Const.aiMoveType moveType;
	public GameObject enemy;
    public GameObject searchColliderGO;
	public float yawspeed = 180f;
	public float fieldOfViewAngle = 180f;
    public float fieldOfViewAttack = 80f;
    public float fieldOfViewStartMovement = 45f;
    public float distToSeeWhenBehind = 2.5f;
	public float sightRange = 50f;
	public float walkSpeed = 0.8f;
	public float runSpeed = 0.8f;
	public float meleeSpeed = 0.5f;
	public float proj1Speed = 0f;
	public float proj2Speed = 0f;
	public float meleeRange = 2f;
	public float proj1Range = 10f;
	public float proj2Range = 20f;
	public float attack3Force = 15f;
	public float attack3Radius = 10f;
	public float timeToPain = 2f; // time between going into pain animation
	public float timeBetweenPain = 5f;
	public float timeTillDead = 1.5f;
	public float timeTillMeleeDamage = 0.5f;
    public float timeTillActualAttack2 = 0.5f;
    public float gracePeriodFinished;
    [HideInInspector]
    public float meleeDamageFinished;
	public float timeBetweenMelee = 1.2f;
	public float timeBetweenProj1 = 1.5f;
	public float timeBetweenProj2 = 3f;
	public float changeEnemyTime = 3f; // Time before enemy will switch to different attacker
    public float idleSFXTimeMin = 5f;
    public float idleSFXTimeMax = 12f;
    public float attack2MinRandomWait = 1f;
    public float attack2MaxRandomWait = 3f;
    public float attack2RandomWaitChance = 0.5f; //50% chance of waiting a random amount of time before attacking to allow for movement
	public float impactMelee = 10f;
	public float impactMelee2 = 10f;
    public float verticalViewOffset = 0f;
    public Const.AttackType attack1Type = Const.AttackType.Melee;
    public Const.AttackType attack2Type = Const.AttackType.Projectile;
    public Const.AttackType attack3Type = Const.AttackType.Projectile;
    public Vector3 explosionOffset;
	public AudioClip SFXIdle;
	public AudioClip SFXFootstep;
	public AudioClip SFXSightSound;
	public AudioClip SFXAttack1;
	public AudioClip SFXAttack2;
	public AudioClip SFXAttack3;
	public AudioClip SFXPainClip;
	public AudioClip SFXDeathClip;
	public AudioClip SFXInspect;
	public AudioClip SFXInteracting;
	public bool rememberEnemyIfOutOfSight = false;
	public bool walkPathOnStart = false;
    public bool dontLoopWaypoints = false;
	public bool visitWaypointsRandomly = false;
	public bool hasMelee = true;
	public bool hasProj1 = false;
	public bool hasProj2 = false;
	public Transform[] walkWaypoints; // point(s) for NPC to walk to when roaming or patrolling
	public bool inSight = false;
    public bool infront;
    public bool inProjFOV;
    public bool LOSpossible;
    public bool goIntoPain = false;
	public bool explodeOnAttack3 = false;
    public bool ignoreEnemy = false;
	public float rangeToEnemy = 0f;
	public GameObject[] meleeDamageColliders;
    public GameObject muzzleBurst;
    public GameObject rrCheckPoint;
	[HideInInspector]
	public GameObject attacker;
	private bool hasSFX;
	public bool firstSighting;
	private bool dyingSetup;
	private bool ai_dying;
	private bool ai_dead;
	private int currentWaypoint;
	private float idleTime;
	private float attack1SoundTime;
	private float attack2SoundTime;
	private float attack3SoundTime;
	private float timeBetweenMeleeFinished;
	private float timeTillEnemyChangeFinished;
	private float timeTillDeadFinished;
	private float timeTillPainFinished;
	private AudioSource SFX;
	private NavMeshAgent nav;
	private Rigidbody rbody;
	private HealthManager healthManager;
	private BoxCollider boxCollider;
	private CapsuleCollider capsuleCollider;
	private SphereCollider sphereCollider;
	private MeshCollider meshCollider;
	private float tick;
	private float tickFinished;
    public float huntTime = 5f;
    public float huntFinished;
    private float breadFinished;
    private float breadCrumbTick = 2f; // "drop" a breadcrumb and store it in the list every 2 seconds
	private bool hadEnemy;
    private Vector3 lastKnownEnemyPos;
    private List<Vector3> enemyBreadcrumbs;
    private Vector3 tempVec;
    private bool randSpin = false;
    private bool shotFired = false;
    private DamageData damageData;
    private RaycastHit tempHit;
    private bool useBlood;
    private HealthManager tempHM;
    private float randomWaitForNextAttack2Finished;
    public GameObject visibleMeshEntity;
    public GameObject gunPoint;
	public Vector3 idealTransformForward;
    public Vector3 idealPos;
	public float attackFinished;
    public float attack2Finished;
    public float attack3Finished;
    public Vector3 targettingPosition;

	// Initialization and find components
	void Awake () {
		//resetPosition = new Vector3(0f,-100000f,0f); // Null position below playable area
		nav = GetComponent<UnityEngine.AI.NavMeshAgent>();
		nav.updatePosition = true;
		nav.angularSpeed = yawspeed;
        nav.speed = 0;
        nav.SetDestination(transform.position);
        nav.updateRotation = false;
        //anim = GetComponent<Animator>();
        rbody = GetComponent<Rigidbody>();
		rbody.isKinematic = true;
		healthManager = GetComponent<HealthManager>();

	    boxCollider = GetComponent<BoxCollider>();
		sphereCollider = GetComponent<SphereCollider>();
		meshCollider = GetComponent<MeshCollider>();
		capsuleCollider = GetComponent<CapsuleCollider>();
        if (searchColliderGO != null) searchColliderGO.SetActive(false);

        for (int i = 0; i < meleeDamageColliders.Length; i++) {
            meleeDamageColliders[i].SetActive(false); // turn off melee colliders
        }

        currentState = Const.aiState.Idle;
		currentWaypoint = 0;
		enemy = null;
		firstSighting = true;
		inSight = false;
		hasSFX = false;
		goIntoPain = false;
		dyingSetup = false;
		ai_dead = false;
		ai_dying = false;
		attacker = null;
        shotFired = false;
		idleTime = Time.time + Random.Range(idleSFXTimeMin,idleSFXTimeMax);
		attack1SoundTime = Time.time;
		attack2SoundTime = Time.time;
		attack3SoundTime = Time.time;
		timeBetweenMeleeFinished = Time.time;
		timeTillEnemyChangeFinished = Time.time;
        huntFinished = Time.time;
		attackFinished = Time.time;
		attack2Finished = Time.time;
        attack3Finished = Time.time;
        timeTillPainFinished = Time.time;
		timeTillDeadFinished = Time.time;
        meleeDamageFinished = Time.time;
        gracePeriodFinished = Time.time;
        randomWaitForNextAttack2Finished = Time.time;
        breadFinished = Time.time;
        enemyBreadcrumbs = new List<Vector3>();
        damageData = new DamageData();
        tempHit = new RaycastHit();
        tempVec = new Vector3(0f, 0f, 0f);
        SFX = GetComponent<AudioSource>();
		if (SFX == null)
			Debug.Log("WARNING: No audio source for npc at: " + transform.position.x.ToString() + ", " + transform.position.y.ToString() + ", " + transform.position.z + ".");
		else
			hasSFX = true;

		if (walkWaypoints.Length > 0 && walkWaypoints[currentWaypoint] != null && walkPathOnStart) {
            nav.SetDestination(walkWaypoints[currentWaypoint].transform.position);
            currentState = Const.aiState.Walk; // If waypoints are set, start walking them from the get go
		} else {
            currentState = Const.aiState.Idle; // No waypoints, stay put
        }
			
		//RuntimeAnimatorController ac = anim.runtimeAnimatorController;
		//for (int i=0;i<ac.animationClips.Length;i++) {
		//	if (ac.animationClips[i].name == "Death") {
		//		timeTillDead = ac.animationClips[i].length;
		//		break;
		//	}
		//}
		tick = 0.05f;
		tickFinished = Time.time + tick;

		//QUAKE based AI
		attackFinished = Time.time + 1f;
		idealTransformForward = transform.forward;
	}

	void FixedUpdate () {
		if (PauseScript.a != null && PauseScript.a.paused) {
			//anim.speed = 0f; // don't animate, we're paused
			nav.isStopped = true;  // don't move, we're paused
			return; // don't do any checks or anything else...we're paused!
		} else {
			//anim.speed = 1f;
			//nav.isStopped = false;
		}

        if(moveType == Const.aiMoveType.None) nav.isStopped = true;

        // Only think every tick seconds to save on CPU and prevent race conditions
        if (tickFinished < Time.time) {
			Think();
			tickFinished = Time.time + tick;
		}

        // Rotation and Special movement that must be done every FixedUpdate
        if (currentState != Const.aiState.Dead) {
            if (currentState != Const.aiState.Idle) {
                idealTransformForward = nav.destination - transform.position;
                idealTransformForward.y = 0;
                Quaternion rot = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(idealTransformForward), yawspeed * Time.deltaTime);
                transform.rotation = rot;
            }

            if (moveType == Const.aiMoveType.Fly) {
                float distUp = 0;
                float distDn = 0;
                tempVec = transform.position;
                tempVec.y += verticalViewOffset;
                Vector3 floorPoint = new Vector3();
                floorPoint = Vector3.zero;
                int layMask = 1 << 9;
                //layMask = -layMask;

                if (Physics.Raycast(tempVec, transform.up * -1, out tempHit, sightRange)) {
                    //drawMyLine(tempVec, tempHit.point, Color.green, 2f);
                    distDn = Vector3.Distance(tempVec, tempHit.point);
                    floorPoint = tempHit.point;
                }

                if (Physics.Raycast(tempVec, transform.up, out tempHit, sightRange, layMask)) {
                    //drawMyLine(tempVec, tempHit.point, Color.green, 2f);
                    distUp = Vector3.Distance(tempVec, tempHit.point);
                }

                float distT = (distUp + distDn) * 0.75f;
                //Debug.Log("(" + distUp.ToString() + " + " + distDn.ToString() + ") * 0.25f = " + distT.ToString());
                idealPos = floorPoint + new Vector3(0,distT, 0);

                visibleMeshEntity.transform.position = Vector3.MoveTowards(visibleMeshEntity.transform.position, idealPos, runSpeed * Time.deltaTime);
            }
        }
	}

	void Think () {
		if (healthManager.health <= 0) {
			// If we haven't gone into dying and we aren't dead, going into dying
			if (!ai_dying && !ai_dead) {
				ai_dying = true; //no going back
				currentState = Const.aiState.Dying; //start to collapse in a heap, melt, explode, etc.
			}
		}

		switch (currentState) {
			case Const.aiState.Idle: 			Idle(); 		break;
			case Const.aiState.Walk:	 		Walk(); 		break;
			case Const.aiState.Run: 			Run(); 			break;
			case Const.aiState.Attack1: 		Attack1(); 		break;
			case Const.aiState.Attack2: 		Attack2(); 		break;
			case Const.aiState.Attack3: 		Attack3(); 		break;
			case Const.aiState.Pain: 			Pain();			break;
			case Const.aiState.Dying: 		    Dying(); 		break;
			case Const.aiState.Dead: 			Dead(); 		break;
			case Const.aiState.Inspect: 		Inspect(); 		break;
			case Const.aiState.Interacting: 	Interacting();	break;
			default: 					Idle(); 		break;
		}

		if (currentState == Const.aiState.Dead || currentState == Const.aiState.Dying) return; // Don't do any checks, we're dead

		inSight = CheckIfPlayerInSight();
        //if (inSight) backTurned = CheckIfBackIsTurned();
        if (enemy != null) {
            infront = enemyInFront(enemy);
            inProjFOV = enemyInProjFOV(enemy);
            rangeToEnemy = Vector3.Distance(enemy.transform.position, transform.position);
        } else {
            infront = false;
            rangeToEnemy = sightRange;
        }
	}

	bool CheckPain() {
		if (goIntoPain) {
			currentState = Const.aiState.Pain;
			if (attacker != null) {
				if (timeTillEnemyChangeFinished < Time.time) {
					timeTillEnemyChangeFinished = Time.time + changeEnemyTime;
					enemy = attacker; // Switch to whoever just attacked us
				}
			}
			goIntoPain = false;
			timeTillPainFinished = Time.time + timeToPain;
			return true;
		}
		return false;
	}

	void Idle() {
		if (enemy != null) {
			currentState = Const.aiState.Run;
			return;
		}
		nav.isStopped = true;
        nav.speed = 0;
		if (idleTime < Time.time && SFXIdle) {
			SFX.PlayOneShot(SFXIdle);
			idleTime = Time.time + Random.Range(idleSFXTimeMin, idleSFXTimeMax);
		}
			
		if (CheckPain()) return; // Go into pain if we just got hurt, data is sent by the HealthManager
		CheckIfPlayerInSight();
	}

	void Walk() {
        if (CheckPain()) return; // Go into pain if we just got hurt, data is sent by the HealthManager
        if (inSight || enemy != null) {
            currentState = Const.aiState.Run;
            return;
        }

        if (moveType == Const.aiMoveType.None) return;
        nav.speed = walkSpeed;
        if (WithinAngleToTarget()) {
            nav.isStopped = false;
        } else {
            nav.isStopped = true;
        }

        if (Vector3.Distance(transform.position, walkWaypoints[currentWaypoint].position) < nav.stoppingDistance) {
            if (visitWaypointsRandomly) {
                currentWaypoint = Random.Range(0, walkWaypoints.Length);
            } else {
                currentWaypoint++;
                if ((currentWaypoint >= walkWaypoints.Length) || (walkWaypoints[currentWaypoint] == null)) {
                    if (dontLoopWaypoints) {
                        currentState = Const.aiState.Idle; // Reached end of waypoints, just stop
                        return;
                    } else {
                        currentWaypoint = 0; // Wrap around
                        if (walkWaypoints[currentWaypoint] == null) {
                            currentState = Const.aiState.Idle;
                            return;
                        }
                    }
                }
            }
            nav.isStopped = true;
            nav.SetDestination(walkWaypoints[currentWaypoint].transform.position);
        }
	}

	void Run() {
		if (CheckPain()) return; // Go into pain if we just got hurt, data is sent by the HealthManager
        if (inSight) {
            huntFinished = Time.time + huntTime;
            if (rangeToEnemy < meleeRange) {
                if (hasMelee && infront) {
                    nav.speed = meleeSpeed;
                    timeBetweenMeleeFinished = Time.time + timeBetweenMelee;
                    currentState = Const.aiState.Attack1;
                    return;
                }
            } else {
                if (rangeToEnemy < proj1Range) {
                    if (hasProj1 && infront && inProjFOV && (randomWaitForNextAttack2Finished < Time.time)) {
                        nav.speed = proj1Speed;
                        shotFired = false;
                        attackFinished = Time.time + timeBetweenProj1 + timeTillActualAttack2;
                        gracePeriodFinished = Time.time + timeTillActualAttack2;
                        targettingPosition = enemy.transform.position;
                        currentState = Const.aiState.Attack2;
                        return;
                    }
                } else {
                    if (rangeToEnemy < proj2Range) {
                        if (hasProj2 && infront) {
                            nav.speed = proj2Speed;
                            currentState = Const.aiState.Attack3;
                            return;
                        }
                    }
                }
            }
            if (WithinAngleToTarget()) {
                nav.isStopped = false;
            } else {
                nav.isStopped = true;
            }

            nav.speed = runSpeed;
            nav.SetDestination(enemy.transform.position);
            if (moveType == Const.aiMoveType.None) nav.isStopped = true;
            lastKnownEnemyPos = enemy.transform.position;
            randSpin = false;
            if (breadFinished < Time.time) {
                breadFinished = Time.time + breadCrumbTick;
                enemyBreadcrumbs.Add(enemy.transform.position);
            }
        } else {
            if (huntFinished > Time.time) {
                Hunt();
            } else {
                enemy = null;
                currentState = Const.aiState.Idle;
                return;
            }
		}
	}

    void Hunt() {
        if (WithinAngleToTarget()) {
            nav.isStopped = false;
        } else {
            nav.isStopped = true;
        }
        nav.speed = runSpeed;

        if (!randSpin && Vector3.Distance(transform.position, lastKnownEnemyPos) < nav.stoppingDistance) {
            randSpin = true; // only set destination point once so we aren't chasing our tail spinning in circles
            nav.SetDestination(rrCheckPoint.transform.position);
        } else {
            nav.SetDestination(lastKnownEnemyPos);
        }
    }

	void Attack1() {
		// Used for melee
		if (attack1SoundTime < Time.time && SFXAttack1) {
			SFX.PlayOneShot(SFXAttack1);
			attack1SoundTime = Time.time + timeBetweenMelee;
            for (int i = 0; i < meleeDamageColliders.Length; i++) {
                meleeDamageColliders[i].SetActive(true);
                meleeDamageColliders[i].GetComponent<AIMeleeDamageCollider>().MeleeColliderSetup(index, meleeDamageColliders.Length, impactMelee, gameObject);
            }
        }

        if (WithinAngleToTarget()) {
            nav.isStopped = false;
        } else {
            nav.isStopped = true;
        }
        nav.speed = meleeSpeed;
        nav.SetDestination(enemy.transform.position);

        if (timeBetweenMeleeFinished < Time.time) {
            for (int i = 0; i < meleeDamageColliders.Length; i++) {
                meleeDamageColliders[i].SetActive(false); // turn off melee colliders
            }
            goIntoPain = false; //prevent going into pain after attack
			currentState = Const.aiState.Run;
			return; // Done with attack
		}
	}

    bool WithinAngleToTarget () {
        if (Quaternion.Angle(transform.rotation, Quaternion.LookRotation(idealTransformForward)) < fieldOfViewStartMovement) {
            return true;
        }
        return false;
    }

    bool DidRayHit(Vector3 targPos, float dist) {
        tempVec = targPos;
        tempVec.y += verticalViewOffset;
        tempVec = tempVec - gunPoint.transform.position;
        int layMask = 10;
        layMask = -layMask;

        if (Physics.Raycast(gunPoint.transform.position, tempVec.normalized, out tempHit, sightRange, layMask)) {
            drawMyLine(gunPoint.transform.position,tempHit.point, Color.green, 2f);
            tempHM = tempHit.transform.gameObject.GetComponent<HealthManager>();
            if (tempHit.transform.gameObject.GetComponent<HealthManager>() != null) {
                useBlood = true;
            }
            return true;
        }
        return false;
    }

    GameObject GetImpactType(HealthManager hm) {
        if (hm == null) return Const.a.GetObjectFromPool(Const.PoolType.SparksSmall);
        switch (hm.bloodType) {
            case HealthManager.BloodType.None: return Const.a.GetObjectFromPool(Const.PoolType.SparksSmall);
            case HealthManager.BloodType.Red: return Const.a.GetObjectFromPool(Const.PoolType.BloodSpurtSmall);
            case HealthManager.BloodType.Yellow: return Const.a.GetObjectFromPool(Const.PoolType.BloodSpurtSmallYellow);
            case HealthManager.BloodType.Green: return Const.a.GetObjectFromPool(Const.PoolType.BloodSpurtSmallGreen);
            case HealthManager.BloodType.Robot: return Const.a.GetObjectFromPool(Const.PoolType.SparksSmallBlue);
        }

        return Const.a.GetObjectFromPool(Const.PoolType.SparksSmall);
    }

    void CreateStandardImpactEffects(bool onlyBloodIfHitHasHM) {
        // Determine blood type of hit target and spawn corresponding blood particle effect from the Const.Pool
        if (useBlood) {
            GameObject impact = GetImpactType(tempHM);
            if (impact != null) {
                tempVec = tempHit.normal;
                impact.transform.position = tempHit.point + tempVec;
                impact.transform.rotation = Quaternion.FromToRotation(Vector3.up, tempHit.normal);
                impact.SetActive(true);
            }
        } else {
            // Allow for skipping adding sparks after special override impact effects per attack functions below
            if (!onlyBloodIfHitHasHM) {
                GameObject impact = Const.a.GetObjectFromPool(Const.PoolType.SparksSmall); //Didn't hit an object with a HealthManager script, use sparks
                if (impact != null) {
                    tempVec = tempHit.normal;
                    impact.transform.position = tempHit.point + tempVec;
                    impact.transform.rotation = Quaternion.FromToRotation(Vector3.up, tempHit.normal);
                    impact.SetActive(true);
                }
            }
        }
    }

    void Attack2() {
        if (gracePeriodFinished < Time.time) {
            if (!shotFired) {
                shotFired = true;
                // Typically used for normal projectile attack
                if (attack2SoundTime < Time.time && SFXAttack2) {
                    SFX.PlayOneShot(SFXAttack2);
                    attack2SoundTime = Time.time + timeBetweenProj1;
                }

                if (attack2Type == Const.AttackType.Projectile) {
                    muzzleBurst.SetActive(true);
                    if (DidRayHit(targettingPosition, proj1Range)) {
                        CreateStandardImpactEffects(false);
                        damageData.other = tempHit.transform.gameObject;
                        if (tempHit.transform.gameObject.tag == "NPC") {
                            damageData.isOtherNPC = true;
                        } else {
                            damageData.isOtherNPC = false;
                        }
                        damageData.hit = tempHit;
                        tempVec = transform.position;
                        tempVec.y += verticalViewOffset;
                        tempVec = (enemy.transform.position - tempVec);
                        damageData.attacknormal = tempVec;
                        damageData.damage = 15f;
                        damageData.damage = Const.a.GetDamageTakeAmount(damageData);
                        damageData.owner = gameObject;
                        damageData.attackType = Const.AttackType.Projectile;
                        HealthManager hm = tempHit.transform.gameObject.GetComponent<HealthManager>();
                        if (hm == null) return;
                        hm.TakeDamage(damageData);
                    }
                }
            }
        }

        if (attackFinished < Time.time) {
            if (Random.Range(0f,1f) < attack2RandomWaitChance) {
                randomWaitForNextAttack2Finished = Time.time + Random.Range(attack2MinRandomWait, attack2MaxRandomWait);
            } else {
                randomWaitForNextAttack2Finished = Time.time;
            }
            muzzleBurst.SetActive(false);
            goIntoPain = false; //prevent going into pain after attack
            currentState = Const.aiState.Run;
            return;
        }
	}

	void Attack3() {
		// Typically used for secondary projectile or grenade attack
		if (attack3SoundTime < Time.time && SFXAttack3) {
			SFX.PlayOneShot(SFXAttack3);
			attack3SoundTime = Time.time + timeBetweenProj2;
		}

		if (explodeOnAttack3) {
			ExplosionForce ef = GetComponent<ExplosionForce>();
			DamageData ddNPC = Const.SetNPCDamageData(index, Const.aiState.Attack3,gameObject);
			float take = Const.a.GetDamageTakeAmount(ddNPC);
			ddNPC.other = gameObject;
			ddNPC.damage = take;
			//enemy.GetComponent<HealthManager>().TakeDamage(ddNPC); Handled by ExplodeInner
			if (ef != null) ef.ExplodeInner(transform.position+explosionOffset, attack3Force, attack3Radius, ddNPC);
			healthManager.ObjectDeath(SFXDeathClip);
			return;
		}
	}

	void Pain() {
		if (timeTillPainFinished < Time.time) {
			currentState = Const.aiState.Run; // go into run after we get hurt
			goIntoPain = false;
			timeTillPainFinished = Time.time + timeBetweenPain;
		}
	}

	void Dying() {
		if (!dyingSetup) {
			dyingSetup = true;
			SFX.PlayOneShot(SFXDeathClip);

            // Turn off normal NPC collider and enable corpse collider for searching
            if (boxCollider != null) boxCollider.enabled = false;
            if (sphereCollider != null) sphereCollider.enabled = false;
            if (meshCollider != null) meshCollider.enabled = false;
            if (capsuleCollider != null) capsuleCollider.enabled = false;
            if (searchColliderGO != null) searchColliderGO.SetActive(true);
            gameObject.tag = "Searchable"; // Enable searching

			nav.speed = nav.speed * 0.5f; // half the speed while collapsing or whatever
			timeTillDeadFinished = Time.time + timeTillDead; // wait for death animation to finish before going into Dead()
		}
			
		if (timeTillDeadFinished < Time.time) {
			ai_dead = true;
			ai_dying = false;
			currentState = Const.aiState.Dead;
		}
	}

	void Dead() {
		nav.isStopped = true; // Stop moving
		//anim.speed = 0f; // Stop animation
		ai_dead = true;
		ai_dying = false;
		rbody.isKinematic = true;
		currentState = Const.aiState.Dead;
		firstSighting = false;
		if (healthManager.gibOnDeath) {
			ExplosionForce ef = GetComponent<ExplosionForce>();
			DamageData ddNPC = Const.SetNPCDamageData(index, Const.aiState.Attack3,gameObject);
			float take = Const.a.GetDamageTakeAmount(ddNPC);
			ddNPC.other = gameObject;
			ddNPC.damage = take;
			if (ef != null) ef.ExplodeInner(transform.position+explosionOffset, attack3Force, attack3Radius, ddNPC);
			healthManager.ObjectDeath(SFXDeathClip);
		}
	}

	void Inspect() {
		if (CheckPain()) return; // Go into pain if we just got hurt, data is sent by the HealthManager
	}
		
	void Interacting() {
		if (CheckPain()) return; // Go into pain if we just got hurt, data is sent by the HealthManager
	}

	bool CheckIfEnemyInSight() {
		Vector3 checkline = enemy.transform.position - transform.position; // Get vector line made from enemy to found player
        int layMask = 10;
        layMask = -layMask;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + transform.up, checkline.normalized, out hit, sightRange, layMask)) {
            LOSpossible = true;
            if (hit.collider.gameObject == enemy)
                return true;
        }
        LOSpossible = false;
        return false;
	}

	bool CheckIfPlayerInSight () {
        if (ignoreEnemy) return false;
		if (enemy != null) return CheckIfEnemyInSight();

		GameObject playr1 = Const.a.player1;
		GameObject playr2 = Const.a.player2;
		GameObject playr3 = Const.a.player3;
		GameObject playr4 = Const.a.player4;

		if (playr1 == null) { Debug.Log("WARNING: NPC sight check - no host player 1."); return false; }  // No host player
		if (playr1 != null) {playr1 = playr1.GetComponent<PlayerReferenceManager>().playerCapsule;}
		if (playr2 != null) {playr2 = playr2.GetComponent<PlayerReferenceManager>().playerCapsule;}
		if (playr3 != null) {playr3 = playr3.GetComponent<PlayerReferenceManager>().playerCapsule;}
		if (playr4 != null) {playr4 = playr4.GetComponent<PlayerReferenceManager>().playerCapsule;}

		GameObject tempent = null;
		LOSpossible = false;

		for (int i=0;i<4;i++) {
			tempent = null;
			// Cycle through all the players to see if we can see anybody.  Defaults to earlier joined players. TODO: Add randomization if multiple players are visible.
			if (playr1 != null && i == 0) tempent = playr1;
			if (playr2 != null && i == 1) tempent = playr2;
			if (playr3 != null && i == 2) tempent = playr3;
			if (playr4 != null && i == 4) tempent = playr4;
			if (tempent == null) continue;

            tempVec = tempent.transform.position;
            tempVec.y += verticalViewOffset;
			Vector3 checkline = tempVec - transform.position; // Get vector line made from enemy to found player
            int layMask = 10;
            layMask = -layMask;

			// Check for line of sight
			RaycastHit hit;
            if (Physics.Raycast(transform.position, checkline.normalized, out hit, sightRange,layMask)) {
                //drawMyLine(transform.position,hit.point, Color.green, 2f);
                if (hit.collider.gameObject == tempent)
					LOSpossible = true;  // Clear path from enemy to found player
			}

			float dist = Vector3.Distance(tempent.transform.position,transform.position);  // Get distance between enemy and found player
			float angle = Vector3.Angle(checkline,transform.forward);

			// If clear path to found player, and either within view angle or right behind the enemy
			if (LOSpossible) {
				if (angle < (fieldOfViewAngle * 0.5f)) {
					enemy = tempent;
					if (firstSighting) {
						firstSighting = false;
						if (hasSFX) SFX.PlayOneShot(SFXSightSound);
					}
					return true;
				} else {
					if (dist < distToSeeWhenBehind) {
						enemy = tempent;
						if (firstSighting) {
							firstSighting = false;
							if (hasSFX) SFX.PlayOneShot(SFXSightSound);
						}
						return true;
					}
				}
			}
		}
		return false;
	}
	
    bool enemyInFront (GameObject target) {
        Vector3 vec = Vector3.Normalize(target.transform.position - transform.position);
        float dot = Vector3.Dot(vec,transform.forward);
        if (dot > 0.300) return true; // enemy is within 27 degrees of forward facing vector
        return false;
    }

    bool enemyInProjFOV(GameObject target) {
        Vector3 vec = Vector3.Normalize(target.transform.position - transform.position);
        float dot = Vector3.Dot(vec, transform.forward);
        if (dot > 0.800) return true; // enemy is within 27 degrees of forward facing vector
        return false;
    }

    void drawMyLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        StartCoroutine(drawLine(start, end, color, duration));
    }

    IEnumerator drawLine(Vector3 start, Vector3 end, Color color, float duration = 0.2f)
    {
        GameObject myLine = new GameObject();
        myLine.transform.position = start;
        myLine.AddComponent<LineRenderer>();
        LineRenderer lr = myLine.GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Particles/Additive"));
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        yield return new WaitForSeconds(duration);
        GameObject.Destroy(myLine);
    }
}
