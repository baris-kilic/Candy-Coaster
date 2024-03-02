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

        pool.PoolObjects(10);
        Debug.Log(dimensions);
        grid.InitGridSystem(dimensions);

        StartCoroutine(Demo());
    }

    private IEnumerator Demo()
    {
        /*Matchable m = pool.GetPooledObject();
        m.gameObject.SetActive(true);
        Vector3 randomPosition;

        for (int i = 0; i < 10; i++) {
            randomPosition = new Vector3(Random.Range(-6f, 6f), Random.Range(-4f, 4f));
            yield return StartCoroutine(m.MoveToPosition(randomPosition));
        }*/

        gridOutput.text = grid.ToString();
        yield return new WaitForSeconds(2);

        Matchable a = pool.GetPooledObject();
        a.gameObject.SetActive(true);
        a.name = "a";

        Matchable b = pool.GetPooledObject();
        b.gameObject.SetActive(true);
        b.name = "b";

        grid.PutItemOnGrid(new Vector2Int(2, 3), a);
        grid.PutItemOnGrid(new Vector2Int(4, 1), b);

        gridOutput.text = grid.ToString();
        yield return new WaitForSeconds(2);

        grid.SwapItems(new Vector2Int(2, 3), new Vector2Int(4, 1));

        gridOutput.text = grid.ToString();
        yield return new WaitForSeconds(2);

        grid.RemoveItemFromGrid(new Vector2Int(2, 3));
        grid.RemoveItemFromGrid(new Vector2Int(4, 1));

        gridOutput.text = grid.ToString();
        yield return new WaitForSeconds(2);

        pool.ReturnObjectToPool(a);
        pool.ReturnObjectToPool(b);

        yield return null;
    }
}
