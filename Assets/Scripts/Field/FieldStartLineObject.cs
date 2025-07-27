using System.Collections.Generic;
using UnityEngine;

public class FieldStartLineObject : MonoBehaviour
{
    [SerializeField] List<Transform> Pos5;
    [SerializeField] List<Transform> Pos4;
    [SerializeField] List<Transform> Pos3;
    [SerializeField] List<Transform> Pos2;
    [SerializeField] Transform Pos1;
    public Vector3 GetStartPoint(int playerCount, int playerIndex)
    {
        switch(playerCount)
        {
            case 5:
                return Pos5[playerIndex].position;
            case 4:
                return Pos4[playerIndex].position;
            case 3:
                return Pos3[playerIndex].position;
            case 2:
                return Pos2[playerIndex].position;
            default:
                return Pos1.position;
        }
    }
}
