using SFB;
using UnityEngine;

namespace CyanStars.ChartEditor
{
    public class ResourceImporter : MonoBehaviour
    {
        [ContextMenu("Test Open File Browser")]
        public void OpenFile()
        {
            ExtensionFilter[] extensions = { new ExtensionFilter(".ogg File", "ogg") };
            StandaloneFileBrowser.OpenFilePanelAsync(
                "Open File", "", extensions, false, (string[] paths) =>
                {
                    // TODO: 在这里根据已获取到的文件绝对路径，读取文件实际内容
                    Debug.Log("Open File: " + paths[0]);
                });
        }
    }
}
