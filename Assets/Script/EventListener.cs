using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Soomla.Profile;

public class EventListener : MonoBehaviour {
    const int levelNum = 8;

    public GameObject mainCamera;
	public GameObject Camera2D;
	public GameObject orthographic2Dcenter;
	public GameObject player;
	public GameObject world;
    public GameObject wormhole;

    public Text stepCountText, weaponCountText;
    public Button dimButton, zoomButton, rotateButton;
    public Canvas win_Canvas, lose_Canvas;
    public Sprite image3D, image2D;

    public Image star1, star2, star3;
    public Sprite lightCube, darkCube;

    public GameObject muteToggle;
    public AudioClip audioPlay, audioLose, audioWin;
    public AudioClip audioKill, audioGet;
    private bool isInitialSetToggle = true;

    public static bool interactable = true;

    private GameObject[] cubeSet;
    private GameObject[] weaponSet;
    private GameObject[] monsterSet;
    private AudioSource audioSrc, audioOneShot;

    private Rect dimRect, zoomRect, rotateRect;

    private bool is2D;
	private bool isGameOver, isWin;
    private bool fbPressed;

	private bool isMouseButtonDown;
	private Vector3 firstMouseButtonDownPosition;
	private Vector3 previousMousePosition;

	private static int forwardAnimationCount;
	private static bool isIdleState;

	private float maxDistanceBetweenCubes;
	private List<int> findFirstTouchTwoPointIndex;
	private List<Vector2> firstTouchTwoPointPosition;
	private float previousDistanceBetweenTwoTouchPoint;
	private Vector2 previousRotateVectorDifference;

	private enum Mode{None, Adjust, Rotate};
	private Mode mode2D;
	private static Rect current2DRotateRect;
	private static float rectLength;
	private static Vector2 backPosition;
	private static float[,] tagRectPosition;
	
	public static int stepCount, weaponCount;

    private bool isFirstMode;

	private Animator animator_3D, animator_2D;

    void Start() {
        cubeSet = GameObject.FindGameObjectsWithTag("Cube");
        weaponSet = GameObject.FindGameObjectsWithTag("Weapon");
        monsterSet = GameObject.FindGameObjectsWithTag("Monster");
        audioSrc = GameObject.Find("Audio Source").GetComponent<AudioSource>();
        audioOneShot = GameObject.Find("Audio One Shot").GetComponent<AudioSource>();

        stepCountText.text = stepCount.ToString();
        weaponCountText.text = weaponCount.ToString();

        audioSrc.loop = true;
        if (!audioSrc.clip || !audioSrc.clip != audioPlay)
            audioSrc.clip = audioPlay;
        if (!audioSrc.isPlaying)
            audioSrc.Play();
        audioSrc.mute = DataBase.isMute;
        muteToggle.GetComponent<Toggle>().isOn = DataBase.isMute;
        audioSrc.mute = DataBase.isMute;
        audioOneShot.mute = DataBase.isMute;
        isInitialSetToggle = false;

        win_Canvas.GetComponent<Canvas>().enabled = false;
        lose_Canvas.GetComponent<Canvas>().enabled = false;
        dimButton.image.sprite = image3D;
		dimRect = calculateButtonRect (dimButton);
		zoomRect = calculateButtonRect (zoomButton);
        rotateRect = calculateButtonRect(rotateButton);

        calculateCubeSetCenter ();
		calculate2DCameraSize ();

		is2D = false;
        isFirstMode = true;
		set2DCameraAndButtonActive (false);
		isGameOver = false;
        isWin = false;
		isMouseButtonDown = false;
		stepCount = 0;
        weaponCount = 0;
		forwardAnimationCount = 0;
		isIdleState = true;
		mode2D = Mode.Rotate;

		calculateCurrent2DRotateRect ();
		current2DRotateRect = new Rect(tagRectPosition[1,0], tagRectPosition[1,1], rectLength, rectLength);

		findFirstTouchTwoPointIndex = new List<int> ();
		firstTouchTwoPointPosition = new List<Vector2> ();

		animator_3D = mainCamera.GetComponent<Animator> ();
		animator_2D = Camera2D.GetComponent<Animator> ();

        fbPressed = false;
    }

