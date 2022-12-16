using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Test
{
    public class Test : MonoBehaviour
    {
        public GameData data;
        // Start is called before the first frame update
        void Start()
        {
            data.levelcreator = "Me";
            data.songcreator = "Tsunku";
            data.end = 999;
            data.jumps = new List<float>{1, 2, 3};
            RandomList lst = new RandomList{}; lst.list = new List<float>();
            lst.list.Add(1); lst.list.Add(2); lst.list.Add(3);
            data.random = new List<RandomList>{lst, lst};
            data.skillstar = 143;
            string dat = JsonUtility.ToJson(data, true);
            System.IO.File.WriteAllText(Application.dataPath + "/test.json", dat);
        }

        // Update is called once per frame
        void Update()
        {
            
        }
    }

    [System.Serializable]
    public class GameData
    {
        public string levelcreator;
        public string songcreator;
        public float bpm;
        public float beatparts;
        public float end;
        public List<float> jumps;
        public List<RandomList> random;
        public int skillstar;
    }

    [System.Serializable]
    public class RandomList
    {
        public List<float> list;
    }
}
