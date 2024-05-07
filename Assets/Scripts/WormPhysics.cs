using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WormPhysics : MonoBehaviour
{

    
    private float currentTorque = 0f;
    private float torqueVelocity = 0f;
    public float torqueSmoothTime = 0.3f; // Time taken to reach the target torque, adjust as needed


    [SerializeField]
    Rigidbody wormHead;
    [SerializeField] private List<Rigidbody> bodySegments;

    public float torqueAmount = 20f;
    public float baseMovementForce = 15f;
    public float segmentDecrementFactor = 0.1f;
    private float currentMovementForce;
    public float maxSpeed = 20f;  // Maximum speed that worm can reach
    public float accelerationRate = 0.2f;  // Rate at which speed increases
    private float currentSpeed;  // Current speed of the worm

   void Start() {
        bodySegments = new List<Rigidbody>();
        var segments = GameObject.FindGameObjectsWithTag("Worm");
        foreach (var segment in segments) {
            bodySegments.Add(segment.GetComponent<Rigidbody>());
        }
        UpdateMovementForce();
        currentSpeed = baseMovementForce;
        foreach (Rigidbody segment in bodySegments) {
            segment.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            segment.angularDrag = 10f;  // Tune based on need
        }

    }

    void Update() {
        HandleTurning();
        HandleMovement();
        AdjustRotation();
    }

    void UpdateMovementForce() {
        currentMovementForce = baseMovementForce + (bodySegments.Count * segmentDecrementFactor);
        currentMovementForce = Mathf.Max(currentMovementForce, 5); // Ensure there's a minimum force
        wormHead.mass = wormHead.mass + bodySegments.Count;
    }
    
    void AdjustRotation() {
        RaycastHit hit;
        if (Physics.Raycast(wormHead.position, -Vector3.up, out hit, 1.5f)) {
            Vector3 newForward = Vector3.Cross(wormHead.transform.TransformDirection(Vector3.right), hit.normal);
            Quaternion targetRotation = Quaternion.LookRotation(newForward, hit.normal);
            wormHead.transform.rotation = Quaternion.Slerp(wormHead.transform.rotation, targetRotation, Time.deltaTime * 2f);
        }
    }

    void HandleMovement() {
        Vector3 forwardForceDirection = wormHead.transform.forward;
        if (Input.GetKey(KeyCode.UpArrow)) {
            currentSpeed += accelerationRate * Time.deltaTime;  // Increase speed
            currentSpeed = Mathf.Clamp(currentSpeed, 20, maxSpeed);  // Clamp speed between minimum and maximum
            wormHead.AddForce(forwardForceDirection * currentSpeed, ForceMode.Force);
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            currentSpeed -= accelerationRate * Time.deltaTime;  // Decrease speed
            currentSpeed = Mathf.Clamp(currentSpeed, 20, maxSpeed);  // Clamp speed between minimum and maximum
            wormHead.AddForce(forwardForceDirection * currentSpeed, ForceMode.Force);
        } else {
            currentSpeed = 0;  // Reset to base speed if no keys are pressed
            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);  // Clamp speed between minimum and maximum
            wormHead.AddForce(forwardForceDirection * currentSpeed, ForceMode.Force);
        }
        
    }

    void ApplyTorque(float targetTorque) {
        currentTorque = Mathf.SmoothDamp(currentTorque, targetTorque, ref torqueVelocity, torqueSmoothTime);
        wormHead.AddTorque(new Vector3(0, currentTorque, 0), ForceMode.Force);
    }

    void HandleTurning() {
        float targetTorque = 0f;
        if (Input.GetKey(KeyCode.RightArrow)) {
            targetTorque = torqueAmount;
        } else if (Input.GetKey(KeyCode.LeftArrow)) {
            targetTorque = -torqueAmount;
        }
        ApplyTorque(targetTorque);
    }


}




