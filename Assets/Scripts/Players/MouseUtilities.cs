using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Entities;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using System;


namespace Zoxel
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    public class MouseUtilities
    {
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }
        public struct RECT
        {
            public int Left, Top, Right, Bottom;

            public override string ToString()
            {
                return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
            }
        }


        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern bool SetWindowPos(IntPtr hwnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);


        // [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        // private static extern Rect GetWindowRect(IntPtr hwnd, Rect lpRect);
        public enum SystemMetric : int
        {
            SM_CXSCREEN = 0,  // 0x00
            SM_CYSCREEN = 1,  // 0x01
            SM_CXVSCROLL = 2,  // 0x02
            SM_CYHSCROLL = 3,  // 0x03
            SM_CYCAPTION = 4,  // 0x04
            SM_CXBORDER = 5,  // 0x05
            SM_CYBORDER = 6,  // 0x06
                              // [...] shortened ...
        }
        [DllImport("user32.dll")]
        static extern int GetSystemMetrics(SystemMetric smIndex);
        [DllImport("user32.dll")]
        static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(System.String className, System.String windowName);

        public static void SetPosition(int x, int y, int resX = 0, int resY = 0)
        {
            SetWindowPos(FindWindow(null, "My window title"), 0, x, y, resX, resY, resX * resY == 0 ? 1 : 0);
        }

        public static Rect GetGameViewPosition()
        {
#if UNITY_EDITOR
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");  //UnityEditor
            UnityEditor.EditorWindow gameview = UnityEditor.EditorWindow.GetWindow(T);
            return gameview.position;   // gameview.position;
#else
            return new Rect();
#endif
        }
        public static Vector2 PixelOffsetFromScreen()
        {
            RECT WinR;
            GetWindowRect(GetActiveWindow(), out WinR);
            int bw = GetSystemMetrics(SystemMetric.SM_CXBORDER);
            int bh = GetSystemMetrics(SystemMetric.SM_CYBORDER);
            int ch = GetSystemMetrics(SystemMetric.SM_CYCAPTION);
            // try to retrieve the Window's client rect and its position on screen
            Vector2 Res;
#if UNITY_EDITOR
            Rect Pom = GetGameViewPosition();// gameview.position;   // gameview.position;
            Res = new Vector2(WinR.Left + bw + Pom.xMin, WinR.Top + ch + bh + Pom.yMin);
#else
            Res = new Vector2(WinR.Left + bw, WinR.Top + ch + bh);
#endif
            return Res;
        }
    }
#endif


}