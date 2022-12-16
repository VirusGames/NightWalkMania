using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlologueCript : MonoBehaviour
{
    public GameObject ballonPrefab;
    public Transform ballons;
    public Animator gameName;
    public AudioSource musicSource;
    public SpriteRenderer black;
    private List<Color> ballonColours = new List<Color>{new Color(0.1373f, 0.8471f, 0.8157f, 1), new Color(0.9922f, 0.235f, 0, 1), 
                                                        new Color(0.9765f, 0.4902f, 0.9451f, 1), new Color(0.9294f, 0.8627f, 0.0863f, 1),
                                                        new Color(0, 0.7686f, 0.2353f, 1)};
    private string scene;
    // Start is called before the first frame update
    void Start()
    {
        scene = PlayerPrefs.GetString("Scene", "MainGame");
        for(int i=0; i<25; i++)
        {
            PrologueBallonScript ballon = Instantiate(ballonPrefab, new Vector3(Random.Range(-8, 8), 4-i*2, 0), Quaternion.identity, ballons).GetComponent<PrologueBallonScript>();
            ballon.spriteRenderer.color = ballonColours[i%5];
        }
        StartCoroutine(MainScript());
    }

    private IEnumerator MainScript()
    {
        yield return new WaitForSeconds(0.8f);
        black.color = new Color(0, 0, 0, 0);
        yield return new WaitForSeconds(0.2f);
        gameName.Play("PrologueName");
        musicSource.Play();
        yield return new WaitForSeconds(4f);
        black.color = new Color(0, 0, 0, 1);
        SceneManager.LoadScene(scene);
    }

}
