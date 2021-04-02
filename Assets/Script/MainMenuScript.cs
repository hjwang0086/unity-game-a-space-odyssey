using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuScript : MonoBehaviour {

    public Canvas canvas;
    public Image bg;
    public GameObject muteToggle;
    public AudioClip clip;

    private AudioSource audioSource;
    private bool isInitialSetToggle = true;

    void Start()
    {
        audioSource = GameObject.Find("Audio Source").GetComponent<AudioSource>();

        muteToggle.GetComponent<Toggle>().isOn = DataBase.isMute;
        isInitialSetToggle = false;

        audioSource.loop = true;
        if (!audioSource.clip || !audioSource.clip != clip)
            audioSource.clip = clip;
        if (!audioSource.isPlaying)
            audioSource.Play();
        audioSource.mute = DataBase.isMute;
    }

	public void onClickStart()
    {
        StartCoroutine(fadeCanvas(canvas, DataBase.menuName));
    }

    public void onClickAbout()
    {
        Application.LoadLevel(DataBase.aboutName);
    }

    public void onClickSetting()
    {
        muteToggle.SetActive(!muteToggle.activeSelf);
    }

    public void onClickQuit()
    {
        Application.Quit();
        
    }

    public void onToggleMute()
    {
        if (!isInitialSetToggle)
        {
            DataBase.isMute = !DataBase.isMute;
            audioSource.mute = DataBase.isMute;
        }
    }


    IEnumerator fadeCanvas(Canvas canvas, string target)
    {
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        while(canvasGroup.alpha > 0.3)
        {
            canvasGroup.alpha -= Time.deltaTime;
            yield return null;
        }
        canvasGroup.interactable = false;
        yield return null;

        Application.LoadLevel(target);
    }
}
