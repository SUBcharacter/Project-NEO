using System.Collections.Generic;
using UnityEngine;

public class PatrolPoints : MonoBehaviour
{
    [SerializeField] private List<Vector3> points = new List<Vector3>();
    public int PointCount => points.Count;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (points.Count > 0) return; 

        foreach (Transform child in transform)
        {
            points.Add(child.position);
            child.gameObject.SetActive(false);
        }
    }

    public Vector3 GetRandomPoint()
    {
        if (points.Count == 0) return transform.position;
        return points[Random.Range(0, points.Count)];
    }

}
