using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Fader loadingScreen;

    [SerializeField]
    private Movable levelMenu;

    [SerializeField]
    private Movable settingsMenu;

    private AudioManager audioManager;

    private void Start()
    {
        audioManager = AudioManager.Instance;
        if (PlayerPrefs.GetInt("Music") != 2)
            audioManager.PlayMusic();
        loadingScreen.Hide(false);
        StartCoroutine(loadingScreen.Fade(0));
    }

    private IEnumerator StartPlayGame()
    {
        Transform level1 = levelMenu.gameObject.transform.Find("Level1");
        Text level1Text = level1.Find("Level Info").GetComponent<Text>();
        level1Text.text = "Level 1 - 20 Moves";

        Text level1Score = level1.Find("Score Info").GetComponent<Text>();
        if (PlayerPrefs.GetInt("level1highest") != 0)
        {
            level1Score.text = "Highest Score: " + PlayerPrefs.GetInt("level1highest") + " - " + "Target Score: 400";
        } else
        {
            level1Score.text = "No Highest Score " + "Target Score: 400";
        }

        SetLevelInfo(2);

        yield return StartCoroutine(levelMenu.MoveToPosition(new Vector3(0, 0)));

        Button level1Button = level1.gameObject.transform.Find("Play Button").GetComponent<Button>();

        level1Button.onClick.AddListener(() =>
        {
            // Your code here for handling button click
            Debug.Log("Level 1 button clicked!");
            StartCoroutine(LoadPlayScene(20, 400, 1));
        });

        /*yield return StartCoroutine(loadingScreen.Fade(1));
        PlayerPrefs.SetInt("moveCount", 20);
        PlayerPrefs.SetInt("targetScore", 400);
        PlayerPrefs.SetInt("levelNumber", 1);
        SceneManager.LoadScene("Play");*/
    }

    private void SetLevelInfo(int levelNumber)
    {
        int targetScore = GetTargetScore(levelNumber);
        int moveCount = GetLevelMoves(levelNumber);
        string levelName = "Level" + levelNumber;
        Transform level = levelMenu.gameObject.transform.Find(levelName);
        Button playButton = level.gameObject.transform.Find("Play Button(Unlocked)").GetComponent<Button>();

        Text levelText = level.Find("Level Info").GetComponent<Text>();
        Text levelScore = level.Find("Score Info").GetComponent<Text>();

        // Set level name
        levelText.text = "Level " + levelNumber + " - " + moveCount + " Moves";

        playButton.onClick.AddListener(() =>
        {
            // Your code here for handling button click
            Debug.Log("Level " + levelNumber + " button clicked!");
            StartCoroutine(LoadPlayScene(moveCount, targetScore, levelNumber));
        });

        // Check if the level is locked or unlocked
        if (!PlayerPrefs.HasKey("level" + levelNumber))
        {
            // If locked
            levelScore.text = "Locked Level";
            level.gameObject.transform.Find("Play Button(Unlocked)").gameObject.SetActive(false);
            level.gameObject.transform.Find("Play Button(Locked)").gameObject.SetActive(true);
        }
        else
        {
            // If unlocked
            int highestScore = PlayerPrefs.GetInt("level" + levelNumber + "highest", 0);           
            if (highestScore != 0)
            {
                levelScore.text = "Highest Score: " + highestScore + " - " + " Target Score: " + targetScore;
            }
            else
            {
                levelScore.text = "No Highest Score " + " Target Score: " + targetScore;
            }
            level.gameObject.transform.Find("Play Button(Unlocked)").gameObject.SetActive(true);
            level.gameObject.transform.Find("Play Button(Locked)").gameObject.SetActive(false);
        }
    }

    private int GetLevelMoves(int levelNumber)
    {
        if (levelNumber == 2)
            return 18; 
        else if (levelNumber == 3)
            return 15;
        return 0;
    }

    private int GetTargetScore(int levelNumber)
    {
        if (levelNumber == 2)
            return 450; 
        else if (levelNumber == 3)
            return 600;
        return 0;
    }

    private IEnumerator LoadPlayScene(int moveCount, int targetScore, int levelNumber)
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        PlayerPrefs.SetInt("moveCount", moveCount);
        PlayerPrefs.SetInt("targetScore", targetScore);
        PlayerPrefs.SetInt("levelNumber", levelNumber);
        SceneManager.LoadScene("Play");
    }

    public IEnumerator OpenSettingsMenu()
    {
        yield return StartCoroutine(settingsMenu.MoveToPosition(new Vector3(0, 0)));
        Transform musicSettings = settingsMenu.gameObject.transform.Find("Music");
        Transform hintSettings = settingsMenu.gameObject.transform.Find("Hint");
        Toggle musicButton = musicSettings.gameObject.transform.Find("Button_sound").gameObject.transform.Find("Toggle").GetComponent<Toggle>();
        Toggle hintButton = hintSettings.gameObject.transform.Find("Button_hint").gameObject.transform.Find("Toggle").GetComponent<Toggle>();
        if (PlayerPrefs.GetInt("Music") == 2)
            musicButton.isOn = true;
        else
            musicButton.isOn = false;

        if (PlayerPrefs.GetInt("Hint") == 2)
            hintButton.isOn = true;
        else
            hintButton.isOn = false;
        musicButton.onValueChanged.AddListener(isOff => {
            if (isOff)
            {
                PlayerPrefs.SetInt("Music", 2);
                audioManager.StopMusic();
            }
            else
            {
                PlayerPrefs.SetInt("Music", 1);
                audioManager.PlayMusic();
            }
        });

        hintButton.onValueChanged.AddListener(isOff => {
            if (isOff)
            {
                PlayerPrefs.SetInt("Hint", 2);
            }
            else
            {
                PlayerPrefs.SetInt("Hint", 1);
            }
        });

    }

    public void PlayButtonPressed()
    {
        StartCoroutine(StartPlayGame());
    }

    public void SettingsButtonPressed()
    {
        StartCoroutine(OpenSettingsMenu());
    }

    private IEnumerator CloseLevelMenu()
    {
        yield return StartCoroutine(levelMenu.MoveToPosition(new Vector3(0, 9)));
    }

    public void CloseButtonPressedForLevel()
    {
        StartCoroutine(CloseLevelMenu());
    }

    private IEnumerator CloseSettingsMenu()
    {
        yield return StartCoroutine(settingsMenu.MoveToPosition(new Vector3(0, -9)));
    }

    public void CloseButtonPressedForSettings()
    {
        StartCoroutine(CloseSettingsMenu());
    }
}
