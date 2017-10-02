using System.Collections.Generic;
using NodeEditorFramework;
using UnityEditor;
using UnityEngine;
using UnityEngine.PostProcessing;

public class NodeInspectorWindow : EditorWindow
{


    
    public Texture2D m_tex;
    private NodeEditorWindow m_Source;
    private Vector2 m_ScrollPos;
    void OnDestroy()
    {

    }

 
    public static NodeInspectorWindow Init(NodeEditorWindow _src)
    {

        NodeInspectorWindow window = GetWindow<NodeInspectorWindow>();

        window.m_Source = _src;
        if (_src == null)
            Debug.LogError("init Node Inspector Window with null source");
        window.titleContent=new GUIContent("TextureWang");
        window.Show();
        window.Repaint();

        return window;

    }


    void OnGUI()
    {
//        GUILayout.BeginArea(new Rect(0, 0, 256, 600));
        GUILayout.BeginVertical();

        m_ScrollPos = GUILayout.BeginScrollView(m_ScrollPos, false, true);//, GUILayout.Width(256), GUILayout.MinHeight(200), GUILayout.MaxHeight(1000), GUILayout.ExpandHeight(true));
        GUI.changed = false;
        if(m_Source!=null)  
            m_Source.DrawSideWindow();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        if (GUI.changed)
        {
            GUI.changed = false;
            if (m_Source != null)
                m_Source.Repaint();
        }
    }
}