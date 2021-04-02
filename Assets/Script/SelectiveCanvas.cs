using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SelectiveCanvas : MonoBehaviour {
    public Sprite open, close;
    public Image template;
    public Button expand, retry, stage, menu, setting;
    public GameObject settingPanel;

	// Use this for initialization
	void Start () {
        expand.image.sprite = close;

        template.enabled = false;
        retry.image.enabled = false;
        stage.image.enabled = false;
        menu.image.enabled = false;
        setting.image.enabled = false;
        
	}

    public void onClickExpand()
    {
        expand.image.sprite = (template.enabled) ? close : open;

        template.enabled = !template.enabled;
        retry.image.enabled = !retry.image.enabled;
        stage.image.enabled = !stage.image.enabled;
        menu.image.enabled = !menu.image.enabled;
        setting.image.enabled = !setting.image.enabled;
    }

    public void onClickRetry()
    {
        Application.LoadLevel(Application.loadedLevelName);
    }

    public void onClickStage()
    {
        Application.LoadLevel(DataBase.menuName);
    }

    public void onClickMenu()
    {
        Application.LoadLevel(DataBase.mainMenuName);
    }

    public void onClickSetting()
    {
        settingPanel.SetActive(true);

        onClickExpand();
    }
	
}
