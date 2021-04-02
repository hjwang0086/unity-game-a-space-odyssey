using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TriggerEvent_scene1 : MonoBehaviour {

    public Text text;

    public GameObject player, cube_A, cube_B;
    public GameObject panel;

    private bool isWaitingA = true, isWaitingB = true;

    void Start()
    {
        StartCoroutine(PlayStreamingVideo("start5.mp4"));
    }

    void Update () {
        EventListener.interactable = !panel.activeSelf;

        // wait for cube_A event
        if (isWaitingA && Vector3.Distance(player.transform.position, cube_A.transform.position) < 0.01f)
        {
            isWaitingA = false;

            panel.GetComponent<ActiveStateToggler>().ToggleActive();
            text.text = "Welcome to A Space Odyssey!\n"
                   + "For more tutor guidelines about manipulations, please refer to \"ABOUT\" in main menu.\n\n"
                   + "In this game, we need to go to wormhole to escape from here.\nNow try to swipe forward,"
                   + "to move from here to the plane in front of you!\n\n"
                   + "WARNING: If there's no plane in front of you but still try to swipe forward, game over would occur!";
        }

        // wait for cube_B event
        else if(isWaitingB && Vector3.Distance(player.transform.position, cube_B.transform.position) < 0.01f)
        {
            isWaitingB = false;

            panel.GetComponent<ActiveStateToggler>().ToggleActive();
            text.text = "Good, you'd successfully teleported!\n"
                   + "Now try to swipe leftward, to rotate 90 degrees. Likewise, swipe it rightward would rotate -90 degrees.\n"
                   + "Note that each of your swipes counts to the amount of steps. So try to figure out the path of least steps,"
                   + " to the destination!";
        }
    }

    private IEnumerator PlayStreamingVideo(string url)
    {
        Handheld.PlayFullScreenMovie(url, Color.white, FullScreenMovieControlMode.CancelOnInput,
            FullScreenMovieScalingMode.AspectFill);
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
    }
}
