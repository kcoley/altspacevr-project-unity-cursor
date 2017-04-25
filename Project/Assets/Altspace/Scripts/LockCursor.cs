using UnityEngine;
using System.Collections;

public class LockCursor : MonoBehaviour {
	void Update ()
	{
		Cursor.lockState = CursorLockMode.Locked;

		if (Input.GetKey(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
