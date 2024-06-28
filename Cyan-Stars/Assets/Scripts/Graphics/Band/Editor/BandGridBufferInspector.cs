using CyanStars.Graphics.Band;
using UnityEditor;
using UnityEngine;

namespace CyanStars.Graphics.Editor
{
    [CustomEditor(typeof(BandGridBuffer))]
    public class BandGridBufferInspector : UnityEditor.Editor
    {
        private BandGridBuffer bandGridBuffer;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            bandGridBuffer = target as BandGridBuffer;

            if (GUILayout.Button("Generate"))
            {
                bandGridBuffer.GenerateBuffer();
            }
        }
    }
}
