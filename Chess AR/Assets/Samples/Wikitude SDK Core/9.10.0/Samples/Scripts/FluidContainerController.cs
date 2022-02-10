using UnityEngine;

/* Class that communicates with the fluid material shader to control its oscillation. */
public class FluidContainerController : MonoBehaviour
{

    public int OscillationMaxHeightDivider = 5;
    public int OscillationMaxTimes = 5;

    private Material _fluidMaterial;

    private bool _isOscillationAscendent = true;
    private float _oscillationIndex = 0.0f;
    private float _oscillationMaxTimesCount = 0.0f;

    int lastDegrees = 0;

    private void Start() {
        _fluidMaterial = GetComponent<Renderer>().material;
        _fluidMaterial.SetFloat("_OscillationMaxHeightDivider", OscillationMaxHeightDivider);
        _fluidMaterial.SetFloat("_OscillationMaxTimes", OscillationMaxTimes);

        _oscillationMaxTimesCount = OscillationMaxTimes;
    }

    private void Update() {
        float zEulerAngles = transform.eulerAngles.z;
        if (zEulerAngles > 180.0f) {
            zEulerAngles -= 360.0f;
        } else if (zEulerAngles < -180.0f) {
            zEulerAngles += 360.0f;
        }

        // Not restarting the oscillation times until rotation is enough
        if ((int)zEulerAngles > lastDegrees + 2 || (int)zEulerAngles < lastDegrees - 2) {
            lastDegrees = (int)zEulerAngles;
            _oscillationMaxTimesCount = 0;
        }

        if (_oscillationIndex == 0) {
            // Not modifying the oscillation index if maximum times are reached
            if (_oscillationMaxTimesCount < OscillationMaxTimes) {
                if (_isOscillationAscendent) {
                    _oscillationIndex++;
                    _oscillationMaxTimesCount++;
                } else {
                    _oscillationIndex--;
                }
            }
        } else if (_oscillationIndex > 0) {
            if (_isOscillationAscendent) {
                _oscillationIndex++;
                if (_oscillationIndex == OscillationMaxHeightDivider) {
                    _isOscillationAscendent = false;
                }
            } else {
                _oscillationIndex--;
            }
        } else {
            if (_isOscillationAscendent) {
                _oscillationIndex++;
            } else {
                _oscillationIndex--;
                if (_oscillationIndex == -OscillationMaxHeightDivider) {
                    _isOscillationAscendent = true;
                }
            }
        }

        _fluidMaterial.SetVector("_LocalPosition", transform.position - new Vector3(0.0f, transform.localPosition.y, 0.0f));
        _fluidMaterial.SetFloat("_ObjectRotation", zEulerAngles);
        _fluidMaterial.SetFloat("_OscillationIndex", _oscillationIndex);
        _fluidMaterial.SetFloat("_OscillationMaxTimesCount", _oscillationMaxTimesCount);
    }
}
