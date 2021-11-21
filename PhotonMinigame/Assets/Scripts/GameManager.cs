using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
<<<<<<< Updated upstream
    public SpawnManager spawner;
=======
    public SpawnManager spwnManager;
    public SkyboxManager skyboxManager;
    public GameObject escapeMenu;
    public GameObject dethMenu;

    public static SpawnManager spawnManager;
    public static GameObject escMenu;
    public static GameObject deathMenu;
    public static GameObject LocalPlayerInstance;

    public void Awake()
    {
        spawnManager = spwnManager;
        escMenu = escapeMenu;
        deathMenu = dethMenu;
    }
>>>>>>> Stashed changes

    public void Start()
    {
        if (PlayerManager.LocalPlayerInstance == null)
        {
            spawner.StartSpawnNew();
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
