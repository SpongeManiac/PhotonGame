using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public bool team;
    public float spread = 0.3f;
    public float force = 300f;
    //create timer to destroy bullet

    private void Start()
    {
        //add force to bullet
        GetComponent<Rigidbody>().AddForce(transform.forward * force, ForceMode.Impulse);

        StartCoroutine(Timer(10f));
    }

    private IEnumerator Timer(float x)
    {
        yield return new WaitForSeconds(x);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Looking for Team{(team? 1:2)}");
        Debug.Log($"hit {collision.gameObject} with tag {collision.gameObject.tag}");
        if (collision.gameObject.tag == $"Team{(team? 1:2)}")
        {
            Debug.Log("bullet hit player");
            PlayerManager player = collision.gameObject.GetComponent<PlayerManager>();
            player.health -= 10;
        }
        Destroy(gameObject);
    }
}