	void Update() {
        stepCountText.text = stepCount.ToString();
        weaponCountText.text = weaponCount.ToString();

        wormhole.transform.Rotate(Vector3.forward * Time.deltaTime * 25, Space.Self);

        dimButton.enabled = interactable;
        rotateButton.enabled = interactable;
        zoomButton.enabled = interactable;
        if (!interactable) return;

        if (isGameOver)
        {
            if(isFirstMode)
            {
                isFirstMode = false;
                audioSrc.Pause();
                audioSrc.clip = audioLose;
                audioSrc.loop = false;
                audioSrc.Play();
            }

            lose_Canvas.GetComponent<Canvas>().enabled = true;
            dimButton.enabled = false;
        }
        else if (isWin)
        {
            if (isFirstMode)
            {
                isFirstMode = false;
                audioOneShot.clip = audioWin;
                audioOneShot.Play();
            }

            DataBase.storeCurrData(Application.loadedLevelName, stepCount);

            star1.sprite = (DataBase.currRank >= 1) ? lightCube : darkCube;
            star2.sprite = (DataBase.currRank >= 2) ? lightCube : darkCube;
            star3.sprite = (DataBase.currRank >= 3) ? lightCube : darkCube;

            win_Canvas.GetComponent<Canvas>().enabled = true;
            dimButton.enabled = false;
        }
        else
		{
            // the interactable one is to-be-changed
            zoomButton.GetComponent<Button>().interactable = (mode2D == Mode.Rotate);
            rotateButton.GetComponent<Button>().interactable = (mode2D == Mode.Adjust);

            set2DCameraAndButtonActive(is2D);

            if (is2D)
            {
                dimButton.image.sprite = image2D;

				setAll2DRotationsFalse();

				if(mode2D == Mode.Adjust) {
                    Debug.Log("adjust mode");

					adjust2DCameraSizeAndPosition();
				}
				else if(mode2D == Mode.Rotate) {
                    Debug.Log("rotate mode");

               		rotate2DScene();
				}
            }
            else
            {
                dimButton.image.sprite = image3D;

                setAll3DBehaviorsFalse();

                if (!animator_3D.GetCurrentAnimatorStateInfo(0).IsName("player_forward")
				    && forwardAnimationCount > 0)
                {
                    animator_3D.SetBool("forward", true);
                }
                else
                { // no forward
                    animator_3D.speed = 1; // default speed
                }

                executeMotionBehavior();

                judgeWormholeTrigger();
                judgeWeaponRetrieval();
                judgeMonsterTrigger();
            }
        }
    }

	Rect calculateButtonRect(Button button) {
		return new Rect(button.GetComponent<RectTransform>().position.x + button.GetComponent<RectTransform>().rect.x,
		                button.GetComponent<RectTransform>().position.y + button.GetComponent<RectTransform>().rect.y,
		                button.GetComponent<RectTransform>().rect.width,
		                button.GetComponent<RectTransform>().rect.height);
	}

	void calculateCubeSetCenter() {
		Vector3 positionSum = new Vector3 (0, 0, 0);
		foreach (GameObject cube in cubeSet) {
			positionSum += cube.transform.position;
		}
		orthographic2Dcenter.transform.position = positionSum / cubeSet.Length; // scene center position = cubeSet center position
	}

	void calculate2DCameraSize() {
		// calcute distance between all two distinct cubes and find max distance
		maxDistanceBetweenCubes = 0f;
		for (int i = 0; i < cubeSet.Length - 1; i++) {
			for (int j = i + 1; j < cubeSet.Length; j++) {
				if(Vector3.Distance(cubeSet[i].transform.position, cubeSet[j].transform.position) > maxDistanceBetweenCubes) {
					maxDistanceBetweenCubes = Vector3.Distance(cubeSet[i].transform.position, cubeSet[j].transform.position);
				}
			}
		}
        Camera2D.GetComponent<Camera>().orthographicSize = maxDistanceBetweenCubes * 0.4f;
	}

