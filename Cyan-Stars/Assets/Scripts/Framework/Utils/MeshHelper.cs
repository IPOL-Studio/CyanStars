using UnityEngine;

namespace CyanStars.Framework.Utils
{
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
        /// 创建一个立方体Mesh
        /// </summary>
        /// <param name="width">X length</param>
        /// <param name="height">Y length</param>
        /// <param name="length">Z length</param>
        /// <param name="px">Pivot X</param>
        /// <param name="py">Pivot Y</param>
        /// <param name="pz">Pivot Z</param>
        /// <returns></returns>
        public static Mesh CreateCubeMesh(float width, float height, float length, float px, float py, float pz)
        {
            var leftWidth = Mathf.Clamp(px, 0f, width);
            var rightWidth = width - leftWidth;

            var downHeight = Mathf.Clamp(py, 0f, height);
            var upHeight = height - downHeight;

            var rearLength = Mathf.Clamp(pz, 0f, length);
            var frontLength = length - rearLength;

            return new Mesh
            {
                vertices = new[]
                {
                    new Vector3(rightWidth, upHeight, -rearLength),
                    new Vector3(rightWidth, -downHeight, -rearLength),
                    new Vector3(-leftWidth, -downHeight, -rearLength),
                    new Vector3(-leftWidth, upHeight, -rearLength),

                    new Vector3(rightWidth, upHeight, frontLength),
                    new Vector3(rightWidth, -downHeight, frontLength),
                    new Vector3(-leftWidth, -downHeight, frontLength),
                    new Vector3(-leftWidth, upHeight, frontLength),
                },
                triangles = CubeMeshTriangles
            };
        }

        /// <summary>
        /// 创建适用于hold的mesh
        /// </summary>
        /// <param name="width">hold的宽度</param>
        /// <param name="length">hold的长度</param>
        /// <returns></returns>
        public static Mesh CreateHoldMesh(float width, float length)
        {
            var halfWidth = width * 0.5f;
            return CreateCubeMesh(width, width, length, halfWidth, halfWidth, 0);
        }
    }
}
