using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEditor.SceneManagement;

namespace EditorFC
{
    public class DebugHelperWindow : EditorWindow
    {
        [MenuItem("Human/Debug Helper Window", false, 2002)]
        static void Init()
        {
            saveMin = DateTime.Now.Minute;
            saveHour = DateTime.Now.Hour;
            DebugHelperWindow dhWindow = (DebugHelperWindow)EditorWindow.GetWindow(typeof(DebugHelperWindow));
            dhWindow.minSize = new Vector2(70, 18);
            dhWindow.Show();
        }

        void OnGUI()
        {

            isAutoSave = EditorGUILayout.BeginToggleGroup("自动保存", isAutoSave);
            intervalTime = EditorGUILayout.IntSlider("自动保存间隔（分钟）", intervalTime, 1, 30);
            GUILayout.Label(String.Format("上次保存时间：{0}:{1}", saveHour, saveMin), EditorStyles.boldLabel);
            EditorGUILayout.EndToggleGroup();
        }

        void Update()
        {
            if (isAutoSave && !EditorApplication.isPlaying)
            {
                curMin = DateTime.Now.Minute;
                curHour = DateTime.Now.Hour;
                if (curMin >= (saveMin + intervalTime))
                    DoSave();
                else if (curHour > saveHour && (curMin + 60) >= (saveMin + intervalTime))
                    DoSave();
            }
        }

        private void DoSave()
        {
            EditorSceneManager.SaveOpenScenes();
            saveHour = curHour;
            saveMin = curMin;
        }
        /// 自动保存
        public bool isAutoSave = true;
        private int curMin;
        private int curHour;
        private static int saveMin;
        private static int saveHour;
        public int intervalTime = 3;
    }
}