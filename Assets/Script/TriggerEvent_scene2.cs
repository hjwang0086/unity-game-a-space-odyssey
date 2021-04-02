using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TriggerEvent_scene2 : MonoBehaviour {

    public Text text;

    public GameObject player, cube_A;
    public GameObject panel;

    private bool isWaitingA = true;

    // Update is called once per frame
    void Update()
    {
        EventListener.interactable = !panel.activeSelf;

        // wait for cube_A event
        if (isWaitingA && Vector3.Distance(player.transform.position, cube_A.transform.position) < 0.01f)
        {
            isWaitingA = false;

            panel.GetComponent<ActiveStateToggler>().ToggleActive();
        }
    }
}
