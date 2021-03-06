using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public GameObject team1Prefab;
    public GameObject team2Prefab;

    public Transform Center;

    public List<Transform> team1spawns;
    public List<Transform> team2spawns;

    public bool team = false;

    //spawn timers
    public Dictionary<Transform, bool> spawnTimers = new Dictionary<Transform, bool>();

    private void Awake()
    {
        //init dictionary
        foreach(var transform in team1spawns) {
            
            spawnTimers[transform] = false;
            //point spawn toward center of map
            transform.LookAt(Center);
        }
        foreach (var transform in team2spawns) {
            spawnTimers[transform] = false;
            transform.LookAt(Center);

        }
    }

    public void StartSpawnNew()
    {
        StartCoroutine(CheckSpawnNew(1f));
    }

    public void StartSpawn()
    {
        StartCoroutine(CheckSpawn());
    }

    bool SpawnNew()
    {
        Debug.Log("Spawning New Player");
        var prefab = team ? team1Prefab : team2Prefab;
        var list = team ? team1spawns : team2spawns;
        
        //get random spawn for team
        int spawnNum = Random.Range(0, list.Count);
        //check if spawn has a cooldown
        if (spawnTimers[list[spawnNum]])
        {
            //spawn is not available
            Debug.Log($"Spawn not available. {spawnTimers[list[spawnNum]]}");
            return false;
        }
        else
        {
            //spawn is available, spawn user here
            GameManager.LocalPlayerInstance = PhotonNetwork.Instantiate(prefab.name,
                list[spawnNum].position, list[spawnNum].rotation);
            team = !team;
            return true;
        }
        
    }

    public bool Respawn()
    {
        if (GameManager.LocalPlayerInstance == null)
        {
            return false;
        }
        GameObject player = GameManager.LocalPlayerInstance;
        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        var list = playerManager.team ? team1spawns : team2spawns;
        //get random spawn for team
        int spawnNum = Random.Range(0, list.Count);
        //check if spawn has a cooldown
        if (spawnTimers[list[spawnNum]])
        {
            //spawn is not available
            return false;
        }
        else
        {
            //reset player's plane
            player.transform.position = list[spawnNum].position;
            player.transform.rotation = list[spawnNum].rotation;
            playerManager.dead = false;
            return true;
        }

    }

    public IEnumerator CheckSpawn()
    {
        bool spawned = false;
        while (!spawned)
        {
            yield return new WaitForSeconds(1f);
            spawned = Respawn();
            Debug.Log("Checking spawn.... ");
            Debug.Log($"{spawned}");

        }
    }

    public IEnumerator CheckSpawnNew(float interval)
    {
        bool spawned = false;
        while (!spawned && GameManager.LocalPlayerInstance == null)
        {
            yield return new WaitForSeconds(interval);

            //check if can spawn
            if (SpawnNew())
            {
                spawned = true;
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(team);
        }
        else
        {
            team = (bool)stream.ReceiveNext();
        }
    }
}
