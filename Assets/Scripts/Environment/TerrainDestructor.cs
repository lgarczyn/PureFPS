using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDestructor : Singleton<TerrainDestructor> {


    float[,] normalHeights;
    float[,,] normalAlphas;

    public AnimationCurve terrainCurve;
    public AnimationCurve colorCurve;
    public float colorMultiplier = 1f;
    public float rangeMultiplier = 1f;

    private void Start()
    {
        var data = GetComponent<Terrain>().terrainData;
        normalHeights = data.GetHeights(0, 0, data.heightmapResolution, data.heightmapResolution);
        normalAlphas = data.GetAlphamaps(0, 0, data.alphamapResolution, data.alphamapResolution);
        DestroyTerrain(new Vector3(-67, -1212, -127), 10f, 1f, 1f);
    }

    public void DestroyTerrain(Vector3 pos, float range, float delay, float depthMultiplier = 0.3f)
    {
        StartCoroutine(DestroyCoroutine(pos, range, delay, depthMultiplier, colorMultiplier));
    }

    private IEnumerator DestroyCoroutine(Vector3 pos, float range, float delay, float depthMultiplier, float colorMultiplier)
    {
        yield return new WaitForSeconds(delay);

        Terrain terrain = GetComponent<Terrain>();
        var data = terrain.terrainData;
        int res = data.heightmapResolution;

        pos = transform.InverseTransformPoint(pos);

        pos.x = pos.x * res / data.bounds.max.x;
        pos.y = pos.y / data.heightmapScale.y;
        pos.z = pos.z * res / data.bounds.max.z;

        float rrange = Mathf.Max(range, range * rangeMultiplier);

        int minX = Mathf.Max(Mathf.RoundToInt(pos.x - rrange), 0);
        int minZ = Mathf.Max(Mathf.RoundToInt(pos.z - rrange), 0);

        int maxX = Mathf.Min(Mathf.RoundToInt(pos.x + rrange), res - 2);
        int maxZ = Mathf.Min(Mathf.RoundToInt(pos.z + rrange), res - 2);

        int lenX = maxX - minX;
        int lenZ = maxZ - minZ;

        //float y = data.GetInterpolatedHeight(pos.x, pos.z);

        if (lenX <= 0 || lenZ <= 0)
            yield break;

        var heights = data.GetHeights(minX, minZ, lenX, lenZ);
        var alphas = data.GetAlphamaps(minX, minZ, lenX, lenZ);

        for (int z = 0; z < lenZ; z++)//TODO check damn optimized order
            for (int x = 0; x < lenX; x++)
            {
                float posX = x + minX - pos.x;
                float posZ = z + minZ - pos.z;

                float height = Mathf.Sqrt((range * range) - (posX * posX) - (posZ * posZ));

                if (height > 0)
                {
                    float expSize = height / data.heightmapScale.y * depthMultiplier * terrainCurve.Evaluate(height / range);
                    float expHeight = pos.y;
                    float currentHeight = heights[z, x];

                    if (currentHeight > expHeight + expSize)
                        heights[z, x] = currentHeight - expSize * 2 * 0.8f;
                    else if (currentHeight > expHeight - expSize)
                        heights[z, x] = Mathf.Lerp(heights[z, x], expHeight - expSize, 0.8f);
                }
                if (x < alphas.GetLength(1) && z < alphas.GetLength(0))
                {
                    height = Mathf.Sqrt((range * range * rangeMultiplier * rangeMultiplier) - (posX * posX) - (posZ * posZ));

                    if (float.IsNaN(height))
                        continue;

                    float ratio = height / (range * rangeMultiplier);

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
            }

        data.SetHeights(minX, minZ, heights);
        data.SetAlphamaps(minX, minZ, alphas);

        terrain.terrainData = data;
    }

    private void OnDestroy()
    {
        GetComponent<Terrain>().terrainData.SetHeights(0, 0, normalHeights);
        GetComponent<Terrain>().terrainData.SetAlphamaps(0, 0, normalAlphas);
    }
}
