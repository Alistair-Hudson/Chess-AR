using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private InputField playerNameInput;
    [SerializeField]
    private GameObject loginContainer;

    [SerializeField]
    private Button connectButton;
    [SerializeField]
    private Text connectionStaus;

    private void Awake()
    {
        connectButton.onClick.AddListener(OnConnectClicked);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            loginContainer.SetActive(false);
        }
        else
        {
            loginContainer.SetActive(true);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        connectionStaus.text = "Connection Status: " + PhotonNetwork.NetworkClientState;
    }

    private void OnConnectClicked()
    {
        if (string.IsNullOrEmpty(playerNameInput.text))
        {
            return;
        }

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = playerNameInput.text;
            PhotonNetwork.ConnectUsingSettings();
        }
    }
}
