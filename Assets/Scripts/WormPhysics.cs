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
    public float baseHeadMass = 10f;
    public float baseMaxSpeed = 25f;
    public float maxSpeed = 20f;  // Maximum speed that worm can reach
    public float accelerationRate = 0.2f;  // Rate at which speed increases
    private float currentSpeed;  // Current speed of the worm

   void Start() {
        bodySegments = new List<Rigidbody>();
        GameObject[] wormObjects = GameObject.FindGameObjectsWithTag("Worm");  // Find all GameObjects tagged with "Worm"
        Debug.Log(wormObjects[0]);
        foreach (GameObject worm in wormObjects) {
            foreach (Transform child in worm.transform) {  // Iterate over each child of the worm GameObject
                Rigidbody rb = child.GetComponent<Rigidbody>();  // Get the Rigidbody component from the child
                if (rb != null) {
                    bodySegments.Add(rb);  // Add the Rigidbody to the bodySegments list
                }
            }
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
        wormHead.mass = baseHeadMass + bodySegments.Count;
        maxSpeed = baseMaxSpeed + bodySegments.Count * 2;
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

    public GameObject segmentPrefab;

    void AddSegment() {
        // Find the last segment before the end piece
        Rigidbody lastSegment = bodySegments[bodySegments.Count - 1];

        // Instantiate the new segment
        GameObject newSegment = Instantiate(segmentPrefab, lastSegment.position - lastSegment.transform.forward * 0.2f, Quaternion.identity);
        Rigidbody newSegmentRb = newSegment.GetComponent<Rigidbody>();
        newSegmentRb.transform.rotation = lastSegment.transform.rotation;

        // Add to the list
        bodySegments.Insert(bodySegments.Count, newSegmentRb);

        // Update or add a character joint
        CharacterJoint joint = newSegment.GetComponent<CharacterJoint>();
        joint.connectedBody = lastSegment;

        // Update movement force as needed
        UpdateMovementForce();
        
    }


    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Target")) {  // Assuming "Target" is the tag of the object to trigger segment addition
            AddSegment();
            RepositionTarget(other.gameObject);
         
        }
    }

    void RepositionTarget(GameObject target) {
        float terrainWidth = 125f;  // Assuming the terrain is 125 units wide
        float terrainLength = 125f;  // Assuming the terrain is 125 units long

        Vector3 newPosition = new Vector3(
            Random.Range(0, terrainWidth / 2),
            0, // This will be set by the terrain height
            Random.Range(0, terrainLength / 2)
        );

        newPosition.y = GetTerrainHeight(newPosition) + 0.5f; // Ensure the target is slightly above the terrain to avoid clipping
        target.transform.position = newPosition;
    }


    float GetTerrainHeight(Vector3 position) {
        // If using Unity Terrain, you can directly access the height at a position:
        // return Terrain.activeTerrain.SampleHeight(position) + Terrain.activeTerrain.transform.position.y;

        // Using raycasting:
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(position.x, 1000f, position.z), Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain"))) {
            return hit.point.y;  // Return the y position of the terrain hit
        }

        return 0f;  // Default to 0 if no terrain was hit (adjust as needed)
    }



}




