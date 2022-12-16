using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(AudioSource))]
public class Conductor : MonoBehaviour
{
    public static Conductor Instance { get; set; }

    public GameManager gameManager;

    //это - основные параметры
    [Header("Assignables")]
    public float bpm = 120;
    public float beatParts = 4;
    public float secPerBeat;
    public float songPosition;
    private float songPositionInBeatsExact;
    private int songPositionInBeats;
    public float beatPosition;

    public float lastReportedBeat = 0f;

    //остальное тоже нужно
    public float dspSongTime;
    public float songTimeInBeats;
    public float firstBeatOffset;
    public AudioSource musicSource;
    public AudioSource metronome_audioSrc;
    public float secPerRealBeat;

    public float songLength;

    public bool onBeat = false;

    public double songPosBeat = 0;

    public float beatTime;

    public bool metronome;

    public bool autoPlay = true;

    public TMP_Text currentBeatTime;

    public static Conductor instance;

    public bool inEditor = false;

    public bool loop;

    private int point25Times = 0;

    private int firstBeatTime = 0;

    private bool pastFirstBeat = false;

    private int times;

    public int loopBeat = 0;

    public int loopNumber = 0;
    private bool haveLooped;

    void Awake()
    {
        instance = this;
    }

    // if anything bad happens for any reason me changing this from Start to Awake might be the problem but idk lol
    void Start()
    {
        Conductor.Instance = this;
        //Load the AudioSource attached to the Conductor GameObject
        musicSource.GetComponent<AudioSource>();
        //Metronome
        //metronome_audioSrc.GetComponent<AudioSource>();
        //Calculate the number of seconds in each beat
        secPerRealBeat = 60f / bpm;
        secPerBeat = 60/beatParts/ bpm;
        //Record the time when the music starts
        dspSongTime = (float)musicSource.time;
        //Start the music
        //musicSource.time = 10f;

        //if (autoPlay)
            //musicSource.Play();
        //else
        loop = musicSource.loop;
        StartCoroutine(MusicOffsetStart()); //это я добавил, чтобы музыка начинала играть после загрузки всего остального

        if (musicSource.clip) songLength = musicSource.clip.length;
        songTimeInBeats = songLength / secPerRealBeat;
        if(loop)
            loopBeat = (int)Mathf.Round(songTimeInBeats);
    }
    //все английские комментарии оставлены Starpelly

    void Update()
    {
        //if(onBeat)
        //    Debug.Log("onbeat");
        lastReportedBeat = songPositionInBeats;
        if (musicSource.isPlaying)
        {
            if(musicSource.time<secPerRealBeat&&!haveLooped)
            {
                loopNumber++;
                haveLooped = true;
            }
            //beatParts позволяют работать не только с 1/4 бита, но и с 1/6, которые сне здесь и нужны
            secPerRealBeat = 60f / bpm;
            secPerBeat = 60/beatParts / bpm;

            //позиция песни и бита в ней
            songPosition = (float)(musicSource.time - dspSongTime - firstBeatOffset);
            if(songPosition>=secPerRealBeat*2)
                haveLooped = false;
            beatPosition = (float)Mathf.Round(songPosition / secPerBeat / beatParts * 100) / 100f + loopBeat * loopNumber; // the reason i do this is so we can only get the first 2 decimal places in the float. we dont need any more than that.
            if (inEditor) currentBeatTime.text = beatPosition.ToString();

            //determine how many beats since the song started
            songPositionInBeatsExact = songPosition / secPerBeat;
            songPositionInBeats = (int)songPositionInBeatsExact + loopBeat * loopNumber * (int)beatParts;
            ReportBeat();

            /*if (Input.GetKeyDown(KeyCode.J))
            {
                Debug.Log(songPosBeat);
            }*/

            //золотце, котрое действительно пригождается
            songPosBeat = Math.Round(songPositionInBeats / beatParts * 100000) / 100000; //0.66667
            //songPosBeat = Mathf.Round(Mathf.Round(songPositionInBeats / beatParts * 1000000)/10) / 100000; //0.66667
            /*if(songPosBeat.ToString().Length>2)
            {
                //var digit = Convert.ToString(songPosBeat);
                //var lastDigit = digit.Substring(digit.Length - 1);
                var tempPosBeat = ToInt(songPosBeat);
                while(songPosBeat.ToString().Length<"0.66667".Length)
                {
                    float tmp = songPosBeat;
                    songPosBeat/=10;
                    int num = (int)Mathf.Floor(tmp*10); num = num%10-(num-num%10);
                    songPosBeat+=(int)Mathf.Floor(tmp)+num/10;
                }
            }*/

            beatTime = (float)Mathf.Round(Mathf.Repeat(beatPosition, 1.0f) * 100) / 100f;
        }

        if (songPosBeat == 1/beatParts && point25Times < 1) // if i don't do this getting a beat at 0.25 doesn't work. ONLY 0.25. and i don't know why
        {
            onBeat = true;
            point25Times++;
        }
    }
    /*
    double songPosBeat=122.6667d;
    double num=0d;
    double tmp = songPosBeat;
    double tempPosBeat = int.Parse(songPosBeat.ToString().Split(".")[1]);
    //tempPosBeat /= (float)Math.Pow(10, tempPosBeat.ToString().Length);
    while(tempPosBeat.ToString().Length<"66667".Length)
    {
        tmp = songPosBeat;
        songPosBeat/=10;
        num = Math.Floor(tmp*10);
        num = num%10-(num%100-num%10)/10;
        songPosBeat+=Math.Floor(tmp-songPosBeat)+num/10;
        tempPosBeat*=10;
    } 
    Console.WriteLine(songPosBeat);
    */
    private int ToInt(float num)
    {
        float power = (float)Math.Pow(10, 1.66667.ToString().Length-2);
        int result = (int)Math.Ceiling(1.66667*power);
        return result;
    }

    void ReportBeat()
    {
        // Debug.Log($"{lastReportedBeat}, {songPositionInBeats}");

        //честно, хер его знает, что тут происходит. Я попытался поменять, но всё вышло плачевно
        if (lastReportedBeat != 0)
        {
            if (lastReportedBeat < songPositionInBeats)
            {
                onBeat = true;
                times += 1;
                // beatTime += 0.25f;
                pastFirstBeat = true;
                QuarterBeat();
                lastReportedBeat = songPositionInBeats;
            }
            else
            {
                onBeat = false;
            }
        }
        else
        {
            if (pastFirstBeat)
            {
                pastFirstBeat = false;
                firstBeatTime = 0;
                point25Times = 0;
            }
            if (firstBeatTime < 1)
            {
                onBeat = true;
                times += 1;
                // beatTime += 0.25f;
                QuarterBeat();
                firstBeatTime++;
            }
            else
            {
                firstBeatTime++;
                onBeat = false;
            }
        }
    }

    //хз, но надо
    public void QuarterBeat()
    {
        //gameManager.BeatEvent();
        if (times == beatParts)
        {
            beatTime = 0;
            times = 0;
            FullBeat();
        }
    }

    //бип бип
    public void FullBeat()
    {
        if (metronome == true)
        {
            Debug.Log("Beat");
            metronome_audioSrc.Play();
        }
    }

     private IEnumerator MusicOffsetStart()
    {
        //yield return new WaitUntil(() => songPosBeat != 0);
        yield return new WaitForEndOfFrame();
        musicSource.Play();
    }
}