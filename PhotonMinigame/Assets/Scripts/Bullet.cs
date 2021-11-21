using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
<<<<<<< Updated upstream
    private void OnCollisionEnter(Collision collision)
    {
        Destroy(this.gameObject);
=======

    public bool team = false;

    public float spread = .01f;
    //create timer to destroy bullet

    private void Start()
    {
        if(tag == "Bullet2")
        {
            team = true;
        }
        var rigid = GetComponent<Rigidbody>();
        //Calculate spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        //Add force to bullet
        //Calculate new direction with spread;
        Vector3 directionWithSpread = transform.forward + new Vector3(x, y, 0);
        transform.forward = directionWithSpread.normalized;
        rigid.AddForce(directionWithSpread.normalized * 600f, ForceMode.Impulse);
        //rigid.AddForce(Camera.main.transform.up * upwardForce, ForceMode.Impulse);
        StartCoroutine(Timer(10f));
    }

    private IEnumerator Timer(float x)
    {
        yield return new WaitForSeconds(x);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Bullet hit something");
        if (collision.gameObject.tag == $"Team{(team? 1:2)}")
        {
            Debug.Log($"Hit player: {collision.gameObject}");
            //damage player
            //Debug.Log
            PlayerManager player = collision.gameObject.GetComponent<PlayerManager>();
            player.health -= 10;
        }

        Destroy(gameObject);

>>>>>>> Stashed changes
    }
}
