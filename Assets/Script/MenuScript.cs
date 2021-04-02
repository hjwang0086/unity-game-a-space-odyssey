using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MenuScript : MonoBehaviour {

    public Canvas canvas;
    public Image bg;
    public Sprite full_star, empty_star;
	public GameObject loadProgressText;
    public AudioClip clip;

    private AudioSource audioSource;
    private GameObject[] stageSet;
    private int unlockedLevel;
	private int loadProgress = 0;

    void Start()
    {
        audioSource = GameObject.Find("Audio Source").GetComponent<AudioSource>();

        audioSource.loop = true;
        if (!audioSource.clip || !audioSource.clip != clip)
            audioSource.clip = clip;
        if (!audioSource.isPlaying)
            audioSource.Play();
        audioSource.mute = DataBase.isMute;
        this.GetComponent<AudioSource>().mute = DataBase.isMute;
		
		loadProgressText.SetActive (false);
    }

    void Update()
    {
        Image[] stageChild;
        string bestRankKey;
        int currLevel = 0;

        stageSet = GameObject.FindGameObjectsWithTag("Scene");
        unlockedLevel = PlayerPrefs.GetInt("unlockedLevel");

        foreach (GameObject stage in stageSet)
        {
            stageChild = stage.GetComponentsInChildren<Image>();
            currLevel = int.Parse(stage.name);
            bestRankKey = "bestRankOf" + currLevel;

            if (currLevel <= unlockedLevel+1) // this level is unlocked
            {
                stage.GetComponent<Button>().enabled = true;

                foreach (Image image in stageChild)
                {
                    if (image.gameObject.name == "box_1")
                        image.sprite = (PlayerPrefs.GetInt(bestRankKey) >= 1) ? full_star : empty_star;
                    if (image.gameObject.name == "box_2")
                        image.sprite = (PlayerPrefs.GetInt(bestRankKey) >= 2) ? full_star : empty_star;
                    if (image.gameObject.name == "box_3")
                        image.sprite = (PlayerPrefs.GetInt(bestRankKey) >= 3) ? full_star : empty_star;
                    if (image.gameObject.name == "lock")
                        image.enabled = false;

                }
            }
            else
            {
                stage.GetComponent<Button>().enabled = false;
                stage.GetComponent<Button>().image.sprite = stage.GetComponent<Button>().spriteState.disabledSprite;

                foreach (Image image in stageChild)
                {
                    if (image.gameObject.name == "box_1")
                        image.enabled = false;
                    if (image.gameObject.name == "box_2")
                        image.enabled = false;
                    if (image.gameObject.name == "box_3")
                        image.enabled = false;
                    if (image.gameObject.name == "lock")
                        image.enabled = true;

                }
            }
        }
    }

    public void onClickStage(RectTransform level)
    {
        StartCoroutine(fadeCanvas(canvas, "scene_" + level.name));
    }

    public void onClickBack()
    {
        StartCoroutine(fadeCanvas(canvas,DataBase.mainMenuName));
    }

    IEnumerator fadeCanvas(Canvas canvas, string target)
    {
        if (target != DataBase.mainMenuName)
            loadProgressText.SetActive (true);

		loadProgressText.GetComponent<Text>().text = loadProgress + "%";

        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        while (canvasGroup.alpha > 0.3)
        {
            canvasGroup.alpha -= Time.deltaTime;
            yield return null;
        }
        canvasGroup.interactable = false;
        yield return null;

        AsyncOperation async = Application.LoadLevelAsync(target);
		while (!async.isDone) {
			loadProgress = (int)(async.progress * 100);
			loadProgressText.GetComponent<Text>().text = loadProgress + "%";
			yield return null;
		}
    }
}
