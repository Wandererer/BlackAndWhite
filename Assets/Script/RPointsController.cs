using UnityEngine;
using System.Collections;

public class RPointsController : MonoBehaviour {

    public int points;

    int[] limit= {8,6,4,2,0};


    TextChanger[] changer;
	// Use this for initialization
	void Start () {
        changer = transform.GetComponentsInChildren<TextChanger>();
	}
	
	// Update is called once per frame
	void Update () {


        try
        {


            CheckPoint();
        }

        catch{

        }

	}

    void CheckPoint()
    {
        for(int i=0;i<limit.Length;i++)
        {
            if((points/10)<limit[i])
            {
                changer[i].ChangeColor();
            }
        }
    }

    void SetPoint(int point)
    {
        points = point;
    }
}
