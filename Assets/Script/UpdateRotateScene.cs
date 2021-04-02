using UnityEngine;
using System.Collections;

public class UpdateRotateScene : MonoBehaviour {

	public GameObject camera2DPosition;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	Vector3 calculateScenePosition(Vector3 direction) {
		if(Vector3.Angle(direction, Vector3.back) < 1f) {
			EventListener.tagCurrent2DRotateRect(0);
			return new Vector3(0, 0, 100); // world front position
		}
		else if(Vector3.Angle(direction, Vector3.forward) < 1f) {
			EventListener.tagCurrent2DRotateRect(1);
			return new Vector3(0, 0, -100); // world back position
		}
		else if(Vector3.Angle(direction, Vector3.down) < 1f) {
			EventListener.tagCurrent2DRotateRect(2);
			return new Vector3(0, 100, 0); // world up position
		}
		else if(Vector3.Angle(direction, Vector3.up) < 1f) {
			EventListener.tagCurrent2DRotateRect(3);
			return new Vector3(0, -100, 0); // world down position
		}
		else if(Vector3.Angle(direction, Vector3.right) < 1f) {
			EventListener.tagCurrent2DRotateRect(4);
			return new Vector3(-100, 0, 0); // world left position
		}
		else if(Vector3.Angle(direction, Vector3.left) < 1f) {
			EventListener.tagCurrent2DRotateRect(5);
			return new Vector3(100, 0, 0); // world right position
		}
		return Vector3.zero;
	}

	public void rotateUpScene() {
		camera2DPosition.transform.Rotate (-90, 0, 0);
		camera2DPosition.transform.localPosition = calculateScenePosition(camera2DPosition.transform.forward);
		EventListener.rotationEnded ();
	}
	
	public void rotateDownScene() {
		camera2DPosition.transform.Rotate (90, 0, 0);
		camera2DPosition.transform.localPosition = calculateScenePosition(camera2DPosition.transform.forward);
		EventListener.rotationEnded ();
	}
	
	public void rotateLeftScene() {
		camera2DPosition.transform.Rotate (0, -90, 0);
		camera2DPosition.transform.localPosition = calculateScenePosition(camera2DPosition.transform.forward);
		EventListener.rotationEnded ();
	}
	
	public void rotateRightScene() {
		camera2DPosition.transform.Rotate (0, 90, 0);
		camera2DPosition.transform.localPosition = calculateScenePosition(camera2DPosition.transform.forward);
		EventListener.rotationEnded ();
	}

	public void rotateClockwiseScene() {
		camera2DPosition.transform.Rotate (0, 0, 90);
		camera2DPosition.transform.localPosition = calculateScenePosition(camera2DPosition.transform.forward);
		EventListener.rotationEnded ();
	}

	public void rotateCounterclockwiseScene() {
		camera2DPosition.transform.Rotate (0, 0, -90);
		camera2DPosition.transform.localPosition = calculateScenePosition(camera2DPosition.transform.forward);
		EventListener.rotationEnded ();
	}
}
