using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDestructor : Singleton<TerrainDestructor>
{


    float[,] normalHeights;
    float[,,] normalAlphas;

    public AnimationCurve terrainCurve;
    public AnimationCurve colorCurve;
    public float colorMultiplier = 1f;

    private void Start()
    {
        var data = GetComponent<Terrain>().terrainData;
        normalHeights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);
        normalAlphas = data.GetAlphamaps(0, 0, data.alphamapResolution, data.alphamapResolution);
    }

    public void DestroyTerrain(Vector3 pos, float range, float delay, float depthMultiplier = 0.3f)
    {
        StartCoroutine(DestroyCoroutine(pos, range, delay, depthMultiplier, colorMultiplier));
    }

    private IEnumerator DestroyCoroutine(Vector3 pos, float range, float delay, float depthMultiplier, float colorMultiplier)
    {
        if (range < 0.2f)
            yield break;
        if (range < 1.5f)
        {
            depthMultiplier = 0f;
            colorMultiplier *= (range / 1.5f);
            range = 1.5f;
        }

        yield return new WaitForSeconds(delay);



        Terrain terrain = GetComponent<Terrain>();
        var data = terrain.terrainData;

        // Getting explosion position inside terrain space
        pos = transform.InverseTransformPoint(pos);

        int res = data.heightmapResolution;
        pos.x = pos.x * res / data.bounds.max.x;
        pos.y = pos.y / data.heightmapScale.y;
        pos.z = pos.z * res / data.bounds.max.z;

        int minX = Mathf.Max(Mathf.RoundToInt(pos.x - range), 0);
        int minZ = Mathf.Max(Mathf.RoundToInt(pos.z - range), 0);

        int maxX = Mathf.Min(Mathf.RoundToInt(pos.x + range), res - 1);
        int maxZ = Mathf.Min(Mathf.RoundToInt(pos.z + range), res - 1);

        int lenX = maxX - minX;
        int lenZ = maxZ - minZ;

        //float y = data.GetInterpolatedHeight(pos.x, pos.z);

        if (lenX <= 0 || lenZ <= 0)
            yield break;

        var heights = data.GetHeights(minX, minZ, lenX, lenZ);
        var alphas = data.GetAlphamaps(minX, minZ, lenX, lenZ);

        if (lenX > alphas.GetLength(1) || lenZ > alphas.GetLength(0))
        {
            Debug.LogWarning("OUT OF BOUND EXPLOSION");
            yield break;
        }

        for (int z = 0; z < lenZ; z++)
            for (int x = 0; x < lenX; x++)
            {
                Vector2 p = new Vector2(x - range, z - range);

                //Get the height (of a sphere centered on explosion) at location
                float height = Mathf.Sqrt((range * range) - p.sqrMagnitude);

                //If out of range, bail
                if (float.IsNaN(height))
                    continue;

                //Get explosion size and height at point
                float expSize = height / data.heightmapScale.y * depthMultiplier * terrainCurve.Evaluate(height / range);
                float currentHeight = heights[z, x];

                if (currentHeight > pos.y + expSize)
                    heights[z, x] = currentHeight - expSize * 2 * 0.8f;
                else if (currentHeight > pos.y - expSize)
                    heights[z, x] = Mathf.Lerp(currentHeight, pos.y - expSize, 0.8f);
                else
                    continue;
                //Update colors
                float ratio = height / range;

                if (ratio > 1f)
                    continue;
                if (ratio < 0f)
                    continue;
                ratio = colorCurve.Evaluate(ratio) * colorMultiplier;

                alphas[z, x, 0] *= Mathf.Max(1 - ratio, 0f);
                alphas[z, x, 1] *= Mathf.Max(1 - ratio, 0f);
                alphas[z, x, 2] *= Mathf.Max(1 - ratio, 0f);
                alphas[z, x, 3] *= Mathf.Max(1 - ratio, 0f);

                alphas[z, x, 3] += ratio;
            }

        data.SetHeights(minX, minZ, heights);
        data.SetAlphamaps(minX, minZ, alphas);
    }

    private void OnDestroy()
    {
        GetComponent<Terrain>().terrainData.SetHeights(0, 0, normalHeights);
        GetComponent<Terrain>().terrainData.SetAlphamaps(0, 0, normalAlphas);
    }
}
