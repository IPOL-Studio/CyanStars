using System;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    public class BranchController : MonoBehaviour
    {
        [SerializeField]
        private GameObject optionPrefab;

        // 添加硬限制，最多五个
        private (GameObject, BranchOptionView)[] optionViews = new (GameObject, BranchOptionView)[5];
        private int count;

        private void Awake()
        {
            GameRoot.Event.AddListener(CreateBranchOptionsEventArgs.EventName, CreateBranchOptions);
        }

        private void OnDestroy()
        {
            GameRoot.Event.RemoveListener(CreateBranchOptionsEventArgs.EventName, CreateBranchOptions);
        }

        private void CreateBranchOptions(object sender, EventArgs e)
        {
            var eventArgs = e as CreateBranchOptionsEventArgs;
            var options = eventArgs.Options;

            if (options.Count > count)
            {
                CreateOptionView(options.Count - count);
            }

            for (int i = 0; i < options.Count; i++)
            {
                var (go, view) = optionViews[i];
                go.SetActive(true);

                view.Text.text = options[i].Text;
                int index = i;

                view.Button.onClick.AddListener(() =>
                {
                    options[index].IsSelected = true;
                    CloseOptions();
                });
            }
        }

        private void CreateOptionView(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject go = Instantiate(optionPrefab, transform);
                BranchOptionView view = go.GetComponent<BranchOptionView>();

                optionViews[this.count] = (go, view);
                this.count++;
            }
        }

        private void CloseOptions()
        {
            for (int i = 0; i < count; i++)
            {
                var (go, view) = optionViews[i];
                go.SetActive(false);
                view.Button.onClick.RemoveAllListeners();
            }
        }
    }
}
