using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEditor.Graphs;
using UnityEditor.TreeViewExamples;

namespace NodeEditorFramework
{

    public class NewTextureWangPopup : EditorWindow
    {
        int m_Width = 1024;
        int m_Height = 1024;
        private NodeEditorWindow m_Parent;

        
        public static void Init(NodeEditorWindow _inst)
        {
            
            NewTextureWangPopup window = ScriptableObject.CreateInstance<NewTextureWangPopup>();
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
                m_Parent.NewNodeCanvas(m_Width,m_Height);
                this.Close();
            }
            GUILayout.EndHorizontal();
        }
    }

    public class NodeEditorWindow : EditorWindow , ITreeDataProvider
    {
        private string m_Name;
        private string m_LastLoadedName;
        // Information about current instance
        private static NodeEditorWindow _editor;
        private static NodeEditorWindow _editor2;
        public static NodeEditorWindow editor { get { AssureEditor (); return _editor; } }
		public static void AssureEditor () { if (_editor == null) CreateEditor (); }

		// Opened Canvas
		public NodeCanvas mainNodeCanvas;
		public NodeEditorState mainEditorState;
		public static NodeCanvas MainNodeCanvas { get { return editor.mainNodeCanvas; } }
		public static NodeEditorState MainEditorState { get { return editor.mainEditorState; } }
		public void AssureCanvas () { if (mainNodeCanvas == null) NewNodeCanvas (); }
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
        #region General 

        [MenuItem ("Window/TextureWang")]
		public static void CreateEditor ()
        {
            
            _editor = GetWindow<NodeEditorWindow> ();
            _editor.m_Name = "Copy";
            _editor.minSize = new Vector2 (800, 600);
			NodeEditor.ClientRepaints += _editor.Repaint;
			NodeEditor.initiated = NodeEditor.InitiationError = false;

			iconTexture = ResourceManager.LoadTexture (EditorGUIUtility.isProSkin? "Textures/Icon_Dark.png" : "Textures/Icon_Light.png");
			_editor.titleContent = new GUIContent ("Node Editor", iconTexture);
            _editor.m_NodeSelectionWindow = MultiColumnWindow.GetWindow(_editor);

            _editor.m_InspectorWindow = NodeInspectorWindow.Init(_editor);


        }
        public static void CreateEditorCopy()
        {
            
            _editor2 = CreateInstance<NodeEditorWindow>();
            _editor2.m_Name = "Copy";
            _editor2.minSize = new Vector2(800, 600);
            NodeEditor.ClientRepaints += _editor2.Repaint;
            //NodeEditor.initiated = NodeEditor.InitiationError = false;

            //iconTexture = ResourceManager.LoadTexture(EditorGUIUtility.isProSkin ? "Textures/Icon_Dark.png" : "Textures/Icon_Light.png");
            _editor2.titleContent = new GUIContent("Node Editor2", iconTexture);
//            _editor.m_NodeSelectionWindow = MultiColumnWindow.GetWindow(_editor);
        }

        /// <summary>
        /// Handle opening canvas when double-clicking asset
        /// </summary>
        [UnityEditor.Callbacks.OnOpenAsset(1)]
		public static bool AutoOpenCanvas (int instanceID, int line) 
		{
			if (Selection.activeObject != null && Selection.activeObject.GetType () == typeof(NodeCanvas))
			{
				string NodeCanvasPath = AssetDatabase.GetAssetPath (instanceID);
				NodeEditorWindow.CreateEditor ();
				EditorWindow.GetWindow<NodeEditorWindow> ().LoadNodeCanvas (NodeCanvasPath);
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
        public void OnDestroy () 
		{
			NodeEditor.ClientRepaints -= _editor.Repaint;

	#if UNITY_EDITOR
			// Remove callbacks
			EditorLoadingControl.lateEnteredPlayMode -= LoadCache;
			EditorLoadingControl.justLeftPlayMode -= LoadCache;
			EditorLoadingControl.justOpenedNewScene -= LoadCache;

			NodeEditorCallbacks.OnAddNode -= SaveNewNode;
	#endif
		}

		// Following section is all about caching the last editor session

		private void OnEnable () 
		{
			tempSessionPath = Path.GetDirectoryName (AssetDatabase.GetAssetPath (MonoScript.FromScriptableObject (this)));
			LoadCache ();

	#if UNITY_EDITOR
			// This makes sure the Node Editor is reinitiated after the Playmode changed
			EditorLoadingControl.lateEnteredPlayMode -= LoadCache;
			EditorLoadingControl.lateEnteredPlayMode += LoadCache;

			EditorLoadingControl.justLeftPlayMode -= LoadCache;
			EditorLoadingControl.justLeftPlayMode += LoadCache;

			EditorLoadingControl.justOpenedNewScene -= LoadCache;
			EditorLoadingControl.justOpenedNewScene += LoadCache;

			NodeEditorCallbacks.OnAddNode -= SaveNewNode;
			NodeEditorCallbacks.OnAddNode += SaveNewNode;
	#endif
		}

		#endregion

		#region GUI

		private void OnGUI () 
		{
            //Debug.Log("ongui "+m_Name);
			// Initiation
			NodeEditor.checkInit ();
			if (NodeEditor.InitiationError) 
			{
				GUILayout.Label ("Node Editor Initiation failed! Check console for more information!");
				return;
			}
			AssureEditor ();
			AssureCanvas ();

			// Specify the Canvas rect in the EditorState
			mainEditorState.canvasRect = canvasWindowRect;
			// If you want to use GetRect:
//			Rect canvasRect = GUILayoutUtility.GetRect (600, 600);
//			if (Event.current.type != EventType.Layout)
//				mainEditorState.canvasRect = canvasRect;

			// Perform drawing with error-handling
			try
			{
				NodeEditor.DrawCanvas (mainNodeCanvas, mainEditorState);
			}
			catch (Exception e)
			{ // on exceptions in drawing flush the canvas to avoid locking the ui.
				//NewNodeCanvas ();
				//NodeEditor.ReInit (true);
				Debug.LogError ("Unloaded Canvas due to exception when drawing!");
				Debug.LogException (e);
			}


            /*
                        // Draw Side Window
                        sideWindowWidth = Math.Min (1800, Math.Max (200, (int)(position.width / 5)));
                        NodeEditorGUI.StartNodeGUI (); 
                        GUILayout.BeginArea (sideWindowRect, GUI.skin.box);
                        DrawSideWindow ();
                        GUILayout.EndArea ();
            */
            NodeEditorGUI.EndNodeGUI();
            if (Event.current.type == EventType.Repaint)
                m_InspectorWindow.Repaint();
            //if (Event.current.type == EventType.Repaint)
            {
                if (mainEditorState.selectedNode != mainEditorState.wantselectedNode)
                {
                    mainEditorState.selectedNode = mainEditorState.wantselectedNode;
                    NodeEditor.RepaintClients();
                    Repaint();
                }

            }
		    if (!m_Docked)
		    {
                Docker.Dock(this, m_InspectorWindow, Docker.DockPosition.Right);
                Docker.Dock(this, m_NodeSelectionWindow, Docker.DockPosition.Left);
		        m_Docked = true;

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
            if (mainNodeCanvas == null)
                return;
			GUILayout.Label (new GUIContent ("TextureWang (" + mainNodeCanvas.name + ")", "Opened Canvas path: " + openedCanvasPath), NodeEditorGUI.nodeLabelBold);

			if (GUILayout.Button (new GUIContent ("Save Canvas", "Saves the Canvas to a Canvas Save File in the Assets Folder")))
			{
				string path = EditorUtility.SaveFilePanelInProject ("Save Node Canvas", m_LastLoadedName, "asset", "", NodeEditor.editorPath + "Resources/Saves/");
			    if (!string.IsNullOrEmpty(path))
			    {
			        SaveNodeCanvas(path);

                }
			}
/*
            if (GUILayout.Button(new GUIContent("New Canvas",
                    "Create a copy")))
            {
                CreateEditorCopy();
            }
*/
                if (GUILayout.Button (new GUIContent ("Load Canvas", "Loads the Canvas from a Canvas Save File in the Assets Folder"))) 
			{
				string path = EditorUtility.OpenFilePanel ("Load Node Canvas", NodeEditor.editorPath + "Resources/Saves/", "asset");
				if (!path.Contains (Application.dataPath)) 
				{
					if (!string.IsNullOrEmpty (path))
						ShowNotification (new GUIContent ("You should select an asset inside your project folder!"));
				}
				else
				{
					path = path.Replace (Application.dataPath, "Assets");
					LoadNodeCanvas (path);
                    int indexOf = path.LastIndexOf("/");
                    if (indexOf == -1)
                        indexOf = path.LastIndexOf("\\");
                    if (indexOf >= 0)
                        m_LastLoadedName = path.Substring(indexOf+1);
                }
			}

            if (GUILayout.Button(new GUIContent("New TextureWang", "Create a new TextureWang Canvas")))
            {
                NewTextureWangPopup.Init(this);
            }
				

			if (GUILayout.Button (new GUIContent ("Recalculate All", "Initiates complete recalculate. Usually does not need to be triggered manually.")))
				NodeEditor.RecalculateAll (mainNodeCanvas);

			if (GUILayout.Button ("Force Re-Init"))
				NodeEditor.ReInit (true);

			NodeEditorGUI.knobSize = EditorGUILayout.IntSlider (new GUIContent ("Handle Size", "The size of the Node Input/Output handles"), NodeEditorGUI.knobSize, 12, 20);
			mainEditorState.zoom = EditorGUILayout.Slider (new GUIContent ("Zoom", "Use the Mousewheel. Seriously."), mainEditorState.zoom, 0.2f, 4);
            mainNodeCanvas.scaleMode = (ScaleMode)EditorGUILayout.EnumPopup(new GUIContent("ScaleMode", ""), mainNodeCanvas.scaleMode, GUILayout.MaxWidth(200));



    //        m_OpType = (TexOP)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
            if (mainNodeCanvas != null)
            {
                EditorGUILayout.LabelField("width: " + mainNodeCanvas.m_TexWidth);
                EditorGUILayout.LabelField("height: " + mainNodeCanvas.m_TexHeight);
            }

            if (mainEditorState.selectedNode != null)
               // if (Event.current.type != EventType.Ignore)
                {
                    RTEditorGUI.Seperator();
                    GUILayout.Label(mainEditorState.selectedNode.name);
                    RTEditorGUI.Seperator();
                    mainEditorState.selectedNode.DrawNodePropertyEditor();
                    if (GUI.changed)
                        NodeEditor.RecalculateFrom(PriorLoop(mainEditorState.selectedNode));

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

        #region Cache

        private void SaveNewNode (Node node) 
		{
			if (!mainNodeCanvas.nodes.Contains (node))
				throw new UnityException ("Cache system: Writing new Node to save file failed as Node is not part of the Cache!");
			string path = tempSessionPath + "/LastSession.asset";
			if (AssetDatabase.GetAssetPath (mainNodeCanvas) != path)
				throw new UnityException ("Cache system error: Current Canvas is not saved as the temporary cache!");
			NodeEditorSaveManager.AddSubAsset (node, path);
			foreach (NodeKnob knob in node.nodeKnobs)
				NodeEditorSaveManager.AddSubAsset (knob, path);

			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
		}

		private void SaveCache () 
		{
			string canvasName = mainNodeCanvas.name;
			EditorPrefs.SetString ("NodeEditorLastSession", canvasName);
			NodeEditorSaveManager.SaveNodeCanvas (tempSessionPath + "/LastSession.asset", false, mainNodeCanvas, mainEditorState);
			mainNodeCanvas.name = canvasName;

			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
		}

		private void LoadCache () 
		{
			string lastSessionName = EditorPrefs.GetString ("NodeEditorLastSession");
			string path = tempSessionPath + "/LastSession.asset";
			mainNodeCanvas = NodeEditorSaveManager.LoadNodeCanvas (path, false);
			if (mainNodeCanvas == null)
				NewNodeCanvas ();
			else 
			{
				mainNodeCanvas.name = lastSessionName;
				List<NodeEditorState> editorStates = NodeEditorSaveManager.LoadEditorStates (path, false);
				if (editorStates == null || editorStates.Count == 0 || (mainEditorState = editorStates.Find (x => x.name == "MainEditorState")) == null )
				{ // New NodeEditorState
					mainEditorState = CreateInstance<NodeEditorState> ();
					mainEditorState.canvas = mainNodeCanvas;
					mainEditorState.name = "MainEditorState";
					NodeEditorSaveManager.AddSubAsset (mainEditorState, path);
					AssetDatabase.SaveAssets ();
					AssetDatabase.Refresh ();
				}
                NodeEditor.RecalculateAll(mainNodeCanvas);
                Repaint();

            }
        }

		private void DeleteCache () 
		{
			string lastSession = EditorPrefs.GetString ("NodeEditorLastSession");
			if (!String.IsNullOrEmpty (lastSession))
			{
				AssetDatabase.DeleteAsset (tempSessionPath + "/" + lastSession);
				AssetDatabase.Refresh ();
			}
			EditorPrefs.DeleteKey ("NodeEditorLastSession");
		}

		#endregion

		#region Save/Load
		
		/// <summary>
		/// Saves the mainNodeCanvas and it's associated mainEditorState as an asset at path
		/// </summary>
		public void SaveNodeCanvas (string path) 
		{
			NodeEditorSaveManager.SaveNodeCanvas (path, true, mainNodeCanvas, mainEditorState);
			Repaint ();
		}
		
		/// <summary>
		/// Loads the mainNodeCanvas and it's associated mainEditorState from an asset at path
		/// </summary>
		public void LoadNodeCanvas (string path) 
		{
			// Load the NodeCanvas
			mainNodeCanvas = NodeEditorSaveManager.LoadNodeCanvas (path, true);
			if (mainNodeCanvas == null) 
			{
				Debug.Log ("Error loading NodeCanvas from '" + path + "'!");
				NewNodeCanvas ();
				return;
			}
			
			// Load the associated MainEditorState
			List<NodeEditorState> editorStates = NodeEditorSaveManager.LoadEditorStates (path, true);
			if (editorStates.Count == 0) 
			{
				mainEditorState = ScriptableObject.CreateInstance<NodeEditorState> ();
				Debug.LogError ("The save file '" + path + "' did not contain an associated NodeEditorState!");
			}
			else 
			{
				mainEditorState = editorStates.Find (x => x.name == "MainEditorState");
				if (mainEditorState == null) mainEditorState = editorStates[0];
			}
			mainEditorState.canvas = mainNodeCanvas;

			openedCanvasPath = path;
			NodeEditor.RecalculateAll (mainNodeCanvas);
			SaveCache ();
			Repaint ();
		}

		/// <summary>
		/// Creates and opens a new empty node canvas
		/// </summary>
		public void NewNodeCanvas (int _texWidth=1024,int _texHeight=1024) 
		{
			// New NodeCanvas
			mainNodeCanvas = CreateInstance<NodeCanvas> ();
			mainNodeCanvas.name = "New Canvas";
		    mainNodeCanvas.m_TexWidth = _texWidth;
            mainNodeCanvas.m_TexHeight = _texHeight;
            // New NodeEditorState
            mainEditorState = CreateInstance<NodeEditorState> ();
			mainEditorState.canvas = mainNodeCanvas;
			mainEditorState.name = "MainEditorState";

			openedCanvasPath = "";
			SaveCache ();
		}
		
		#endregion
	}
}