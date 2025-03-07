using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public Transform GetWaypoint(int waypoint)
    {
        return transform.GetChild(waypoint);
    }

    public int GetNextWaypointIndex(int currentwaypointIndex)
    {
        int nextWaypointIndex = currentwaypointIndex + 1;
        if (nextWaypointIndex == transform.childCount)
        {
            nextWaypointIndex = 0;
        }
        return nextWaypointIndex;
    }
}
