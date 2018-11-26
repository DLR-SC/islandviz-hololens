using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTracker : MonoBehaviour {

    Light light;
    GameObject mainCamera;

	// Use this for initialization
	void Start ()
    {
        light = GetComponent<Light>();
        mainCamera = MixedRealityCameraManager.Instance.gameObject;

    }
	
	// Update is called once per frame
	void Update ()
    {
        float opposite = Camera.main.transform.localRotation.eulerAngles.y;
        light.transform.localRotation = Quaternion.Euler(50, opposite, 0);
	}
}
