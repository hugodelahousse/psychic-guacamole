using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockScript : MonoBehaviour {

	enum state {
		FIXED,
		HELD,
		LAUNCHED,
	}
	private state currentState = state.FIXED;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
