using System.Collections.Generic;
using System.Threading.Tasks;
using CyanStars.Framework;
using CyanStars.Framework.Dialogue;
using CyanStars.Framework.Timer;
using UnityEngine;

namespace CyanStars.Gameplay.Dialogue
{
    public class BranchChooserImpl : MonoBehaviour, IBranchChooser
    {
        private struct SelectTaskCompletionSource
        {
            public TaskCompletionSource<BranchOption> Value;

            public bool IsCompleted => Value.Task.IsCompleted;
            public bool HasValue => Value != null;

            public SelectTaskCompletionSource(TaskCompletionSource<BranchOption> value)
            {
                Value = value;
            }

            public void SetResultAndDiscard(BranchOption result)
            {
                Value.SetResult(result);
                Value = null;
            }

            public void DiscardIfCompleted()
            {
                if (HasValue && IsCompleted)
                {
                    Value = null;
                }
            }
        }

        [SerializeField]
        private GameObject optionPrefab;

        // 添加硬限制，最多五个
        private (GameObject, BranchOptionView)[] optionViews = new (GameObject, BranchOptionView)[5];
        private int count;

        private int selectedIndex = -1;
        private IList<BranchOption> curBranchOptions;

        private SelectTaskCompletionSource tcs;

        private void Start()
        {
            GameRoot.Dialogue.RegisterOrReplaceService<IBranchChooser>(this);
        }

        private void OnDestroy()
        {
            GameRoot.Dialogue.UnregisterService<IBranchChooser>();
        }

        public void OnRegister()
        {
            GameRoot.Timer.UpdateTimer.Add(OnUpdate);
        }

        public void OnUnregister()
        {
            GameRoot.Timer.UpdateTimer.Remove(OnUpdate);
        }

        public Task ShowOptionsAsync(IList<BranchOption> options)
        {
            if (options.Count > count)
            {
                CreateOptionView(options.Count - count);
            }

            for (int i = 0; i < options.Count; i++)
            {
                var (go, view) = optionViews[i];
                go.SetActive(true);

                view.Text.text = options[i].Text;
            }

            curBranchOptions = options;

            tcs = new SelectTaskCompletionSource(new TaskCompletionSource<BranchOption>());
            if (curBranchOptions.Count <= 0)
            {
                tcs.Value.SetResult(null);
            }

            return Task.CompletedTask;
        }

        public Task<BranchOption> GetSelectOptionAsync()
        {
            var task = tcs.Value.Task;
            tcs.DiscardIfCompleted();
            return task;
        }

        private void CreateOptionView(int count)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject go = Instantiate(optionPrefab, transform);
                BranchOptionView view = go.GetComponent<BranchOptionView>();

                var index = this.count;
                optionViews[index] = (go, view);
                view.Button.onClick.AddListener(() => selectedIndex = index);
                this.count++;
            }
        }

        private void CloseOptions()
        {
            for (int i = 0; i < count; i++)
            {
                optionViews[i].Item1.SetActive(false);
            }
        }

        private void OnUpdate(float deltaTime, object userdata)
        {
            if (tcs.HasValue && !tcs.IsCompleted && selectedIndex >= 0)
            {
                var option = curBranchOptions[selectedIndex];
                curBranchOptions = null;
                selectedIndex = -1;
                CloseOptions();
                tcs.SetResultAndDiscard(option);
            }
        }
    }
}
