using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScorManager : MonoBehaviour
{

    public int score;
    public string result;
    public Sprite[] screens = new Sprite[3];
    public AudioClip[] musicList = new AudioClip[10];
    public TMP_Text sayer;
    public TMP_Text ranks;
    public TMP_Text grade;
    public AudioManager audioManager;
    public TextAsset textJSON;
    private ResultText resultText;
    private int[] rankList;
    private List<string> okResults;
    public AudioSource fillSound;
    public SpriteRenderer fillout;
    public SpriteRenderer fill;
    public TMP_Text scoreText;
    public TMP_Text NHS;
    public TMP_Text just;
    public AudioSource musicSource;
    public AudioClip musicStart;
    public SpriteRenderer black;
    public SpriteRenderer picture;
    private TMP_Text epilogText;

    void Awake()
    {
        int rankNum = PlayerPrefs.GetInt("RankNum", 2);
        rankList = new int[rankNum];
        if(rankNum==0)
            rankList = new int[]{0};
        else            
            for(int i=0; i<rankNum; i++)
                rankList[i] = PlayerPrefs.GetInt("Rank"+i.ToString(), -1);
        score = PlayerPrefs.GetInt("Score", 30);
        if(score>=80)
            result = "superb";
        else if(score>=60)
            result = "ok";
        else
            result = "tryAgain";
        resultText = JsonUtility.FromJson<ResultText>(textJSON.text);
    }
    // Start is called before the first frame update
    void Start()
    {
        epilogText = picture.GetComponentInChildren<TMP_Text>();
        okResults = new List<string>{"Hm...", "I don't know...", "Good enough...", "I guess that was all right.", "For now that'll do."};
        sayer.text = resultText.sayer;
        string output = "";
        if(result=="tryAgain")
        {
            epilogText.text = resultText.tryagain[resultText.tryagain.Length-1];
            picture.sprite = screens[0];
            musicSource.clip = musicList[1];
            musicStart = musicList[0];
            grade.text = "Try Again";
			grade.fontSize = 3f;
            grade.color = new Color(0, 167f/255, 1, 1);
            for(int i=0; i<rankList.Length; i++)
            {
                if(rankList[i]==-1)
                {
                    if(output!=""&&resultText.tryagain[i]!="")
                        output+="\nAlso, ";
                    output += resultText.tryagain[i];
                }
            }
            if(output=="")
                output="That...could have been better.";
        }
        else if(result=="ok")
        {
            epilogText.text = resultText.ok[resultText.ok.Length-1];
            picture.sprite = screens[1];
            musicSource.clip = musicList[4];
            musicStart = musicList[3];
            grade.text = "OK";
			grade.fontSize = 5f;
            grade.color = new Color(0, 160f/255, 0, 1);
            for(int i=0; i<rankList.Length; i++)
            {
                if(rankList[i]==1)
                {
                    if(output!=""&&resultText.superb[i]!="")
                        output+="\nAlso, ";
                    output += resultText.superb[i];
                }
            }
            if(output=="")
                output=okResults[Random.Range(0, okResults.Count)];
            if(score==60||score==61)
                output="You barely made it.";
        }
        else if(result=="superb")
        {
            epilogText.text = resultText.superb[resultText.superb.Length-1];
            picture.sprite = screens[2];
            musicSource.clip = musicList[7];
            musicStart = musicList[6];
            grade.text = "Superb";
			grade.fontSize = 4f;
            grade.color = new Color(1, 24f/255, 0, 1);
            for(int i=0; i<rankList.Length; i++)
            {
                if(rankList[i]==1)
                {
                    if(output!=""&&resultText.superb[i]!="")
                        output+="\nAlso, ";
                    output += resultText.superb[i];
                }
            }
            if(output=="")
                output="That was great! Really great!";
        }
        else
            Debug.Log("perfect");
        ranks.text = output;
        sayer.gameObject.SetActive(false);
        ranks.gameObject.SetActive(false);
        grade.gameObject.SetActive(false);
        picture.gameObject.SetActive(false);
        StartCoroutine(ShowRanks());
    }

    private IEnumerator ShowRanks()
    {
        float speed = 2.56f/fillSound.clip.length;
        float innerScore = 0; 
        yield return new WaitForSeconds(1);
        sayer.gameObject.SetActive(true);
        audioManager.Play("first");
        yield return new WaitForSeconds(1.25f);
        ranks.gameObject.SetActive(true);
        audioManager.Play("end");
        yield return new WaitForSeconds(1f);
        fillout.gameObject.SetActive(true);
        fill.size = new Vector2(0, 0.32f);
        yield return new WaitForSeconds(0.2f);
        fillSound.Play();
        while(fillSound.time<fillSound.clip.length/100*score)
        {
            fill.size = new Vector2(speed*fillSound.time, 0.32f);
            innerScore = 100/fillSound.clip.length*fillSound.time;
            scoreText.text = Mathf.Floor(innerScore).ToString();
            if(Input.GetButtonDown("Fire1"))
                break;
            yield return null;
        }
        scoreText.text = score.ToString();
        fill.size = new Vector2(speed*fillSound.clip.length/100*score, 0.32f);
        fillSound.Stop();
        if(score>PlayerPrefs.GetInt("Max Score", 0))
        {
            PlayerPrefs.SetInt("Max Score", score);
            PlayerPrefs.Save();
            NHS.gameObject.SetActive(true);
            audioManager.Play("scoreFinishNHS");
        }
        else
            audioManager.Play("scoreFinish");
        yield return new WaitForSeconds(0.6f);
        audioManager.Play(result);
        grade.gameObject.SetActive(true);
        if(grade.text=="OK"&&!okResults.Contains(ranks.text)&&(score!=60||score!=61))
            just.gameObject.SetActive(true);
        yield return new WaitWhile(() => audioManager.SoundIsPlaying(result));
        musicSource.PlayOneShot(musicStart);
		musicSource.PlayScheduled(AudioSettings.dspTime + musicStart.length);
        yield return StartCoroutine(Wait());
		yield return new WaitForSeconds(0.05f);
        yield return StartCoroutine(Blackout());
        yield return new WaitForSeconds(0.5f);
        sayer.gameObject.SetActive(false);
        ranks.gameObject.SetActive(false);
        fillout.gameObject.SetActive(false);
        grade.gameObject.SetActive(false);
        just.gameObject.SetActive(false);
        GameObject.FindGameObjectWithTag("Box").SetActive(false);
        picture.gameObject.SetActive(true);
        black.color = new Color(0, 0, 0, 0);
        audioManager.Play(result+"Jingle");
    }

    private IEnumerator Blackout()
    {
        for(float i=0; i<60; i++)
        {
            musicSource.volume = 1-1f/60*i;
            black.color = new Color(0, 0, 0, 1f/60*i);
            yield return null;
        }
        musicSource.Stop();
    }

    private IEnumerator Wait()
    {
        yield return new WaitUntil(() => Input.GetButtonDown("Fire1"));
        audioManager.Play("textAdvance1", 200f);
        yield return new WaitUntil(() => Input.GetButtonUp("Fire1"));
        audioManager.Play("textAdvance2", 200f);
    }

    [System.Serializable]
    public class ResultText
    {
        public string sayer;
        public string[] tryagain;
        public string[] ok;
        public string[] superb;
    }
}
