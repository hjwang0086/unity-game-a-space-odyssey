using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TriggerEvent_scene4 : MonoBehaviour
{

    public Text text;

    public GameObject player, cube_A;
    public GameObject panel;

    private bool isWaitingA = true;
    private bool isWaiting2D = true;
    private bool is2DPressed = false;

    // Update is called once per frame
    void Update()
    {
        EventListener.interactable = !panel.activeSelf;

        // wait for cube_A event
        if (isWaitingA && Vector3.Distance(player.transform.position, cube_A.transform.position) < 0.01f)
        {
            isWaitingA = false;

            panel.GetComponent<ActiveStateToggler>().ToggleActive();
            text.text = "Finally, in this stage we know in 3D mode the eyesight is restricted."
                + "Try to press the button on the lower right to change into the 2D mode.\n\n"
                + "In 2D mode, you cannot move your character, but you're able to see the 6-sided 2D projections"
                + "on the whole maps, which you can figure out the location of the cubes you caanot see in 3D mode.\n\n"
                + "Have fun for figuring out least steps of path!";
        }

        if(isWaiting2D == true && is2DPressed)
        {
            isWaiting2D = false;

            panel.GetComponent<ActiveStateToggler>().ToggleActive();
            text.text = "In 2D mode, there're 2 modes to control your camera projection, which can be"
                + "changed from the lower left buttons(rotate/slide mode, the white one indicates the current mode).\n"
                + "Besides, you can use two fingers to rotate the screen CW/ CCW in rotate mode, and to resize"
                + "the screen size in slide mode.\n\n"
                + "For the lower left button, the yellow tag indicates current project direction.";
        }
    }

    public void onClickDim()
    {
        is2DPressed = !is2DPressed;
    }
}
