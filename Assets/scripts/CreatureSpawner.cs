using UnityEngine;
using System.Collections.Generic;

public class CreatureSpawner : MonoBehaviour
{
    [SerializeField] private GameObject creaturePrefab;
    [SerializeField] private List<Transform> waypoints = new List<Transform>();

    public Creature Spawn(EnemyTypeData type, int coinMin, int coinMax)
    {
        GameObject prefab = type.prefab != null ? type.prefab : creaturePrefab;
        if (prefab == null || waypoints.Count < 2)
        {
            Debug.LogWarning($"{name}: assign a creature prefab and at least 2 waypoints.");
            return null;
        }

        List<Vector3> points = new List<Vector3>();
        foreach (Transform point in waypoints) points.Add(point.position);

        GameObject creatureObj = ObjectPool.Get(prefab, points[0], Quaternion.identity);
        creatureObj.transform.localScale = prefab.transform.localScale * type.scale;

        Creature creature = creatureObj.GetComponent<Creature>();
        if (creature != null) creature.Initialize(points, type.speed, type.hp, coinMin, coinMax, type.tint);
        return creature;
    }
}