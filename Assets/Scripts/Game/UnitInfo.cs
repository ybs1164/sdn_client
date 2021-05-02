using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Unit", order = 3)]
public class UnitInfo : ScriptableObject
{
    public AnimationClip anime;

    public float minY;
    public float maxY;
}
