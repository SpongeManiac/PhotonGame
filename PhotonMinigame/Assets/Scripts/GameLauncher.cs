using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class GameLauncher : MonoBehaviourPunCallbacks
{
    
    //client version
    string gameVersion = "0.1";

    public GameObject connectControls;
    public GameObject joinControls;
    public GameObject inputControls;

    public Text playerNameInput;
    public Text roomNameInput;

    public string roomTry = "";

    // Store the PlayerPref Key to avoid typos
    const string playerNamePrefKey = "PlayerName";

    private void Awake()
    {
        //automatically syncs peers to master when level changes
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey(playerNamePrefKey))
        {
            //playerNameInput.text = PlayerPrefs.GetString(playerNamePrefKey);
            SetNickname(playerNamePrefKey);
        }

        if (PhotonNetwork.IsConnected)
        {
            connectControls.SetActive(false);
        }
        else
        {
            ConnectCloud();
        }
    }

    void SetNickname(string name)
    {
        //check if name is empty or nothing
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }

        PlayerPrefs.SetString(playerNamePrefKey, name);
        PhotonNetwork.NickName = name;
    }

    public void OnConnectClick()
    {
        if (PhotonNetwork.IsConnected)
        {
            //joinControls.SetActive(true);
            connectControls.SetActive(false);
        }
        else
        {
            ConnectCloud();
        }
    }

    public void OnJoinCreateClick()
    {
        string name = playerNameInput.text;
        string room = roomNameInput.text;
        SetNickname(playerNameInput.text);
        //join a specific room
        roomTry = room;
        PhotonNetwork.JoinRoom(room);
        //OnJoinRoomFailed will handle errors
    }

    public void OnJoinRandomClick()
    {
        SetNickname(playerNameInput.text);
        PhotonNetwork.JoinRandomRoom();
    }
    
    public void ConnectCloud()
    {
        //check if connected to the cloud. If connected, join specific loby.
        //If not, connect to cloud.
        if (PhotonNetwork.IsConnected)
        {
            return;
        }
        else
        {
            connectControls.SetActive(false);
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }
    }

    public bool Join(string room)
    {
        if(PhotonNetwork.IsConnected){
            PhotonNetwork.JoinRoom(room);
            return true;
        }
        return false;
    }

    #region PunCallbacks
    public override void OnConnectedToMaster()
    {
        //base.OnConnectedToMaster();
        Debug.Log("Connected to a master.");
        //display room controls
        joinControls.SetActive(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        //base.OnDisconnected(cause);
        Debug.Log("Disconnected");
        connectControls.SetActive(true);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to create room");
        base.OnCreateRoomFailed(returnCode, message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Room {roomTry} did not exist, creating one.");
        PhotonNetwork.CreateRoom(roomTry);
    }
    public override void OnJoinedRoom()
    {
        //base.OnJoinedRoom();
        Debug.Log("On Joined Room");
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            //we are the first player, load the scene
            PhotonNetwork.LoadLevel("DefaultMap");
        }
    }
    #endregion

}
