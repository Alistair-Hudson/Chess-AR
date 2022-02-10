using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessPieceMovementController : MonoBehaviour
{
    public void GoTo(Transform location)
    {
        StartCoroutine(SmoothMovement(location));
    }

    private IEnumerator SmoothMovement(Transform location)
    {
        var oldLocation = transform.position;
        float time = 0;
        while (time < 1)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Slerp(oldLocation, location.position, time);
            yield return null;
        }
        transform.position = location.position;
        if (location.gameObject.layer == LayerMask.NameToLayer("Piece"))
        {
            Destroy(location.gameObject);
        }
    }
}
