using Unity.Collections;
using UnityEngine;
using NaughtyAttributes;

[RequireComponent(typeof(AudioSource))]
public class Pin : MonoBehaviour {

    [SerializeField]
    private AudioClip[] hitSounds;

    private new Rigidbody rigidbody;
    private AudioSource audioSource;
    private Vector3 startPosition;
    public bool HasFallen {get; private set;}

    public void Reset() {
        transform.SetPositionAndRotation(startPosition, Quaternion.identity);
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        gameObject.SetActive(true);
    }

    void Awake() {
        startPosition = transform.position;
    }

    void Start() {
        audioSource = GetComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
    }

    void Update() {

        float angleX = transform.rotation.eulerAngles.x;
        float angleZ = transform.rotation.eulerAngles.z;

        if((angleX > 5 && angleX < 355) || (angleZ > 5 && angleZ < 355)) {
            HasFallen = true;
        } else {
            HasFallen = false;
        }

    }

    void OnCollisionEnter(Collision collision) {

        bool isBowlingBall = collision.collider.CompareTag("BowlingBall") || collision.collider.CompareTag("Player");
        bool isPin = collision.collider.CompareTag("Pin");

        if(!isBowlingBall && !isPin) {
            return;
        }

        int randomIndex = Random.Range(0, hitSounds.Length);
        AudioClip hitSound = hitSounds[randomIndex];
        audioSource.PlayOneShot(hitSound);
    }

}