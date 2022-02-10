using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputControl : MonoBehaviour
{
    [SerializeField]
    private Camera arCamera;
    [SerializeField]
    private GameObject raycastPoint;

    private GameObject piece = null;

    private bool isRaySent = false;

    void Update()
    {
        if (Input.touchCount <= 0)
        {
            isRaySent = false;
        }
        if (Input.touchCount > 0 && !isRaySent)
        {
            isRaySent = true;
            Ray ray = arCamera.ScreenPointToRay(raycastPoint.transform.position);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log(hit.transform.tag);
                if (piece == null && hit.transform.gameObject.layer == LayerMask.NameToLayer("Piece"))
                {
                    piece = hit.transform.gameObject;
                }
                else if (piece != null && piece.transform.tag == hit.transform.tag)
                {
                    piece = hit.transform.gameObject;
                }
                else if (piece != null)
                {
                    piece.GetComponent<ChessPieceMovementController>().GoTo(hit.transform);
                    piece = null;
                }
            }
        }
    }
}
