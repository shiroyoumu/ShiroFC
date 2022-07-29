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
        [MenuItem("Human/Auto Save Window", false, 2002)]
        static void Init()
        {
            saveMin = DateTime.Now.Minute;
            saveHour = DateTime.Now.Hour;
            DebugHelperWindow dhWindow = (DebugHelperWindow)EditorWindow.GetWindow(typeof(DebugHelperWindow));
            dhWindow.minSize = new Vector2(70, 18);
            dhWindow.titleContent = new GUIContent("自动保存");
            dhWindow.Show();
        }
        void OnEnable()
        {
            saveHour = curHour;
            saveMin = curMin;
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
                {
                    DoSave();
                    Repaint();
                }
                else if (curHour > saveHour && (curMin + 60) >= (saveMin + intervalTime))
                { 
                    DoSave();
                    Repaint();
                }
            }
        }
        private void DoSave()
        {
            EditorSceneManager.SaveOpenScenes();
            saveHour = curHour;
            saveMin = curMin;
        }
        public bool isAutoSave = true;
        int curMin;
        int curHour;
        static int saveMin;
        static int saveHour;
        public int intervalTime = 3;
    }
}