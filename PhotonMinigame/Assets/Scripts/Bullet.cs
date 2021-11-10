using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    //create timer to destroy bullet

    private void Start()
    {
        StartCoroutine(Timer(10f));
    }

    private IEnumerator Timer(float x)
    {
        yield return new WaitForSeconds(x);
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
