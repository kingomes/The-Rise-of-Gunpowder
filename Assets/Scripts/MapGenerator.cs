using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour 
{

	public enum DrawMode {NoiseMap, ColorMap, Mesh};
	public DrawMode drawMode;

	public const int mapChunkSize = 241;
	[Range(0,6)]
	public int levelOfDetail;
	public float noiseScale;

	public int octaves;
	[Range(0,1)]
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool autoUpdate;

	public TerrainType[] regions;

	public float[,] noiseMap;

	void Awake()
	{
		seed = Random.Range(int.MinValue, int.MaxValue);
		GenerateMap();
	}

	public void GenerateMap() {
		noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

		Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
		for (int y = 0; y < mapChunkSize; y++) 
		{
			for (int x = 0; x < mapChunkSize; x++) 
			{
				float currentHeight = noiseMap[x, y];
				for (int i = 0; i < regions.Length; i++) 
				{
					if (currentHeight <= regions[i].height) 
					{
						colorMap[y * mapChunkSize + x] = regions[i].color;
						break;
					}
				}
			}
		}

		MapDisplay display = FindAnyObjectByType<MapDisplay>();
		if (drawMode == DrawMode.NoiseMap) 
		{
			display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
		} 
		else if (drawMode == DrawMode.ColorMap) 
		{
			display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
		} 
		else if (drawMode == DrawMode.Mesh) 
		{
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChunkSize, mapChunkSize));
		}
	}

	public TerrainType GetTerrainAtPosition(Vector3 worldPosition) 
	{
        int x = Mathf.FloorToInt(worldPosition.x);
        int y = Mathf.FloorToInt(worldPosition.z);

        if (x < 0 || x >= mapChunkSize || y < 0 || y >= mapChunkSize)
            return default;

        float height = noiseMap[x, y];
        foreach (TerrainType region in regions) 
		{
            if (height <= region.height) 
			{
                return region;
            }
        }
        return default;
    }

	void OnValidate() 
	{
		if (lacunarity < 1) 
		{
			lacunarity = 1;
		}
		if (octaves < 0) 
		{
			octaves = 0;
		}
	}
}

[System.Serializable]
public struct TerrainType 
{
	public string name;
	public float height;
	public Color color;
}