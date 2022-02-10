using UnityEngine;
using System.Collections.Generic;
using Wikitude;

public class MultipleTargetsController : MonoBehaviour
{
    private readonly Dictionary<ImageTarget, Dinosaur> _visibleDinosaurs = new Dictionary<ImageTarget, Dinosaur>();

    private void Awake() {
        var trackables = FindObjectsOfType<ImageTrackable>();
        if (trackables.Length == 0) {
            Debug.LogError("No ImageTrackables were found!");
        }
        foreach (var trackable in trackables) {
            trackable.OnImageRecognized.AddListener(OnImageRecognized);
            trackable.OnImageLost.AddListener(OnImageLost);
        }
    }

    private void OnImageRecognized(ImageTarget target) {
        /* Whenever a new dinosaur is recognized, keep track of it in the _visibleDinosaurs variable.
         * Because the ImageTrackable has a prefab assigned to the Drawable property, we don't need to take
         * care of instantiating the dinosaurs manually.
         */
        _visibleDinosaurs.Add(target, target.Drawable.transform.GetChild(0).GetComponent<Dinosaur>());
    }

    private void OnImageLost(ImageTarget target) {
        var lostDinosaur = _visibleDinosaurs[target];
        _visibleDinosaurs.Remove(target);

        /* If the lost dinosaur was engaged in battle with another dinosaur,
         * notify the other dinosaur so that it can disengage and return to its idle position.
         */
        foreach (var dinosaur in _visibleDinosaurs.Values) {
            if (dinosaur.AttackingDinosaur == lostDinosaur) {
                dinosaur.OnAttackerDisappeared();
            } else if (dinosaur.TargetDinosaur == lostDinosaur) {
                dinosaur.OnTargetDisappeared();
            }
        }
    }

    private void Update() {
        if (_visibleDinosaurs.Count > 1) {
            /* If we have more than two dinosaurs, try to pair them in battles. */
            Dinosaur availableDinosaur = null;
            foreach (var dinosaur in _visibleDinosaurs.Values) {
                if (!dinosaur.InBattle) {
                    if (availableDinosaur == null) {
                        availableDinosaur = dinosaur;
                    } else {
                        availableDinosaur.Attack(dinosaur);
                        availableDinosaur = null;
                    }
                }
            }
        }
    }
}
