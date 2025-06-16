using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CorrectAudioBar : MonoBehaviour
{
    public TMP_Text Text;
    public bool EnableDelete;
    public Button DeleteButton;

    private void Start()
    {
        DeleteButton.gameObject.SetActive(EnableDelete);
    }

    /// <summary>
    /// 自动接收 LoopScrollRect 回调
    /// </summary>
    /// <remarks>别删，删了会报错</remarks>
    /// <param name="index">自身元素的序号下标，会在每次进入可视范围时调用</param>
    void ScrollCellIndex(int index)
    {
    }
}
