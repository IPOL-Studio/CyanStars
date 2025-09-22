using CyanStars.ChartEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CorrectAudioBar : MonoBehaviour, ILoopScrollRectItem
{
    public TMP_Text Text;
    public bool EnableDelete;
    public Button DeleteButton;

    private void Start()
    {
        DeleteButton.gameObject.SetActive(EnableDelete);
    }

    public void ScrollCellIndex(int index)
    {
    }
}
