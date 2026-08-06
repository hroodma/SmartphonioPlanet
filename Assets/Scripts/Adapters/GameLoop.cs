using UnityEngine;

public sealed class GameLoop : MonoBehaviour
{
    private GameManager game;

    public void Bind(GameManager game)
    {
        this.game = game;
    }

    private void FixedUpdate()
    {
        game?.FixedTick(Time.fixedDeltaTime);
    }

    private void Update()
    {
        game?.FrameTick(Time.deltaTime);
    }
}