    /* for touch event */
	void adjust2DCameraSizeAndPosition() {
		if (!isMouseButtonDown && isIdleState) { /* findFirstTouchTwoPointIndex.Count < 2 */
			checkTouchCount();
			if (findFirstTouchTwoPointIndex.Count == 1) { // adjust Camera2D position
				previousMousePosition = (Vector3)firstTouchTwoPointPosition[0];
				isMouseButtonDown = true;
			}
			else if (findFirstTouchTwoPointIndex.Count == 2) { // adjust Camera2D size
				float distanceBetweenTwoTouchPoint = Vector2.Distance(firstTouchTwoPointPosition[0], firstTouchTwoPointPosition[1]);
				previousDistanceBetweenTwoTouchPoint = distanceBetweenTwoTouchPoint;
				isMouseButtonDown = true;
			}
		}
		else if(isMouseButtonDown) {
			if(findFirstTouchTwoPointIndex.Count == 1) {
				checkTouchCount();
				if(findFirstTouchTwoPointIndex.Count == 2) {
					float distanceBetweenTwoTouchPoint = Vector2.Distance(firstTouchTwoPointPosition[0], firstTouchTwoPointPosition[1]);
					previousDistanceBetweenTwoTouchPoint = distanceBetweenTwoTouchPoint;
					return;
				}

				// adjust Camera2D position
				if (Input.GetTouch(findFirstTouchTwoPointIndex[0]).phase == TouchPhase.Ended) {
					findFirstTouchTwoPointIndex.Clear();
					firstTouchTwoPointPosition.Clear();
					isMouseButtonDown = false;
				}
				else if (Input.GetTouch(findFirstTouchTwoPointIndex[0]).phase == TouchPhase.Moved) {
					firstTouchTwoPointPosition[0] = Input.GetTouch(findFirstTouchTwoPointIndex[0]).position;
					Vector3 positionDifference = (Vector3)firstTouchTwoPointPosition[0] - previousMousePosition;

					Vector3 newCameraPosition = Camera2D.gameObject.transform.parent.transform.localPosition - (calculateMouseVector(positionDifference)) * 0.1F
																				* (Camera2D.GetComponent<Camera>().orthographicSize / (maxDistanceBetweenCubes * 0.4f));
					Camera2D.gameObject.transform.parent.transform.localPosition = calculateCameraPosition(newCameraPosition);

					previousMousePosition = (Vector3)firstTouchTwoPointPosition[0];
				}
			}
			else /* if(findFirstTouchTwoPointIndex.Count == 2) */ { // adjust Camera2D size
				for(int i = 0; i < 2; i++) {
					if (Input.GetTouch(findFirstTouchTwoPointIndex[i]).phase == TouchPhase.Ended) {
						findFirstTouchTwoPointIndex.Clear();
						firstTouchTwoPointPosition.Clear();
						isMouseButtonDown = false;
						return;
					}
					else if (Input.GetTouch(findFirstTouchTwoPointIndex[i]).phase == TouchPhase.Moved) {
						firstTouchTwoPointPosition[i] = Input.GetTouch(findFirstTouchTwoPointIndex[i]).position;
					}
				}
				float distanceBetweenTwoTouchPoint = Vector2.Distance(firstTouchTwoPointPosition[0], firstTouchTwoPointPosition[1]);
				float newCameraSize = Camera2D.GetComponent<Camera>().orthographicSize
										- (distanceBetweenTwoTouchPoint - previousDistanceBetweenTwoTouchPoint) * 0.1f;

				if(newCameraSize < maxDistanceBetweenCubes * 0.1f) {
					Camera2D.GetComponent<Camera>().orthographicSize = maxDistanceBetweenCubes * 0.1f;
				}
				else if(newCameraSize > maxDistanceBetweenCubes * 0.7f) {
					Camera2D.GetComponent<Camera>().orthographicSize = maxDistanceBetweenCubes * 0.7f;
				}
				else {
					Camera2D.GetComponent<Camera>().orthographicSize = newCameraSize;
				}

				previousDistanceBetweenTwoTouchPoint = distanceBetweenTwoTouchPoint;
			}
		}
	}

	void checkTouchCount() {
		for (int i = 0; i < Input.touchCount && findFirstTouchTwoPointIndex.Count < 2; i++) {
			if (Input.GetTouch (i).phase == TouchPhase.Began && !findFirstTouchTwoPointIndex.Contains(i)
			    && !dimRect.Contains(Input.GetTouch (i).position)
			    && !zoomRect.Contains(Input.GetTouch (i).position)
                && !rotateRect.Contains(Input.GetTouch(i).position)) {
				firstTouchTwoPointPosition.Add (Input.GetTouch (i).position);
				findFirstTouchTwoPointIndex.Add (i);
			}
		}
	}

