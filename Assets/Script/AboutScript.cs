using UnityEngine;
using System.Collections;

public class AboutScript : MonoBehaviour {

    public AudioClip clip;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GameObject.Find("Audio Source").GetComponent<AudioSource>();

        audioSource.loop = true;
        if (!audioSource.clip || !audioSource.clip != clip)
            audioSource.clip = clip;
        if (!audioSource.isPlaying)
            audioSource.Play();
        audioSource.mute = DataBase.isMute;
    }

	public void onClickBack()
    {
        Application.LoadLevel(DataBase.mainMenuName);
    }
}
