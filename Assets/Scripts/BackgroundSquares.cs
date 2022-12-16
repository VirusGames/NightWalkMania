using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundSquares : MonoBehaviour
{
    public float speed;

    void Awake()
    {
        
    }

    void Start()
    {
        float newSize = Random.Range(1f, 4f);
        transform.localScale = new Vector3(newSize, newSize, 1);
        transform.Rotate(0, 0, Random.Range(0, 360));
        speed = newSize;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(-speed*Time.deltaTime, 0, 0, Space.World);
        if(transform.position.x<=-10.22f)
            GameObject.Destroy(gameObject);
        transform.Rotate(0, 0, Mathf.Pow(speed, 2)*15*Time.deltaTime);
    }
}
