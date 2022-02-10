using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Wikitude;

public class RuntimeTrackerController : SampleController
{
    public InputField AddressField;
    public GameObject DefaultDrawable;
    public GameObject LoadTrackerButton;
    public GameObject DeleteTrackerButton;
    public ToastStack ToastStack;
    public GameObject TargetInstructions;

    [System.Serializable]
    public class SampleTrackerAddresses {
        public string targetCollection;
        public string assetBundleAndroid;
        public string assetBundleIOS;
        public string[] targetThumbnails;
    }

    private string _defaultAddress = "https://wikitude-web-hosting.s3-eu-west-1.amazonaws.com/sdk/sample_assets/unity_ee/remote_loading/config.json";
    private string _defaultAddressPlayerPrefsKey = "WikitudeSamples.RuntimeTrackerAddress";
    private GameObject _activeTracker;
    private List<GameObject> _targetThumbnails;
    private bool _enableTargetInstructionsOnLoad;

    protected override void Awake() {
        base.Awake();

        AddressField.text = PlayerPrefs.GetString(_defaultAddressPlayerPrefsKey, _defaultAddress);
        LoadTrackerButton.SetActive(true);
        DeleteTrackerButton.SetActive(false);
        TargetInstructions.SetActive(false);
        _targetThumbnails = new List<GameObject>();
        _enableTargetInstructionsOnLoad = false;
    }

    public void LoadTrackerData() {
        DeleteTrackerData();
        LoadTrackerButton.GetComponent<Button>().interactable = false;
        CreateTracker(AddressField.text, DefaultDrawable);
        PlayerPrefs.SetString(_defaultAddressPlayerPrefsKey, AddressField.text);
    }

    private void CreateTracker(string address, GameObject drawable) {
        switch (Path.GetExtension(address).ToLower()) {
            case ".png":
            case ".jpeg":
            case ".jpg":
                _enableTargetInstructionsOnLoad = true;
                StartCoroutine(LoadThumbnail(address));
                CreateImageTracker(address, drawable);
                break;
            case ".zip":
            case ".wtc":
                CreateImageTracker(address, drawable);
                break;
            case ".wto":
                CreateObjectTracker(address, drawable);
                break;
            case ".json":
                StartCoroutine(CreateTrackerWithConfig(address));
                break;
            default:
                Debug.LogError("Input address is not valid and has to either end with .zip, .wtc, .wto or .json!");
                DisplayToast("Input address is not valid!");
                break;
        }
    }

    public void DeleteTrackerData() {
        Destroy(_activeTracker);
        LoadTrackerButton.SetActive(true);
        LoadTrackerButton.GetComponent<Button>().interactable = true;
        DeleteTrackerButton.SetActive(false);
        TargetInstructions.SetActive(false);
        foreach (GameObject thumbnail in _targetThumbnails) {
            Destroy(thumbnail);
        }
        _targetThumbnails.Clear();
        _enableTargetInstructionsOnLoad = false;
    }

    public void ResetAddress() {
        AddressField.text = _defaultAddress;
        PlayerPrefs.SetString(_defaultAddressPlayerPrefsKey, _defaultAddress);
        DeleteTrackerData();
    }

    private void CreateImageTracker(string address, GameObject drawable) {
        _activeTracker = new GameObject("ImageTracker");
        ImageTracker tracker = _activeTracker.AddComponent<ImageTracker>();
        tracker.TargetSourceType = TargetSourceType.TargetCollectionResource;
        tracker.TargetCollectionResource = new TargetCollectionResource();

        ImageTrackable trackable = (new GameObject("ImageTrackable")).AddComponent<ImageTrackable>();
        trackable.Drawable = drawable;
        trackable.transform.parent = _activeTracker.transform;

        SetTargetCollectionResource(tracker.TargetCollectionResource, address);
    }

