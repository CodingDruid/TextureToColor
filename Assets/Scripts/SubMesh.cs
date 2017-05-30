﻿using UnityEngine;

/**
 * This class holds data from a mesh's submesh with its related material
 * **/
public class SubMesh
{
    public Material m_material { get; set; }
    public Mesh m_mesh { get; set; }
    public Matrix4x4 m_transformMatrix { get; set; }

    public SubMesh(Mesh mesh, Material material, Matrix4x4 transformMatrix)
    {
        m_mesh = mesh;
        m_material = material;
        m_transformMatrix = transformMatrix;
    }

    /**
    * Convert a textured mesh to a color mesh
    **/
    public void ConvertToColor()
    {

    }
}