	Vector3 calculateMouseVector(Vector3 positionDifference) {
		Vector3 worldMouseVector = Vector3.zero;
		
		if(isInDirection(Camera2D.transform.right.x) != 0) {
			worldMouseVector.x = positionDifference.x * isInDirection(Camera2D.transform.right.x);
		}
		else if(isInDirection(Camera2D.transform.right.y) != 0) {
			worldMouseVector.y = positionDifference.x * isInDirection(Camera2D.transform.right.y);
		}
		else if(isInDirection(Camera2D.transform.right.z) != 0) {
			worldMouseVector.z = positionDifference.x * isInDirection(Camera2D.transform.right.z);
		}
		if(isInDirection(Camera2D.transform.up.x) != 0) {
			worldMouseVector.x = positionDifference.y * isInDirection(Camera2D.transform.up.x);
		}
		else if(isInDirection(Camera2D.transform.up.y) != 0) {
			worldMouseVector.y = positionDifference.y * isInDirection(Camera2D.transform.up.y);
		}
		else if(isInDirection(Camera2D.transform.up.z) != 0) {
			worldMouseVector.z = positionDifference.y * isInDirection(Camera2D.transform.up.z);
		}
		
		return worldMouseVector;
	}
	
	int isInDirection(float value) {
		if(1f - 0.001f <= value && value <= 1f + 0.001f)
			return 1;
		if(-1f - 0.001f <= value && value <= -1f + 0.001f)
			return -1;
		return 0;
	}

	Vector3 calculateCameraPosition(Vector3 newCameraPosition) {
		Vector3 adjustPosition = newCameraPosition;
		float leftmostX = orthographic2Dcenter.transform.position.x, upmostY = orthographic2Dcenter.transform.position.y,
			rightmostX = orthographic2Dcenter.transform.position.x, downmostY = orthographic2Dcenter.transform.position.y,
			frontmostZ = orthographic2Dcenter.transform.position.z, backmostZ = orthographic2Dcenter.transform.position.z;
		foreach (GameObject cube in cubeSet) {
			if(cube.transform.position.x < leftmostX) {
				leftmostX = cube.transform.position.x;
			}
			if(cube.transform.position.y > upmostY) {
				upmostY = cube.transform.position.y;
			}
			if(cube.transform.position.x > rightmostX) {
				rightmostX = cube.transform.position.x;
			}
			if(cube.transform.position.y < downmostY) {
				downmostY = cube.transform.position.y;
			}
			if(cube.transform.position.z > frontmostZ) {
				frontmostZ = cube.transform.position.z;
			}
			if(cube.transform.position.z < backmostZ) {
				backmostZ = cube.transform.position.z;
			}
		}

		if(adjustPosition.x < leftmostX) {
			adjustPosition.x = leftmostX;
		}
		if(adjustPosition.x > rightmostX) {
			adjustPosition.x = rightmostX;
		}
		if(adjustPosition.y < downmostY) {
			adjustPosition.y = downmostY;
		}
		if(adjustPosition.y > upmostY) {
			adjustPosition.y = upmostY;
		}
		if(adjustPosition.z > frontmostZ) {
			adjustPosition.z = frontmostZ;
		}
		if(adjustPosition.z < backmostZ) {
			adjustPosition.z = backmostZ;
		}

		if(isInDirection(Camera2D.transform.forward.x) != 0) {
			adjustPosition.x = 100 * -isInDirection(Camera2D.transform.forward.x);
		}
		else if(isInDirection(Camera2D.transform.forward.y) != 0) {
			adjustPosition.y = 100 * -isInDirection(Camera2D.transform.forward.y);
		}
		else if(isInDirection(Camera2D.transform.forward.z) != 0) {
			adjustPosition.z = 100 * -isInDirection(Camera2D.transform.forward.z);
		}

		return adjustPosition;
	}
	
	void set2DCameraAndButtonActive(bool set) {
		Camera2D.SetActive (set);
		mainCamera.SetActive (!set);
		zoomButton.image.enabled = set;
        rotateButton.image.enabled = set;
	}

