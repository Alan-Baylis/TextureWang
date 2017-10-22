using System.IO;
using NodeEditorFramework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace TextureWang
{

    public class NewTextureWangPopup : EditorWindow
    {
        int m_Width = 1024;
        int m_Height = 1024;
        private NodeEditorTWWindow m_Parent;
        private string m_Path = "Assets/TextureWang/OutputTextures";
        public bool m_CreateUnityTex = true;
        public bool m_CreateUnityMaterial = true;
        public bool m_LoadTestCubeScene = true;

        public static void Init(NodeEditorTWWindow _inst)
        {

            NewTextureWangPopup window = ScriptableObject.CreateInstance<NewTextureWangPopup>();
            window.m_Parent = _inst;
            window.position = new Rect(_inst.canvasWindowRect.x + _inst.canvasWindowRect.width*0.5f,
                _inst.canvasWindowRect.y + _inst.canvasWindowRect.height*0.5f, 350, 250);
            window.titleContent = new GUIContent("New TextureWang Canvas");
            window.ShowUtility();
        }

        public string MakePNG(string path, string _append)
        {


            string name = path.Replace(".png", _append + ".png");
            var tex = new Texture2D(m_Width, m_Height, TextureParam.ms_TexFormat, false);
            byte[] bytes = tex.EncodeToPNG();

            if (!string.IsNullOrEmpty(name))
            {
                File.WriteAllBytes(name, bytes);
            }
            return name;
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
            m_CreateUnityTex = EditorGUILayout.Toggle("Create UnityTextureOutput nodes and textures", m_CreateUnityTex);
            m_LoadTestCubeScene = EditorGUILayout.Toggle("Throw Away current Scene and load test cube",
                m_LoadTestCubeScene);
            GUI.enabled = m_CreateUnityTex;
            m_CreateUnityMaterial = EditorGUILayout.Toggle("Create new material with new textures",
                m_CreateUnityMaterial);



            m_Path = EditorGUILayout.TextField(m_Path);
            if (GUILayout.Button(new GUIContent("Browse Output Path", "Path to Output Textures to")))
            {
                m_Path = EditorUtility.SaveFilePanelInProject("Save Node Canvas", "", "png", "", m_Path);
            }
            GUI.enabled = true;
            EditorGUILayout.Separator();



            //            m_Height = EditorGUILayout.IntField(m_Height);
            //            m_Noise = EditorGUILayout.FloatField(m_Noise);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel"))
                this.Close();
            if (GUILayout.Button("Create"))
            {
                m_Parent.NewNodeCanvas(m_Width, m_Height);
                if (m_CreateUnityTex)
                {
                    if (m_Path.EndsWith("_albedo.png"))
                        m_Path = m_Path.Replace("_albedo.png", ".png");
                    if (m_Path.EndsWith("_normal.png"))
                        m_Path = m_Path.Replace("_albedo.png", ".png");
                    if (m_Path.EndsWith("_MetalAndRoughness.png"))
                        m_Path = m_Path.Replace("_MetalAndRoughness.png", ".png");
                    if (m_Path.EndsWith("_height.png"))
                        m_Path = m_Path.Replace("_height.png", ".png");
                    if (m_Path.EndsWith("_occlusion.png"))
                        m_Path = m_Path.Replace("_occlusion.png", ".png");

                    if (m_LoadTestCubeScene)
                        EditorSceneManager.OpenScene("Assets/TextureWang/Scenes/testcube.unity");
                    //Sigh, unity destroys scriptable objects when you call OpenScene, and you cant use dontdestroyonload
                    NodeEditor.ReInit(true);
                    m_Parent.NewNodeCanvas(m_Width, m_Height);

                    //required to add nodes to canvas
                    NodeEditor.curNodeCanvas = NodeEditorTWWindow.canvasCache.nodeCanvas;

                    float yOffset = 200;
                    var albedo = MakeTextureNodeAndTexture("_albedo", new Vector2(0, 0));
                    var norms = MakeTextureNodeAndTexture("_normal", new Vector2(0, 1*yOffset), true);

                    var height = MakeTextureNodeAndTexture("_height", new Vector2(0, 2*yOffset));
                    var metal = MakeTextureNodeAndTexture("_MetalAndRoughness", new Vector2(0, 3*yOffset));
                    var occ = MakeTextureNodeAndTexture("_occlusion", new Vector2(0, 3*yOffset));
                    if (m_CreateUnityMaterial)
                    {
                        var m = new Material(Shader.Find("Standard"));
                        m.mainTexture = albedo;
                        m.SetTexture("_BumpMap", norms);
                        m.SetTexture("_ParallaxMap", height);
                        m.SetTexture("_MetallicGlossMap", metal);
                        m.SetTexture("_OcclusionMap", occ);
                        AssetDatabase.CreateAsset(m, m_Path.Replace(".png", "_material.mat"));

                        var mr = FindObjectOfType<MeshRenderer>();
                        if (mr != null)
                            mr.material = m;
                    }
                }


                this.Close();

            }
            GUILayout.EndHorizontal();
        }

        private Texture2D MakeTextureNodeAndTexture(string texName, Vector2 _pos, bool _isNorm = false)
        {
            string albedo = MakePNG(m_Path, texName);
            AssetDatabase.Refresh();
            TextureImporter importer = (TextureImporter) TextureImporter.GetAtPath(albedo);
            if (_isNorm)
            {
                importer.textureType = TextureImporterType.NormalMap;
            }
            else
            {
                importer.textureType = TextureImporterType.Default;


            }
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.isReadable = true;
            AssetDatabase.ImportAsset(albedo, ImportAssetOptions.ForceSynchronousImport);
            Texture2D albedoTexture = (Texture2D) AssetDatabase.LoadAssetAtPath(albedo, typeof (Texture2D));



            var n = Node.Create("UnityTextureOutput", _pos);
            UnityTextureOutput uto = n as UnityTextureOutput;
            if (uto != null)
            {
                uto.m_Output = albedoTexture;
                uto.m_TexName = albedo;
            }
            return albedoTexture;
        }
    }
}