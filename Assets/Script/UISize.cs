using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UISize : MonoBehaviour {
    public Text gameOverText, winText;
    public Image stepCountImage, weaponCountImage;
    public Button retryButton, dimButton, adjustButton, rotateButton;

    private const float devPC_width = 1366.0f;
    private const float devPC_height = 598.0f;
    private float W_const, H_const;
    private float shift;

    // Use this for initialization
    void Start () {
        W_const = Screen.width / devPC_width;
        H_const = Screen.height / devPC_height;

        shift = (W_const + H_const) / 2;
	}
	
	// Update is called once per frame
	void Update () {
        retryButton.GetComponent<RectTransform>().sizeDelta
            = new Vector2(150 * shift, 150 * shift);
        dimButton.GetComponent<RectTransform>().sizeDelta
            = new Vector2(200 * shift, 200 * shift);
        stepCountImage.GetComponent<RectTransform>().sizeDelta
            = new Vector2(52 * shift, 52 * shift);
        weaponCountImage.GetComponent<RectTransform>().sizeDelta
            = new Vector2(52 * shift, 52 * shift);
        gameOverText.GetComponent<RectTransform>().sizeDelta
            = new Vector2(600 * shift, 100 * shift);
        winText.GetComponent<RectTransform>().sizeDelta
            = new Vector2(600 * shift, 100 * shift);
        rotateButton.GetComponent<RectTransform>().sizeDelta
            = new Vector2(100 * shift, 100 * shift);
        adjustButton.GetComponent<RectTransform>().sizeDelta
            = new Vector2(100 * shift, 100 * shift);
    }
}
