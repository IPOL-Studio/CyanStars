#nullable enable

using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using CyanStarsJsonSerializer = CyanStars.Framework.File.JsonSerializer;

namespace CyanStars.Test.EditMode
{
    /// <summary>
    /// 测试 JsonSerializer 的序列化结果与异常处理
    /// </summary>
    public class JsonSerializerTest
    {
        /// <summary>
        /// 用于序列化往返测试的数据对象
        /// </summary>
        private sealed class SampleData
        {
            public string Name { get; set; } = "";

            public int Index { get; set; }

            public string[] Tags { get; set; } = Array.Empty<string>();
        }

        /// <summary>
        /// 读取属性时抛异常，用于验证序列化失败处理
        /// </summary>
        private sealed class ThrowingData
        {
            public string Broken => throw new InvalidOperationException("测试序列化失败");
        }


        [Test]
        public void TrySerializeProducesReadableJson()
        {
            var data = new SampleData { Name = "谱面", Index = 42, Tags = new[] { "a", "b" } };

            Assert.IsTrue(CyanStarsJsonSerializer.TrySerialize(data, out string json));
            Assert.IsNotEmpty(json);

            SampleData? restored = JsonConvert.DeserializeObject<SampleData>(json);
            Assert.IsNotNull(restored);
            Assert.AreEqual(data.Name, restored!.Name);
            Assert.AreEqual(data.Index, restored.Index);
            CollectionAssert.AreEqual(data.Tags, restored.Tags);
        }

        [Test]
        public void TrySerializeReportsFailure()
        {
            LogAssert.Expect(LogType.Error, new Regex("^序列化时出现异常："));

            Assert.IsFalse(CyanStarsJsonSerializer.TrySerialize(new ThrowingData(), out string json));
            Assert.AreEqual("", json);
        }
    }
}