	void setAll3DBehaviorsFalse() {
		animator_3D.SetBool ("forward", false);
		animator_3D.SetBool ("turn left", false);
		animator_3D.SetBool ("turn right", false);
		animator_3D.SetBool ("flip", false);
	}

	void setAll2DRotationsFalse() {
		animator_2D.SetBool ("rotate up", false);
		animator_2D.SetBool ("rotate down", false);
		animator_2D.SetBool ("rotate left", false);
		animator_2D.SetBool ("rotate right", false);
		animator_2D.SetBool ("rotate counterclockwise", false);
		animator_2D.SetBool ("rotate clockwise", false);
	}

	void executeMotionBehavior() {
		if (Input.GetMouseButtonDown(0) && isIdleState && !dimRect.Contains(Input.mousePosition)) {
			firstMouseButtonDownPosition = Input.mousePosition;
			isMouseButtonDown = true;
		}
		if (Input.GetMouseButtonUp(0) && isMouseButtonDown) {
			isMouseButtonDown = false;

			if(Vector3.Distance(firstMouseButtonDownPosition, Input.mousePosition) > 50f) {
				Vector3 vectorDifference = Input.mousePosition - firstMouseButtonDownPosition;

				if(Vector3.Angle(vectorDifference, Vector3.up) < 30f) { // forward
					calculateForwardAnimationCount ();

					if (forwardAnimationCount > 0) {
						animator_3D.speed = forwardAnimationCount;
						animator_3D.SetBool ("forward", true);
						isIdleState = false;

						/*if(isKilledMonster()) {
							// monster falling animation
						}*/
					}
					else {
						isGameOver = true;
					}
				}

                if (Vector3.Angle(vectorDifference, Vector3.down) < 30f) { // flip
                    animator_3D.SetBool("flip", true);
					isIdleState = false;

					/*if(isKilledMonster()) {
						// monster falling animation
					}*/
				}

                if (Vector3.Angle(vectorDifference, Vector3.left) < 30f) { // turn left
                    animator_3D.SetBool ("turn left", true);
					isIdleState = false;
				}

				if(Vector3.Angle(vectorDifference, Vector3.right) < 30f) { // turn right
                    animator_3D.SetBool ("turn right", true);
					isIdleState = false;
				}
			}
		}
	}

    /** (digression) calculateForwardAnimationCount()
     * if set wormhole_light's "sphere collider" enabled, player cannot forward to the light's direction...
     * cannot figure out why?  
     */
    void calculateForwardAnimationCount() {
		RaycastHit hitInfo = new RaycastHit ();
		if (Physics.Raycast (mainCamera.transform.position, mainCamera.transform.forward.normalized, out hitInfo)) { // forward with collision
			forwardAnimationCount = 0;
		}
		else {
			RaycastHit hitInfo2 = new RaycastHit ();
			if (Physics.Raycast (player.transform.position, player.transform.forward.normalized, out hitInfo2)) {
				forwardAnimationCount = (int)Mathf.Ceil(hitInfo2.distance / 10);
			}
			else { // forward without cubes
				forwardAnimationCount = 0;
			}
		}
	}

    void judgeWeaponRetrieval()
    {
        foreach(GameObject weapon in weaponSet)
        {
            Transform playerModel = player.transform.Find("Player Model");
            Transform weaponModel = weapon.transform.Find("Sword Model");

            if (Vector3.Distance(playerModel.position, weaponModel.position) < 1.001f) // attached
            {
                audioOneShot.PlayOneShot(audioGet, 1.0f);
                weaponCount++;

                weapon.SetActive(false);
                Destroy(weapon);
                weaponSet = GameObject.FindGameObjectsWithTag("Weapon");
            }
        }
    }

    void judgeWormholeTrigger()
    {
        Transform playerModel = player.transform.Find("Player Model");
        Transform wormholeModel = wormhole.transform.Find("Wormhole Model");

        if(Vector3.Distance(playerModel.position, wormholeModel.position) < 0.01f)
            isWin = true;
    }

