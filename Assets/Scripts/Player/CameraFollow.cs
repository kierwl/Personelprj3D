using System.Collections;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 10f;
    public Vector3 offset = new Vector3(0, 2, -5);
    public float minDistance = 3f;
    public float maxDistance = 10f;
    public float zoomSpeed = 2f;
    public float minVerticalAngle = -30f;
    public float maxVerticalAngle = 60f;
    public float collisionOffset = 0.2f; // Distance to keep from obstacles
    public LayerMask collisionLayers; // Layers to check for collision

    private float currentZoom;
    private float currentRotationX;
    private float currentRotationY;
    private Vector3 currentVelocity = Vector3.zero;
    private float targetZoom; // Target zoom level for smooth collision adjustment
    private float zoomVelocity = 0f; // For SmoothDamp

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target specified for CameraFollow!");
            return;
        }

        currentZoom = targetZoom = offset.magnitude;
        currentRotationY = transform.eulerAngles.y;
        currentRotationX = transform.eulerAngles.x;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        // Update zoom based on scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            targetZoom -= scrollInput * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minDistance, maxDistance);
        }

        // Smoothly adjust current zoom towards target zoom
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, 0.2f);

        // Calculate ideal camera position (without collision)
        Vector3 direction = new Vector3(0, 0, -currentZoom);
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        Vector3 desiredPosition = target.position + rotation * direction;

        // Handle collision detection with environment
        float adjustedZoom = HandleCameraCollision(rotation, desiredPosition);

        // Smoothly adjust camera position if collision occurred
        if (adjustedZoom < currentZoom)
        {
            // Calculate new position based on adjusted zoom
            Vector3 adjustedDirection = new Vector3(0, 0, -adjustedZoom);
            desiredPosition = target.position + rotation * adjustedDirection;
        }

        // Apply smooth movement to the camera
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, Time.deltaTime * smoothSpeed);

        // Look at point slightly above target
        transform.LookAt(target.position + Vector3.up * 1.5f);
    }

    // Method to receive rotation inputs from PlayerController
    public void UpdateCameraRotation(float horizontalInput, float verticalInput)
    {
        // Update camera rotation angles
        currentRotationY += horizontalInput;
        currentRotationX -= verticalInput; // Invert for natural camera control

        // Clamp vertical rotation to prevent flipping
        currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);

        // Normalize the rotation Y angle to keep it between 0-360
        if (currentRotationY > 360f) currentRotationY -= 360f;
        if (currentRotationY < 0f) currentRotationY += 360f;
    }

    private float HandleCameraCollision(Quaternion rotation, Vector3 desiredPosition)
    {
        // Calculate a point slightly above the player for better raycasting
        Vector3 targetPos = target.position + Vector3.up * 1.5f;

        // Direction from target to desired camera position
        Vector3 direction = desiredPosition - targetPos;
        float distance = direction.magnitude;

        // Check for collisions along the ray
        RaycastHit hit;
        if (Physics.SphereCast(targetPos, 0.3f, direction.normalized, out hit, distance, collisionLayers))
        {
            // Calculate new distance that will keep camera in front of obstacles
            float adjustedDistance = hit.distance - collisionOffset;

            // Clamp the adjusted distance
            adjustedDistance = Mathf.Clamp(adjustedDistance, minDistance, targetZoom);

            return adjustedDistance;
        }

        return currentZoom;
    }

    // Use this when transitioning to new camera positions (like entering a building)
    public void SetCameraPosition(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
        currentRotationX = rotation.eulerAngles.x;
        currentRotationY = rotation.eulerAngles.y;
    }

    // Add zooming methods that can be called from player input if needed
    public void ZoomIn(float amount)
    {
        targetZoom -= amount;
        targetZoom = Mathf.Clamp(targetZoom, minDistance, maxDistance);
    }

    public void ZoomOut(float amount)
    {
        targetZoom += amount;
        targetZoom = Mathf.Clamp(targetZoom, minDistance, maxDistance);
    }
}