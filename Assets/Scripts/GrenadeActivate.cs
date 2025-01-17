using UnityEngine;
using System.Collections;

public class GrenadeActivate : MonoBehaviour {
	public float nearforce;
	public float nearradius;
	public bool active = false;
	public bool proxSensed = false;
	public float tickTime = 0.8f;
	public Texture2D mainGrenTexture;
	public Texture2D alternateTexture;
	public GameObject explosionEffect;
	private UseableObjectUse useRef;
	private float timeFinished;
	private float tickFinished;
	private bool explodeOnContact = false;
	private bool useTimer = false;
	private bool useProx = false;
	private bool textureFlip = false;
	private ExplosionForce explosion;
	private BoxCollider boxCol;
	private SphereCollider sphereCol;
	private MeshCollider meshCol;
	private CapsuleCollider capCol;
	public AudioClip deathSound;
	private GrenadeCurrent grenadeCurrent;

	void Awake () {
		meshCol = GetComponent<MeshCollider>();
		boxCol = GetComponent<BoxCollider>();
		sphereCol = GetComponent<SphereCollider>();
		capCol = GetComponent<CapsuleCollider>();
	}

	void Update () {
		if (active) {
			if (useTimer) {
				if (timeFinished < Time.time) {
					Explode();
				}
			}

			if (useProx) {
				if (proxSensed) {
					Explode();
				}
			}

			if (tickFinished < Time.time) {
				tickFinished = Time.time + tickTime;
				ToggleTexture(); // blink the lights on the grenade to show we are active!
			}
		}
	}

	void ToggleTexture() {
		if (alternateTexture == null || mainGrenTexture == null) return;

		if (textureFlip) {
			gameObject.GetComponent<MeshRenderer>().material.mainTexture = alternateTexture;
		} else {
			gameObject.GetComponent<MeshRenderer>().material.mainTexture = mainGrenTexture;
		}
		textureFlip = !textureFlip;
	}

	public void Activate (GrenadeCurrent gc) {
		grenadeCurrent = gc;
		useRef = GetComponent<UseableObjectUse>();
		switch(useRef.useableItemIndex) {
			case 7: explodeOnContact = true; break;
			case 8: explodeOnContact = true; break;
			case 9: explodeOnContact = true; break;
			case 10: timeFinished = Time.time + gc.earthShakerTimeSetting; useTimer = true; break;
			case 11: useProx = true; break;
			case 12: timeFinished = Time.time + gc.nitroTimeSetting; useTimer = true; break;
			case 13: explodeOnContact = true; break;
			default: break;
		}
		ToggleTexture();
		tickFinished = Time.time + tickTime;
		active = true;
	}

	void OnCollisionStay(Collision col) {
		if (active) {
			if (grenadeCurrent != null) {
				if (col.collider == grenadeCurrent.playerCapCollider) {
					Debug.Log("Grenade self hit, ignoring.");
					return; // don't collide with the player who threw the grenade!
				}
			}

			if (explodeOnContact) {
				Explode();
				return;
			}
		}
	}

	public void Explode() {
		// Disable collision
		if (boxCol != null) boxCol.enabled = false;
		if (meshCol != null) meshCol.enabled = false;
		if (sphereCol != null) sphereCol.enabled = false;
		if (capCol != null) capCol.enabled = false;

		explosion = GetComponent<ExplosionForce>();
		if (explosion != null) explosion.ExplodeOuter(transform.position);
		if (explosion != null) explosion.ExplodeInner(transform.position, nearforce, nearradius, null);

		GameObject explosionEffect = Const.a.GetObjectFromPool(Const.PoolType.GrenadeFragExplosions);
		if (explosionEffect != null) {
			explosionEffect.SetActive(true);
			explosionEffect.transform.position = transform.position;
			// TODO: Do I need more than one temporary audio entity for this sort of thing?
			if (deathSound != null) {
				GameObject tempAud = GameObject.Find("TemporaryAudio");
				tempAud.transform.position = transform.position;
				AudioSource aS = tempAud.GetComponent<AudioSource>();
				if (aS != null) aS.PlayOneShot(deathSound);
			}
			gameObject.SetActive(false);
		}

		//if (explosionEffect != null) Instantiate(explosion, transform.position, Quaternion.identity); //TODO: use pool
		//Destroy(this.gameObject); //TODO: use pool
	}
}
