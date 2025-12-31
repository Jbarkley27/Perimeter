using UnityEngine;
using System.Collections.Generic;

public class PositionRing : MonoBehaviour
{
    [Header("Close Range Settings")]
    public int closeSlotCount = 6;
    public float closeRadius = 2f;

    [Header("Mid Range Settings")]
    public int midSlotCount = 10;
    public float midRadius = 4f;

    [Header("Long Range Settings")]
    public int longSlotCount = 14;
    public float longRadius = 6f;

    [HideInInspector] public List<Vector3> closeSlots = new List<Vector3>();
    [HideInInspector] public List<Vector3> midSlots = new List<Vector3>();
    [HideInInspector] public List<Vector3> longSlots = new List<Vector3>();

    private void OnDrawGizmosSelected()
    {
        GenerateAllRings();
        
        // Visualize rings in different colors
        Gizmos.color = Color.red;      // Close
        foreach (var s in closeSlots) Gizmos.DrawSphere(s, 0.5f);

        Gizmos.color = Color.yellow;   // Mid
        foreach (var s in midSlots) Gizmos.DrawSphere(s, 0.5f);

        Gizmos.color = Color.green;    // Long
        foreach (var s in longSlots) Gizmos.DrawSphere(s, 0.5f);
    }

    public void GenerateAllRings()
    {
        closeSlots = GenerateRing(closeSlotCount, closeRadius);
        midSlots = GenerateRing(midSlotCount, midRadius);
        longSlots = GenerateRing(longSlotCount, longRadius);
    }

    private List<Vector3> GenerateRing(int slotCount, float radius)
    {
        List<Vector3> generated = new List<Vector3>();
        float angleStep = 360f / slotCount;

        for (int i = 0; i < slotCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = transform.position + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            generated.Add(pos);
        }

        return generated;
    }

    // 📌 Helper Methods for External Scripts (AI / Spawners)
    public Vector3 GetRandomClose() => closeSlots[Random.Range(0, closeSlots.Count)];
    public Vector3 GetRandomMid()   => midSlots[Random.Range(0, midSlots.Count)];
    public Vector3 GetRandomLong()  => longSlots[Random.Range(0, longSlots.Count)];


    public Vector3 GetRandomSlotWithinDistance(AttackRange range, Vector3 enemyPos, Vector3 currentSlot, float maxDistance = 60f)
    {
        // Select correct ring list
        List<Vector3> ring = range switch
        {
            AttackRange.Close => closeSlots,
            AttackRange.Mid   => midSlots,
            AttackRange.Long  => longSlots,
            _ => midSlots
        };

        // Filter:
        // 1. Not the current slot
        // 2. Within distance so the enemy doesnt pick a position too far away or behind the player
        var validSlots = new List<Vector3>();
        foreach (var slot in ring)
        {
            if (slot == currentSlot) continue; // don't re-pick same node
            if (Vector3.Distance(enemyPos, slot) <= maxDistance)
                validSlots.Add(slot);
        }

        // If no valid spots, fallback to the nearest slot in range
        if (validSlots.Count == 0)
        {
            Vector3 nearest = ring[0];
            float bestDist = Mathf.Infinity;

            foreach (var slot in ring)
            {
                float distance = Vector3.Distance(enemyPos, slot);
                if (distance < bestDist)
                {
                    bestDist = distance;
                    nearest = slot;
                }
            }
            return nearest;
        }

        // Otherwise, pick a random valid one
        return validSlots[Random.Range(0, validSlots.Count)];
    }

}
