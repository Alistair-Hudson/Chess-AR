using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConnectionController : MonoBehaviour
{
    public Transform FromRootTransform;
    public Transform ToRootTransform;

    public GameObject SuccessNotification;
    public GameObject InteractionNotification;
    public Text ElementLetterText;

    public List<GameObject> Placeholders;
    public List<GameObject> Silhouettes;
    public MeshRenderer OriginSilhouetteRenderer;

    public Transform From { private get; set; }
    public Transform To { private get; set; }
    public bool IsLinked { get; private set; }

    /* Distance threshold to evaluate if an element is almost at the position of the placeholder. */
    private float _distanceThreshold = 0.01f;

    public void ShowOriginSilhouette(bool value) {    
        OriginSilhouetteRenderer.enabled = value;
    }

    public void SetElementLetter(char value) {
        ElementLetterText.text = value.ToString();
    }

    private void Update() {
        transform.position = From.position;
        transform.LookAt(To.position);
        FromRootTransform.rotation = From.rotation;
        ToRootTransform.rotation = To.rotation;
        
        /* Calculate if the element is within a certain threshold to the placeholder. */
        IsLinked = _distanceThreshold >= Vector3.Distance(To.position, ToRootTransform.position);

        if (IsLinked) {
            /* Activate silhouettes, deactivate placeholders and toggle notifications, if not already done. */
            if (SuccessNotification.activeSelf) {
                return;
            }

            foreach (GameObject silhouette in Silhouettes) {
                silhouette.SetActive(true);
            }
            foreach (GameObject placeholder in Placeholders) {
                placeholder.SetActive(false);
            }

            SuccessNotification.SetActive(true);
            InteractionNotification.SetActive(false);
        } else {
            /* Deactivate silhouettes, activate placeholders and toggle notifications, if not already done. */
            if (InteractionNotification.activeSelf) {
                return;
            }

            foreach (GameObject silhouette in Silhouettes) {
                silhouette.SetActive(false);
            }
            foreach (GameObject placeholder in Placeholders) {
                placeholder.SetActive(true);
            } 

            SuccessNotification.SetActive(false);
            InteractionNotification.SetActive(true);
        }
    }
}
