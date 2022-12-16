using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameSelect : MonoBehaviour
{
    public SpriteRenderer bg;
    public AudioSource musicSource;
    public AudioClip musicStart;
    public GameObject squarePrefab;
    public Transform squares;
    public RectTransform message;
    public SpriteRenderer black;
    public RawImage black2; //Or should I play White 2?
    public string type;
    public Vector2Int position;
    public List<string> tiles = new List<string>{"Options", "Game"};
    public string state;
    public Button BGBUtton;
    private float h;
    public AudioManager audioManager;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = GameObject.FindObjectOfType<AudioManager>();
        message.localScale = new Vector3(0, 0, 0);
        BGBUtton.gameObject.SetActive(false);
        musicSource.PlayOneShot(musicStart);
		musicSource.PlayScheduled(AudioSettings.dspTime + musicStart.length);
        for(int i=0; i<32; i++)
        {
            Instantiate(squarePrefab, new Vector3(10.22f-0.681f*i, Random.Range(-4.68f, 4.68f), 0), Quaternion.identity, squares);
        }
        state = "InMenu";
        StartCoroutine(SquareFactory());
    }

    // Update is called once per frame
    void Update()
    {
        bg.color = Color.HSVToRGB(h, 0.8f, 1);
        h += 1*Time.deltaTime/5;
        if(h>=1)
            h-=1;
        //if(Input.anyKeyDown)
        //    InputCheck();
    }

    private IEnumerator SquareFactory()
    {
        while(true)
        {
            Instantiate(squarePrefab, new Vector3(10.22f, Random.Range(-4.68f, 4.68f), 0), Quaternion.identity, squares);
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void InputCheck()
    {
        if(Input.GetAxis("Horizontal")>0)
        {
            position.x += 1;
            if(position.x>1)
                position.x -=2;
        }
        if(Input.GetAxis("Horizontal")<0)
        {
            position.x -= 1;
            if(position.x<0)
                position.x +=2;
        }
        if(Input.GetButtonDown("Fire1"))
        {
            position.x = 1;
            if(state=="InMenu")
                OpenList();
        }
        if(Input.GetButtonDown("Fire2"))
        {
            if(state=="Choising")
                CloseList();
        }
    }

    public IEnumerator OpenList()
    {
        audioManager.Play("select");
        BGBUtton.gameObject.SetActive(true);
        for(int i=0; i<=30; i++)
        {
            message.localScale = new Vector3(1f/30*i, 1f/30*i, 1);
            black.color = new Color(0, 0, 0, 0.5f/30*i);
            yield return null;
        }
        state = "Choising";
    }

    public IEnumerator CloseList()
    {
        audioManager.Play("cancel");
        BGBUtton.gameObject.SetActive(false);
        for(int i=30; i>=0; i--)
        {
            message.localScale = new Vector3(1f/30*i, 1f/30*i, 1);
            black.color = new Color(0, 0, 0, 0.5f/30*i);
            yield return null;
        }
        state = "InMenu";
    }

    public void StupidShit(string type)
    {
        if(type=="Open")
            StartCoroutine(OpenList());
        if(type=="Close")
            StartCoroutine(CloseList());
    }

    public void IWantToDie(string scene)
    {
        StartCoroutine(EnterGame(scene));
    }

    private IEnumerator EnterGame(string scene)
    {
        audioManager.Play("enterGame");
        PlayerPrefs.SetString("Scene", scene);
        black2.gameObject.SetActive(true);
        for(int i=0; i<=60; i++)
        {
            musicSource.volume = 100-100f/60*i;
            black2.color = new Color(0, 0, 0, 1f/60*i);

            yield return null;
        }
        SceneManager.LoadScene("Prologue");
    }

}
