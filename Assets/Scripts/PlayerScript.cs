using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public Animator anim;
    private GameManager gameManager;
    private Conductor conductor; 
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        gameManager = GameObject.FindObjectOfType<GameManager>();
        conductor = GameObject.FindObjectOfType<Conductor>();
    }

    public void Jump()
    {
        anim.Play("Jump");
    }
    public IEnumerator Fall()
    {
        gameManager.audioManager.Play("missFall");
        double beat = conductor.songPosBeat+2;
        float speed = (-5-transform.position.y)/conductor.secPerRealBeat;
        while(conductor.songPosBeat!=beat)
        {
            transform.Translate(new Vector3(0, speed*Time.deltaTime, 0));
            yield return null;
        }
    }

    public IEnumerator FlyEnd()
    {
        anim.Play("flyEnd");
        double beat = conductor.songPosBeat+11;
        float speed = (5-transform.position.y)/conductor.secPerRealBeat/9;
        while(conductor.songPosBeat!=beat)
        {
            transform.Translate(new Vector3(0, speed*Time.deltaTime, 0));
            yield return null;
        }
    }
    
}
