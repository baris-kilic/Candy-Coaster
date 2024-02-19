using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public Player player;
    public GameObject prefab;

    private void Start()
    {
        Instantiate(prefab);
        Instantiate(prefab);

        player = Player.Instance;
        print(player.health);
    }

}
