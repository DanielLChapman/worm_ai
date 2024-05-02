using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormPhysics : MonoBehaviour
{
    [SerializeField]
    Rigidbody wormHead;

    [SerializeField] private List<Rigidbody> bodySegments;

    public float maxAcceleration = 15f;
    public float accelerationIncrement = 0.5f;
    public float minAcceleration = -6f;
    public float currentAcceleration = 0;

    public float topSpeed = 10f; // Define the top speed of your worm
    public float currentSpeed;

    public float baseForceMagnitude = 10f;
    public float currentForceMagnitude = 0f;
    public float forceIncreaseRate = 5f;

    public float torqueAmount = 20f; // Added torque amount for turning



    public float offsetFront = 0.75f;
    public float offsetBack = 0.75f;

    private Vector3 currentForceDirection = Vector3.zero;
    public float directionChangeSpeed = 5f;  // Control how quickly the force direction changes


    public void ApplyForce(Rigidbody rb, Vector3 direction, float forceMagnitude) {
        // Apply a minimum threshold force to overcome initial inertia
        float effectiveForce = forceMagnitude + (forceMagnitude == 0 ? 0 : (forceMagnitude > 0 ? 15 : -15));
        Vector3 appliedForce = direction * effectiveForce;
        rb.AddForce(appliedForce, ForceMode.Force);

        Debug.DrawRay(rb.position, appliedForce, Color.red, 0.1f);
    }


    // Start is called before the first frame update
    void Start() {
        bodySegments = new List<Rigidbody>();
        var segments = GameObject.FindGameObjectsWithTag("Worm");
        foreach (var segment in segments) {
            bodySegments.Add(segment.GetComponent<Rigidbody>());
        }
    }
    void Update() {
        HandleTurning();
        HandleMovement();
    }

    void HandleTurning() {
        if (Input.GetKey(KeyCode.RightArrow)) {
        wormHead.AddTorque(0, torqueAmount, 0, ForceMode.Force); // Adjust the torqueAmount to get the desired turning speed
    } else if (Input.GetKey(KeyCode.LeftArrow)) {
        wormHead.AddTorque(0, -torqueAmount, 0, ForceMode.Force);
    }
    }

    public void HandleMovement() {
        Vector3 desiredDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow)) {
            desiredDirection = wormHead.transform.forward;
            currentAcceleration += accelerationIncrement * Time.deltaTime;
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            desiredDirection = -wormHead.transform.forward;
            currentAcceleration -= accelerationIncrement * Time.deltaTime;
        } else {
            // Gradually reduce the acceleration towards zero when no key is pressed
            if (Mathf.Abs(currentAcceleration) > 0) {
                currentAcceleration -= Mathf.Sign(currentAcceleration) * accelerationIncrement *2* Time.deltaTime;
                currentAcceleration = (Mathf.Abs(currentAcceleration) < 0.01f) ? 0 : currentAcceleration;
            }
        }

        // Clamp the acceleration to its maximum and minimum values
        currentAcceleration = Mathf.Clamp(currentAcceleration, minAcceleration, maxAcceleration);

        // Smoothly update the current force direction towards the desired direction
        currentForceDirection = Vector3.Lerp(currentForceDirection, desiredDirection, Time.deltaTime * directionChangeSpeed);

        // Apply the force in the current (smoothly updated) direction
        if (currentForceDirection != Vector3.zero) {
            ApplyForce(wormHead, currentForceDirection.normalized, Mathf.Abs(currentAcceleration));
        }
    }



}