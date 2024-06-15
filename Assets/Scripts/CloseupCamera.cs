using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseupCamera : MonoBehaviour
{
    [SerializeField]
    private new Camera camera;

    void Awake() {
        camera.gameObject.SetActive(false);
    }

    public void DisableCamera() {
        camera.gameObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other) {
        if(other.tag == "Player") {
            camera.gameObject.SetActive(true);
        }
    }

}
