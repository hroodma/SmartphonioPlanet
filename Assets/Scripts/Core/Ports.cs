using UnityEngine;

public interface IBody
{
    Vector3 Position { get; }
    Vector3 Forward { get; }
    Vector3 Right { get; }
    void Apply(Vector3 velocity, Vector3 up, Vector3 forward);
}

public interface IUnitView
{
    void Render(float normalizedSpeed, bool shouldPlayInteract);
}

public interface IUnitSound
{
    void UpdateFootstepVolume(float speed);
}

public interface IPlayerInput
{
    PlayerCommand Read();
}

public interface IPlayerUI
{
    void UpdateUI(PlayerData playerData, float timer);
}

public interface IUnitFactory
{
    void SpawnWave();
}

public interface IUnitSink
{
    void Respawn(int id);
    void Uncaught(int id);
}
