using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HumanAPI;
using UnityEditor;
using System;
using Multiplayer;
using UnityEngine.Profiling;
using System.Reflection;

namespace DebugHelper
{
    [DisallowMultipleComponent]
    [AddComponentMenu("ShiroFC/Debug Helper 20220517")]
    public class DH : MonoBehaviour
    {
        void Start()
        {
            tar = GameObject.CreatePrimitive(PrimitiveType.Sphere);     //落点盘物体
            line = GameObject.CreatePrimitive(PrimitiveType.Cube);      //连线物体
            DontDestroyOnLoad(tar);
            DontDestroyOnLoad(line);
            GameObject.Destroy(tar.GetComponent<SphereCollider>());     //删除碰撞器
            GameObject.Destroy(line.GetComponent<BoxCollider>());
            tar.transform.localScale = new Vector3(5, 5, 0.2f) * targetPlateSize;   //设置大小
            line.transform.localScale = new Vector3(0.2f, 0.2f, 1);
            m = new Material(Shader.Find("Standard"))		//创建材质
            {
                color = targetPlateColor
            };
            tar.GetComponent<Renderer>().material = m;      //设置材质
            line.GetComponent<Renderer>().material = m;
            tar.SetActive(false);       //隐藏
            line.SetActive(false);
            try
            {
                asm = Assembly.LoadFile(editorPath);
            }
            catch (NullReferenceException)
            { Debug.LogError("文件未找到，请检查文件路径并重新运行！"); }
            FindObj();
            SetConscious();
            SetSubtitle();
        }

        void Update()
        {
            RestartCP();
            LockC();
            DoDrop();
            SetTempCheckpoint();
            TelePort();
            TPFlying();
            ToggleFps();
            ShowNetInfo();
            ToggleBulletTime();
            GetSpeed();
            MaxGameWin();
            GetSceneCamPos();
        }

        void FixedUpdate()
        {
            GetAveSpeed();
        }

        void OnGUI()
        {
            if (showSpeed)
            {
                fontStyle2.normal.textColor = speedColor;   //设置字体颜色
                fontStyle2.fontSize = speedSize;
                GUI.Label(new Rect(5, Screen.height - 14 - speedSize * 4, 70, 30), string.Format("当前速度：{0:f1} m/s", spd), fontStyle2);
                GUI.Label(new Rect(5, Screen.height - 11 - speedSize * 3, 70, 30), string.Format("过去 {0:f0} s内平均速度：{1:f1} m/s", aveTick / 60f, accumSpeed / aveTick), fontStyle2);
                GUI.Label(new Rect(5, Screen.height - 8 - speedSize * 2, 70, 30), string.Format("最大速度：{0:f1} m/s", maxSpd), fontStyle2);
                GUI.Label(new Rect(5, Screen.height - 5 - speedSize, 70, 30), string.Format("大约可跳跃 {0:f2} m水平距离", distance), fontStyle2);
            }
        }
        void OnDrawGizmos()
        {
            if (flag2)
            {
                Gizmos.color = checkpointColor;
                Gizmos.DrawSphere(tempCP, checkpointSize);
            }
        }

        /// <summary>
        /// 初始化字幕系统
        /// </summary>
        void SetSubtitle()
        {
            SubtitleManager.instance.subtitleText.enableAutoSizing = false;
            SubtitleManager.instance.subtitleText.color = Color.white;
            SubtitleManager.instance.subtitleText.fontSize = tipsFontSize;
        }

        /// <summary>
        /// 找脚本引用
        /// </summary>
        void FindObj()
        {
            g = GameObject.Find("Game(Clone)").GetComponent<Game>();
            h = GameObject.Find("Ball").GetComponent<Human>();
            c = GameObject.Find("GameCamera(Clone)");
            n = g.transform.Find("NetGame/Canvas");
        }

