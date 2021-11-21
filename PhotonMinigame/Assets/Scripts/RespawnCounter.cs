using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RespawnCounter : MonoBehaviour
{
    Coroutine countdown;
    public Text respawnCounterText;
    int _count = 3;
    public int count
    {
        get => _count;
        set
        {
            if (value <= 0)
            {
                value = 0;
                StopCoroutine(countdown);
                Respawn();
            }
            _count = value;
            respawnCounterText.text = $"Respawning in {_count}...";
        }
    }

    public void StartCountdown()
    {
        _count = 3;
        countdown = StartCoroutine(CountDown());
    }

    IEnumerator CountDown()
    {
        while (count > 0)
        {
            yield return new WaitForSeconds(1f);
            count--;
        }
    }

    void Respawn()
    {
        //respawn player
        GameManager.spawnManager.Respawn();
    }
}
