using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public SpawnManager spawner;
    public Material[] skybox;
    public PhotonView photonView;

    public void Start()
    {
        photonView = GetComponent<PhotonView>();
        if (PlayerManager.LocalPlayerInstance == null)
        {
            spawner.StartSpawnNew();
        }
        photonView.RPC("NewSkyBox", RpcTarget.AllViaServer, 0);

    }

    public void NewSkyBox()
    {
        RenderSettings.skybox = skybox[Random.Range(0, 5)];
    }

    public override void OnLeftRoom()
    {
        //loads launcher when player leaves room
        SceneManager.LoadScene(0);
    }

    public void LeaveRoom()
    {
        //disconnects player
        PhotonNetwork.LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} with id {newPlayer.UserId} joined.");

        //base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master Client joined.");
        }
        RenderSettings.skybox = selectedSkybox;


    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} with id {otherPlayer.UserId} left.");
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master Client left.");
        }
    }
}
