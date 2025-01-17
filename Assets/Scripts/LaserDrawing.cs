﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDrawing : MonoBehaviour {
	public GameObject followStarter;
	public Vector3 startPoint;
	public Vector3 endPoint;
    public float lineLife = 0.15f;
	public LineRenderer line;

	void Awake () {
		line = GetComponent<LineRenderer>();
		line.startWidth = 0.2f;
		line.endWidth = 0.2f;
        line.enabled = true;
	}

	void Update () {
		//if (followStarter != null) {
		//	endPoint = followStarter.transform.position;
//
		//}
		line.SetPosition(0,startPoint);
		line.SetPosition(1,endPoint);
	}

    void OnEnable() {
        StartCoroutine(DisableLine());
        line.enabled = true;
    }

    IEnumerator DisableLine() {
        yield return new WaitForSeconds(lineLife);
        Vector3 sp = new Vector3(5000f, 5000f, 5000f);
        Vector3 ep = sp;
        line.SetPosition(0, sp);
        line.SetPosition(1, ep);
        line.enabled = false;
    }
}
