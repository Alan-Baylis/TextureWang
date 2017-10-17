﻿using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Security.Policy;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEditor.Graphs;
using UnityEditor.Rendering;
using UnityEditor.SceneManagement;
using UnityEditor.TreeViewExamples;

namespace NodeEditorFramework
{

    public class NewTextureWangPopup : EditorWindow
    {
        int m_Width = 1024;
        int m_Height = 1024;
        private NodeEditorTWWindow m_Parent;
        private string m_Path= "Assets/TextureWang/OutputTextures";
        public bool m_CreateUnityTex=true;
        public bool m_CreateUnityMaterial = true;
        public bool m_LoadTestCubeScene = true;
        public static void Init(NodeEditorTWWindow _inst)
        {
            
            NewTextureWangPopup window = ScriptableObject.CreateInstance<NewTextureWangPopup>();
            window.m_Parent = _inst;
            window.position = new Rect(_inst.canvasWindowRect.x+ _inst.canvasWindowRect.width*0.5f, _inst.canvasWindowRect.y + _inst.canvasWindowRect.height * 0.5f, 350, 250);
            window.titleContent=new GUIContent("New TextureWang Canvas");
            window.ShowUtility();
        }
        public string MakePNG(string path,string _append)
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
            m_LoadTestCubeScene = EditorGUILayout.Toggle("Throw Away current Scene and load test cube", m_LoadTestCubeScene);
            GUI.enabled = m_CreateUnityTex;
            m_CreateUnityMaterial = EditorGUILayout.Toggle("Create new material with new textures", m_CreateUnityMaterial);
            


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
                m_Parent.NewNodeCanvas(m_Width,m_Height);
                if (m_CreateUnityTex)
                {
                    if(m_LoadTestCubeScene)
                        EditorSceneManager.OpenScene("Assets/TextureWang/Scenes/testcube.unity");

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
            if (_isNorm)
            {
                AssetDatabase.Refresh();
                TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(albedo);
                importer.textureType = TextureImporterType.NormalMap;

            }

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
            window.position = new Rect(_inst.canvasWindowRect.x + _inst.canvasWindowRect.width * 0.5f, _inst.canvasWindowRect.y + _inst.canvasWindowRect.height * 0.5f, 350, 250);
            window.titleContent = new GUIContent("Welcome To TextureWang");
            window.www = new WWW("http://ec2-52-3-137-47.compute-1.amazonaws.com/demo/");
            window.ShowUtility();
        }

