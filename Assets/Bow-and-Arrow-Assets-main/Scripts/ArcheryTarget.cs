using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcheryTarget : MonoBehaviour
{
    public PointCount pointCounter;
    private bool hasScored = false;

    public void hitDecide(TargetPoints t)
    {
        if (hasScored) return;

        hasScored = true;
        t.Hit(pointCounter);

    }

    public void ResetScored()
    {
        hasScored = false;
    }
}
