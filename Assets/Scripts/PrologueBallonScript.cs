using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrologueBallonScript : MonoBehaviour
{

    public SpriteRenderer spriteRenderer;
    public float speed;
    public Color colour;

    void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        speed = Random.Range(3f, 4f);
        //spriteRenderer.color = colour;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(0,speed*Time.deltaTime,0);
    }
}
