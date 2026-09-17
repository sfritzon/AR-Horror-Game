using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARObjectPlacer : MonoBehaviour
{
    [SerializeField] private GameObject objectPrefab;

    private ARRaycastManager raycastManager;
    private Camera arCamera;

    private static readonly List<ARRaycastHit> hits = new();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        arCamera = Camera.main;
    }

    private void Update()
    {
        if (Touchscreen.current == null)
            return;

        if (!Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return;

        Vector2 touchPosition =
            Touchscreen.current.primaryTouch.position.ReadValue();

        // Först: kolla om vi tryckte på ett collectible
        Ray ray = arCamera.ScreenPointToRay(touchPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Collectible collectible = hit.collider.GetComponent<Collectible>();

            if (collectible != null)
            {
                collectible.Collect();
                return;
            }
        }

        // Annars: försök placera objektet på en AR-yta
        if (raycastManager.Raycast(
                touchPosition,
                hits,
                TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;

            Instantiate(
                objectPrefab,
                hitPose.position,
                hitPose.rotation);
        }
    }
}