    void judgeMonsterTrigger()
    {
        foreach(GameObject monster in monsterSet)
        {
            Transform playerModel = player.transform.Find("Player Model");
            Transform monsterModel = monster.transform.Find("Monster Model");

            if (Vector3.Distance(playerModel.position, monsterModel.position) < 0.001f) // attached
            {
                if (weaponCount <= 0)
                {
                    monster.SetActive(false);
                    Destroy(monster);
                    isGameOver = true;
                }
                else
                {
                    audioOneShot.PlayOneShot(audioKill, 0.3f);
                    weaponCount--;

                    monster.SetActive(false);
                    Destroy(monster);
                    monsterSet = GameObject.FindGameObjectsWithTag("Monster");
                }
            }
        }
    }

    public static void decreaseOneForwardAnimationCount() {
		if(forwardAnimationCount > 0) {
			forwardAnimationCount--;
		}
		if (forwardAnimationCount == 0) {
			animationEnded ();
		}
	}

	public void calculateCurrent2DRotateRect() {
		Vector3 buttonPosition = dimButton.GetComponent<RectTransform> ().position;
		float scale = dimButton.transform.parent.GetComponent<RectTransform> ().localScale.x * dimButton.GetComponent<RectTransform> ().rect.width;
		backPosition = new Vector2 (buttonPosition.x - 274f/404f * scale, Screen.height - buttonPosition.y - 234f/404f * scale);
		rectLength = 76f/404f * scale;
		/* {p.x, p.y}, front/back/up/down/left/right */
		tagRectPosition = new float[,] {
			{backPosition.x + 2 * rectLength, backPosition.y}, {backPosition.x, backPosition.y},
			{backPosition.x, backPosition.y - rectLength}, {backPosition.x, backPosition.y + rectLength},
			{backPosition.x - rectLength, backPosition.y}, {backPosition.x + rectLength, backPosition.y}
		};
	}

	public static void tagCurrent2DRotateRect(int dir) {
		current2DRotateRect.x = tagRectPosition[dir,0];
		current2DRotateRect.y = tagRectPosition[dir,1];
	}

	public static void animationEnded() {
		stepCount++; // increase one stepCount
		isIdleState = true; // set state to idle state
	}

	public static void rotationEnded() {
		isIdleState = true; // set state to idle state
	}

	void rotate2DScene() {
		if (!isMouseButtonDown && isIdleState) {
			checkTouchCount();
			if (findFirstTouchTwoPointIndex.Count == 1) { // rotate left/right/up/down
				firstMouseButtonDownPosition = (Vector3)firstTouchTwoPointPosition[0];
				isMouseButtonDown = true;
			}
			else if (findFirstTouchTwoPointIndex.Count == 2) { // rotate counterclockwise/clockwise
				previousRotateVectorDifference = firstTouchTwoPointPosition[1] - firstTouchTwoPointPosition[0];
				isMouseButtonDown = true;
			}
		}
		else if(isMouseButtonDown) {
			if(findFirstTouchTwoPointIndex.Count == 1) {
				checkTouchCount();
				if(findFirstTouchTwoPointIndex.Count == 2) {
					previousRotateVectorDifference = firstTouchTwoPointPosition[1] - firstTouchTwoPointPosition[0];
					return;
				}
				
				// rotate left/right/up/down
				if (Input.GetTouch(findFirstTouchTwoPointIndex[0]).phase == TouchPhase.Ended) {
					if(Vector3.Distance(firstMouseButtonDownPosition, (Vector3)Input.GetTouch(findFirstTouchTwoPointIndex[0]).position) > 50f) {
						Vector3 vectorDifference = (Vector3)Input.GetTouch(findFirstTouchTwoPointIndex[0]).position - firstMouseButtonDownPosition;
						
						if(Vector3.Angle(vectorDifference, Vector3.up) < 30f) { // rotate up, vertical
							animator_2D.SetBool("rotate up", true);
							isIdleState = false;
						}
						else if (Vector3.Angle(vectorDifference, Vector3.down) < 30f) { // rotate down, vertical
							animator_2D.SetBool("rotate down", true);
							isIdleState = false;
						}
						else if (Vector3.Angle(vectorDifference, Vector3.left) < 30f) { // rotate left, horizontal
							animator_2D.SetBool("rotate left", true);
							isIdleState = false;
						}
						else if(Vector3.Angle(vectorDifference, Vector3.right) < 30f) { // rotate right, horizontal
							animator_2D.SetBool("rotate right", true);
							isIdleState = false;
						}
					}

					findFirstTouchTwoPointIndex.Clear();
					firstTouchTwoPointPosition.Clear();
					isMouseButtonDown = false;
				}
			}
			else /* if(findFirstTouchTwoPointIndex.Count == 2) */ { // rotate counterclockwise/clockwise
				for(int i = 0; i < 2; i++) {
					if (Input.GetTouch(findFirstTouchTwoPointIndex[i]).phase == TouchPhase.Ended) {
						Vector2 rotateVectorDifference = Input.GetTouch(findFirstTouchTwoPointIndex[1]).position - Input.GetTouch(findFirstTouchTwoPointIndex[0]).position;
						Vector3 rotateCrossProduct = Vector3.Cross((Vector3)previousRotateVectorDifference, (Vector3)rotateVectorDifference);
						if(rotateCrossProduct.normalized == Vector3.back) { // rotate clockwise
							animator_2D.SetBool("rotate clockwise", true);
							isIdleState = false;
						}
						else if(rotateCrossProduct.normalized == Vector3.forward) { // rotate counterclockwise
							animator_2D.SetBool("rotate counterclockwise", true);
							isIdleState = false;
						}

						findFirstTouchTwoPointIndex.Clear();
						firstTouchTwoPointPosition.Clear();
						isMouseButtonDown = false;
						return;
					}
				}
			}
		}
	}


