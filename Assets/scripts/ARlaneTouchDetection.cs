
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlaneTouchDetection : MonoBehaviour
{
    [Header("AR References")]
    public ARRaycastManager arRaycastManager;
    public ARPlaneManager arPlaneManager;

    [Header("Portal Settings")]
    public GameObject portalPrefab;

    [Header("UI References")]
    public Text debugText;

    private List<ARRaycastHit> hits =
        new List<ARRaycastHit>();

    private bool portalSpawned = false;

    void Start()
    {
        if (debugText != null)
            debugText.text = "Scan your environment";

        if (arRaycastManager == null)
            Debug.LogError("AR Raycast Manager is missing!");

        if (arPlaneManager == null)
            Debug.LogError("AR Plane Manager is missing!");

        if (portalPrefab == null)
            Debug.LogError("Portal Prefab is missing!");
    }

    void Update()
    {
        // Stop checking for touches after portal spawns
        if (portalSpawned)
            return;

        Vector2 touchPosition;

        if (GetTouchPosition(out touchPosition))
        {
            CheckPlaneTouch(touchPosition);
        }
    }

    bool GetTouchPosition(out Vector2 position)
    {
        position = Vector2.zero;

        // Android / iOS touch detection
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            position = Touchscreen.current.primaryTouch
                .position.ReadValue();

            return true;
        }

        // Unity Editor mouse testing
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            position = Mouse.current.position.ReadValue();

            return true;
        }

        return false;
    }

    void CheckPlaneTouch(Vector2 screenPosition)
    {
        if (arRaycastManager == null ||
            arPlaneManager == null ||
            portalPrefab == null)
            return;

        hits.Clear();

        // Raycast against detected plane polygons
        bool planeTouched = arRaycastManager.Raycast(
            screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon
        );

        if (planeTouched && hits.Count > 0)
        {
            SpawnPortal();
        }
        else
        {
            if (debugText != null)
                debugText.text = "False";

            Debug.Log("No plane touched!");
        }
    }

    void SpawnPortal()
    {
        // Get the AR plane associated with the raycast hit
        ARPlane detectedPlane =
            arPlaneManager.GetPlane(hits[0].trackableId);

        Vector3 spawnPosition;
        Quaternion spawnRotation;

        if (detectedPlane != null)
        {
            // Place portal at the centre of the detected plane
            spawnPosition = detectedPlane.transform.position;
            spawnRotation = detectedPlane.transform.rotation;
        }
        else
        {
            // Fallback to the raycast hit pose
            spawnPosition = hits[0].pose.position;
            spawnRotation = hits[0].pose.rotation;
        }

        // Instantiate portal
        Instantiate(
            portalPrefab,
            spawnPosition,
            spawnRotation
        );

        // Prevent additional portals
        portalSpawned = true;

        if (debugText != null)
            debugText.text = "Portal Spawned!";

        Debug.Log("Portal successfully instantiated!");

        // Hide all detected planes
        arPlaneManager.SetTrackablesActive(false);

        // Stop further plane detection
        arPlaneManager.enabled = false;
    }
}
