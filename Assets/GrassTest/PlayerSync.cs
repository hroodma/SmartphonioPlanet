using UnityEngine;
using static UnityEditor.PlayerSettings;

public class PlayerSync : MonoBehaviour
{
    private static readonly int PlayerPosID = Shader.PropertyToID("_PlayerPos");

    //void Update()
    //{
    //    Shader.SetGlobalVector(PlayerPosID, transform.position);

    //    Debug.Log($"Player Pos: {pos}");
    //}

    void Update()
    {
        Vector3 pos = transform.position;
        Shader.SetGlobalVector(PlayerPosID, pos);

        // Раскомментируйте эту строку, чтобы видеть позицию в консоли
        //Debug.Log($"Player Pos: {pos}");
    }
}