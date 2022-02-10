using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wikitude;


public class MoleculeController : MonoBehaviour
{
    public GameObject ConnectionPrefab;
    public GameObject MoleculeFoundPrefab;

    private List<GameObject> _atomsInScene = new List<GameObject>();
    private List<ConnectionController> _connectionsInScene = new List<ConnectionController>();

    /* If a molecule is correctly combined, a molecule found notification object is created. */
    private GameObject _moleculeFoundNotification;

    /* An array of possible molecules with the first letter of the string being the central atom. */
    private string[] _possibleMolecules = new string[]{"CHHO", "OHH", "COO", "CO", "OO"};

    /* This string is set, if there is a possible molecule combination possible. */
    private string _possibleMoleculeCombination = "";
    /* Furthermore, this array is set with the atoms of the possible molecule combination. */
    private GameObject[] _combinableAtoms;

    public void OnObjectRecognized(ObjectTarget target) {
        _atomsInScene.Add(target.Drawable);
        CalculateCombinations();
    }

    public void OnObjectLost(ObjectTarget target) {
        _atomsInScene.Remove(target.Drawable);
        CalculateCombinations();
    }

    private void Update() {
        if (_possibleMoleculeCombination.Length >= 2) {
            /* Check if the possible molecule combination has changed by comparing the connections counts. */
            if (_possibleMoleculeCombination.Length - 1 != _connectionsInScene.Count) {
                DeleteConnections();
            }

            /* Count the number of correctly linked connections. */
            int linkedCount = 0;
            for (int i = 0; i < _possibleMoleculeCombination.Length - 1; i++) {
                 /* Create a new connection if this index doesn't exist yet. */
                if (i > _connectionsInScene.Count - 1) {
                    CreateConnection(_combinableAtoms[0], _combinableAtoms[i + 1], _possibleMoleculeCombination[i + 1]);
                }

                if (_connectionsInScene[i].IsLinked) {
                    linkedCount++;
                }

                /* Only show origin silhouette on the first link. */
                _connectionsInScene[i].ShowOriginSilhouette(linkedCount == 1);
            } 
            
            /* Check if all connections are correctly linked and show notification if so. */
            if (linkedCount == _possibleMoleculeCombination.Length - 1) {
                SetFoundNotification(true);
            } else {
                SetFoundNotification(false);
            }
        } else {
            if (_connectionsInScene.Count > 0) {
                DeleteConnections();
            }

            SetFoundNotification(false);
        }
    }

    private void CreateConnection(GameObject from, GameObject to, char element) {      
        GameObject obj = Instantiate(ConnectionPrefab);

        var connection = obj.GetComponent<ConnectionController>();
        connection.From = from.transform;
        connection.To = to.transform;
        connection.SetElementLetter(element);

        _connectionsInScene.Add(connection);
    }

    private void DeleteConnections() {
        foreach (ConnectionController connection in _connectionsInScene) {
            GameObject.Destroy(connection.gameObject);
        }
        _connectionsInScene.Clear();
    }

    private void CalculateCombinations() {   
        _possibleMoleculeCombination = "";
        _combinableAtoms = new GameObject[0];

        foreach (string molecule in _possibleMolecules) {
            string checkString = molecule;
            _combinableAtoms = new GameObject[molecule.Length];

            /* Find atoms in scene to be combined to form the molecule. */
            foreach (GameObject atom in _atomsInScene) {
                char atomChar;
                if(atom.name.Contains("carbon")) {
                    atomChar = 'C';
                } else if (atom.name.Contains("hydrogen")) {
                    atomChar = 'H';
                } else {
                    atomChar = 'O';
                }

                /* Add the atom to the combinable atoms array, if it is part of the molecule. */
                int index = checkString.IndexOf(atomChar);
                if (index >= 0) {
                    _combinableAtoms[index] = atom;
                    /* Replace found atom in the check string. */
                    checkString = checkString.Remove(index, 1).Insert(index, "X");
                }
            }

            /* Check if all atoms have been replaced in the string */
            if (checkString == new string('X', checkString.Length)) {
                /* We found our biggest possible molecule combination with the atoms in the scene. */
                _possibleMoleculeCombination = molecule;
                return;
            }

        }
    }

    /* Set a molecule notification text with the correct name. */
    private void SetFoundNotification(bool value) {
        if (value) {
            if (_moleculeFoundNotification) {
                _moleculeFoundNotification.transform.position = _combinableAtoms[0].transform.position;
            } else {
                string molecule = "";

                switch (_possibleMoleculeCombination) {
                    case "CHHO": 
                        molecule = "Formaldehyde"; 
                        break;
                    case "OHH": 
                        molecule = "Water"; 
                        break;
                    case "COO": 
                        molecule = "Carbon dioxide";
                        break;
                    case "CO": 
                        molecule = "Carbon monoxide"; 
                        break;
                    case "OO": 
                        molecule = "Molecular oxygen"; 
                        break;
                    default: break;
                }

                _moleculeFoundNotification = GameObject.Instantiate(MoleculeFoundPrefab);
                Transform moleculeText = _moleculeFoundNotification.transform.Find("Pivot/Canvas/MoleculeText");
                if(moleculeText) {
                    moleculeText.gameObject.GetComponent<Text>().text = molecule;
                }
            }
        } else {
            if (_moleculeFoundNotification) {
                GameObject.Destroy(_moleculeFoundNotification.gameObject);
                _moleculeFoundNotification = null;
            }
        }
    }
}
