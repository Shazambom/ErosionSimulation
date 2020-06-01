using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class GenerateTerrain : MonoBehaviour
{
    public int x_scale = 20;
    public int z_scale = 20;

    public float amplitude_a = .1f;
    public float amplitude_b = .2f;
    public float amplitude_c = .3f;
    public float amplitude_d = .4f;
    public float amplitude_e = .5f;
    
    public float scale_a = 5f;
    public float scale_b = 4f;
    public float scale_c = 3f;
    public float scale_d = 2f;
    public float scale_e = 1f;

    private Vector3[] heightmap;
    private Vector3[] flow;
    private Mesh planeMesh;
    private int[] tris;
    private int[][][] neighbors;
    private Color[] colors;

    // Start is called before the first frame update
    void Start()
    {
        planeMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = planeMesh;
        neighbors = GenerateNeighborNetwork();

        CreateShape();
        flow = GetFlowGraph();
        colors = new Color[flow.Length];
        for (int i = 0; i < flow.Length; i++) {
            colors[i] = Color.Lerp(Color.gray, Color.red, flow[i].y);
            Debug.Log(flow[i].y);
            Debug.Log(colors[i]);
        }


        UpdateMesh();

    }


    void CreateShape() {
        heightmap = new Vector3[(x_scale + 1) * (z_scale + 1)];
        int heightCount = 0;
        for (int z = 0; z <= z_scale; z++) {
            for (int x = 0; x <= x_scale; x++) {
                float sample = getModifiedPerlinNoise(x, z);
                heightmap[heightCount] = new Vector3(x, sample, z);
                heightCount++;
            }
        }
        tris = getTri();
    }

    void UpdateMesh() {
        planeMesh.vertices = heightmap;
        planeMesh.triangles = tris;
        planeMesh.colors = colors;
        planeMesh.RecalculateNormals();
        planeMesh.RecalculateTangents();
        planeMesh.RecalculateBounds();


    }
    int[] getTri() {
        int[] triangles = new int[6 * z_scale * x_scale];

        int vert = 0;
        int tris = 0;
        for (int z = 0; z < z_scale; z++) {
            for (int x = 0; x < x_scale; x++) {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + x_scale + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + x_scale + 1;
                triangles[tris + 5] = vert + x_scale + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        return triangles;
    }
    float getModifiedPerlinNoise(int x, int y) {
        float a = Mathf.PerlinNoise(x * amplitude_a, y * amplitude_a) * scale_a;
        float b = Mathf.PerlinNoise(x * amplitude_b, y * amplitude_b) * scale_b;
        float c = Mathf.PerlinNoise(x * amplitude_c, y * amplitude_c) * scale_c;
        float d = Mathf.PerlinNoise(x * amplitude_d, y * amplitude_d) * scale_d;
        float e = Mathf.PerlinNoise(x * amplitude_e, y * amplitude_e) * scale_e;

        return (a + b + c + d + e) - ((scale_a + scale_b + scale_c + scale_d + scale_e) / 2);

    }


    void SimulateRainFall() {



        UpdateMesh();
    }

    Vector3[] GetFlowGraph() {
        Vector3[] flowDirection = new Vector3[(x_scale + 1) * (z_scale + 1)];
        int heightCount = 0;
        for (int z = 0; z <= z_scale; z++) {
            for (int x = 0; x <= x_scale; x++) {
                int[] index = GetNeighbors(x, z);
                Vector3 best = heightmap[heightCount];
                int bestIndex = heightCount;
                for (int i = 0; i < 8; i++) {
                    Vector3 current = heightmap[index[i]];
                    if (best.y > current.y) {
                        best = current;
                        bestIndex = index[i];
                    }
                }

                float delta = heightmap[heightCount].y - best.y;

                // We need next index
                // Max erosion value


                flowDirection[heightCount] = new Vector3(bestIndex, delta, 0);

                heightCount++;
            }
        }
        return flowDirection;
    }


    int[] GetNeighbors(int x, int z) {
        return neighbors[x][z];
    }

    int[][][] GenerateNeighborNetwork() {
        neighbors = new int[x_scale + 1][][];
        for (int x = 0; x <= x_scale; x++) {
            neighbors[x] = new int[z_scale + 1][];
            for (int z = 0; z <= z_scale; z++) {
                neighbors[x][z] = new int[8];
                neighbors[x][z][0] = mod((x - 1), x_scale) + mod((z - 1), z_scale) * x_scale;
                neighbors[x][z][1] = mod((x - 1), x_scale) + (z) * x_scale;
                neighbors[x][z][2] = mod((x - 1), x_scale) + mod((z + 1), z_scale) * x_scale;
                neighbors[x][z][3] = (x) + mod((z + 1), z_scale) * x_scale;
                neighbors[x][z][4] = mod((x + 1), x_scale) + mod((z + 1), z_scale) * x_scale;
                neighbors[x][z][5] = mod((x + 1), x_scale) + (z) * x_scale;
                neighbors[x][z][6] = mod((x + 1), x_scale) + mod((z - 1), z_scale) * x_scale;
                neighbors[x][z][7] = (x) + mod((z - 1), z_scale) * x_scale;
                //Debug.Log(string.Join("", new List<int>(neighbors[x][z]).ConvertAll(i => i.ToString() + " ").ToArray()));
            }
           
        }
        return neighbors;
    }

    int mod(int x, int m) {
        return (x % m + m) % m;
    }
    

    // Update is called once per frame
    void Update()
    {
        
    }
}
