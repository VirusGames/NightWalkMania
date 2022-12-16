using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarScript : MonoBehaviour
{
    public int state = 1;
    public Animator anim;
    public SpriteRenderer render;
    public float speed;
    public Conductor conductor;

    private void Awake()
    {
        anim = gameObject.GetComponent<Animator>();
        render = gameObject.GetComponentInChildren<SpriteRenderer>();
        conductor = FindObjectOfType<Conductor>();
        anim.Play("Star1", 0, Random.Range(0, 28)/28f);
        
    }
    private void Start()
    {
        //Debug.Log(render.sprite.textureRect.height);
    }

    private void Update()
    {
        speed = -2.75f/conductor.secPerRealBeat/3;
        transform.Translate(speed*Time.deltaTime,0,0);
        if(transform.localPosition.x<=-8.94f-render.sprite.textureRect.width*5/100)
        {
            transform.Translate(17.88f+render.sprite.textureRect.width*10/100,0,0);
            transform.localPosition = new Vector3(transform.localPosition.x, Random.Range(-5f, 5f), 0);
        }
    }
}