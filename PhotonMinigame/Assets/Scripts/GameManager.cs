using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    public SpawnManager spawnManager;
    public SkyboxManager skyboxManager;
    public GameObject escapeMenu;

    public static GameObject escMenu;
    public static GameObject LocalPlayerInstance;

    public void Awake()
    {
        escMenu = escapeMenu;
    }

    public void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            skyboxManager.skybox = Random.Range(0, 5);
        }
        if (LocalPlayerInstance == null)
        {
            spawnManager.StartSpawnNew();
        }
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

    public void QuitGame()
    {
        Application.Quit();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.NickName} with id {newPlayer.UserId} joined.");

        //base.OnPlayerEnteredRoom(newPlayer);
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master Client joined.");
            
        }
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
