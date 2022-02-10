using UnityEngine;

public class CircularPathVisualizer : MonoBehaviour
{
    public Material PathMaterial;

    private void Start() {
        /* The LineRenderer will draw the path to be traveled */
        var path = gameObject.AddComponent<LineRenderer>();
        path.useWorldSpace = false;
        path.material = PathMaterial;
        path.startWidth = 0.001f;
        
        /* Set radius as distance to first child */
        float radius = 0f;
        if (transform.childCount != 0) {
            radius = Vector3.Distance(Vector3.zero, transform.GetChild(0).localPosition);
        }
        
        /* Calculate 360 points of a circle + one additional one to close the loop */
        var pointArray = new Vector3[361];
        
        for (int i = 0; i < pointArray.Length; i++) {
            pointArray[i] = new Vector3(Mathf.Sin(Mathf.Deg2Rad * i) * radius, 0, Mathf.Cos(Mathf.Deg2Rad * i) * radius);
        }

        path.positionCount = pointArray.Length;
        path.SetPositions(pointArray);
    }
}