    private void CreateObjectTracker(string address, GameObject drawable) {
        _activeTracker = new GameObject("ObjectTracker");
        ObjectTracker tracker = _activeTracker.AddComponent<ObjectTracker>();
        tracker.TargetCollectionResource = new TargetCollectionResource();

        ObjectTrackable trackable = (new GameObject("ObjectTrackable")).AddComponent<ObjectTrackable>();
        trackable.Drawable = drawable;
        trackable.transform.parent = _activeTracker.transform;

        SetTargetCollectionResource(tracker.TargetCollectionResource, address);
    }

    private void SetTargetCollectionResource(TargetCollectionResource resource, string address) {
        resource.UseCustomURL = true;
        resource.TargetPath = address;

        resource.OnFinishLoading.AddListener(() => {
            base.OnTargetsLoaded();
            DisplayToast("Tracker finished loading!");
            TargetInstructions.SetActive(_enableTargetInstructionsOnLoad);
            LoadTrackerButton.SetActive(false);
            DeleteTrackerButton.SetActive(true);
        });
        resource.OnErrorLoading.AddListener((error) => {
            base.OnErrorLoadingTargets(error);
            DisplayToast("Tracker failed to load target collection!");
            DeleteTrackerData();
        });
    }

    private IEnumerator CreateTrackerWithConfig(string address) {
        Caching.ClearCache();
        UnityWebRequest request = UnityWebRequest.Get(address);
        yield return request.SendWebRequest();

        if (request.error != null) {
            Debug.LogError(request.error);
            DisplayToast("Config file couldn't be loaded!");
        } else {
            string jsonString = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);

            SampleTrackerAddresses addresses = JsonUtility.FromJson<SampleTrackerAddresses>(jsonString);
            StartCoroutine(LoadAssetBundle(addresses));
        }
    }

    private IEnumerator LoadAssetBundle(SampleTrackerAddresses addresses) {
        string drawableAddress = Application.platform == RuntimePlatform.Android ? addresses.assetBundleAndroid : addresses.assetBundleIOS;

        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(drawableAddress);
        yield return request.SendWebRequest();

        if (request.error != null) {
            Debug.LogError(request.error);
            DisplayToast("AssetBundle couldn't be loaded!");
        } else {
            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
            GameObject drawable = DefaultDrawable;
            foreach (string assetName in bundle.GetAllAssetNames()) {
                if (Path.GetExtension(assetName) ==  ".prefab") {
                    drawable = bundle.LoadAsset(assetName) as GameObject;
                    DisplayToast("Custom Prefab loaded!");
                    break;
                }
            }
            foreach (string thumbnailAddress in addresses.targetThumbnails) {
                _enableTargetInstructionsOnLoad = true;
                StartCoroutine(LoadThumbnail(thumbnailAddress));
            }

            string targetCollectionAddress = addresses.targetCollection;

            string[] allowedExtensions = {".png", ".jpg", ".jpeg", ".zip", "wtc", ".wto"};
            if (Array.IndexOf(allowedExtensions, Path.GetExtension(targetCollectionAddress).ToLower()) > -1) {
                CreateTracker(targetCollectionAddress, drawable);
            } else {
                Debug.LogError("Config file doesn't contain a valid target collection address that ends with .zip, .wtc or .wto!");
                DisplayToast("Config file doesn't contain a valid target collection address!");
                DeleteTrackerData();
            }
            bundle.Unload(false);
        }
    }

    private IEnumerator LoadThumbnail(string address) {
        GameObject thumbnail = new GameObject("Thumbnail");
        thumbnail.transform.parent = TargetInstructions.transform;
        _targetThumbnails.Add(thumbnail);

        LayoutElement layoutElement = thumbnail.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = 90;

        RawImage rawImage = thumbnail.AddComponent<RawImage>();

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(address);
        yield return request.SendWebRequest();

        if (request.error == null) {
            rawImage.texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            if (rawImage.texture.width > rawImage.texture.height) {
                thumbnail.transform.localScale = new Vector3(1f, (float)rawImage.texture.height / rawImage.texture.width, 1f);
            } else {
                thumbnail.transform.localScale = new Vector3((float)rawImage.texture.width / rawImage.texture.height, 1f, 1f);
            }
        }
    }

    private void DisplayToast(string message) {
        ToastStack.ShowToast(ToastStack.CreateToast(message));
    }
}