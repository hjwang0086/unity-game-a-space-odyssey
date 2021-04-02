using UnityEngine;
using System.Collections;

public class UpdatePlayer : MonoBehaviour {

    public GameObject player;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void forwardPlayer() {
		player.transform.position += player.transform.forward.normalized * 10;
		EventListener.decreaseOneForwardAnimationCount ();
	}

	public void flipPlayer() {
		player.transform.Rotate (90, 0, 0);
        EventListener.animationEnded ();
	}

	public void turnLeftPlayer() {
		player.transform.Rotate (0, -90, 0);
		EventListener.animationEnded ();
	}

	public void turnRightPlayer() {		
		player.transform.Rotate (0, 90, 0);
		EventListener.animationEnded ();
	}
}
