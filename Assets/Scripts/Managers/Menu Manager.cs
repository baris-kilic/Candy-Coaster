using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private Fader loadingScreen;

    private void Start()
    {
        loadingScreen.Hide(false);
        StartCoroutine(loadingScreen.Fade(0));
    }

    private IEnumerator StartPlayGame()
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        PlayerPrefs.SetInt("moveCount", 20);
        PlayerPrefs.SetInt("targetScore", 400);
        SceneManager.LoadScene("Play");
    }

    public void PlayButtonPressed()
    {
        StartCoroutine(StartPlayGame());
    }

    public void SettingsButtonPressed()
    {
        SceneManager.LoadScene("Settings");
    }

}
