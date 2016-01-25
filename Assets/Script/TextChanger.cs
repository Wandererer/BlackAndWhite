using UnityEngine;
using System.Collections;

public class TextChanger : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


   public void ChangeColor()
    {
      //  GetComponent<TextMesh>().color = Color.white;
       // Destroy(this.gameObject);
        //DestroyImmediate(this.gameObject,true);
        TextMesh tm = gameObject.GetComponent<TextMesh>();
        tm.text = "";
        Debug.Log("call change  "+this.gameObject.name);
        
    }
}
