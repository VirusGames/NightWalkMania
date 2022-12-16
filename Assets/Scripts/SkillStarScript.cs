using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillStarScript : MonoBehaviour
{
    private Conductor conductor;
    private GameManager gameManager;
    private SpriteRenderer spriteRenderer;
    public Sprite newStar;
    public Animator anim;


    void Start()
    {
        gameManager = GameObject.FindObjectOfType<GameManager>();
        conductor = GameObject.FindObjectOfType<Conductor>();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        anim = gameObject.GetComponent<Animator>();
    }
    public IEnumerator SkillStarPrepare(double beat)
    {
        for(int i=0; i<2; i++)
        {
            anim.Play("SkillBop", 0, 0);
            beat++;
            yield return new WaitUntil(() => conductor.songPosBeat==beat);
        }
        anim.Play("SkillBop", 0, 0);
        beat++;
        yield return new WaitUntil(() => conductor.songPosBeat==beat-0.5);
        gameManager.canCapture = true;
        yield return new WaitUntil(() => conductor.songPosBeat==beat+0.5);
        gameManager.canCapture = false;
    }

    public void SkillStarCapture()
    {
        anim.Play("SkillCapture", 0, 0);
        gameManager.audioManager.Play("skillStar");
    }
}
