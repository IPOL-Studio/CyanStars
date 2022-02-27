using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshHelper
{
    private static readonly int[] CubeMeshTriangles = new[]
    {
        0, 1, 2,
        0, 2, 3,
        5, 4, 7,
        5, 7, 6,

        4, 0, 3,
        4, 3, 7,
        1, 5, 6,
        1, 6, 2,

        4, 5, 1,
        4, 1, 0,
        3, 2, 6,
        3, 6, 7
    };

    /// <summary>
    /// 创建适用于hold的mesh
    /// </summary>
    /// <param name="width">hold的宽度</param>
    /// <param name="length">hold的长度</param>
    /// <returns></returns>
    public static Mesh CreateHoldMesh(float width, float length)
    {
        var halfWidth = width * 0.5f;
        // 此处可重用，如果有需要的话
        var mesh = new Mesh
        {
            vertices = new[]
            {
                new Vector3(halfWidth, halfWidth, 0),
                new Vector3(halfWidth, -halfWidth, 0),
                new Vector3(-halfWidth, -halfWidth, 0),
                new Vector3(-halfWidth, halfWidth, 0),

                new Vector3(halfWidth, halfWidth, length),
                new Vector3(halfWidth, -halfWidth, length),
                new Vector3(-halfWidth, -halfWidth, length),
                new Vector3(-halfWidth, halfWidth, length),
            },
            triangles = CubeMeshTriangles
        };
        return mesh;
    }
}