    /* Triggered when dimButton is clicked */
    public void onClickDim()
    {
		if (!is2D && isIdleState) { // 3D, is not moving
			isMouseButtonDown = false;
			is2D = true;
		}
		else if(is2D && isIdleState) {// 2D, is not moving
			isMouseButtonDown = false;
			is2D = false;
		}
    }

    /* Triggered when retryButton is clicked */
    public void onClickRetry()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }

    /* Triggered when backButton is clicked */
    public void onClickBack()
    {
        Application.LoadLevel(DataBase.menuName);
    }


    /* Triggered when adjustButton is clicked */
    public void onClickAdjust()
	{
        if (isIdleState)
        {
            mode2D = Mode.Adjust;
            isMouseButtonDown = false;
        }
	}

    /* Triggered when rotateButton is clicked */
    public void onClickRotate()
    {
        if (isIdleState)
        {
            mode2D = Mode.Rotate;
            isMouseButtonDown = false;
        }
    }

    /* Triggered when nextButton is clicked */
    public void onClickNext()
    {
        if (Application.loadedLevelName.Equals(DataBase.getLevelName(levelNum)))
            Application.LoadLevel(DataBase.menuName);
        else
            Application.LoadLevel(Application.loadedLevel + 1);
    }

    /* Triggered when facebookButton is clicked */
    public void onClickFacebook()
    {
        fbPressed = true;
    }

    public void onToggleMute()
    {
        if (!isInitialSetToggle)
        {
            DataBase.isMute = !DataBase.isMute;

            audioSrc.mute = DataBase.isMute;
            audioOneShot.mute = DataBase.isMute;
        }
    }


	void drawRect(Rect position, Color color) {
		Texture2D texture = new Texture2D(1, 1);
		texture.SetPixel(0,0,color);
		texture.Apply();
		GUI.skin.box.normal.background = texture;
		GUI.Box(position, GUIContent.none);
	}

	void OnGUI() {
		if (is2D) {
			drawRect(current2DRotateRect, Color.yellow);
		}

        if(fbPressed)
        {
            fbPressed = false;

            if (!SoomlaProfile.IsLoggedIn(Provider.FACEBOOK))
            {
                SoomlaProfile.Login(Provider.FACEBOOK);
            }
            else
            {
                SoomlaProfile.UpdateStory(
                    Provider.FACEBOOK,
                    "Check out the BEST game - A Space Odyssey!!",
                    "I passed level " + DataBase.currLevel +  " and got rank " + DataBase.currRank + "!",
                    "A Space Odyssey",
                    "step count is " + stepCount,
                    "https://www.google.com.tw",
                    "https://s-media-cache-ak0.pinimg.com/236x/ef/c9/61/efc9618d697a641f05c42b8059d8b9c6.jpg"
                );
            }
        }
    }
}