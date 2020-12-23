using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using NUnit.Framework;
using NUnit.Framework.Interfaces;


namespace toio.Tests
{
    public class TestSelectView : MonoBehaviour
    {
        private static GameObject res_button = null;
        private GameObject contentObject = null;

        public ITest testRoot = null;
        public List<ITest> tests = new List<ITest>();

        public bool IsFinished { get; private set; }

        public void StopView()
        {
            IsFinished = true;
        }

        async UniTask Awake()
        {
            // リソース読み込み
            if (null == res_button)
            {
                res_button = await Resources.LoadAsync<GameObject>("ViewButton") as GameObject;
                await UniTask.WaitForFixedUpdate();
            }

            // オブジェクト生成
            this.contentObject = GameObject.Find("Content");

            // ボタン追加
            try
            {
                foreach(var t in this.tests)
                {
                    var buttonObj = GameObject.Instantiate(res_button);
                    await UniTask.WaitForFixedUpdate();
                    buttonObj.transform.SetParent(this.contentObject.transform);
                    buttonObj.GetComponent<RectTransform>().localScale = Vector3.one;
                    var button = buttonObj.GetComponent<UnityEngine.UI.Button>();
                    var txt = button.GetComponentInChildren<Text>();
                    txt.text = t.Parent.Name + "/" + t.Name;
                    button.onClick.AddListener(async () =>
                    {
                        var testcase = (t.Parent.Fixture as CubeTestCase);
                        await (IEnumerator)testcase.UnitySetUp();
                        await (IEnumerator)t.Method.Invoke(t.Parent.Fixture, null);
                        await (IEnumerator)testcase.UnityTearDown();
                    });
                }
            }
            catch(Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}
