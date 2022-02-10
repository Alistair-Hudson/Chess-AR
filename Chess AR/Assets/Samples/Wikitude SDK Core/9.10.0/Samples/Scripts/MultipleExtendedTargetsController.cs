using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Wikitude;

public class MultipleExtendedTargetsController : MonoBehaviour
{
    public List<GameObject> DinosaurPrefabs;

    private float _distanceThreshold;
    private Dictionary<GameObject, GameObject> _dinosaursWithTarget = new Dictionary<GameObject, GameObject>();
    private List<ImageTarget> _targets = new List<ImageTarget>();

    private void Awake() {
        foreach (GameObject prefab in DinosaurPrefabs) {
            _distanceThreshold = Dinosaur.DistanceThreshold;
            GameObject dinosaur = Instantiate(prefab);
            dinosaur.SetActive(false);
            _dinosaursWithTarget.Add(dinosaur, new GameObject(dinosaur.name + "_target"));
            dinosaur.GetComponent<Dinosaur>().SetAlignDinosaur(true);
        }
    }

    public void OnImageRecognized(ImageTarget target) {
        _targets.Add(target);
    }

    public void OnImageLost(ImageTarget target) {
        _targets.Remove(target);
    }

    private void Update() {
        foreach(var dinosaur in _dinosaursWithTarget) {
            ImageTarget target = _targets.LastOrDefault(obj => dinosaur.Key.name.IndexOf(obj.Name, StringComparison.OrdinalIgnoreCase) >= 0);
            if (target != null) {
                if (!dinosaur.Key.activeSelf || (dinosaur.Value.transform.position - target.Drawable.transform.position).magnitude > _distanceThreshold) {
                    if (!dinosaur.Key.activeSelf) {
                        dinosaur.Key.transform.SetPositionAndRotation(target.Drawable.transform.position, target.Drawable.transform.rotation);
                        dinosaur.Key.transform.up = Vector3.up;
                        dinosaur.Key.SetActive(true);
                    }
                    dinosaur.Value.transform.SetPositionAndRotation(target.Drawable.transform.position, target.Drawable.transform.rotation);
                    dinosaur.Key.GetComponent<Dinosaur>().StartWalkCoroutine(dinosaur.Value.transform);
                } else if ((dinosaur.Key.gameObject.transform.position - dinosaur.Value.transform.position).magnitude < _distanceThreshold) {
                    dinosaur.Key.GetComponent<Dinosaur>().StopIfWalking();
                }
            } else {
                dinosaur.Value.transform.SetPositionAndRotation(dinosaur.Key.transform.position, dinosaur.Key.transform.rotation);
                dinosaur.Key.GetComponent<Dinosaur>().StopIfWalking();
            }
        }
    }
}