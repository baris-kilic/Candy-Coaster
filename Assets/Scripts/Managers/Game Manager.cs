using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public GameObject prefab;

    private MatchableGrid grid;
    private MatchablePool pool;

    [SerializeField] private Vector2Int dimensions;
    [SerializeField] private Text gridOutput;


    private void Start()
    {
        pool = (MatchablePool) MatchablePool.Instance;
        grid = (MatchableGrid) MatchableGrid.Instance;

        
        Debug.Log(dimensions);
        

        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        //Loading Screen Here
        pool.PoolObjects(dimensions.x * dimensions.y * 2);
        grid.InitGridSystem(dimensions);
        

        yield return null;

        StartCoroutine(grid.PopulateGrid(false, true));

        grid.CheckForMoves();
    }

    public void NoMoreMoves()
    {
        //TODO: End the Game.
    }
}
