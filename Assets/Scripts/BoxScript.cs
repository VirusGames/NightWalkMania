using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxScript : MonoBehaviour
{
    public string sound = "Flower";
    public bool isJump;
    private GameManager gameManager;
    private Conductor conductor; 
    public bool isMoving;
    public float speed; //22, 4.7
    public float beat;
    public int pos;
    public Animator anim;
    public GameObject platform;
    public GameObject lightsPrefab;
    private bool hasAdvanced;
    private ParticleSystem lights;


    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        gameManager = GameObject.FindObjectOfType<GameManager>();
        conductor = GameObject.FindObjectOfType<Conductor>();
            transform.Translate(22,0,0);
        //transform.Translate(-0.8f, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving)
        {
            speed = -2.75f/conductor.secPerRealBeat;
            transform.Translate(speed*Time.deltaTime,0,0);
            if(transform.localPosition.x<=-2.75f)
            {
                hasAdvanced = false;
                if(sound!="End")
                    anim.Play("Idle");
                else
                {
                    anim.Play("Ready");
                    lights = Instantiate(lightsPrefab, transform).GetComponent<ParticleSystem>();
                    //lights.velocityOverLifetime.speedModifierMultiplier = conductor.bpm/120f;
                    var kolin = lights.velocityOverLifetime;
                    kolin.speedModifierMultiplier = conductor.bpm/120f;
                    //lights.velocityOverLifetime = velocityOverLifetime;
                }
                transform.localPosition = new Vector3(transform.localPosition.x+22, gameManager.mainHeight, 0);
                if(gameManager.jumps.Contains(beat))
                {
                    platform.SetActive(false);
                    gameManager.mainHeight+=1.2f;
                    isJump = true;
                }
                else
                    platform.SetActive(true);
                if(sound=="Stop")
                    gameObject.SetActive(false);
            }
            /*if (conductor.onBeat)
            {
                if(conductor.songPosBeat==beat-0.5&&sound!="None")
                {
                    StartCoroutine(InputCheck());
                    //Jump();
                    //Debug.Log(transform.position);
                }
            }*/
        }       
    }

    public void Jump()
    {
        if(sound=="None")
            return;
        if(sound=="End")
        {
            GameObject.Destroy(lights);
            gameManager.stopInput = true;
            anim.Play("End");
            gameManager.audioManager.Play("umbrella");
            foreach(BoxScript box in gameManager.boxes)
                box.isMoving = false;
            gameManager.StarDestroy();
            StartCoroutine(gameManager.playerScript.FlyEnd());            
        }
        else
        {
            anim.Play(sound);
            gameManager.audioManager.Play(sound.ToLower());
            if(gameManager.starNumber<5)
                gameManager.StarAdvance();
            if(isJump)
            {
                StartCoroutine(gameManager.BigJump());
                isJump = false;
            }
        }
        Advance();
    }

    public void StartMoving(int offset = 0)
    {
        isMoving = true;
        for(int j=0; j<gameManager.boxes.Length; j++)
        {
            if(gameManager.boxes[j].transform==transform)
            {
                pos = j;
                beat = (float)conductor.songPosBeat+j+6-offset;
                sound = gameManager.jumpList[pos%8];
                if(gameManager.jumps.Contains(beat))
                {
                    platform.SetActive(false);
                    gameManager.mainHeight+=1.2f;
                    isJump = true;
                }
                else
                    platform.SetActive(true);
                gameManager.inputList.Add(new List<float>{beat});
            }
        }
    }

    public IEnumerator Miss()
    {
        anim.Play("MissPrepare");
        gameManager.audioManager.Play("kick");
        yield return new WaitUntil(() => conductor.beatPosition>=beat+0.5f);
        if(!anim.GetCurrentAnimatorStateInfo(0).IsName("MissPrepare"))
            yield break;
        anim.Play("Miss");
        gameManager.audioManager.Play(sound.ToLower()+"Miss");
        Advance();
    }

    public void Barely()
    {
        if(sound=="Flower")
            gameManager.audioManager.Play("flowerBarely");
        else
            gameManager.audioManager.Play("otherBarely");
        anim.Play(sound+"NG");
        if(isJump)
        {
            StartCoroutine(gameManager.BigJump());
            isJump = false;
        }
        Advance();
    }

    private void Advance()
    {
        if(!hasAdvanced)
        {
            sound = gameManager.jumpList[pos%8];
            beat += 8;
            gameManager.inputList.Add(new List<float>{beat});
            gameManager.currentBox++;
            if(gameManager.currentBox>=8)
                gameManager.currentBox-=8;
            hasAdvanced = true;
        }
    }

    public IEnumerator PlatformDisappear()
    {
        anim.Play("Disappear");
        for(int i=0; i<10; i++)
            yield return new WaitForEndOfFrame();
        platform.SetActive(false);
    }
}
