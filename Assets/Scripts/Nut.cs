using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nut : MonoBehaviour
{
    float z;
    Transform _transformNut;
    public int nutValue = 1;

    void Awake()
    {
        _transformNut = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {

        z += Time.deltaTime * 50;
        _transformNut.rotation = Quaternion.Euler(0, z, 0);

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ScoreManager.instance.ChangeScore(nutValue);
        }
    }
}
