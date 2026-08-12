public interface ISystem
{
    void Run(float dt);
}

public sealed class GameManager
{
    private readonly World world;
    private readonly ISystem[] fixedSystems; // физика / бой / смерть - фиксированный шаг
    private readonly ISystem[] frameSystems; // ввод / вью - кадровый шаг
    private readonly ISystem[] endedSystems; // что крутится после конца матча

    public GameManager(World world, ISystem[] fixedSystems, ISystem[] frameSystems, ISystem[] endedSystems)
    {
        this.world = world;
        this.fixedSystems = fixedSystems;
        this.frameSystems = frameSystems;
        this.endedSystems = endedSystems;
    }

    public void FixedTick(float dt)
    {
        ISystem[] pipeline = world.Match.Over ? endedSystems : fixedSystems;
        for (int i = 0; i < pipeline.Length; i++)
            pipeline[i].Run(dt);
    }

    public void FrameTick(float dt)
    {
        for (int i = 0; i < frameSystems.Length; i++)
            frameSystems[i].Run(dt);
    }
}