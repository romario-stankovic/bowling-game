using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Ball : MonoBehaviour
{

    [SerializeField] float force = 10f;
    [SerializeField] float sensitivityX = 1f;
    [SerializeField] float sensitivityY = 1f;

    [SerializeField] float minYForce = 1f;
    [SerializeField] float maxYForce = 10f;
    [SerializeField] float minXForce = -5f;
    [SerializeField] float maxXForce = 5f;

    [SerializeField] float maxDragY = 0.3f;
    [SerializeField] float maxDragX = 0.3f;

    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] AudioClip ball;

    private Vector3 startPosition;
    private Vector3 dragPosition;
    private Vector2 clickPosition;
    private new Rigidbody rigidbody;
    private AudioSource audioSource;
    private float angle = 0;
    private float startRotationSpeed = 360f;

    public void Reset() {
        transform.position = startPosition;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.isKinematic = true;
        audioSource.Stop();
    }

    void Awake() {
        startPosition = transform.position;
        dragPosition = startPosition;
        startRotationSpeed = rotationSpeed;
    }

    void Start() {
        audioSource = GetComponent<AudioSource>();
        rigidbody = GetComponent<Rigidbody>();
        rigidbody.isKinematic = true;
    }

    // Update is called once per frame
    void Update()
    {
        // Rotate the ball around the X axis
        if(rigidbody.isKinematic) {
            angle += rotationSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Euler(angle, angle / 3, 0);
        }

        #if UNITY_EDITOR
        if(Input.GetKeyDown(KeyCode.Space) && CompareTag("Player") && rigidbody.isKinematic) {
            rigidbody.isKinematic = false;
            rigidbody.AddForce((Vector3.forward * maxYForce) + (Vector3.right * -0.4f), ForceMode.Impulse);
        }
        #endif

        if(rigidbody.isKinematic) {
            transform.position = Vector3.Lerp(transform.position, dragPosition, Time.deltaTime * 10f);
        }

    }

    void OnMouseDown() {
        clickPosition = Input.mousePosition;;
    }

    void OnMouseDrag() {

        // Get the drag position on the screen
        Vector2 dragPosition = Input.mousePosition;

        // Calculate the percentage of the screen the user has dragged multiplied by the sensitivity
        float yPercentage = (dragPosition.y - clickPosition.y) * sensitivityY / Screen.height;
        float xPercentage = (dragPosition.x - clickPosition.x) * sensitivityX / Screen.width;

        // Clamp the values
        yPercentage = Mathf.Clamp(yPercentage, 0, 1f);
        xPercentage = Mathf.Clamp(xPercentage, -1f, 1f);

        float xDrag = Mathf.Clamp(xPercentage * 0.1f, -maxDragX, maxDragX);
        float yDrag = Mathf.Clamp(yPercentage * 0.1f, 0, maxDragY);

        // Move the ball slightly in the direction the user is dragging
        this.dragPosition = new Vector3(startPosition.x + xDrag, startPosition.y + yDrag, startPosition.z + yDrag / 2);

    }

    void OnMouseUp() {
        // Get the release point on the screen
        Vector2 releasePosition = Input.mousePosition;

        // Calculate the percentage of the screen the user has dragged multiplied by the sensitivity
        float yPercentage = (releasePosition.y - clickPosition.y) * sensitivityY / Screen.height;
        float xPercentage = (releasePosition.x - clickPosition.x) * sensitivityX / Screen.width;

        // Clamp the values
        yPercentage = Mathf.Clamp(yPercentage, 0, 1f);
        xPercentage = Mathf.Clamp(xPercentage, -1f, 1f);

        // If the user has dragged less than 30% of the screen, don't throw
        if(yPercentage < 0.3) {
            dragPosition = startPosition;
            return;
        }

        // Enable the rigidbody so the ball can move
        rigidbody.isKinematic = false;

        // Calculate the force to apply
        float xForce = Mathf.Clamp(xPercentage * force, minXForce, maxXForce);
        float yForce = Mathf.Clamp(yPercentage * force, minYForce, maxYForce);

        // Apply the force
        rigidbody.AddForce((Vector3.forward * yForce) + (Vector3.right * xForce), ForceMode.Impulse);

        dragPosition = startPosition;
    }

    void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.CompareTag("Floor") && audioSource.isPlaying == false) {
            audioSource.clip = ball;
            audioSource.Play();
        }
    }

}
