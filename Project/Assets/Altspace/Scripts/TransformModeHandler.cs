using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformModeHandler : MonoBehaviour {
	public enum TransformModes {Off, Translate, Rotate, Scale};
	public TransformModes transformMode = TransformModes.Off;
	public bool axisLock = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyUp(KeyCode.T)) {
			transformMode = TransformModes.Translate;
		}
		else if (Input.GetKeyUp(KeyCode.R)) {
			transformMode = TransformModes.Rotate;
		}
		if (Input.GetKeyUp (KeyCode.S)) {
			transformMode = TransformModes.Scale;
		} else if (Input.GetKeyUp(KeyCode.O)) {
			transformMode = TransformModes.Off;
		}
		
	}
}
