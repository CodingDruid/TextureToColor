using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TextureToColor : MonoBehaviour
{
    public Material m_opaqueColorUnlitMaterial;

    public void Start()
    {
        ConvertChildrenMeshes();
    }

    /**
    * Convert the textured meshes the user placed under this object as children into fully colored meshes
    **/
    public void ConvertChildrenMeshes()
    {
        MeshFilter[] meshFilters = this.GetComponentsInChildren<MeshFilter>();
        GameObject convertedMeshes = new GameObject("ConvertedMeshes");
        convertedMeshes.transform.parent = this.transform;

        for (int i = 0; i != meshFilters.Length; i++)
        {
            Mesh childMesh = meshFilters[i].mesh;
            Vector3[] vertices = childMesh.vertices;
            int[] triangles = childMesh.triangles;

            //First duplicate vertices
            Mesh outputMesh = new Mesh();
            outputMesh.name = childMesh.name;
            Vector3[] outputVertices = new Vector3[triangles.Length];

            for (int p = 0; p != triangles.Length; p++)
            {
                outputVertices[p] = vertices[triangles[p]];
            }

            //triangles are simply n consecutive integers
            int[] outputTriangles = new int[triangles.Length];
            for (int t = 0; t != triangles.Length; t++)
            {
                outputTriangles[t] = t;
            }

            //colors
            Material[] materials = meshFilters[i].GetComponent<MeshRenderer>().materials;
            List<Color> outputColors = new List<Color>();
            for (int m = 0; m != materials.Length; m++)
            {
                PopulateColorsArrayForSubmesh(ref outputColors, childMesh, m, materials[m]);
            }

            outputMesh.vertices = outputVertices;
            outputMesh.triangles = outputTriangles;
            outputMesh.colors = outputColors.ToArray();

            GameObject convertedMeshObject = new GameObject(meshFilters[i].name);
            convertedMeshObject.transform.parent = convertedMeshes.transform;
            convertedMeshObject.transform.localScale = meshFilters[i].transform.localScale;
            convertedMeshObject.transform.position = meshFilters[i].transform.position + new Vector3(0, 0, -1);
            MeshFilter meshFilter = convertedMeshObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = convertedMeshObject.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = outputMesh;
            meshRenderer.sharedMaterial = m_opaqueColorUnlitMaterial;

            //Copy the mesh inside the Assets folder
            AssetDatabase.CreateAsset(outputMesh, "Assets/" + outputMesh.name + ".asset");
            AssetDatabase.SaveAssets();
        }
    }

    private void PopulateColorsArrayForSubmesh(ref List<Color> colors, Mesh mesh, int submeshIndex, Material material)
    {
        Texture2D texture = (Texture2D)material.mainTexture;
        
        int[] triangles = mesh.GetTriangles(submeshIndex);
        Vector2[] uvs = mesh.uv;
        
        Vector2[] triangleUVs = new Vector2[3];

        for (int i = 0; i != triangles.Length; i += 3)
        {            
            triangleUVs[0] = uvs[triangles[i]];
            triangleUVs[1] = uvs[triangles[i + 1]];
            triangleUVs[2] = uvs[triangles[i + 2]];
            Color triangleColor = GetTriangleAverageColor(texture, triangleUVs);

            colors.Add(triangleColor);
            colors.Add(triangleColor);
            colors.Add(triangleColor);
        }
    }

    private struct Triangle
    {
        public Vector2 m_pointA;
        public Vector2 m_pointB;
        public Vector2 m_pointC;
        public Vector2 m_center;

        public Triangle(Vector2 A, Vector2 B, Vector2 C)
        {
            m_pointA = A;
            m_pointB = B;
            m_pointC = C;

            m_center = (A + B + C) / 3.0f;
        }

        public Triangle[] Tesselate()
        {
            Triangle[] tesselatedTriangles = new Triangle[3];
            tesselatedTriangles[0] = new Triangle(m_pointA, m_center, m_pointB);
            tesselatedTriangles[1] = new Triangle(m_pointB, m_center, m_pointC);
            tesselatedTriangles[2] = new Triangle(m_pointC, m_center, m_pointA);

            return tesselatedTriangles;
        }

        public float GetArea()
        {
            return Mathf.Abs(0.5f * Determinant(m_pointB - m_pointA, m_pointC - m_pointA));
        }

        public void GetPixelAtVertex(int vertexIndex, out int x, out int y)
        {
            if (vertexIndex == 0)
            {
                x = Mathf.RoundToInt(m_pointA.x);
                y = Mathf.RoundToInt(m_pointA.y);
            }
            else if (vertexIndex == 1)
            {
                x = Mathf.RoundToInt(m_pointB.x);
                y = Mathf.RoundToInt(m_pointB.y);
            }
            else
            {
                x = Mathf.RoundToInt(m_pointC.x);
                y = Mathf.RoundToInt(m_pointC.y);
            }
        }

        public void GetPixelAtCenter(out int x, out int y)
        {
            x = Mathf.RoundToInt(m_center.x);
            y = Mathf.RoundToInt(m_center.y);
        }

        /**
        * Compute the determinant of 2 vectors
        **/
        private float Determinant(Vector2 u, Vector2 v)
        {
            return u.x * v.y - u.y * v.x;
        }

        public override string ToString()
        {
            return "(" + m_pointA + ", " + m_pointB + ", " + m_pointC + ")";
        }
    }

    private struct Pixel
    {
        public int X;
        public int Y;

        public Pixel(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Pixel))
                return false;

            Pixel other = (Pixel)obj;

            return other.X == X && other.Y == Y;
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ")";
        }
    }

    /**
    * Compute the average color of this triangle by tesselating the given triangle and picking the color of each triangle center
    * The more tesselation steps we have, the more precise the color will be
    **/
    private Color GetTriangleAverageColor(Texture2D texture, Vector2[] triangleUVs)
    {
        //first compute the area of the triangle in pixel size
        for (int i = 0; i != triangleUVs.Length; i++)
        {
            triangleUVs[i].Scale(new Vector2(texture.width, texture.height));
        }

        Triangle triangle = new Triangle(triangleUVs[0], triangleUVs[1], triangleUVs[2]);

        Triangle[] currentTriangles = new Triangle[1];
        currentTriangles[0] = triangle;

        //store here pixels that have been sampled
        List<Pixel> sampledPixels = new List<Pixel>();

        //pick the color at first triangle vertices
        int vertexX, vertexY;
        triangle.GetPixelAtVertex(0, out vertexX, out vertexY);
        sampledPixels.Add(new Pixel(vertexX, vertexY));
        triangle.GetPixelAtVertex(1, out vertexX, out vertexY);
        sampledPixels.Add(new Pixel(vertexX, vertexY));
        triangle.GetPixelAtVertex(2, out vertexX, out vertexY);
        sampledPixels.Add(new Pixel(vertexX, vertexY));

        //pick the color at first triangle center
        int centerX, centerY;
        triangle.GetPixelAtCenter(out centerX, out centerY);
        sampledPixels.Add(new Pixel(centerX, centerY));

        int maxTesselationSteps = 3;
        int currentTesselationStep = 0;

        //tesselate that first triangle and pick color on tesselated triangles centers
        while (currentTesselationStep < maxTesselationSteps)
        {
            Triangle[] tesselatedTriangles = new Triangle[3 * currentTriangles.Length];
            for (int i = 0; i != currentTriangles.Length; i++)
            {
                Triangle[] triangles =  currentTriangles[i].Tesselate();

                //get the color at the center of these tesselated triangles
                triangles[0].GetPixelAtCenter(out centerX, out centerY);
                sampledPixels.Add(new Pixel(centerX, centerY));
                triangles[1].GetPixelAtCenter(out centerX, out centerY);
                sampledPixels.Add(new Pixel(centerX, centerY));
                triangles[2].GetPixelAtCenter(out centerX, out centerY);
                sampledPixels.Add(new Pixel(centerX, centerY));

                //copy them into the global list
                tesselatedTriangles[3 * i] = triangles[0];
                tesselatedTriangles[3 * i + 1] = triangles[1];
                tesselatedTriangles[3 * i + 2] = triangles[2];
            }
            
            currentTriangles = tesselatedTriangles;
            currentTesselationStep++;
        }      

        //remove duplicate elements
        HashSet<Pixel> uniquePixels = new HashSet<Pixel>(sampledPixels);

        //compute the average color
        Color averageColor = new Color(0, 0, 0, 0);
        for (int p = 0; p != uniquePixels.Count; p++)
        {
            averageColor += texture.GetPixel(sampledPixels[p].X, sampledPixels[p].Y);
        }
        
        return averageColor / (float)uniquePixels.Count; ;  
    }
}
