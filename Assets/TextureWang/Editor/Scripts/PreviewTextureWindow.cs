using NodeEditorFramework.Standard;
using UnityEditor;
using UnityEngine;

namespace NodeEditorFramework
{
    
    public class PreviewTextureWindow : EditorWindow
    {
        int m_Width = 1024;
        int m_Height = 1024;
        private NodeEditorTWWindow m_Parent;

    


    public static void Init(NodeEditorTWWindow _inst)
        {
            
            PreviewTextureWindow window = ScriptableObject.CreateInstance<PreviewTextureWindow>();
            window.m_Parent = _inst;
            window.position = new Rect(_inst.canvasWindowRect.x+ _inst.canvasWindowRect.width*0.5f, _inst.canvasWindowRect.y + _inst.canvasWindowRect.height * 0.5f, 350, 250);
            window.titleContent=new GUIContent("New TextureWang Canvas");
            window.ShowUtility();
        }

        void OnGUI()
        {
            

            EditorGUILayout.LabelField("\n Warning: Erases Current Canvas", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Separator();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Width");
            m_Width = EditorGUILayout.IntField(m_Width);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Height");
            m_Height = EditorGUILayout.IntField(m_Height);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
            //            m_Height = EditorGUILayout.IntField(m_Height);
            //            m_Noise = EditorGUILayout.FloatField(m_Noise);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
                this.Close();
            if (GUILayout.Button("Create"))
            {
//miked                m_Parent.NewNodeCanvas(m_Width,m_Height);
                this.Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}