        private int m_Count;
        void OnGUI()
        {
            Focus();
            string str ="\n Welcome to TextureWang \n \n If you find it useful please consider becoming a patreon \nto help support future features \n ";
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

                for (int i = 0; i < (m_Count>>4) % 10; i++) 
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
    public class NodeEditorTWWindow : EditorWindow , ITreeDataProvider
    {
        public static Version m_Version = new Version(0,1,1,2);
        private string m_Name;
        private string m_LastLoadedName;
        // Information about current instance
        private static NodeEditorTWWindow _editor;
        private static NodeEditorTWWindow _editor2;
        public static NodeEditorTWWindow editor { get { AssureEditor (); return _editor; } }
		public static void AssureEditor () { if (_editor == null) OpenNodeEditor(); }

		
		public static string openedCanvasPath;
		public string tempSessionPath;

		// GUI
//		public static int sideWindowWidth = 400;
		private static Texture iconTexture;
//		public Rect sideWindowRect { get { return new Rect (position.width - sideWindowWidth, 0, sideWindowWidth, position.height); } }
		public Rect canvasWindowRect { get { return new Rect (0, 0, position.width , position.height); } }
	    public MultiColumnWindow m_NodeSelectionWindow;
        public NodeInspectorWindow m_InspectorWindow;
        public bool m_Docked = false;
        // Opened Canvas
        public static NodeEditorUserCache canvasCache;
        #region General 

        [MenuItem ("Window/TextureWang")]
		public static NodeEditorTWWindow OpenNodeEditor()
        {
            
            _editor = GetWindow<NodeEditorTWWindow> ();
            _editor.minSize = new Vector2 (600, 400);
            
            NodeEditor.ClientRepaints += _editor.Repaint;
            
            //miked		NodeEditor.initiated = NodeEditor.InitiationError = false;

            iconTexture = ResourceManager.LoadTexture (EditorGUIUtility.isProSkin? "Textures/Icon_Dark.png" : "Textures/Icon_Light.png");
			_editor.titleContent = new GUIContent ("Texture Wang Nodes", iconTexture);
            return _editor;
        }


        /// <summary>
        /// Handle opening canvas when double-clicking asset
        /// </summary>

        [UnityEditor.Callbacks.OnOpenAsset(1)]
        private static bool AutoOpenCanvas(int instanceID, int line)
        {
            if (Selection.activeObject != null && Selection.activeObject is NodeCanvas)
            {
                string NodeCanvasPath = AssetDatabase.GetAssetPath(instanceID);
                NodeEditorTWWindow.OpenNodeEditor();
                canvasCache.LoadNodeCanvas(NodeCanvasPath);
                return true;
            }
            return false;
        }
        int IDCounter = 0;
        MyTreeElement FindAndAddToParent(MyTreeElement _root, string[] _subContents,int _index,string _nodeID)
	    {
	        if (_root.children == null)
                _root.AddChild(new MyTreeElement(_subContents[_index],_root.depth+1,++IDCounter, _nodeID));
	            
	        foreach (var x in _root.children)
	        {
	            if (x.name == _subContents[_index])
	            {
	                if (_index == _subContents.Length - 1)
	                    return x as MyTreeElement;
                    
	                return FindAndAddToParent(x as MyTreeElement, _subContents, _index + 1, _nodeID);
	                
	            }
	        }
            var folder = new MyTreeElement(_subContents[_index], _root.depth+1, ++IDCounter, _nodeID);
            _root.AddChild(folder);
            if (_index == _subContents.Length - 1)
                return null;
            return FindAndAddToParent(folder, _subContents, _index + 1, _nodeID);

            
	    }
	    public IList<MyTreeElement> GetData()
	    {
            
            var treeElements = new List<MyTreeElement>();

            var root = new MyTreeElement("Root", -1, ++IDCounter,"root");
            treeElements.Add(root);
            //var child = new MyTreeElement("Element " + IDCounter, root.depth + 1, ++IDCounter);
            //treeElements.Add(child);

            
            foreach (Node node in NodeTypes.nodes.Keys)
            {

                string path = NodeTypes.nodes[node].adress;
                if (path.Contains("/"))
                {
                    // is inside a group
                    string[] subContents = path.Split('/');
                    string folderPath = subContents[0];
                    FindAndAddToParent(root, subContents, 0, node.GetID);
                }
                else
                {
                    var ele = new MyTreeElement(path, root.depth+1, ++IDCounter,node.GetID);
                    treeElements.Add(ele);

                }


            }
            var sub = new MyTreeElement("Subroutines", 0, ++IDCounter, "folder");
            root.AddChild(sub);
            var subs = GetAtPath<NodeCanvas>("TextureWang/Subroutines");//Resources.LoadAll<NodeCanvas>(NodeEditor.editorPath + "Resources/Saves/");
	        foreach (var x in subs)
	        {
	            var s = new MyTreeElement(x.name, sub.depth+1, ++IDCounter, "");
                sub.AddChild(s);
	            s.m_Canvas = x;
	        }
            sub = new MyTreeElement("UserSubroutines", 0, ++IDCounter, "folder");
            root.AddChild(sub);
            subs = GetAtPath<NodeCanvas>("TextureWang/UserSubroutines");//Resources.LoadAll<NodeCanvas>(NodeEditor.editorPath + "Resources/Saves/");
            foreach (var x in subs)
            {
                var s = new MyTreeElement(x.name, sub.depth + 1, ++IDCounter, "");
                sub.AddChild(s);
                s.m_Canvas = x;
            }

            var res = new List<MyTreeElement>();
            TreeElementUtility.TreeToList(root,res);
            return res;
	    }


        // Following section is all about caching the last editor session


        #endregion

        #region GUI

        [HotkeyAttribute(KeyCode.Delete, EventType.KeyDown)]
        private static void KeyDelete(NodeEditorInputInfo inputInfo)
        {
            inputInfo.SetAsCurrentEnvironment();
            if (inputInfo.editorState.focusedNode != null)
            {
                inputInfo.editorState.focusedNode.Delete();
                inputInfo.inputEvent.Use();
            }
        }
        [HotkeyAttribute(KeyCode.D, EventType.KeyDown)]
        private static void KeyDupe(NodeEditorInputInfo inputInfo)
        {
            inputInfo.SetAsCurrentEnvironment();
            NodeEditorState state = inputInfo.editorState;
            if (state.focusedNode != null)
            { // Create new node of same type
                Node duplicatedNode = Node.Create(state.focusedNode.GetID, NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos), state.connectOutput);
                state.selectedNode = state.focusedNode = duplicatedNode;
                state.connectOutput = null;
                inputInfo.inputEvent.Use();
            }
        }

        [HotkeyAttribute(KeyCode.H, EventType.KeyDown)]
        [ContextEntryAttribute(ContextType.Node, "Preview Node [H]istogram")]
        private static void InspectNode(NodeEditorInputInfo inputInfo)
        {
            inputInfo.SetAsCurrentEnvironment();
            if (inputInfo.editorState.focusedNode != null && inputInfo.editorState.focusedNode is TextureNode)
            {
                (inputInfo.editorState.focusedNode as TextureNode).OpenPreview();
                inputInfo.inputEvent.Use();
            }
        }

        [EventHandlerAttribute(EventType.DragUpdated, 90)] // Priority over hundred to make it call after the GUI
        [EventHandlerAttribute(EventType.DragPerform, 90)] // Priority over hundred to make it call after the GUI
        private static void HandleDragAndDrop(NodeEditorInputInfo inputInfo)
        {
            if (inputInfo.inputEvent.type == EventType.DragUpdated || (inputInfo.inputEvent.type == EventType.DragPerform))
            {

                //Debug.LogError("handle drag " + inputInfo.inputEvent.type);
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (inputInfo.inputEvent.type == EventType.DragPerform && DragAndDrop.objectReferences.Length > 0 &&
                    (DragAndDrop.objectReferences[0] is Texture2D))
                {
                    Debug.Log(" drag texture " + DragAndDrop.objectReferences[0]);
                    UnityTextureInput node =
                        (UnityTextureInput)
                            Node.Create("UnityTextureInput", NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos));
                    node.m_Input = DragAndDrop.objectReferences[0] as Texture2D;
                    inputInfo.inputEvent.Use();

                }
                else if (inputInfo.inputEvent.type == EventType.DragPerform &&
                         DragAndDrop.GetGenericData("GenericDragColumnDragging") != null)
                {
                    DragAndDrop.AcceptDrag();
                    //Debug.Log("dragged generic data "+ DragAndDrop.GetGenericData("GenericDragColumnDragging"));
                    List<UnityEditor.IMGUI.Controls.TreeViewItem> _data =
                        DragAndDrop.GetGenericData("GenericDragColumnDragging") as
                            List<UnityEditor.IMGUI.Controls.TreeViewItem>;
                    var draggedElements = new List<TreeElement>();
                    foreach (var x in _data)
                        draggedElements.Add(((TreeViewItem<MyTreeElement>) x).data);

                    var srcItem = draggedElements[0] as MyTreeElement;
                    if (srcItem.m_Canvas)
                    {
                        SubTreeNode node =
                            (SubTreeNode)
                                Node.Create("SubTreeNode", NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos));
                        node.SubCanvas = srcItem.m_Canvas;
                        string assetPath = AssetDatabase.GetAssetPath(node.SubCanvas);
                        //                    Debug.Log("drag and drop asset canvas path >" + assetPath + "<");
                        if (assetPath.Length > 0)
                        {
                            node.m_CanvasGuid = AssetDatabase.AssetPathToGUID(assetPath);
                            Debug.LogError(" set canvasGuid from asset >" + node.m_CanvasGuid + "<");
                            string fname = Path.GetFileName(assetPath);
                            fname = Path.ChangeExtension(fname, "");
                            node.name = "Sub:" + fname;
                        }


                        node.OnLoadCanvas();

                    }
                    else
                    {
                        Node node = Node.Create(srcItem.m_NodeID, NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos));
                    }
                    inputInfo.inputEvent.Use();
                    
                }
                else if (inputInfo.inputEvent.type == EventType.DragPerform &&
                         DragAndDrop.objectReferences.Length > 0 && (DragAndDrop.objectReferences[0] is NodeCanvas))
                {
                    DragAndDrop.AcceptDrag();
                    //                    Debug.Log(" drag and drop " + DragAndDrop.objectReferences[0] + " co ord " + Event.current.mousePosition);
                    SubTreeNode node =
                        (SubTreeNode)
                            Node.Create("SubTreeNode", NodeEditor.ScreenToCanvasSpace(inputInfo.inputPos));
                    node.SubCanvas = (NodeCanvas) DragAndDrop.objectReferences[0];
                    string assetPath = AssetDatabase.GetAssetPath(node.SubCanvas);
                    //                    Debug.Log("drag and drop asset canvas path >" + assetPath + "<");
                    if (assetPath.Length > 0)
                    {
                        node.m_CanvasGuid = AssetDatabase.AssetPathToGUID(assetPath);
                        Debug.LogError(" set canvasGuid from asset >" + node.m_CanvasGuid + "<");
                        string fname = Path.GetFileName(assetPath);
                        fname = Path.ChangeExtension(fname, "");
                        node.name = "Sub:" + fname;
                    }
                    inputInfo.inputEvent.Use();

                    node.OnLoadCanvas();
                    
                }
            }
        }
        private void OnGUI () 
		{
            // Initiation
            NodeEditor.checkInit(true);
            if (NodeEditor.InitiationError)
            {
                GUILayout.Label("Node Editor Initiation failed! Check console for more information!");
                return;
            }
            AssureEditor();
            canvasCache.AssureCanvas();

            // Specify the Canvas rect in the EditorState
            canvasCache.editorState.canvasRect = canvasWindowRect;
            // If you want to use GetRect:
            //			Rect canvasRect = GUILayoutUtility.GetRect (600, 600);
            //			if (Event.current.type != EventType.Layout)
            //				mainEditorState.canvasRect = canvasRect;
            NodeEditorGUI.StartNodeGUI();

            // Perform drawing with error-handling
            try
            {

                NodeEditor.DrawCanvas(canvasCache.nodeCanvas, canvasCache.editorState);
            }
            catch (UnityException e)
            { // on exceptions in drawing flush the canvas to avoid locking the ui.
                canvasCache.NewNodeCanvas();
                NodeEditor.ReInit(true);
                Debug.LogError("Unloaded Canvas due to an exception during the drawing phase!");
                Debug.LogException(e);
            }


            /*
            // Draw Side Window
			sideWindowWidth = Math.Min(600, Math.Max(200, (int)(position.width / 5)));
			GUILayout.BeginArea(sideWindowRect, GUI.skin.box);
			DrawSideWindow();
			GUILayout.EndArea();
            */

            NodeEditorGUI.EndNodeGUI();
//            if (Event.current.type == EventType.Repaint)
//                m_InspectorWindow.Repaint();
/*
            //if (Event.current.type == EventType.Repaint)
            {
                if (mainEditorState.selectedNode != mainEditorState.wantselectedNode)
                {
                    mainEditorState.selectedNode = mainEditorState.wantselectedNode;
                    NodeEditor.RepaintClients();
                    Repaint();
                }

            }
*/
		    if (!m_Docked)
		    {
                Docker.Dock(this, m_InspectorWindow, Docker.DockPosition.Right);
                Docker.Dock(this, m_NodeSelectionWindow, Docker.DockPosition.Left);
		        m_Docked = true;

		    }
        }

        void OnLoadCanvas(NodeCanvas _canvas)
        {
            foreach (var n in _canvas.nodes)
            {
                if (n is TextureNode)
                {
                    (n as TextureNode).OnLoadCanvas();
                }
            }
        }

        private void OnEnable()
        {
            _editor = this;
            NodeEditor.checkInit(false);

            NodeEditorCallbacks.OnLoadCanvas += OnLoadCanvas;

            NodeEditor.ClientRepaints -= Repaint;
            NodeEditor.ClientRepaints += Repaint;

            EditorLoadingControl.justLeftPlayMode -= NormalReInit;
            EditorLoadingControl.justLeftPlayMode += NormalReInit;
            // Here, both justLeftPlayMode and justOpenedNewScene have to act because of timing
            EditorLoadingControl.justOpenedNewScene -= NormalReInit;
            EditorLoadingControl.justOpenedNewScene += NormalReInit;

            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
            string assetPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(this));
            if (assetPath.Length > 1)
            {
                Debug.LogError("asset path " + assetPath);
                string path = Path.GetDirectoryName(assetPath);
                // Setup Cache
                canvasCache = new NodeEditorUserCache(path);
            }
            else
            {
                canvasCache = new NodeEditorUserCache(); //path);
            }
            canvasCache.SetupCacheEvents();

            m_NodeSelectionWindow = MultiColumnWindow.GetWindow(this);

            m_InspectorWindow = NodeInspectorWindow.Init(this);
            NodeEditor.ClientRepaints += m_InspectorWindow.Repaint;
            StartTextureWangPopup.Init(this);

            

        }

        private void NormalReInit()
        {
            NodeEditor.ReInit(false);
        }

        private void OnDestroy()
        {
            EditorUtility.SetDirty(canvasCache.nodeCanvas);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            NodeEditor.ClientRepaints -= Repaint;
            NodeEditor.ClientRepaints -= m_InspectorWindow.Repaint;

            EditorLoadingControl.justLeftPlayMode -= NormalReInit;
            EditorLoadingControl.justOpenedNewScene -= NormalReInit;

            SceneView.onSceneGUIDelegate -= OnSceneGUI;

            // Clear Cache
            canvasCache.ClearCacheEvents();
        }
        private void OnSceneGUI(SceneView sceneview)
        {
            DrawSceneGUI();
        }

        private void DrawSceneGUI()
        {
            if (canvasCache != null)
            {
                AssureEditor();
                canvasCache.AssureCanvas();
                if (canvasCache.editorState.selectedNode != null)
                    canvasCache.editorState.selectedNode.OnSceneGUI();
                SceneView.lastActiveSceneView.Repaint();
            }
        }
        public static T[] GetAtPath<T>(string path)
        {

            ArrayList al = new ArrayList();
            string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + path);

            foreach (string fileName in fileEntries)
            {
                int assetPathIndex = fileName.IndexOf("Assets");
                string localPath = fileName.Substring(assetPathIndex);

                UnityEngine.Object t = AssetDatabase.LoadAssetAtPath(localPath, typeof(T));

                if (t != null)
                    al.Add(t);
            }
            T[] result = new T[al.Count];
            for (int i = 0; i < al.Count; i++)
                result[i] = (T)al[i];

            return result;
        }

        Node PriorLoop(Node n)
        {
            List<Node> nodes=new List<Node>();
            nodes.Add(n);
            for (int index = 0; index < nodes.Count; index++)
            {
                var node = nodes[index];
                foreach (var i in node.Outputs)
                {
                    if(i!=null)// && i.connection!=null)
                    foreach (var c in i.connections)
                    {
                        Node body = c.connection.body;
                        if (!nodes.Contains(body))
                            nodes.Add(body);
                        if (body is LoopBasic)
                            return body;
                    }
                }
            }
            return n;
        }

        public void DrawSideWindow ()
        {
			GUILayout.Label (new GUIContent ("TextureWang ", "Opened Canvas path: " + openedCanvasPath), NodeEditorGUI.nodeLabelBold);

			if (GUILayout.Button (new GUIContent ("Save Canvas", "Saves the Canvas to a Canvas Save File in the Assets Folder")))
			{
                string path = EditorUtility.SaveFilePanelInProject("Save Node Canvas", "Node Canvas", "asset", "", NodeEditor.editorPath + "Resources/Saves/");
                if (!string.IsNullOrEmpty(path))
                    canvasCache.SaveNodeCanvas(path);
/*
                string path = EditorUtility.SaveFilePanelInProject ("Save Node Canvas", m_LastLoadedName, "asset", "", NodeEditor.editorPath + "Resources/Saves/");
			    if (!string.IsNullOrEmpty(path))
			    {
			        SaveNodeCanvas(path); 

                }
*/
                if(m_NodeSelectionWindow!=null)
			        m_NodeSelectionWindow.ReInit();
			}
            /*
                        if (GUILayout.Button(new GUIContent("New Canvas",
                                "Create a copy")))
                        {
                            CreateEditorCopy();
                        }
            */

            if (GUILayout.Button(new GUIContent("Load Canvas", "Loads the Canvas from a Canvas Save File in the Assets Folder")))
            {
                string path = EditorUtility.OpenFilePanel("Load Node Canvas", NodeEditor.editorPath + "Resources/Saves/", "asset");
                if (!path.Contains(Application.dataPath))
                {
                    if (!string.IsNullOrEmpty(path))
                        ShowNotification(new GUIContent("You should select an asset inside your project folder!"));
                }
                else
                    canvasCache.LoadNodeCanvas(path);
            }

            if (GUILayout.Button(new GUIContent("New TextureWang", "Create a new TextureWang Canvas")))
            {
                NewTextureWangPopup.Init(this);
            }
				

			if (GUILayout.Button (new GUIContent ("Recalculate All", "Initiates complete recalculate. Usually does not need to be triggered manually.")))
				NodeEditor.RecalculateAll (canvasCache.nodeCanvas);

			if (GUILayout.Button ("Force Re-Init"))
				NodeEditor.ReInit (true);

			NodeEditorGUI.knobSize = EditorGUILayout.IntSlider (new GUIContent ("Handle Size", "The size of the Node Input/Output handles"), NodeEditorGUI.knobSize, 12, 20);
            canvasCache.editorState.zoom = EditorGUILayout.Slider(new GUIContent("Zoom", "Use the Mousewheel. Seriously."), canvasCache.editorState.zoom, 0.6f, 2);

            //miked            mainNodeCanvas.scaleMode = (ScaleMode)EditorGUILayout.EnumPopup(new GUIContent("ScaleMode", ""), mainNodeCanvas.scaleMode, GUILayout.MaxWidth(200));



            //        m_OpType = (TexOP)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
            //            if (mainNodeCanvas != null)
            {
                //                EditorGUILayout.LabelField("width: " + mainNodeCanvas.m_TexWidth);
                //                EditorGUILayout.LabelField("height: " + mainNodeCanvas.m_TexHeight);
            }
            
            if (NodeEditor.curEditorState!=null && NodeEditor.curEditorState.selectedNode != null)
                // if (Event.current.type != EventType.Ignore)
                {
                    RTEditorGUI.Seperator();
                    GUILayout.Label(NodeEditor.curEditorState.selectedNode.name);
                    RTEditorGUI.Seperator();
                    NodeEditor.curEditorState.selectedNode.DrawNodePropertyEditor();
                    if (GUI.changed)
                        NodeEditor.RecalculateFrom(PriorLoop(NodeEditor.curEditorState.selectedNode));

            }
            

            //            var assets = UnityEditor.AssetDatabase.FindAssets("NodeCanvas"); 
            //            foreach(var x in assets)
            //                GUILayout.Label(new GUIContent("Node Editor (" + x + ")", "Opened Canvas path: " ), NodeEditorGUI.nodeLabelBold);
            /*
                        if (m_All == null)
                            m_All = GetAtPath<NodeCanvas>("Node_Editor-master/Node_Editor/Resources/Saves");//Resources.LoadAll<NodeCanvas>(NodeEditor.editorPath + "Resources/Saves/");

                        scrollPos =EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(300), GUILayout.Height(600));
                        guiStyle.fontSize = 20;
                        guiStyle.fixedHeight = 20;
                        foreach (var x in m_All)
                             EditorGUILayout.SelectableLabel("(" + x.name + ")", guiStyle);
                        EditorGUILayout.EndScrollView();
            */
        }
        Vector2 scrollPos;
        private NodeCanvas[] m_All;
        private GUIStyle guiStyle = new GUIStyle();
        #endregion




        // Opened Canvas
        





        /// <summary>
        /// Creates and opens a new empty node canvas
        /// </summary>
        public void NewNodeCanvas (int _texWidth=1024,int _texHeight=1024)
        {
            canvasCache.NewNodeCanvas();
/*
            // New NodeCanvas
            mainNodeCanvas = CreateInstance<NodeCanvas> ();
			mainNodeCanvas.name = "New Canvas";
//miked		    mainNodeCanvas.m_TexWidth = _texWidth;
//miked            mainNodeCanvas.m_TexHeight = _texHeight;
            // New NodeEditorState
            mainEditorState = CreateInstance<NodeEditorState> ();
			mainEditorState.canvas = mainNodeCanvas;
			mainEditorState.name = "MainEditorState";
		    NodeEditor.curNodeCanvas = mainNodeCanvas;
            openedCanvasPath = "";
			SaveCache ();
*/
		}
        public void LoadSceneCanvasCallback(object canvas)
        {
            canvasCache.LoadSceneNodeCanvas((string)canvas);
        }
    }
}