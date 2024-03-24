using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public GameObject prefab;

    private MatchableGrid grid;
    private MatchablePool pool;
    private AudioManager audioManager;
    private Cursor cursor;

    [SerializeField] private Fader loadingScreen;
    [SerializeField] private Vector2Int dimensions;

    private void Start()
    {
        pool = (MatchablePool) MatchablePool.Instance;
        grid = (MatchableGrid) MatchableGrid.Instance;
        audioManager = AudioManager.Instance;
        cursor = Cursor.Instance;
        
        Debug.Log(dimensions);
        

        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        cursor.enabled = false;
        //Loading Screen Here
        loadingScreen.Hide(false);

        pool.PoolObjects(dimensions.x * dimensions.y * 2);
        grid.InitGridSystem(dimensions);

        StartCoroutine(loadingScreen.Fade(0));
        audioManager.PlayMusic();
        
        yield return null;
        yield return StartCoroutine(grid.PopulateGrid(false, true));

        grid.CheckForMoves();
        cursor.enabled = true;
    }

    public void NoMoreMoves()
    {
        //TODO: End the Game.
    }
}
