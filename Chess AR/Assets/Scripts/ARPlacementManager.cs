using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
public class ARPlacementManager : MonoBehaviour
{
    [SerializeField]
    private Camera arCamera;
    [SerializeField]
    private GameObject chessBoard;

    private ARRaycastManager raycastManager;
    private static List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    private void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);

        Ray ray = arCamera.ScreenPointToRay(screenCenter);

        if (raycastManager.Raycast(ray, raycastHits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = raycastHits[0].pose;

            Vector3 posPlacement = hitPose.position;
            chessBoard.transform.position = posPlacement;
        }
    }
}