        /// <summary>
        /// 获取场景摄像机位置
        /// </summary>
        void GetSceneCamPos()
        {
            if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null)
            {
                dropManPos = SceneView.lastActiveSceneView.camera.transform.position;
            }
        }

        /// <summary>
        /// 重载存档点
        /// </summary>
        void RestartCP()
        {
            if (Input.GetKeyDown(Key2Keycode(restartCheckpoint)))
            {
                g.RestartCheckpoint();
                if (showTips)
                    SubtitleManager.instance.SetSubtitle("已重载存档点", showTime);
            }
        }

        /// <summary>
        /// 切换锁定鼠标
        /// </summary>
        void LockC()
        {
            if (Input.GetKeyDown(Key2Keycode(lockCursor)))
                cursorLock = !cursorLock;
            if (!cursorLock)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        /// <summary>
        /// 放人
        /// </summary>
        void DoDrop()
        {
            if (Input.GetKeyDown(Key2Keycode(dropMan)) && Application.isPlaying)
            {
                try
                {
                    Transform transform = GameObject.Find("FreeRoamCamera/GameCamera(Clone)").transform;
                    Human.instance.gameObject.transform.position = transform.TransformPoint(new Vector3(0f, 0f, 2f));
                }
                catch (NullReferenceException)
                {
                    if (dropManPos != Vector3.zero)
                        Human.instance.gameObject.transform.position = dropManPos + new Vector3(0f, 0f, 2f);
                }
            }
        }

        /// <summary>
        /// 设置临时存档点
        /// </summary>
        void SetTempCheckpoint()
        {
            if (Input.GetKeyDown(Key2Keycode(tempSave)))
            {
                tempCP = h.transform.position;
                flag2 = true;
                if (showTips)
                    SubtitleManager.instance.SetSubtitle("已保存当前位置", showTime);
            }
            if (Input.GetKeyDown(Key2Keycode(tempLoad)) && flag2)
            {
                if (respawnOrTeleport)
                    h.SpawnAt(tempCP);
                else
                    h.transform.position = tempCP + Vector3.up;
                if (showTips)
                    SubtitleManager.instance.SetSubtitle("已回到保存的位置", showTime);
            }
        }

        /// <summary>
        /// 瞬移
        /// </summary>
        void TelePort()
        {
            if (Input.GetKey(Key2Keycode(teleport)))
            {
                isHit = Physics.Raycast(c.transform.position + new Vector3(0, 0.5f, 0), c.transform.forward + new Vector3(0, 0.2f, 0), out hit, 500, -1, QueryTriggerInteraction.Ignore);
                if (isHit && !isFlying)
                {
                    tar.SetActive(true);
                    line.SetActive(true);
                    float dis = (h.transform.position - hit.point).magnitude;
                    //Debug.DrawLine(c.transform.position, hit.point, Color.green);
                    tar.transform.position = hit.point;
                    line.transform.position = (h.transform.position - hit.point) / 2 + hit.point;
                    tar.transform.rotation = Quaternion.LookRotation(hit.normal.normalized, Vector3.up);
                    line.transform.LookAt(hit.point);
                    float scale = dis / 10;
                    if (scale <= 1)
                        tar.transform.localScale = new Vector3(1 * targetPlateSize, 1 * targetPlateSize, 0.2f);
                    if (scale >= 10)
                        tar.transform.localScale = new Vector3(10 * targetPlateSize, 10 * targetPlateSize, 0.2f);
                    if (scale > 1 && scale < 10)
                        tar.transform.localScale = new Vector3(scale * targetPlateSize, scale * targetPlateSize, 0.2f);
                    line.transform.localScale = new Vector3(0.2f, 0.2f, dis);
                }
                else
                {
                    tar.SetActive(false);
                    line.SetActive(false);
                }
            }
            if (Input.GetKeyUp(Key2Keycode(teleport)))
            {
                //Debug.DrawLine(c.transform.position, hit.point, Color.red);
                if (isHit)
                {
                    h.transform.LookAt(hit.point);
                    isFlying = true;
                }
                else if (showTips)
                    SubtitleManager.instance.SetSubtitle("无目标", showTime);
                tar.SetActive(false);
                line.SetActive(false);
            }
        }

        /// <summary>
        /// Lerp飞
        /// </summary>
        void TPFlying()
        {
            if (isFlying && flyOrTeleport)
            {
                h.transform.position = Vector3.Lerp(h.transform.position + new Vector3(0, 0.2f, 0), hit.point + new Vector3(0, 0.2f, 0), 0.2f);
                if ((h.transform.position - hit.point).magnitude < 2)
                {
                    isFlying = false;
                }
            }
            if (isFlying && !flyOrTeleport)
            {
                h.transform.position = hit.point + Vector3.up;
                isFlying = false;
            }
        }

        /// <summary>
        /// 切换FPS/TPS
        /// </summary>
        void ToggleFps()
        {
            if (Input.GetKeyDown(Key2Keycode(fps)))
            {
                for (int i = 0; i < NetGame.instance.local.players.Count; i++)
                {
                    NetPlayer netPlayer = NetGame.instance.local.players[i];
                    if (netPlayer.cameraController.mode != CameraMode.FirstPerson)
                    {
                        netPlayer.cameraController.mode = CameraMode.FirstPerson;
                        Shell.Print("fps on");
                    }
                    else
                    {
                        netPlayer.cameraController.mode = CameraMode.Far;
                        Shell.Print("fps off");
                    }
                }
            }
        }

        /// <summary>
        /// 显示网络信息
        /// </summary>
        void ShowNetInfo()
        {
            if (Input.GetKeyDown(Key2Keycode(netInfo)))
            {
                n.gameObject.SetActive(!n.gameObject.activeSelf);
            }
        }

        /// <summary>
        /// 切换子弹时间
        /// </summary>
        void ToggleBulletTime()
        {
            if (Input.GetKeyDown(Key2Keycode(bulletTime)))
            {
                isBullet = !isBullet;
                if (isBullet)
                {
                    Time.timeScale = bulletTimeScale;
                    AudioSource[] audios = GetComponent<BuiltinLevel>().gameObject.GetComponentsInChildren<AudioSource>();
                    foreach (var item in audios)
                    {
                        item.pitch *= bulletTimeScale;
                    }
                    ParticleSystem[] pars = GetComponent<BuiltinLevel>().gameObject.GetComponentsInChildren<ParticleSystem>();
                    foreach (var item in pars)
                    {
                        item.playbackSpeed *= bulletTimeScale;
                    }
                }

                else
                {
                    Time.timeScale = 1f;
                    AudioSource[] audios = GetComponent<BuiltinLevel>().gameObject.GetComponentsInChildren<AudioSource>();
                    foreach (var item in audios)
                    {
                        item.pitch /= bulletTimeScale;
                    }
                    ParticleSystem[] pars = GetComponent<BuiltinLevel>().gameObject.GetComponentsInChildren<ParticleSystem>();
                    foreach (var item in pars)
                    {
                        item.playbackSpeed /= bulletTimeScale;
                    }
                }
            }
        }

        /// <summary>
        /// 显示人物速度
        /// </summary>
        void GetSpeed()
        {
            if (h.state != HumanState.Spawning)
            {
                if (showSpeed && !isFlying)
                {
                    spd = h.velocity.magnitude; //计算速度
                                                /////////////////////////////////
                    if (Input.GetKeyDown(Key2Keycode(speedClear)))//清除数据
                    {
                        aveTick = 0;
                        accumSpeed = 0;
                        maxSpd = 0;
                    }
                    ///////////////////////////////////
                    if (spd > maxSpd)   //设置最大速度
                        maxSpd = spd;
                    distance = Mathf.Sqrt(h.velocity.x * h.velocity.x + h.velocity.z * h.velocity.z) * 0.75;    //计算跳跃距离
                }
            }
        }

        /// <summary>
        /// 显示平均速度
        /// </summary>
        void GetAveSpeed()
        {
            if (h.state != HumanState.Spawning)
            {
                if (showSpeed && !isFlying)
                {
                    accumSpeed += h.velocity.magnitude;
                    aveTick++;
                }
            }
        }

        /// <summary>
        /// Game窗口最大化
        /// </summary>
        void MaxGameWin()
        {
            if (Input.GetKeyDown(Key2Keycode(maxGameWindow)))
            {
                try
                {
                    Type t1 = asm.GetType("UnityEditor.GameView");
                    EditorWindow win = EditorWindow.GetWindow(t1);
                    win.maximized = !win.maximized;
                }
                catch (NullReferenceException)
                { Debug.LogError("文件未找到，请检查文件路径并重新运行！"); }
            }
        }

        /// <summary>
        /// 取消假死
        /// </summary>
        void SetConscious()
        {
            Type t = h.GetType();
            FieldInfo fInfo = t.GetField("maxUnconsciousTime", BindingFlags.NonPublic | BindingFlags.Instance);
            if (!isUnconscious)
                fInfo.SetValue(h, 0f);
            else
                fInfo.SetValue(h, 3f);
        }

        /// <summary>
        /// 限制按键
        /// </summary>
        /// <param name="key">输入按键</param>
        /// <returns></returns>
        KeyCode Key2Keycode(Key key)
        {
            switch (key)
            {
                case Key.None: return KeyCode.None;
                case Key.F2: return KeyCode.F2;
                case Key.F3: return KeyCode.F3;
                case Key.F4: return KeyCode.F4;
                case Key.F5: return KeyCode.F5;
                case Key.F7: return KeyCode.F7;
                case Key.F10: return KeyCode.F10;
                case Key.F11: return KeyCode.F11;
                case Key.F12: return KeyCode.F12;
                case Key.Num1: return KeyCode.Alpha1;
                case Key.Num2: return KeyCode.Alpha2;
                case Key.Num3: return KeyCode.Alpha3;
                case Key.Num4: return KeyCode.Alpha4;
                case Key.Num5: return KeyCode.Alpha5;
                case Key.Num6: return KeyCode.Alpha6;
                case Key.Num7: return KeyCode.Alpha7;
                case Key.Num8: return KeyCode.Alpha8;
                case Key.Num9: return KeyCode.Alpha9;
                case Key.Num0: return KeyCode.Alpha0;
                case Key.Pad1: return KeyCode.Keypad1;
                case Key.Pad2: return KeyCode.Keypad2;
                case Key.Pad3: return KeyCode.Keypad3;
                case Key.Pad4: return KeyCode.Keypad4;
                case Key.Pad5: return KeyCode.Keypad5;
                case Key.Pad6: return KeyCode.Keypad6;
                case Key.Pad7: return KeyCode.Keypad7;
                case Key.Pad8: return KeyCode.Keypad8;
                case Key.Pad9: return KeyCode.Keypad9;
                case Key.Pad0: return KeyCode.Keypad0;
                case Key.Backspace: return KeyCode.Backspace;
                case Key.Tab: return KeyCode.Tab;
                case Key.L_Shift: return KeyCode.LeftShift;
                case Key.R_Shift: return KeyCode.RightShift;
                case Key.L_Ctrl: return KeyCode.LeftControl;
                case Key.R_Ctrl: return KeyCode.RightControl;
                case Key.L_Alt: return KeyCode.LeftAlt;
                case Key.R_Alt: return KeyCode.RightAlt;
                case Key.Ins: return KeyCode.Insert;
                case Key.Del: return KeyCode.Delete;
                case Key.Home: return KeyCode.Home;
                case Key.End: return KeyCode.End;
                case Key.PageUp: return KeyCode.PageUp;
                case Key.PageDown: return KeyCode.PageDown;
                case Key.Q: return KeyCode.Q;
                case Key.R: return KeyCode.R;
                case Key.T: return KeyCode.T;
                case Key.U: return KeyCode.U;
                case Key.I: return KeyCode.I;
                case Key.O: return KeyCode.O;
                case Key.P: return KeyCode.P;
                case Key.F: return KeyCode.F;
                case Key.G: return KeyCode.G;
                case Key.H: return KeyCode.H;
                case Key.J: return KeyCode.J;
                case Key.K: return KeyCode.K;
                case Key.L: return KeyCode.L;
                case Key.Z: return KeyCode.Z;
                case Key.X: return KeyCode.X;
                case Key.C: return KeyCode.C;
                case Key.V: return KeyCode.V;
                case Key.B: return KeyCode.B;
                case Key.N: return KeyCode.N;
                case Key.M: return KeyCode.M;
                default: return KeyCode.None;
            }
        }

        Game g;     //Game脚本对象
        Human h;    //Human脚本对象
        GameObject c;   //摄像机物体
        GameObject tar;     //落点指示器
        GameObject line;    //连接线
        Transform n;        //网络信息
        //////////////////////////////////////////////////////////////////////
        [Tooltip("显示提示信息")]
        public bool showTips = true;
        [Tooltip("提示字体大小")]
        [Range(20, 80)]
        public int tipsFontSize = 40;       //提示字体大小
        [Tooltip("提示信息显示时间")]
        [Range(1, 5)]
        public float showTime = 3;      //提示显示时间
        //////////////////////////////////////////////////////////////////////
        [Tooltip("重载存档点快捷键")]
        public Key restartCheckpoint = Key.R;
        //////////////////////////////////////////////////////////////////////
        [Tooltip("锁定/解锁鼠标快捷键")]
        public Key lockCursor = Key.Tab;
        [Tooltip("当前鼠标状态：T：锁定；F：解锁")]
        public bool cursorLock = true;     //鼠标锁定标志
        //////////////////////////////////////////////////////////////////////
        [Tooltip("放人快捷键")]
        public Key dropMan = Key.Q;
        public Vector3 dropManPos;
        //////////////////////////////////////////////////////////////////////
        [Tooltip("临时保存快捷键")]
        public Key tempSave = Key.Num3;
        [Tooltip("临时读取快捷键")]
        public Key tempLoad = Key.Num4;
        Vector3 tempCP;         //临时保存点
        bool flag2;         //是否启用保存点
        [Tooltip("生成模式：T：正常重生，F：传送")]
        public bool respawnOrTeleport = false;      //T：正常重生，F：传送
        [Tooltip("保存点颜色（仅在Scene面板中显示）")]
        public Color checkpointColor = Color.green;
        [Tooltip("保存点大小")]
        [Range(0, 3)]
        public float checkpointSize = 1;
        //////////////////////////////////////////////////////////////////////
        [Tooltip("传送快捷键")]
        public Key teleport = Key.F;
        RaycastHit hit;
        bool isHit = false;     //命中标志
        bool isFlying = false;
        [Tooltip("传送模式：T：飞行；F：传送")]
        public bool flyOrTeleport = true;       //T：飞行；F：传送
        [Tooltip("落点指示器大小")]
        [Range(1, 3)]
        public float targetPlateSize = 1;   //落点指示器大小
        Material m;
        [Tooltip("落点指示器颜色")]
        public Color targetPlateColor = Color.red;
        //////////////////////////////////////////////////////////////////////
        [Tooltip("第一、第三人称切换快捷键")]
        public Key fps = Key.V;
        //////////////////////////////////////////////////////////////////////
        [Tooltip("显示网络信息快捷键")]
        public Key netInfo = Key.F2;
        //////////////////////////////////////////////////////////////////////
        [Tooltip("子弹时间快捷键")]
        public Key bulletTime = Key.B;
        bool isBullet = false;
        [Tooltip("子弹时间速度")]
        [Range(0, 2)]
        public float bulletTimeScale = 0.5f;
        //////////////////////////////////////////////////////////////////////
        [Tooltip("是否开启速度显示")]
        public bool showSpeed = true;
        [Tooltip("速度显示清零")]
        public Key speedClear = Key.P;
        GUIStyle fontStyle2 = new GUIStyle();
        [Tooltip("速度显示字体颜色")]
        public Color speedColor = Color.white;
        [Tooltip("速度显示字体大小")]
        [Range(10, 30)]
        public int speedSize = 18;
        float spd;  //当前速度
        int aveTick;    //节拍
        float accumSpeed;   //累积速度
        float maxSpd;   //最大速度
        double distance;    //跳跃距离
        //////////////////////////////////////////////////////////////////////
        [Tooltip("最大化Game面板快捷键")]
        public Key maxGameWindow = Key.M;
        Assembly asm;
        public string editorPath = @"W:\Unity2017.4.13f1\Editor\Data\Managed\UnityEditor.dll";
        /////////////////////////////////////////////////////////////////////
        [Tooltip("是否落地眩晕")]
        public bool isUnconscious = false;

        public enum Key
        {
            None = KeyCode.None,
            F2 = KeyCode.F2,
            F3 = KeyCode.F3,
            F4 = KeyCode.F4,
            F5 = KeyCode.F5,
            F7 = KeyCode.F7,
            F10 = KeyCode.F10,
            F11 = KeyCode.F11,
            F12 = KeyCode.F12,
            Num1 = KeyCode.Alpha1,
            Num2 = KeyCode.Alpha2,
            Num3 = KeyCode.Alpha3,
            Num4 = KeyCode.Alpha4,
            Num5 = KeyCode.Alpha5,
            Num6 = KeyCode.Alpha6,
            Num7 = KeyCode.Alpha7,
            Num8 = KeyCode.Alpha8,
            Num9 = KeyCode.Alpha9,
            Num0 = KeyCode.Alpha0,
            Pad1 = KeyCode.Keypad1,
            Pad2 = KeyCode.Keypad2,
            Pad3 = KeyCode.Keypad3,
            Pad4 = KeyCode.Keypad4,
            Pad5 = KeyCode.Keypad5,
            Pad6 = KeyCode.Keypad6,
            Pad7 = KeyCode.Keypad7,
            Pad8 = KeyCode.Keypad8,
            Pad9 = KeyCode.Keypad9,
            Pad0 = KeyCode.Keypad0,
            Backspace = KeyCode.Backspace,
            Tab = KeyCode.Tab,
            L_Shift = KeyCode.LeftShift,
            R_Shift = KeyCode.RightShift,
            L_Ctrl = KeyCode.LeftControl,
            R_Ctrl = KeyCode.RightControl,
            L_Alt = KeyCode.LeftAlt,
            R_Alt = KeyCode.RightAlt,
            Ins = KeyCode.Insert,
            Del = KeyCode.Delete,
            Home = KeyCode.Home,
            End = KeyCode.End,
            PageUp = KeyCode.PageUp,
            PageDown = KeyCode.PageDown,
            Q = KeyCode.Q,
            R = KeyCode.R,
            T = KeyCode.T,
            U = KeyCode.U,
            I = KeyCode.I,
            O = KeyCode.O,
            P = KeyCode.P,
            F = KeyCode.F,
            G = KeyCode.G,
            H = KeyCode.H,
            J = KeyCode.J,
            K = KeyCode.K,
            L = KeyCode.L,
            Z = KeyCode.Z,
            X = KeyCode.X,
            C = KeyCode.C,
            V = KeyCode.V,
            B = KeyCode.B,
            N = KeyCode.N,
            M = KeyCode.M,
        }
    }
}