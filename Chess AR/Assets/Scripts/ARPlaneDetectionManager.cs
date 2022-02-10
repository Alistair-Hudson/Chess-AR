using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARPlacementManager), typeof(ARPlaneManager))]
public class ARPlaneDetectionManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerInputControl;
    [SerializeField]
    private Button placeButton;
    [SerializeField]
    private GameObject scaleSlider;

    private ARPlaneManager planeManager;
    private ARPlacementManager placementManager;

    private void Awake()
    {
        playerInputControl.SetActive(false);

        placementManager = GetComponent<ARPlacementManager>();
        planeManager = GetComponent<ARPlaneManager>();

        placeButton.onClick.AddListener(DisableARPlacement);
    }

    void Start()
    {
        placeButton.gameObject.SetActive(true);
        //adjustButton.SetActive(false);
        //searchForGameButton.SetActive(false);
        scaleSlider.SetActive(true);

        //infoPanel.text = "Move Phone to detect planes and place Arena";
    }

    public void DisableARPlacement()
    {
        SetAllPlanesState(false);
        planeManager.enabled = false;
        placementManager.enabled = false;

        placeButton.gameObject.SetActive(false);
        //adjustButton.SetActive(true);
        //searchForGameButton.SetActive(true);
        scaleSlider.SetActive(false);

        playerInputControl.SetActive(true);

        //infoPanel.text = "Search for to enter a battle or readjust arena";
    }

    private void SetAllPlanesState(bool state)
    {
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(state);
        }
    }
}
