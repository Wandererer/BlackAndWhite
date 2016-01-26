using UnityEngine;
using System.Collections;

public class RPointsController : MonoBehaviour {

    public int points;

    int[] limit = { 80, 60, 40, 20, 0 };
    
  

    TextChanger[] changer;
	// Use this for initialization
	void Start () {
        points = 99;
     
	}
	
	// Update is called once per frame
	void Update () {
	}

    public void CheckPoint(string w)
    {
        TextChanger[] changer = transform.GetComponentsInChildren<TextChanger>();
        Debug.Log(limit[0]);



        for(int i=0;i<limit.Length;i++)
        {
           
            if(points<limit[i])
            {
                Debug.Log(limit[i] + " limit " + points);
                GameObject obj = GameObject.Find(w+((limit[i]).ToString()));
                if (obj == null)
                    Debug.Log("sdfsdfsdafsdf");

                Debug.Log(this.gameObject.name);
               
                    obj.GetComponent<TextMesh>().text = "";
            

                //Debug.Log(obj.GetComponent<TextMesh>().text + "  SADfsadfd");
                Debug.Log(points +"  point");
            }
        }
    }

    public int getPoint()
    {
        return points;
    }

    public void SetPoint(int point)
    {
        points = point;
    }
}
