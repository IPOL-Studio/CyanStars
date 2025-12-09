using CyanStars.Framework;
using CyanStars.GamePlay.ChartEditor.Procedure;
using UnityEngine;
using UnityEngine.UI;

public class TEST : MonoBehaviour
{
    private Button button;

    private void Awake()
    {
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(GameRoot.ChangeProcedure<ChartEditorProcedure>);
    }
}
