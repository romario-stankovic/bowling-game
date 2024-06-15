using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour
{
    
    [SerializeField]
    private Rigidbody bowlingBall;

    [SerializeField]
    private List<Pin> pins;

    private Vector3[] pinPositions;
    private Vector3 ballPosition;

    [SerializeField]
    private Animator cleaner;

    [SerializeField]
    private Animator placer;

    [SerializeField]
    private float MinDelay = 1.0f;

    [SerializeField]
    private float MaxDelay = 5.0f;

    private float delay = 0;
    private bool isSimulating = false;

    void Start()
    {
        ballPosition = bowlingBall.transform.position;
        pinPositions = new Vector3[pins.Count];
        for (int i = 0; i < pins.Count; i++) {
            pinPositions[i] = pins[i].transform.position;
        }

        bowlingBall.isKinematic = true;

        if(Random.Range(0, 20) < 10) {
            delay = Random.Range(MinDelay, MaxDelay);
        } else {
            delay = Random.Range(0, MinDelay);
        }

    }

    void Update()
    { 
        if(delay <= 0 && isSimulating == false) {
            isSimulating = true;
            StartCoroutine(Simulate());
        } else {
            delay -= Time.deltaTime;
        }

    }

    IEnumerator Simulate() {

        float xForce = Random.Range(-1.5f, 1.5f);

        bowlingBall.isKinematic = false;
        bowlingBall.AddForce(new Vector3(xForce, 0, 45), ForceMode.Impulse);

        yield return new WaitForSeconds(5);

        cleaner.SetBool("Play", true);

        yield return new WaitForSeconds(3.5f);

        cleaner.SetBool("Play", false);

        placer.SetBool("Play", true);

        foreach(Pin pin in pins) {
            pin.Reset();
            pin.GetComponent<Rigidbody>().isKinematic = true;
        }

        yield return new WaitForSeconds(2.5f);

        placer.SetBool("Play", false);

        foreach(Pin pin in pins) {
            pin.GetComponent<Rigidbody>().isKinematic = false;
        }

        bowlingBall.velocity = Vector3.zero;
        bowlingBall.angularVelocity = Vector3.zero;
        bowlingBall.transform.position = ballPosition;
        bowlingBall.isKinematic = true;

        isSimulating = false;
        delay = Random.Range(MinDelay, MaxDelay);

        bowlingBall.GetComponent<AudioSource>().Stop();

    }

}
