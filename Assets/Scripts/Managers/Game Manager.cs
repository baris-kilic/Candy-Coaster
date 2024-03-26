using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class GameManager : Singleton<GameManager>
{
    private MatchableGrid grid;
    private MatchablePool pool;
    private AudioManager audioManager;
    private Cursor cursor;
    private CameraScript cameraScript;
    private ScoreManager scoreManager;

    [SerializeField] Text resultText;
    [SerializeField] Text scoreText;

    [SerializeField]
    private Text moveCountText;

    private int moveCount = 20;

    private int targetScore = 500;

    public int MoveCount {  get { return moveCount; } }

    [SerializeField] Movable resultsPage;

    [SerializeField] private Fader loadingScreen,
                                    darkener;

    [SerializeField] private Vector2Int dimensions;

    private void Start()
    {
        pool = (MatchablePool) MatchablePool.Instance;
        grid = (MatchableGrid) MatchableGrid.Instance;
        audioManager = AudioManager.Instance;
        cursor = Cursor.Instance;
        cameraScript = CameraScript.Instance;
        scoreManager = ScoreManager.Instance;
        
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        cursor.enabled = false;
        //Loading Screen Here
        loadingScreen.Hide(false);

        SetMoveCount(PlayerPrefs.GetInt("moveCount"));
        SetTargetScore(PlayerPrefs.GetInt("targetScore"));
        pool.PoolObjects(dimensions.x * dimensions.y * 2);
        cameraScript.ArrangeCam(new Vector2Int(dimensions.x, dimensions.y));
        grid.InitGridSystem(dimensions);
        moveCountText.text = moveCount.ToString();

        StartCoroutine(loadingScreen.Fade(0));
        audioManager.PlayMusic();
        Debug.Log("TargetScore:" + targetScore);
        Debug.Log("MoveCount:" + moveCount);

        yield return null;
        yield return StartCoroutine(grid.PopulateGrid(false, true));

        grid.CheckForMoves();
        cursor.enabled = true;
    }

    public void SetMoveCount(int moveCount)
    {
        this.moveCount = moveCount;
        moveCountText.text = moveCount.ToString();
    }

    public void SetTargetScore(int targetScore)
    {
        this.targetScore = targetScore;
    }

    public void NoMoreMoves()
    {
        //TODO: End the Game.
        GameOver();
    }

    public void GameOver()
    {
        audioManager.StopMusic();
        if (scoreManager.Score >= targetScore)
        {
            audioManager.PlaySound(SoundEffects.win);
            resultText.text = "Congratulations";
            int levelNumber = PlayerPrefs.GetInt("levelNumber") + 1;
            PlayerPrefs.SetInt("level" + levelNumber, 5);
        } else
        {
            audioManager.PlaySound(SoundEffects.lose);
            resultText.text = "No More Moves";
        }
        scoreText.text = "Score : " + scoreManager.Score.ToString();
        PlayerPrefs.SetInt(("level" + PlayerPrefs.GetInt("levelNumber") + "highest"), scoreManager.Score);
        cursor.enabled = false;

        darkener.Hide(false);
        StartCoroutine(darkener.Fade(0.75f));

        StartCoroutine(resultsPage.MoveToPosition(new Vector2(0, 0)));
    }

    private IEnumerator TryAgain()
    {
        StartCoroutine(resultsPage.MoveToPosition(new Vector2(0, -15)));
        yield return StartCoroutine(darkener.Fade(0));
        darkener.Hide(true);
        audioManager.PlayMusic();

        cursor.Reset();
        scoreManager.Reset();
        Hint.Instance.CancelHint();
        SetMoveCount(PlayerPrefs.GetInt("moveCount"));

        yield return StartCoroutine(grid.Reset());       
        cursor.enabled = true;
    }

    public void TryAgainButtonPressed()
    {
        StartCoroutine(TryAgain());
    }

    private IEnumerator Quit()
    {
        yield return StartCoroutine(loadingScreen.Fade(1));
        SceneManager.LoadScene("Main Menu");
    }

    public void QuitButtonPressed()
    {
        StartCoroutine(Quit());
    }
}
