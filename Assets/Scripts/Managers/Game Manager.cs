using System.Collections;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager>
{
    public Player player;
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
        /*Matchable m = pool.GetPooledObject();
        m.gameObject.SetActive(true);
        Vector3 randomPosition;

        for (int i = 0; i < 10; i++) {
            randomPosition = new Vector3(Random.Range(-6f, 6f), Random.Range(-4f, 4f));
            yield return StartCoroutine(m.MoveToPosition(randomPosition));
        }*/

        //Loading Screen Here
        pool.PoolObjects(dimensions.x * dimensions.y * 2);
        grid.InitGridSystem(dimensions);
        

        yield return null;

        StartCoroutine(grid.PopulateGrid(false, true));
    }
}
