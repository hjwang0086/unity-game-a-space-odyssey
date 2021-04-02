using UnityEngine;
using Soomla.Profile;

public class DataBase : MonoBehaviour {
    private const int levelNum = 8;

    public static int[] rank = new int[levelNum];
    public static string mainMenuName = "main_menu";
    public static string menuName = "menu";
    public static string aboutName = "about";
    public static int currLevel = 0;
    public static int currRank = 0;
    public static bool isMute = false;

    public AudioSource audioSource, audioOneShot;

    private static string[] levelName = new string[levelNum];
    private static bool isInit = true;

    void Awake()
    {
        Debug.Log("Initial Start");
        isInit = false;

        GameObject.DontDestroyOnLoad(this);
        GameObject.DontDestroyOnLoad(audioSource);
        GameObject.DontDestroyOnLoad(audioOneShot);
        SoomlaProfile.Initialize();

        for (int i = 0; i < levelNum; i++)
        {
            rank[i] = new int();
            levelName[i] = "scene_" + (i + 1);
        }
    }

    void Start()
    {
        Application.LoadLevel(mainMenuName);
    }

    public static string getLevelName(int level)
    {
        return levelName[level-1];
    }

    public static void storeCurrData(string name, int stepCount)
    {
        int currIndex = 0; // currIndex = currLevel-1
        int bestStepCount = 0;

        for(int i = 0; i < levelNum; i++)
        {
            if (name.Equals(levelName[i])) {
                currIndex = i;
                currLevel = i + 1;
                break;
            }
        }

        if (currLevel > PlayerPrefs.GetInt("unlockedLevel"))
            PlayerPrefs.SetInt("unlockedLevel", currLevel);

        switch (currLevel) // analyzed by human...
        { // case index = stage index
            case 1:
                bestStepCount = 3; break;
            case 2:
                bestStepCount = 4; break;
            case 3:
                bestStepCount = 5; break;
            case 4:
                bestStepCount = 6; break;
            case 5:
                bestStepCount = 8; break;
            case 6:
                bestStepCount = 9; break;
            case 7: // copied from 8
                bestStepCount = 8; break;
            case 8:
                bestStepCount = 11; break;
            case 9:
                bestStepCount = 17; break;
            case 10:
                bestStepCount = 8; break;
            case 11:
                bestStepCount = 16; break;

        }

        if (stepCount == bestStepCount)
            rank[currIndex] = 3;
        else if (stepCount - bestStepCount <= 2)
            rank[currIndex] = 2;
        else if (stepCount - bestStepCount <= 4)
            rank[currIndex] = 1;

        currRank = rank[currIndex];

        // store highest rank into PlayerPrefs cache
        if (rank[currIndex] > PlayerPrefs.GetInt("bestRankOf" + currLevel))
           PlayerPrefs.SetInt("bestRankOf" + currLevel, rank[currIndex]);

    }
}
