#nullable enable

using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using CyanStars.Utils.RadioButton;

namespace CyanStars.Test.EditMode
{
    public class RadioButtonTests
    {
        private GameObject rootGo;
        private RadioButtonGroup group;
        private RadioButton btn1;
        private RadioButton btn2;
        private RadioButton btn3;

        [SetUp]
        public void Setup()
        {
            // 初始化测试环境，创建 GameObject 和组件
            rootGo = new GameObject("TestRoot");
            group = rootGo.AddComponent<RadioButtonGroup>();

            btn1 = CreateRadioButton("Btn1", group);
            btn2 = CreateRadioButton("Btn2", group);
            btn3 = CreateRadioButton("Btn3", group);
        }

        [TearDown]
        public void Teardown()
        {
            // 清理测试环境
            Object.DestroyImmediate(rootGo);
        }

        private RadioButton CreateRadioButton(string name, RadioButtonGroup parentGroup)
        {
            var go = new GameObject(name);
            go.transform.SetParent(rootGo.transform);
            var btn = go.AddComponent<RadioButton>();
            btn.Group = parentGroup;
            return btn;
        }

        [Test]
        public void Group_Add_AutoAssignsFirstChecked()
        {
            btn1.IsChecked = true;
            Assert.AreEqual(btn1, group.SelectedItem, "第一个被选中的按钮应该被Group记录");
            Assert.IsTrue(btn1.IsChecked);
        }

        [Test]
        public void Selection_Switch_ChangesStateCorrectly()
        {
            btn1.IsChecked = true;
            btn2.IsChecked = true; // 切换到btn2

            Assert.IsFalse(btn1.IsChecked, "旧按钮应该被取消选中");
            Assert.IsTrue(btn2.IsChecked, "新按钮应该被选中");
            Assert.AreEqual(btn2, group.SelectedItem, "Group的选中项应该更新为新按钮");
        }

        [Test]
        public void AllowSwitchOff_True_CanUncheck()
        {
            group.AllowSwitchOff = true;
            btn1.IsChecked = true;

            // 玩家点击取消选中
            btn1.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });

            Assert.IsFalse(btn1.IsChecked, "AllowSwitchOff=true时，玩家应该可以取消选中");
            Assert.IsNull(group.SelectedItem, "取消选中后，Group的SelectedItem应该为null");
        }

        [Test]
        public void AllowSwitchOff_False_CannotUncheck()
        {
            group.AllowSwitchOff = false;
            btn1.IsChecked = true;

            // 尝试通过代码取消选中
            btn1.IsChecked = false;
            Assert.IsTrue(btn1.IsChecked, "AllowSwitchOff=false时，代码不应该能取消选中唯一选中的按钮");

            // 尝试通过玩家点击取消选中
            btn1.OnPointerClick(new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left });
            Assert.IsTrue(btn1.IsChecked, "AllowSwitchOff=false时，玩家不应该能取消选中唯一选中的按钮");
        }

        // [Test]
        // public void SetIsCheckedWithoutNotify_DoesNotFireEvent()
        // {
        //     bool eventFired = false;
        //     btn1.OnValueChanged.AddListener((val) => eventFired = true);
        //
        //     btn1.SetIsCheckedWithoutNotify(true);
        //
        //     Assert.IsTrue(btn1.IsChecked, "状态应该被改变");
        //     Assert.IsFalse(eventFired, "事件不应该被触发");
        // }

        [Test]
        public void OnDisable_RemovesFromGroup_DoesNotDestroyGroupStateIfOthersExist()
        {
            btn1.IsChecked = true;
            btn2.IsChecked = false;

            // 禁用btn2
            btn2.gameObject.SetActive(false); // 会触发 OnDisable

            Assert.AreEqual(btn1, group.SelectedItem, "禁用未选中的按钮不影响当前选中状态");

            // 禁用btn1
            btn1.gameObject.SetActive(false);
            Assert.AreEqual(btn3, group.SelectedItem, "禁用了被选中的按钮后，由于 AllowSwitchOff 默认为 false，Group 应选中下一个可用的按钮");
        }
    }
}
