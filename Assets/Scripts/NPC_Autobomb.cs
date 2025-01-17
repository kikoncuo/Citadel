using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_Autobomb : MonoBehaviour {
	public bool explodeOnDeath = false; // do we explode when we die?
	public bool explodeOnAttack = false; // do we self-destruct as our attack?
	public bool dead = false; // are we dead?
	public bool dying = false; // are we dying?
	public bool inPain = false; // are we doing pain action?
	public bool painStagger = false; // do we move when doing pain action?
	public bool grounded = false; // are we touching the ground
	public bool inWater = false; // are we under water?  TODO: set this with a trigger volume for all liquids
	public bool canWalk = true; // default to a walking enemy on the ground
	public bool canFly = false; // are we a flying enemy
	public bool canSwim = false; // are we a swimming enemy
	public bool attacking = false; // are we currently attacking
	public bool canMoveWhileAttacking = false; // you get what this bit does right?
	public bool interacting = false; // are we currently interacting
	public bool canMoveWhileInteracting = false; //hopefully this doesn't need a comment
	public bool firstSighting; // only make sight sound when we first see an enemy or first get attacked
	public bool visitWaypointsRandomly = false; // do we wander from random waypoint to random waypoint
	public bool moveFrameLimited = false; // are we limited to only give movement impluse force on a certain animation frame?
	public bool hasMelee = false;
	public bool hasProjectile1 = false;
	public bool hasProjectile2 = false;
	public bool turning = false;
	public bool moveWhileTurning = true;
	public bool moving = false;
	public int index = 0; // NPC reference index for looking up constants in tables in Const.cs
	public int currentWaypoint = 0;
	private int tempInt = 0;
	public Const.aiState currentState;
	public float gridCheckingDistance = 2.56f;
	public float currentSpeed = 0f;
	public float walkSpeed = 0.8f;
	public float runSpeed = 0.8f;
	public float meleeRange = 2.56f;
	public float proj1Range = 10.24f;
	public float proj2Range= 51.2f - 0.16f;
	public float waypointChangeRange = 0.64f;
	public float rangeToEnemy = 0f;
	public float tickTime = 0.05f;
	public float yawSpeed = 50f;
	public float attack3Force = 8f;
	public float attack3Radius = 6f;
	public float verticalViewOffset = 0.42f;
	public float idleTime;
	public float tickFinished;
	public float attackFinished;
	public float lastHealth;
	public float acceleration = 20f;
	public float tempFloat;
	public float moveFrameMin = 0.1f; // if moveframe limited, only give move impulse after  this amount of time has passed
	public float moveFrameMax = 0.2f; // if moveframe limited, only give move impulse before this amount of time has passed
	// More or less constant floats
	public float maxSlope = 60f;
	public float fieldOfViewAngle = 160f;
	public float fieldOfViewAttack = 80f;
	public float fieldOfViewMove = 40f;
	public float distToSeeWhenBehind = 2.5f;
	public float sightRange = 51.2f;
	public float changeEnemyTime = 3f; // grace period time before enemy will even acknowledge attacks to switch to different attacker
	public string animNameIdle = "Idle";
	public string animNameWalk = "Walk";
	public string animNameRun = "Run";
	public string animNamePain = "Pain";
	public string animNameInteract = "Interact";
	public string animNameDying = "Die";
	public string animNameDead = "Dead";
	public Vector3 viewVerticalOffset;
	public Vector3 explosionOffset;
	public Vector3 navigationTargetPosition;
	public Vector3 idealTransformForward;
	public Vector3 goalPoint;
	private Vector3 pCloudForward;
	private Vector3 pCloudBack;
	private Vector3 pCloudLeft;
	private Vector3 pCloudRight;
	private Vector3 pCloudUp;
	private Vector3 pCloudDown;
	private Vector3 pCloudForwardUp;
	private Vector3 pCloudForwardDown;
	private Vector3 pCloudRightUp;
	private Vector3 pCloudRightDown;
	private Vector3 pCloudLeftUp;
	private Vector3 pCloudLeftDown;
	private Vector3 pCloudBackUp;
	private Vector3 pCloudBackDown;
	private Vector3 pCloudDiagForwardRight;
	private Vector3 pCloudDiagForwardLeft;
	private Vector3 pCloudDiagBackLeft;
	private Vector3 pCloudDiagBackRight;
	private Vector3 pCloudDiagForwardRightUp;
	private Vector3 pCloudDiagForwardRightDown;
	private Vector3 pCloudDiagForwardLeftUp;
	private Vector3 pCloudDiagForwardLeftDown;
	private Vector3 pCloudDiagBackLeftUp;
	private Vector3 pCloudDiagBackLeftDown;
	private Vector3 pCloudDiagBackRightUp;
	private Vector3 pCloudDiagBackRightDown;
	[HideInInspector]
	public GameObject attacker;
	public GameObject enemy;
	public AudioClip SFXFootstep;
	public AudioClip SFXSightSound;
	public AudioClip SFXAttack1;
	public AudioClip SFXInspect;
	public AudioClip SFXInteracting;
	public AudioSource SFX;
	public Animation anim;
	public CapsuleCollider capsuleCollider;
	public BoxCollider boxCollider;
	public SphereCollider sphereCollider;
	public MeshCollider meshConvexCollider;
	private Rigidbody rbody;
	public Transform[] walkWaypoints; // point(s) for NPC to walk to when roaming or patrolling
	public HealthManager healthManager;
	public GoalType[] selfGoals;
	public GameObject searchColliderGO;


	void Start () {
		// Initialize everything to default states so everything is happy
		dead = false;
		firstSighting = true;
		currentState = Const.aiState.Idle;
		tickFinished = Time.time + tickTime;
		attackFinished = Time.time;

		// Get some components that we should have
		healthManager = GetComponent<HealthManager>();
		rbody = GetComponent<Rigidbody>();
		SFX = GetComponent<AudioSource>();
		anim = GetComponent<Animation>();
		lastHealth = healthManager.health; // Initialize last health so we can check if we got hurt and go into pain state
		idealTransformForward = transform.forward; // prefer to go forward initially, then rotate from there later towards attacker or waypoint, or node, etc.
		enemy = null; // start out with no enemy
		attacker = null; // start out with no attacker
		goalPoint = Vector3.zero;

		// If we accidentally removed an AudioSource component from the GameObject or if we never added one, let us know where this is so we can select it
		if (SFX == null) Debug.Log ("WARNING: No audio source for npc at: " + transform.position.x.ToString () + ", " + transform.position.y.ToString () + ", " + transform.position.z + ".");

		if (healthManager.health > 0) {
			if (walkWaypoints.Length > 0 && walkWaypoints[currentWaypoint] != null) {
				currentState = Const.aiState.Walk; // If waypoints are set, start walking to them
			} else {
				currentState = Const.aiState.Idle; // Default to idle
			}
		} else {
			dead = true;
			currentState = Const.aiState.Dead;
		}
	}

	// Using fixed update because this script affects the movement of this GameObject using physics calculations applied to a Rigidbody
	void FixedUpdate () {
		if (PauseScript.a != null && PauseScript.a.paused) return; // don't do any checks or anything else...we're paused!

		// Only think every tick seconds to save on CPU and prevent race conditions
		if (tickFinished < Time.time) {
			Think(); // hmm think think think
			tickFinished = Time.time + tickTime; // Reset timer to delay thinking by tick amount of seconds
		}
	}

	void Think () {
		//CheckAndUpdateState ();

		//switch (currentState) {
		//case Const.aiState.Idle: 			Idle(); 		break;
		//case Const.aiState.Walk:	 		Walk(); 		break;
		//case Const.aiState.Run: 			Run(); 			break;
		//case Const.aiState.Attack1: 		Attack(); 		break;
		//default: 							break;
		//}



		// AI_GENERIC think function.  Go through the list, and do anything we are able to do.
		// Multiple things can happen each frame, such as moving while attacking or moving while in pain.
		// This should be pretty self explanatory...
		if (dead) { Dead(); return;}
		if (dying) { Dying(); return;}
		if (CanMove()) { if (NeedMove()) { Move(); moving = true; } else { moving = false; } }
		if (CanAttack()) if (NeedAttack()) Attack();
		if (CanPain()) if (NeedPain()) Pain();
		if (CanInteract()) if (NeedInteract()) Interact();
		if (CanIdle()) if (NeedIdle()) Idle();
	}

	void Dead() {} // dead, do nothing

	void Dying() {
		if (!dead) {
			dead = true;

			if (explodeOnDeath) {
				ExplodeOnDeath();
			} else {
				if (boxCollider != null) boxCollider.enabled = false;
				if (sphereCollider != null) sphereCollider.enabled = false;
				if (capsuleCollider != null) capsuleCollider.enabled = false;
				if (meshConvexCollider != null) meshConvexCollider.enabled = false;
				if (searchColliderGO !=null) searchColliderGO.SetActive(true);
			}
			currentState = Const.aiState.Dead;
		}
	}

	void ExplodeOnDeath() {
		ExplosionForce ef = GetComponent<ExplosionForce> ();
		DamageData ddNPC = Const.SetNPCDamageData (index, Const.aiState.Attack3, gameObject);
		float take = Const.a.GetDamageTakeAmount (ddNPC);
		ddNPC.other = gameObject;
		ddNPC.damage = take;
		//enemy.GetComponent<HealthManager>().TakeDamage(ddNPC); Handled by ExplodeInner
		if (ef != null)
			ef.ExplodeInner ((transform.position+viewVerticalOffset) + explosionOffset, attack3Force, attack3Radius, ddNPC);
		healthManager.ObjectDeath (SFXAttack1);
		GetComponent<MeshRenderer> ().enabled = false;
	}

	/*
	// All state changes go in here.  Check for stuff.  See what we need to be doing.  Need to change?
	void CheckAndUpdateState() {
		if (currentState == Const.aiState.Dead)	return;

		// Check to see if we got hurt
		if (healthManager.health < lastHealth) {
			if (healthManager.health <= 0) {
				currentState = Const.aiState.Dying;
				return;
			}

			lastHealth = healthManager.health;
			// Make initial sight sound if we aren't running, attacking, or already in pain
			if (currentState != Const.aiState.Run || currentState != Const.aiState.Attack1 || currentState != Const.aiState.Attack2 || currentState != Const.aiState.Attack3 || currentState != Const.aiState.Pain)
				SightSound ();

			enemy = healthManager.attacker;
			currentState = Const.aiState.Run;
			return;
		} else {
			lastHealth = healthManager.health;
		}

		// Take a look around for enemies
		if (enemy == null) {
			// we don't have an enemy yet so let's look to see if we can see one
			if (CheckIfPlayerInSight ()) {
				currentState = Const.aiState.Run; // quit standing around and start fighting
				return;
			}
		} else {
			// We have an enemy now what...
			if (CheckIfEnemyInSight ()) {
				float dist = Vector3.Distance (transform.position, enemy.transform.position);
				// See if we are close enough to attack
				if (dist < meleeRange) {
					if (CheckIfEnemyInFront (enemy)) {
						currentState = Const.aiState.Attack1;
						return;
					}
				}
			}
		}
	}*/

	bool CanIdle() {
		if (moving) return false;
		return true;
	}

	bool NeedIdle() {
		return true;
	}

	void Idle() {
		// Set animation state
		anim[animNameIdle].wrapMode = WrapMode.Loop;
		anim.Play(animNameIdle);
	}

	void SightSound() {
		if (firstSighting) {
			firstSighting = false;
			if (SFX != null) SFX.PlayOneShot(SFXSightSound);
		}
	}

	// Sets grounded based on normal angle of the impact point (NOTE: This is not the surface normal!)
	void OnCollisionStay (Collision collision  ){
		if (!PauseScript.a.paused) {
			foreach(ContactPoint contact in collision.contacts) {
				if (Vector3.Angle(contact.normal,Vector3.up) < maxSlope) {
					grounded = true;
				}
			}
		}
	}

	bool CanMove() {
		if ((grounded && canWalk) || (canFly) || (canSwim && inWater)) {
			if (!attacking || canMoveWhileAttacking) {
				if (!inPain || painStagger) {
					if (!interacting || canMoveWhileInteracting) {
						if (!turning || moveWhileTurning) {
							if (moveFrameLimited) {
								string animToCheck = animNameRun;
								if (enemy != null) { animToCheck = animNameRun; } else { animToCheck = animNameWalk; }
								if (anim[animToCheck].time > moveFrameMin && anim[animToCheck].time < moveFrameMax) return true;
							} else {
								return true;
							}
						}
					}
				}
			}
		}
		return false;
	}

	// Pretty hairy, but basically generate a bunch of points around the enemy for checking where we can move
	void GeneratePointCloud() {
		pCloudForward = (transform.position+viewVerticalOffset) + (transform.forward * gridCheckingDistance);
		pCloudBack = (transform.position+viewVerticalOffset) + (transform.forward * gridCheckingDistance * -1f);
		pCloudRight = (transform.position+viewVerticalOffset) + (transform.right * gridCheckingDistance);
		pCloudLeft = (transform.position+viewVerticalOffset) + (transform.right * gridCheckingDistance * -1f);
		pCloudForwardDown = pCloudForward;
		pCloudForwardDown.y += (gridCheckingDistance * -1f);
		pCloudBackDown = pCloudBack;
		pCloudBackDown.y += (gridCheckingDistance * -1f);
		pCloudRightDown = pCloudRight;
		pCloudRightDown.y += (gridCheckingDistance * -1f);
		pCloudLeftDown = pCloudLeft;
		pCloudLeftDown.y += (gridCheckingDistance * -1f);

		pCloudDiagForwardLeft = (transform.position+viewVerticalOffset) + (transform.forward * gridCheckingDistance * Mathf.Sqrt(2f) * 0.5f) + (transform.right * gridCheckingDistance * -1f * Mathf.Sqrt(2f) * 0.5f);
		pCloudDiagForwardRight = (transform.position+viewVerticalOffset) + (transform.forward * gridCheckingDistance * Mathf.Sqrt(2f) * 0.5f) + (transform.right * gridCheckingDistance * Mathf.Sqrt(2f) * 0.5f);
	}

	void DrawDebugLinesToCloud() {
		// return;  //Uncomment to disable debug lines
		drawMyLine((transform.position+viewVerticalOffset),pCloudForward,Color.cyan,0.1f);
		drawMyLine((transform.position+viewVerticalOffset),pCloudBack,Color.cyan,0.1f);
		drawMyLine((transform.position+viewVerticalOffset),pCloudRight,Color.cyan,0.1f);
		drawMyLine((transform.position+viewVerticalOffset),pCloudLeft,Color.cyan,0.1f);
		drawMyLine((transform.position+viewVerticalOffset),pCloudForwardDown,Color.cyan,0.1f);
		drawMyLine((transform.position+viewVerticalOffset),pCloudBackDown,Color.cyan,0.1f);
		drawMyLine((transform.position+viewVerticalOffset),pCloudRightDown,Color.cyan,0.1f);
		drawMyLine((transform.position+viewVerticalOffset),pCloudLeftDown,Color.cyan,0.1f);

		drawMyLine((transform.position+viewVerticalOffset),pCloudDiagForwardLeft,Color.cyan,0.1f);
	}

	bool NeedMove() {
		if (enemy != null || NeedInteract() || (painStagger && inPain) || AIGoals.a.CheckIfInGoals(selfGoals,GoalType.Patrol) || AIGoals.a.CheckIfInGoals(selfGoals,GoalType.Wander)) {
			//Debug.Log("Need to move, check!");
			return true;
		}
		return false;
	}

	bool CheckIfClearToPoint(Vector3 p) {
		Vector3 checkline = p - (transform.position+viewVerticalOffset); // Get vector line made from enemy to found player
		RaycastHit hit;
		if(Physics.Raycast((transform.position+viewVerticalOffset), checkline.normalized, out hit, sightRange)) {
			if (hit.collider.gameObject == enemy) {
				return true;
			}
		}
		return false;
	}

	void GetMoveDirection() {
		GeneratePointCloud();
		DrawDebugLinesToCloud();
		idealTransformForward = transform.forward;
		goalPoint = pCloudForward;
		if (enemy != null) {
			if (CheckIfClearToPoint(enemy.transform.position)) {
				goalPoint = enemy.transform.position;
				idealTransformForward = (goalPoint - (transform.position+viewVerticalOffset));
			} else {
				Vector3[] points = new Vector3[]{pCloudForward,pCloudDiagForwardRight,pCloudDiagForwardLeft,pCloudRight,pCloudLeft};
				tempInt = 0;
				bool noClearPointFoundYet = true;
				while (noClearPointFoundYet) {
					if (tempInt > points.Length) {
						goalPoint = pCloudBack;
						break;
					}

					if (tempInt == 0) {
						if (CheckIfClearToPoint(points[tempInt])) {
							goalPoint = points[tempInt];
							break;
						} else {
							tempInt++;
						}
					} else {
						if (Random.Range(0f,100f) < 50f) {
							if (CheckIfClearToPoint(points[tempInt])) {
								goalPoint = points[tempInt];
								break;
							}
						} else {
							if (CheckIfClearToPoint(points[tempInt+1])) {
								goalPoint = points[tempInt];
								break;
							}
						}
						tempInt += 2;
					}
				}
			}
		}

		if (AIGoals.a.CheckIfInGoals(selfGoals,GoalType.Wander)) {
			//if (Random.Range(0.0f,100f) <= 100f) {
			//	idealTransformForward = pCloudRight-(transform.position + viewVerticalOffset);
			//	goalPoint = pCloudRight;
			//}
			Vector3[] points = new Vector3[]{pCloudForward,pCloudDiagForwardRight,pCloudDiagForwardLeft,pCloudRight,pCloudLeft};
			tempInt = 0;
			bool noClearPointFoundYet = true;
			while (noClearPointFoundYet) {
				if (tempInt > (points.Length-1)) {
					goalPoint = pCloudBack;
					break;
				}

				if (tempInt == 0) {
					if (CheckIfClearToPoint(points[tempInt])) {
						goalPoint = points[tempInt];
						break;
					} else {
						tempInt++;
					}
				} else {
					if (Random.Range(0f,100f) < 50f) {
						if (CheckIfClearToPoint(points[tempInt])) {
							goalPoint = points[tempInt];
							break;
						}
					} else {
						if (CheckIfClearToPoint(points[tempInt+1])) {
							goalPoint = points[tempInt];
							break;
						}
					}
					tempInt++;
					tempInt++;
				}
			}
			idealTransformForward = goalPoint - (transform.position+viewVerticalOffset);
		}
	}

	void Move() {
		if (enemy != null) {
			anim[animNameWalk].wrapMode = WrapMode.Loop;
			anim.Play (animNameWalk);
			tempFloat = runSpeed;
		} else {
			anim[animNameRun].wrapMode = WrapMode.Loop;
			anim.Play (animNameRun);
			tempFloat = walkSpeed;
		}

		GetMoveDirection();
		if (Vector3.Angle(transform.forward,idealTransformForward) > fieldOfViewMove) {
			turning = true;
			AI_Face(goalPoint);
		} else {
			turning = false;
		}
		
		if (!turning || moveWhileTurning) {
			if (rbody.velocity.magnitude < tempFloat) {
				rbody.AddForce (transform.forward * tempFloat * acceleration * tickTime, ForceMode.Impulse); // moving forward!
			}
		}
	}

	bool CanAttack() {
		if (attackFinished < Time.time) {
			if (enemy != null) {
				tempFloat = Vector3.Distance((transform.position+viewVerticalOffset),enemy.transform.position);
				if ((tempFloat < meleeRange && hasMelee) || (tempFloat < proj1Range && hasProjectile1) || (tempFloat < proj2Range && hasProjectile2)) return true;
			}
		}
		return false;
	}

	bool NeedAttack() {
		if (AIGoals.a.CheckIfInGoals(selfGoals,GoalType.AttackAnyPlayer) || AIGoals.a.CheckIfInGoals(selfGoals,GoalType.AttackPlayer1) || AIGoals.a.CheckIfInGoals(selfGoals,GoalType.AttackPlayer2) || AIGoals.a.CheckIfInGoals(selfGoals,GoalType.AttackPlayer3)  || AIGoals.a.CheckIfInGoals(selfGoals,GoalType.AttackPlayer4)) return true;
		return false;
	}

	bool CanPain() {
		return false;
	}

	bool NeedPain() {
		return false;
	}

	void Pain() {

	}

	bool CanInteract() {
		return false;
	}

	bool NeedInteract() {
		return false;
	}

	void Interact() {

	}

	void AI_Face(Vector3 goalLocation) {
		Vector3 dir = (goalLocation - (transform.position+viewVerticalOffset)).normalized;
		dir.y = 0f;
		Quaternion lookRot = Quaternion.LookRotation(dir,Vector3.up);
		transform.rotation = Quaternion.Slerp (transform.rotation, lookRot, tickTime * yawSpeed); // rotate as fast as we can towards goal position
	}

	void Attack() {
		Dying ();
	}

	// Sub functions
	bool CheckIfEnemyInFront (GameObject target) {
		Vector3 vec = Vector3.Normalize(target.transform.position - (transform.position+viewVerticalOffset));
		float dot = Vector3.Dot(vec,transform.forward);
		if (dot > 0.800) return true; // enemy is within 18 degrees of forward facing vector
		return false;
	}


	bool CheckIfEnemyInSight() {
		Vector3 checkline = enemy.transform.position - (transform.position+viewVerticalOffset); // Get vector line made from enemy to found player
		RaycastHit hit;
		if(Physics.Raycast((transform.position+viewVerticalOffset) + transform.up, checkline.normalized, out hit, sightRange)) {
			if (hit.collider.gameObject == enemy) {
				if (CheckIfEnemyInFront(enemy))
					return true;
			}
		}
		return false;
	}
		
	bool CheckIfPlayerInSight() {
		GameObject tempent = Const.a.player1.GetComponent<PlayerReferenceManager>().playerCapsule;
		Vector3 viewOffsetPoint = transform.position;
		viewOffsetPoint.y += verticalViewOffset;
		Vector3 checkline = Vector3.Normalize(tempent.transform.position - viewOffsetPoint); // Get vector line made from enemy to found player
		//drawMyLine(tempent.transform.position,viewOffsetPoint,Color.green,2f);

		RaycastHit hit;
		if(Physics.Raycast(viewOffsetPoint, checkline, out hit, sightRange)) {
			if (hit.collider.gameObject == tempent) {
				//drawMyLine (hit.point, viewOffsetPoint, Color.red, 3f);
				float dist = Vector3.Distance (tempent.transform.position, (transform.position+viewVerticalOffset));  // Get distance between enemy and found player
				float dot = Vector3.Dot (checkline, transform.forward.normalized);
				if (dot > 0.10f) {
					// enemy is within 81 degrees of forward facing vector
					if (firstSighting) {
						firstSighting = false;
						if (SFX != null) SFX.PlayOneShot (SFXSightSound);
					}
					enemy = tempent;
					return true; // time to fight!
				} else {
					if (dist < distToSeeWhenBehind) {
						SightSound ();
						enemy = tempent;
						return true; // time to turn around and face your executioner!
					}
				}
			} else {
				//drawMyLine(hit.point,viewOffsetPoint,Color.blue,1f);
			}
		}
		return false;
	}

	void drawMyLine(Vector3 start , Vector3 end, Color color,float duration = 0.2f){
		StartCoroutine( drawLine(start, end, color, duration));
	}

	IEnumerator drawLine(Vector3 start , Vector3 end, Color color,float duration = 0.2f){
		GameObject myLine = new GameObject ();
		myLine.transform.position = start;
		myLine.AddComponent<LineRenderer> ();
		LineRenderer lr = myLine.GetComponent<LineRenderer> ();
		lr.material = new Material (Shader.Find ("Particles/Additive"));
		lr.startColor = color;
		lr.endColor = color;
		lr.startWidth = 0.1f;
		lr.endWidth = 0.1f;
		lr.SetPosition (0, start);
		lr.SetPosition (1, end);
		yield return new WaitForSeconds(duration);
		GameObject.Destroy (myLine);
	}
}
