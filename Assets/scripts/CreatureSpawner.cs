using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Place TWO of these in the scene: one with exactly 3 control point
// Transforms (quadratic - offset the middle one from the straight line
// to get a visible arc), one with exactly 4 (cubic - offset the two
// middle ones in opposite directions for an S-curve). Give both spawners
// the SAME final control point Transform so they converge on one target.
public class CreatureSpawner : MonoBehaviour
{
    [SerializeField] private GameObject creaturePrefab;
    [SerializeField] private List<Transform> controlPoints = new List<Transform>();
    [SerializeField] private float spawnInterval = 1.5f;
    [SerializeField] private int creaturesPerWave = 10;
    [SerializeField] private float travelTime = 4f;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        for (int i = 0; i < creaturesPerWave; i++)
        {
            SpawnCreature();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnCreature()
    {
        if (creaturePrefab == null || controlPoints.Count < 3)
        {
            Debug.LogWarning($"{name}: assign 3 (quadratic) or 4 (cubic) control points.");
            return;
        }

        List<Vector3> points = new List<Vector3>();
        foreach (Transform point in controlPoints) points.Add(point.position);

        GameObject creatureObj = Instantiate(creaturePrefab, points[0], Quaternion.identity);
        Creature creature = creatureObj.GetComponent<Creature>();
        if (creature != null) creature.Initialize(points, travelTime);
    }
}
