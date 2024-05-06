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

    public float torqueAmount = 20f; // Added torque amount for turning

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

AdjustRotation();
        
    }
    
    
    void AdjustRotation() {
        RaycastHit hit;
        if (Physics.Raycast(wormHead.position, -Vector3.up, out hit, 1.5f)) {
            Vector3 newForward = Vector3.Cross(wormHead.transform.TransformDirection(Vector3.right), hit.normal);
            Quaternion targetRotation = Quaternion.LookRotation(newForward, hit.normal);
            wormHead.transform.rotation = Quaternion.Slerp(wormHead.transform.rotation, targetRotation, Time.deltaTime * 2f);
        }
    }

    private float currentTorque = 0f;
    private float torqueVelocity = 0f;
    public float torqueSmoothTime = 0.3f; // Time taken to reach the target torque, adjust as needed

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

        
    public float movementForce = 10f;  // The force to apply for moving forward or backward
    public float brakingForce = 20f;   // Stronger force applied for braking

    public Quaternion previousRotation;

    void HandleMovement() {
        Vector3 forwardForceDirection = wormHead.transform.forward;
        float rotationChangeMagnitude = Quaternion.Angle(wormHead.transform.rotation, previousRotation);

        float forceModifier = Mathf.Clamp01((90 - rotationChangeMagnitude) / 90);  // Reduces force as rotation change increases

        if (Input.GetKey(KeyCode.UpArrow)) {
            wormHead.AddForce(forwardForceDirection * movementForce * forceModifier, ForceMode.Force);
        } else if (Input.GetKey(KeyCode.DownArrow)) {
            wormHead.AddForce(-forwardForceDirection * movementForce * forceModifier, ForceMode.Force);
        }

        previousRotation = wormHead.transform.rotation;
    }


void ApplyForceAtPoint(Vector3 point, Vector3 force) {
    wormHead.AddForceAtPosition(force, point, ForceMode.Force);
    Debug.DrawRay(point, force, Color.red);  // Visualize the force application
}




}




