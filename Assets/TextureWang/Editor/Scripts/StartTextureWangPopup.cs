using System;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;

namespace TextureWang
{

    public class StartTextureWangPopup : EditorWindow
    {
        int m_Width = 1024;
        int m_Height = 1024;
        private NodeEditorTWWindow m_Parent;
        private WWW www;

        public static void Init(NodeEditorTWWindow _inst)
        {

            StartTextureWangPopup window = ScriptableObject.CreateInstance<StartTextureWangPopup>();

            window.m_Parent = _inst;
            window.position = new Rect(_inst.canvasWindowRect.x + _inst.canvasWindowRect.width*0.5f,
                _inst.canvasWindowRect.y + _inst.canvasWindowRect.height*0.5f, 350, 250);
            window.titleContent = new GUIContent("Welcome To TextureWang");
            window.www = new WWW("http://ec2-52-3-137-47.compute-1.amazonaws.com/demo/");
            window.ShowUtility();
        }

        private int m_Count;

        void OnGUI()
        {
            Focus();
            string str =
                "\n Welcome to TextureWang \n \n If you find it useful please consider becoming a patreon \nto help support future features \n ";
            if (www.isDone)
            {
                try
                {


                    Version v = new Version(www.text);

                    str += "\n\nLatest version available " + v + " your version: " + NodeEditorTWWindow.m_Version;

                    if (v.CompareTo(NodeEditorTWWindow.m_Version) > 0)
                    {
                        str += "New version available " + v + " yours: " + NodeEditorTWWindow.m_Version;
                        EditorGUILayout.LabelField(str, EditorStyles.wordWrappedLabel);
                        if (GUILayout.Button("Go get New version "))
                        {
                            this.Close();
                            Application.OpenURL("https://github.com/dizzy2003/TextureWang");
                        }
                        if (GUILayout.Button("https://www.patreon.com/TextureWang"))
                        {
                            this.Close();
                            Application.OpenURL("https://www.patreon.com/TextureWang");
                        }
                        if (GUILayout.Button("Ignore new version"))
                        {

                            this.Close();
                        }
                        return;

                    }
                }
                catch (Exception)
                {


                }

            }
            else
            {
                str += "\n\nConnecting to Server";
                m_Count++;

                for (int i = 0; i < (m_Count >> 4)%10; i++)
                    str += ".";
            }



            EditorGUILayout.LabelField(str, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("https://www.patreon.com/TextureWang"))
            {
                this.Close();
                Application.OpenURL("https://www.patreon.com/TextureWang");
            }
            if (GUILayout.Button("OK"))
            {

                this.Close();
            }

        }
    }
}