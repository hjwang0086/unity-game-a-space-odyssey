using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenScript : MonoBehaviour {

    public Text screenSizeText;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        screenSizeText.text = Screen.width.ToString() + " x " + Screen.height.ToString();
	}
}
