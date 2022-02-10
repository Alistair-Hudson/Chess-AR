using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChessGameManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private InputField roomNameInput;
    [SerializeField]
    private Button joinButton;

    private void Awake()
    {
        joinButton.onClick.AddListener(OnJoinClicked);
    }

    private void OnJoinClicked()
    {
        if (string.IsNullOrEmpty(roomNameInput.text))
        {
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            PhotonNetwork.JoinRoom(roomNameInput.text);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateAndJoinRoom();
    }

    public override void OnJoinedRoom()
    {
        
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        
    }

    public override void OnLeftRoom()
    {
        
    }

    private void CreateAndJoinRoom()
    {
        string roomName = "Room" + UnityEngine.Random.Range(0, 1000).ToString();
        if (!string.IsNullOrEmpty(roomNameInput.text))
        {
            roomName = roomNameInput.text;
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
}
