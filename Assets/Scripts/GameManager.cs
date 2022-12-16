using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextAsset textJSON;

    public string json;

    //это объекты, хотя только один из них объект, но nah
    [Header("Objects")]
    public Conductor conductor;
    private Animator anim;
    public GameObject comment;
    private Animator commentButton;
    private TMP_Text commentText;
    public Camera mainCamera;
    public PlayerScript playerScript;

    //Штучки, которые нужны для работы игры. Хотя тут всё нужно для игры...
    [Header("Game Things")]
    public AudioClip[] musicList = new AudioClip[3];
    public string[] jumpList = new string[8];
    public BoxScript[] boxes;
    public Transform stars;
    public GameObject starPrefab;
    public List<List<StarScript>> starList = new List<List<StarScript>>{new List<StarScript>{}, new List<StarScript>{}, new List<StarScript>{}, new List<StarScript>{}, new List<StarScript>{}};
    private List<StarScript> allStarList = new List<StarScript>();
    public int starNumber = 1;
    public float mainHeight = 0;
    public Transform parent;
    public int currentBox;
    public List<double> checkList = new List<double>{};
    public float score = 0;
    public float totalInputs;
    public List<List<float>> inputList = new List<List<float>>();
    public bool canInput;
    public SkillStarScript skillStar; //147
    public bool canCapture;
    public BallonnScript[] ballonns = new BallonnScript[7];
    public GameObject balloonPrefab;
    public Sprite[] balloonSprites = new Sprite[6];
    public SpriteRenderer black;
    private double lastCheckedBeat = -3.141;

    
    //один единственный звук
    [Header("Sounds")]
    public AudioManager audioManager;

    //то, что берётся из json файла
    public string levelcreator;
    public string songcreator;
    public float end; //189
    public List<float> jumps;
    public List<RandomList> random;
    public float skillstar;
    public GameData loadedGameData;
    public StatesList myStatesList = new StatesList();
    public JumpsList jumpsTypes = new JumpsList();

    //это вообще не используется
    [Header("Other")]
    public bool autoPlay = true;
    public bool mainGame = false;
    public bool stopInput;
    public bool checkImmediately;

    //Я сказала стартуем!
    private void Awake()
    {
        if (mainGame)
            LoadDataNonPath(textJSON.text);
    }
    private void Start()
    {
        PlayerPrefs.DeleteKey("Score"); PlayerPrefs.DeleteKey("Rank0"); PlayerPrefs.DeleteKey("Rank1");
        AddRandom();
        //Debug.Log(starList.Count);
        for(int i=0; i<30; i++)
        {
            GameObject star = Instantiate(starPrefab, new Vector3(Random.Range(-8.94f, 8.94f), Random.Range(-5f, 5f), 0), Quaternion.identity, stars);
            starList[0].Add(star.GetComponent<StarScript>()); //Play Yan: (239, 417), ballons border: (209, 282, 244, 332)
            allStarList.Add(star.GetComponent<StarScript>()); //Play Yan: (0, 0), ballons border: (-0.12, 1.18, 1.38, 2.68)[local]
        }
        for(int i=0; i<7; i++) 
        {
            ballonns[i] = Instantiate(balloonPrefab, new Vector3(Random.Range(-3.27f, -2.47f), Random.Range(0.4f, 1.6f)), Quaternion.identity, playerScript.transform).GetComponent<BallonnScript>();
            ballonns[i].spriteRenderer.sortingOrder = 10-i;
            ballonns[i].spriteRenderer.sprite = balloonSprites[i%5];
        }
        GameObject[] tmpList = GameObject.FindGameObjectsWithTag("Box");
        boxes = new BoxScript[tmpList.Length];
        commentText = comment.GetComponentInChildren<TMP_Text>();
        commentButton = comment.GetComponentInChildren<Animator>();
        //Debug.Log(commentText);
        //Debug.Log(commentButton);
        for(int i=0; i<tmpList.Length; i++)
        {
            boxes[i] = tmpList[i].GetComponent<BoxScript>();
        }
        boxes = boxes.OrderBy(go=>go.name).ToArray();
        parent = boxes[0].GetComponentInParent<SpriteMask>().gameObject.transform;
        //Debug.Log(ToLowercaseNamingConvention("JumpFlowerLollipopUmbrella4", false));
        //Это не работает без спрайта на game manager
        anim = gameObject.GetComponent<Animator>();
        anim.SetFloat("Speed", conductor.bpm/120);
        playerScript.anim.SetFloat("BPMSpeed", conductor.bpm/120);
        totalInputs = jumpsTypes.jumpstypes[jumpsTypes.jumpstypes.Length-3].beat - jumpsTypes.jumpstypes[0].beat;
        StartCoroutine(FadeImage(black, false));
    }

    //загружаем всё из json
    public void LoadDataNonPath(string text)
    {
        loadedGameData = JsonUtility.FromJson<GameData>(text);
        myStatesList = JsonUtility.FromJson<StatesList>(text);
        jumpsTypes = JsonUtility.FromJson<JumpsList>(text);

        conductor.bpm = loadedGameData.bpm;
        conductor.beatParts = loadedGameData.beatparts;
        levelcreator = loadedGameData.levelcreator;
        songcreator = loadedGameData.songcreator;
        end = loadedGameData.end;
        jumps = loadedGameData.jumps;
        random = loadedGameData.random;
        skillstar = loadedGameData.skillstar;
        for(int i=0; i<myStatesList.states.Count; i++)
            checkList.Add(myStatesList.states[i].beat);
        for(int j=0; j<jumpsTypes.jumpstypes.Length; j++)
        {
            if(!checkList.Contains(jumpsTypes.jumpstypes[j].beat-8.75))
                checkList.Add(jumpsTypes.jumpstypes[j].beat-8.75);
        }
    }

    private void Update()
    {
        //если бит кратен 6, то можно чё-нить сделать
        if (conductor.onBeat||checkImmediately)
        {
            // if (conductor.songPosBeat == 0.66667) Debug.Log("Кротовуха");

            if(checkImmediately)
                conductor.songPosBeat = 0;

            if(checkList.Contains(conductor.songPosBeat)&&conductor.songPosBeat!=lastCheckedBeat)
            {
                lastCheckedBeat = conductor.songPosBeat;
                for (int i = 0; i < jumpsTypes.jumpstypes.Length; i++)
                    {
                        //этот перебор нужен, так как в states у нас 2 параметра
                        if (jumpsTypes.jumpstypes[i].beat == lastCheckedBeat+8.75)
                        {
                            string[] jumpState = ToLowercaseNamingConvention(jumpsTypes.jumpstypes[i].state, false).Split("_");
                            int step = int.Parse(jumpState[jumpState.Length-1]);
                            int n = 0;
                            while(n<8)
                            {
                                if(jumpState.Length-2==1)
                                    jumpList[n] = jumpState[1];
                                else if(jumpState.Length-2==2)
                                {
                                    if(n%step==step-1)
                                        jumpList[n] = jumpState[2];
                                    else
                                        jumpList[n] = jumpState[1];
                                }
                                else
                                {
                                    if(n%step==step-1)
                                        jumpList[n] = jumpState[3];
                                    else if(n%step==step/2-1)
                                        jumpList[n] = jumpState[2];
                                    
                                    else
                                        jumpList[n] = jumpState[1];
                                }
                                //Debug.Log(n);
                                //Debug.Log(jumpList[n]);
                                n++;
                            }
                            
                        }
                    }
                CheckState();
            }

            if (conductor.songPosBeat == end)
            {
                PlayerPrefs.SetInt("Rank0", 1);
                StartCoroutine(End());
                return;
            }

            if(conductor.songPosBeat==skillstar-3)
                StartCoroutine(skillStar.SkillStarPrepare(conductor.songPosBeat));
            
            if(canInput&&!stopInput)
                if(autoPlay&&conductor.songPosBeat==inputList[0][0])
                    CheckInput(boxes[currentBox].beat);
            if(canInput&&!stopInput)
            {
                if(inputList.Count>0)
                {
                    if(conductor.songPosBeat==inputList[0][0])
                    {
                        if(boxes[currentBox].anim.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
                            StartCoroutine(boxes[currentBox].Miss());
                    }
                    if(conductor.songPosBeat==inputList[0][0]+0.25f)
                    {
                        inputList.RemoveAt(0);
                        if(boxes[currentBox].isJump)
                        {
                            StopCoroutine("Miss");
                            StartCoroutine(Fall((float)conductor.songPosBeat));
                        }
                    }
                }
                else
                    canInput = false;

            }
            else
                if(inputList.Count>0)
                    canInput = true;
            if(checkImmediately)
                checkImmediately = false;
        }

        if(Input.GetButtonDown("Fire1"))
        {
            if(canInput&&!stopInput)
                if(conductor.beatPosition>inputList[0][0]-0.3f)
                    CheckInput(conductor.beatPosition);
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            autoPlay = !autoPlay;
        }

        if(Input.GetKeyDown(KeyCode.Keypad1))
        {
            PlayerPrefs.SetInt("Score", (int)Mathf.Floor(30));
            PlayerPrefs.SetInt("Rank0", -1);
            PlayerPrefs.SetInt("Rank1", -1);
            SceneManager.LoadScene("ResultScreen");
        }

        if(Input.GetKeyDown(KeyCode.Keypad2))
        {
            PlayerPrefs.SetInt("Score", (int)Mathf.Floor(65));
            PlayerPrefs.SetInt("Rank0", -1);
            PlayerPrefs.SetInt("Rank1", -1);
            SceneManager.LoadScene("ResultScreen");
        }

        if(Input.GetKeyDown(KeyCode.Keypad3))
        {
            PlayerPrefs.SetInt("Score", (int)Mathf.Floor(69));
            PlayerPrefs.SetInt("Rank0", 0);
            PlayerPrefs.SetInt("Rank1", 1);
            SceneManager.LoadScene("ResultScreen");
        }

        if(Input.GetKeyDown(KeyCode.Keypad4))
        {
            PlayerPrefs.SetInt("Score", (int)Mathf.Floor(90));
            PlayerPrefs.SetInt("Rank0", 1);
            PlayerPrefs.SetInt("Rank1", 1);
            SceneManager.LoadScene("ResultScreen");
        }
    }

    private void CheckState()
    {
        for (int i = 0; i < myStatesList.states.Count; i++)
                    {
                        string animState = myStatesList.states[i].state;

                        //этот перебор нужен, так как в states у нас 2 параметра
                        if (myStatesList.states[i].beat == lastCheckedBeat)
                        {
                            //Debug.Log(animState);
                            if(animState=="CanInput")
                            {
                                if(myStatesList.states[i].extra[0]=="true")
                                    stopInput = false;
                                else
                                    stopInput = true;
                            }
                            if(animState=="CountIn")
                            {
                                StopCoroutine("CountIn");
                                StartCoroutine(CountIn((float)lastCheckedBeat));
                            }
                            if(animState=="Comment")
                            {
                                StartCoroutine(ShowComment((float)lastCheckedBeat, myStatesList.states[i].extra));
                            }
                            if(animState=="Dialog")
                            {
                                StartCoroutine(Dialog(myStatesList.states[i].extra, i));
                            }
                            if(animState=="StartImmediately")
                            {
                                StartCoroutine(Dialog(myStatesList.states[i].extra, i));
                                animState="StartMoving";
                            }
                            if(animState=="StartMoving")
                            {
                                for(int j=0; j<boxes.Length; j++)
                                {
                                    boxes[j].StartMoving(int.Parse(myStatesList.states[i].extra[0]));
                                }
                            }
                            if(animState=="BoxesDissapper")
                            {
                                StartCoroutine(BoxesDisappear((float)lastCheckedBeat));
                            }
                            if(animState=="PlayMusic")
                            {
                                StartCoroutine(SetMusic(myStatesList.states[i].extra));
                            }
                            if(animState=="WaitUntil")
                            {
                                StartCoroutine(WaitUntil(myStatesList.states[i].extra, (float)lastCheckedBeat, i));
                            }
                            if(animState=="WaitScore")
                            {
                                StartCoroutine(WaitScore(myStatesList.states[i].extra, i));
                            }
                            if(animState=="Ending")
                            {
                                StartCoroutine(End(myStatesList.states[i].extra[0]));
                            }
                        }

                        if(animState.Substring(0, 4)=="Wait")
                            return;
                    }
    }

    public string ShittyJumpSystemTM(float beat, float input) //[absolutely] FREE!!!!1!!1!
    {
        float diff = beat-input;
        float absDiff = Mathf.Abs(diff);
        float isNegative = Mathf.Sign(diff);
        score += 1-1/26f*absDiff;
        if(absDiff<=0.05)
            return "Ace";
        if(absDiff>0.05&&absDiff<=0.15)
            return "OK";
        if(absDiff>0.15&&absDiff<=0.2)
        {
            return "Nearly";
        }
        if(absDiff>0.2&&absDiff<=0.25)
        {
            if(isNegative>0)
                return "BarelyE";
            if(isNegative<0)
                return "BarelyL";
        }
        return "Nah";
    }

    private void CheckInput(float input)
    {
        inputList.RemoveAt(0);
        if(inputList.Count==0)
            canInput = false;
        string jump = ShittyJumpSystemTM(boxes[currentBox].beat, input);
        //Debug.Log(jump);
        playerScript.Jump();
        if(jump=="Ace")
        {
            boxes[currentBox].Jump();
            if(canCapture)
                skillStar.SkillStarCapture();
            return;
        }
        if(jump=="OK")
        {
            boxes[currentBox].Jump();
            return;
        }
        if(jump=="Nearly")
        {
            boxes[currentBox].Jump();
            StopCoroutine("SkillStarPrepare");
            canCapture = false;
            return;
        }
        boxes[currentBox].Barely();
        playerScript.Jump();
    }
    
    private IEnumerator CountIn(float beat) //my code really lacks of comments
    {
        for(int i=0; i<2; i++)
        {
            audioManager.Play("cowbell");
            StartCoroutine(ballonns[i].Blow());
            beat+=2;
            yield return new WaitUntil(() => conductor.songPosBeat==beat);
        }
        for(int i=0; i<4; i++)
        {
            audioManager.Play("cowbell");
            StartCoroutine(ballonns[i+2].Blow());
            beat++;
            yield return new WaitUntil(() => conductor.songPosBeat==beat);
        }
        yield return new WaitUntil(() => conductor.beatPosition<=beat+0.7);
        StartCoroutine(ballonns[6].Blow());
        if(playerScript.anim.GetCurrentAnimatorStateInfo(0).IsName("fly"))
            playerScript.anim.Play("Walking");
    }

    private IEnumerator ShowComment(float beat, List<string> extra)
    {
        comment.SetActive(true);
        commentText.text = extra[0];
        beat += int.Parse(extra[1]);
        yield return new WaitUntil(() => conductor.songPosBeat==beat);
        comment.SetActive(false);
    }

    private IEnumerator Dialog(List<string> extra, int place)
    {
        for(int i=0; i<extra.Count-1; i++)
        {
            comment.SetActive(true);
            commentText.text = extra[i];
            yield return new WaitForSeconds(0.3f);
            yield return StartCoroutine(Wait());
            yield return new WaitForSeconds(0.3f);
            comment.SetActive(false);
        }
        int deleteNum = int.Parse(extra[extra.Count-1]);
        for(int h=0; h<deleteNum; h++)
        {
            DeleteStates(place-deleteNum+2);
        }
        checkImmediately = true;
    }

    private IEnumerator WaitScore(List<string> extra, int place)
    {
        int scoreToReach = int.Parse(extra[0]);
        yield return new WaitUntil(() => score>=scoreToReach);
        int deleteNum = int.Parse(extra[1]);
        for(int h=0; h<deleteNum; h++)
        {
            DeleteStates(place-deleteNum+1);
        }
        checkImmediately = true;
    }

    private IEnumerator WaitUntil(List<string> extra, float beat, int place)
    {
        Debug.Log("Waiting Until");
        beat += int.Parse(extra[0]);
        yield return new WaitUntil(() => conductor.songPosBeat==beat);
        Debug.Log("Waited Until successfully");
        int deleteNum = int.Parse(extra[1]);
        for(int h=0; h<deleteNum; h++)
        {
            DeleteStates(place-deleteNum+1);
        }
        checkImmediately = true;
    }

    private void DeleteStates(int num)
    {
        myStatesList.states.RemoveAt(num);
    }

    private IEnumerator Wait()
    {
        commentButton.Play("WaitUntilPresssed");
        yield return new WaitUntil(() => Input.GetButtonDown("Fire1"));
        audioManager.Play("textAdvance1", 200f);
        commentButton.Play("ButtonPressed");
        yield return new WaitUntil(() => Input.GetButtonUp("Fire1"));
        audioManager.Play("textAdvance2", 200f);
        commentButton.Play("ButtonPull");
    }

    private IEnumerator SetMusic(List<string> extra)
    {
        if(extra[1]=="true")
        {
            for(int i=0; i<30; i++)
            {
                conductor.musicSource.volume = 1-1f/30*i;
                yield return null;
            }
        }
        conductor.musicSource.Stop();
        conductor.loopNumber = -1;
        if(extra[2]=="true")
        {
            conductor.musicSource.loop = true;
            conductor.loopBeat = int.Parse(extra[3]);
        }
        else
            conductor.musicSource.loop = false;
        conductor.musicSource.volume = 1;
        conductor.musicSource.clip = musicList[int.Parse(extra[0])];
        conductor.musicSource.Play();
    }

    public string ToLowercaseNamingConvention(string s, bool toLowercase)
    {
        var r = new Regex(@"
        (?<=[A-Z])(?=[A-Z][a-z]) |
        (?<=[^A-Z])(?=[A-Z]) |
        (?<=[A-Za-z])(?=[^A-Za-z])", RegexOptions.IgnorePatternWhitespace);
        if (toLowercase)
            return r.Replace(s, "_").ToLower();
        else
            return r.Replace(s, "_");
    }

    private void AddRandom()
    {
        //Debug.Log(random.Count);
        foreach(RandomList lst in random)
        {
            for(float i=lst.list[0]; i<lst.list[1]; i++)
            {
                if(Random.value<=1/4f)
                    jumps.Add(i);
            }
        }
        
    }

    public void StarAdvance()
    {
        int len = starList[starNumber-1].Count;
        StarScript star = starList[starNumber-1][Random.Range(0, len-1)];
        star.state++;
        star.anim.Play("Star"+starNumber.ToString()+"to"+(starNumber+1).ToString());
        starList[starNumber-1].Remove(star);
        starList[starNumber].Add(star);
        if(len==1)
            starNumber++;
    }

    public void StarDestroy()
    {
        for(int i=0; i<allStarList.Count; i++)
        {
            allStarList[i].anim.Play("StarDestroy");
        }
    }

    public IEnumerator BigJump()
    {
        float time = conductor.secPerRealBeat*0.1f/15;
        float speed = 1.2f/15;
        for(int i=0; i<15; i++)
        {
            parent.Translate(0, -speed, 0);
            for(int j=0; j<allStarList.Count; j++)
            {
                allStarList[j].transform.Translate(0, -speed, 0);
                if(allStarList[j].transform.localPosition.y<=-5f-allStarList[j].render.sprite.textureRect.height*4/100)
                {
                    allStarList[j].transform.Translate(0,10f+allStarList[j].render.sprite.textureRect.height*8/100,0);
                    allStarList[j].transform.localPosition = new Vector3(Random.Range(-8.94f, 8.94f), allStarList[j].transform.localPosition.y, 0);
                }
            }
            yield return new WaitForSeconds(time);
        }
    }

    public IEnumerator BoxesDisappear(float beat)
    {
        for(int i=0; i<8; i++)
            boxes[i].isMoving = false;
        int num = currentBox-3;
        if(num<0)
            num+=8;
        //Debug.Log(num.ToString()+"/"+beat.ToString());
        for(int i=0; i<8; i++)
        {
            if(!boxes[num].gameObject.activeSelf)
            {
                //Debug.Log(boxes[num].gameObject.activeSelf);
                yield break;
            }
            audioManager.Play("boxDisappear");
            StartCoroutine(boxes[num].PlatformDisappear());
            num++;
            if(num>=8)
                num-=8;
            beat++;
            //Debug.Log(num.ToString()+"/"+beat.ToString());
            yield return new WaitUntil(() => conductor.songPosBeat==beat);
        }
    }

    private IEnumerator SHUTUP()
    {
        double beat = conductor.songPosBeat+2;
        float speed = conductor.musicSource.volume/conductor.secPerRealBeat/2;
        while(conductor.songPosBeat!=beat)
        {
            conductor.musicSource.volume -= speed*Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator Fall(float beat)
    {
        audioManager.Play("missVoice");
        inputList.Clear();
        canInput = false;
        playerScript.transform.position = new Vector3(playerScript.transform.position.x, -3.3f, 0);
        playerScript.anim.Play("HangStart");
        for(int i=0; i<8; i++)
            boxes[i].isMoving = false;
        yield return new WaitUntil(() => conductor.songPosBeat==beat+1);
        StartCoroutine(BoxesDisappear(beat+1f));
        yield return new WaitUntil(() => conductor.songPosBeat==beat+2);
        StartCoroutine(SHUTUP());
        StartCoroutine(playerScript.Fall());
        yield return new WaitUntil(() => conductor.songPosBeat==beat+8);
        PlayerPrefs.SetInt("Rank0", -1);
        totalInputs *= 1000;
        StartCoroutine(End());
    }

    private IEnumerator End(string nextScene = "ResultScreen")
    {
        PlayerPrefs.SetInt("Score", (int)Mathf.Floor(score/totalInputs*100));
        if(starNumber==5)
            PlayerPrefs.SetInt("Rank1", 1);
        else
            PlayerPrefs.SetInt("Rank1", 0);
        StartCoroutine(FadeImage(black));
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(nextScene);
    }

    IEnumerator FadeImage(SpriteRenderer img, bool fadeAway = true, float seconds = 1)
    {
        if (fadeAway)
        {
            for (float i=0; i<=60*seconds; i++)
            {
                img.color = new Color(0, 0, 0, 1f/(60*seconds)*i);
                yield return null;
            }
        }
        else
        {
            for (float i=60*seconds; i>=0; i--)
            {
                img.color = new Color(0, 0, 0, 1/(60*seconds)*i);
                yield return null;
            }
        }
    }

    //это нужно для поиска данных из json
    [System.Serializable]
    public class GameData
    {
        public string levelcreator;
        public string songcreator;
        public float bpm;
        public int beatparts;
        public float end;
        public List<float> jumps;
        public List<RandomList> random;
        public int skillstar;
    }

    [System.Serializable]
    public class StatesJSON
    {
        public float beat;
        public string state;
        public List<string> extra;
    }

    [System.Serializable]
    public class StatesList
    {
        public List<StatesJSON> states;
    }

    [System.Serializable]
    public class JumpsJSON
    {
        public float beat;
        public string state;
    }

    [System.Serializable]
    public class JumpsList
    {
        public JumpsJSON[] jumpstypes;
    }

    [System.Serializable]
    public class RandomList
    {
        public List<float> list;
    }
    ///----------------------------------------------------------------------------------------------
    ///
}
