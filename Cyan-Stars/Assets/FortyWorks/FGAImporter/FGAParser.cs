using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FortyWorks.FGAImporter
{
    public class FGAParser
    {
        private readonly string _fgaContent;
        private readonly string _outputPath;
        private const int HeaderLength = 10;

        public FGAParser(string fgaContent, string outputPath)
        {
            _fgaContent = fgaContent;
            _outputPath = outputPath;
        }

        private float[] ParseFgaContent()
        {
            return _fgaContent
                .Replace("\r", "")
                .Replace("\n", "")
                .Split(',').Select(TextToFloat)
                .ToArray();
        }

        private float TextToFloat(string x)
        {
            float value;
            float.TryParse(x, out value);
            return value;
        }

        private Texture3D CreateEmptyTexture(float[] parsedValue)
        {
            var sizeX = (int) parsedValue.ElementAtOrDefault(0);
            var sizeY = (int) parsedValue.ElementAtOrDefault(1);
            var sizeZ = (int) parsedValue.ElementAtOrDefault(2);
            
            return new Texture3D(
                sizeX,
                sizeY,
                sizeZ,
                TextureFormat.RGBAFloat,
                false
            )
            {
                wrapMode = TextureWrapMode.Clamp
            };
        }

        public void Parse()
        {
            var allValues = ParseFgaContent();
            var vectorField = CreateEmptyTexture(allValues);
            
            var contentLength = allValues.Length - HeaderLength;
            var resultLength = Mathf.RoundToInt(contentLength / 3f);
            var mainVectors = allValues.Skip(HeaderLength - 1).ToArray();
            var colors = Enumerable.Range(0, resultLength).Select(i =>
            {
                var v = new Vector3(
                    mainVectors[i * 3 + 0],
                    mainVectors[i * 3 + 1],
                    mainVectors[i * 3 + 2]
                ).normalized;
                return new Color(v.x, v.y, v.z, 1f);
            }).ToArray();
            
            vectorField.SetPixels(colors);
            vectorField.Apply(false);
            
            AssetDatabase.CreateAsset(vectorField, _outputPath);
        }
    }
}