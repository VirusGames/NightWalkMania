using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallonnScript : MonoBehaviour
{

    public bool isMoving;
    private float step;
    private float argo;
    private float speed;
    public Conductor conductor;
    public Sprite blownTexture;
    public SpriteRenderer spriteRenderer;
    private float x;

    void Awake()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        argo = 1f;
        x = transform.localPosition.x;
        conductor = GameObject.FindObjectOfType<Conductor>();
        speed = 1f/conductor.secPerRealBeat;
        step = Random.Range(0f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if(isMoving)
        {
            if(step<0f)
                argo=1f;
            else if(step>1f)
                argo=-1f;
            step+=speed*argo*Time.deltaTime;
            transform.localPosition = new Vector3(x+EasingFunction.EaseInOutQuad(-0.1f, 0.1f, step), transform.localPosition.y, transform.localPosition.z);
        }
    }

    public IEnumerator Blow()
    {
        isMoving = false;
        spriteRenderer.sprite = blownTexture;
        for(int i=0; i<10; i++)
            yield return null;
        GameObject.Destroy(gameObject);
    }
}
