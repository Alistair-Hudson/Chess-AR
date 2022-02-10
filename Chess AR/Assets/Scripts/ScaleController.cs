using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class ScaleController : MonoBehaviour
{
    [SerializeField]
    private Slider scaleSlider;

    private ARSessionOrigin sessionOrigin;

    private void Awake()
    {
        sessionOrigin = GetComponent<ARSessionOrigin>();
    }

    // Start is called before the first frame update
    void Start()
    {
        scaleSlider.onValueChanged.AddListener(OnSliderChange);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSliderChange(float scaleValue)
    {
        if (scaleSlider == null)
        {
            return;
        }

        sessionOrigin.transform.localScale = Vector3.one * scaleValue;
    }
}
