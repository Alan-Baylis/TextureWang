//#define NODE_EDITOR_LINE_CONNECTION

using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEditor.TreeViewExamples;

namespace NodeEditorFramework
{
	/// <summary>
	/// Central class of NodeEditor providing the GUI to draw the Node Editor Canvas, bundling all other parts of the Framework
	/// Only Calculation is yet to be split from this
	/// </summary>
	public static class NodeEditor 
	{
		public static string editorPath = "Assets/Plugins/Node_Editor/";

		// The NodeCanvas which represents the currently drawn Node Canvas; globally accessed
		public static NodeCanvas curNodeCanvas;
		public static NodeEditorState curEditorState;

		// Temp GUI state variables
		private static bool unfocusControls;
		private static Vector2 mousePos;

		// GUI callback control
		internal static Action NEUpdate;
		public static void Update () { if (NEUpdate != null) NEUpdate (); }
		public static Action ClientRepaints;
		public static void RepaintClients () { if (ClientRepaints != null) ClientRepaints (); }

		#region Setup

		public static bool initiated;
		public static bool InitiationError;
	    private static Vector2 lastClickPos;
        


        /// <summary>
        /// Initiates the Node Editor if it wasn't yet
        /// </summary>
        public static void checkInit () 
		{
			if (!initiated && !InitiationError)
				ReInit (true);
		}

		/// <summary>
		/// Re-Inits the NodeCanvas regardless of whetehr it was initiated before
		/// </summary>
		public static void ReInit (bool GUIFunction) 
		{
			CheckEditorPath ();

			// Init Resource system. Can be called anywhere else, too, if it's needed before.
			ResourceManager.SetDefaultResourcePath (editorPath + "Resources/");
			
			// Init NE GUI. I may throw an error if a texture was not found.	
			if (!NodeEditorGUI.Init (GUIFunction)) 
			{	
				InitiationError = true;
				return;
			}

			// Run fetching algorithms searching the script assemblies for Custom Nodes / Connection Types
			ConnectionTypes.FetchTypes ();
			NodeTypes.FetchNodes ();

		    

            // Setup Callback system
            NodeEditorCallbacks.SetupReceivers ();
			NodeEditorCallbacks.IssueOnEditorStartUp ();

			// Init GUIScaleUtility. This fetches reflected calls and my throw a message notifying about incompability.
			GUIScaleUtility.CheckInit ();

	#if UNITY_EDITOR
			UnityEditor.EditorApplication.update -= Update;
			UnityEditor.EditorApplication.update += Update;
			RepaintClients ();
	#endif
			initiated = true;
		}

		/// <summary>
		/// Checks the editor path and corrects it when possible.
		/// </summary>
		public static void CheckEditorPath () 
		{
	#if UNITY_EDITOR
			Object script = UnityEditor.AssetDatabase.LoadAssetAtPath (editorPath + "Framework/NodeEditor.cs", typeof(Object));
			if (script == null) 
			{
				string[] assets = UnityEditor.AssetDatabase.FindAssets ("NodeEditorCallbackReceiver"); // Something relatively unique
				if (assets.Length != 1) 
				{
					assets = UnityEditor.AssetDatabase.FindAssets ("ConnectionTypes"); // Another try
					if (assets.Length != 1) 
						throw new UnityException ("Node Editor: Not installed in default directory '" + editorPath + "'! Correct path could not be detected! Please correct the editorPath variable in NodeEditor.cs!");
				}
				
				string correctEditorPath = UnityEditor.AssetDatabase.GUIDToAssetPath (assets[0]);
				int subFolderIndex = correctEditorPath.LastIndexOf ("Framework/");
				if (subFolderIndex == -1)
					throw new UnityException ("Node Editor: Not installed in default directory '" + editorPath + "'! Correct path could not be detected! Please correct the editorPath variable in NodeEditor.cs!");
				correctEditorPath = correctEditorPath.Substring (0, subFolderIndex);
				
				Debug.LogWarning ("Node Editor: Not installed in default directory '" + editorPath + "'! " +
				                  "Editor-only automatic detection adjusted the path to " + correctEditorPath + ", but if you plan to use at runtime, please correct the editorPath variable in NodeEditor.cs!");
				editorPath = correctEditorPath;
			}
	#endif
		}
		
		#endregion
		
		#region GUI

		/// <summary>
		/// Draws the Node Canvas on the screen in the rect specified by editorState
		/// </summary>
		public static void DrawCanvas (NodeCanvas nodeCanvas, NodeEditorState editorState)  
		{
			if (!editorState.drawing)
				return;
			checkInit ();

			NodeEditorGUI.StartNodeGUI ();
			OverlayGUI.StartOverlayGUI ();
			DrawSubCanvas (nodeCanvas, editorState);
			OverlayGUI.EndOverlayGUI ();
			NodeEditorGUI.EndNodeGUI ();
		}

		/// <summary>
		/// Draws the Node Canvas on the screen in the rect specified by editorState without one-time wrappers like GUISkin and OverlayGUI. Made for nested Canvases (WIP)
		/// </summary>
		public static void DrawSubCanvas (NodeCanvas nodeCanvas, NodeEditorState editorState)  
		{
			if (!editorState.drawing)
				return;
            // Store and restore later on in case of this being a nested Canvas
            NodeCanvas prevNodeCanvas = curNodeCanvas;
			NodeEditorState prevEditorState = curEditorState;
			
			curNodeCanvas = nodeCanvas;
			curEditorState = editorState;

			if (Event.current.type == EventType.Repaint) 
			{ // Draw Background when Repainting
				GUI.BeginClip (curEditorState.canvasRect);
				
				float width = NodeEditorGUI.Background.width / curEditorState.zoom;
				float height = NodeEditorGUI.Background.height / curEditorState.zoom;
				Vector2 offset = curEditorState.zoomPos + curEditorState.panOffset/curEditorState.zoom;
				offset = new Vector2 (offset.x%width - width, offset.y%height - height);
				int tileX = Mathf.CeilToInt ((curEditorState.canvasRect.width + (width - offset.x)) / width);
				int tileY = Mathf.CeilToInt ((curEditorState.canvasRect.height + (height - offset.y)) / height);
				
				for (int x = 0; x < tileX; x++) 
				{
					for (int y = 0; y < tileY; y++) 
					{
						GUI.DrawTexture (new Rect (offset.x + x*width, 
												   offset.y + y*height, 
												   width, height), 
										 NodeEditorGUI.Background);
					}
				}
				GUI.EndClip ();
			}
			
			// Check the inputs
			InputEvents ();
			if (Event.current.type != EventType.Layout)
				curEditorState.ignoreInput = new List<Rect> ();

			// We're using a custom scale method, as default one is messing up clipping rect
			Rect canvasRect = curEditorState.canvasRect;
			curEditorState.zoomPanAdjust = GUIScaleUtility.BeginScale (ref canvasRect, curEditorState.zoomPos, curEditorState.zoom, false);
            //GUILayout.Label ("Scaling is Great!"); -> TODO: Test by changing the last bool parameter

            // ---- BEGIN SCALE ----
            // Draw group nodes first, they are just backgrounds
            foreach (Node node in curNodeCanvas.nodes)
            {
                if (node is GroupNode)
                    node.DrawNode(false);
            }
            // Some features which require drawing (zoomed)
            if (curEditorState.navigate) 
			{ // Draw a curve to the origin/active node for orientation purposes
				RTEditorGUI.DrawLine ((curEditorState.selectedNode != null? curEditorState.selectedNode.rect.center : curEditorState.panOffset) + curEditorState.zoomPanAdjust, 
										ScreenToGUIPos (mousePos) + curEditorState.zoomPos * curEditorState.zoom, 
										Color.black, null, 3); 
				RepaintClients ();
			}
			if (curEditorState.connectOutput != null)
			{ // Draw the currently drawn connection
				NodeOutput output = curEditorState.connectOutput;
				Vector2 startPos = output.GetGUIKnob ().center;
				Vector2 endPos = ScreenToGUIPos (mousePos) + curEditorState.zoomPos * curEditorState.zoom;
				Vector2 endDir = output.GetDirection ();
				NodeEditorGUI.DrawConnection (startPos, endDir, endPos, 
												NodeEditorGUI.GetSecondConnectionVector (startPos, endPos, endDir), 
												ConnectionTypes.GetTypeData (output.type, true).Color);
				RepaintClients ();
			}
			if (curEditorState.makeTransition != null)
			{ // Draw the currently made transition
				RTEditorGUI.DrawLine (curEditorState.makeTransition.rect.center + curEditorState.zoomPanAdjust, 
										ScreenToGUIPos (mousePos) + curEditorState.zoomPos * curEditorState.zoom,
										Color.grey, null, 3); 
				RepaintClients ();
			}

			// Push the active node at the bottom of the draw order.
			if (Event.current.type == EventType.Layout && curEditorState.selectedNode != null)
			{
				curNodeCanvas.nodes.Remove (curEditorState.selectedNode);
				curNodeCanvas.nodes.Add (curEditorState.selectedNode);
			}

            // Draw the transitions and connections. Has to be drawn before nodes as transitions originate from node centers
		    foreach (Node node in curNodeCanvas.nodes)
		    {
                if(node)
		            node.DrawConnections();
		    }

		    // Draw non group nodes
			foreach (Node node in curNodeCanvas.nodes)
			{
			    if (node == null)
			        continue;
			    if (node is GroupNode)
			        continue;
                node.DrawNode (false);
				if (Event.current.type == EventType.Repaint)
					node.DrawKnobs ();
			}

			// ---- END SCALE ----

			// End scaling group
			GUIScaleUtility.EndScale ();
			
			// Check events with less priority than node GUI controls
			LateEvents ();

            curNodeCanvas = prevNodeCanvas;
			curEditorState = prevEditorState;
		}
		
		#endregion
		
		#region GUI Functions

		/// <summary>
		/// Returns the node at the position in the current canvas spcae. Depends on curEditorState and curNodecanvas
		/// </summary>
		public static Node NodeAtPosition (Vector2 pos,Node ignore=null,bool _onlyGroupNodes = true)
		{
			return NodeAtPosition (curEditorState, curNodeCanvas, pos, ignore, _onlyGroupNodes);
		}
		/// <summary>
		/// Returns the node at the position in specified canvas space.
		/// </summary>
		public static Node NodeAtPosition (NodeEditorState editorState, NodeCanvas nodeCanvas, Vector2 pos,Node ignore, bool _onlyGroupNodes)
		{	
			if (!editorState.canvasRect.Contains (pos))
				return null;
            //pass1 ignore group nodes
		    if (!_onlyGroupNodes)
		    {
		        for (int nodeCnt = nodeCanvas.nodes.Count - 1; nodeCnt >= 0; nodeCnt--)
		        {
		            // Check from top to bottom because of the render order
		            Node node = nodeCanvas.nodes[nodeCnt];
		            if (node == ignore)
		                continue;
		            if (node is GroupNode)
		                continue;
		            if (CanvasGUIToScreenRect(node.rect).Contains(pos)) // Node Body
		                return node;
		            foreach (NodeKnob knob in node.nodeKnobs)
		            {
		                // Any edge control
		                if (knob == null)
		                {
		                    Debug.LogError("null knob in "+node);
		                }
                        else
		                if (knob.GetScreenKnob().Contains(pos))
		                    return node;
		            }
		        }
		    }
		    //pass2 only group nodes if we didnt hit a node
            for (int nodeCnt = nodeCanvas.nodes.Count-1; nodeCnt >= 0; nodeCnt--) 
			{ // Check from top to bottom because of the render order
				Node node = nodeCanvas.nodes [nodeCnt];
			    if (node == ignore)
			        continue;
                if (!(node is GroupNode))
                    continue;
                if (CanvasGUIToScreenRect (node.rect).Contains (pos)) // Node Body
					return node;
				foreach (NodeKnob knob in node.nodeKnobs)
				{ // Any edge control
					if (knob.GetScreenKnob ().Contains (pos))
						return node;
				}
			}
			return null;
		}

		/// <summary>
		/// Transforms the Rect in GUI space into Screen space. Depends on curEditorState
		/// </summary>
		public static Rect CanvasGUIToScreenRect (Rect rect) 
		{
			return CanvasGUIToScreenRect (curEditorState, rect);
		}
		/// <summary>
		/// Transforms the Rect in GUI space into Screen space
		/// </summary>
		public static Rect CanvasGUIToScreenRect (NodeEditorState editorState, Rect rect) 
		{
			rect.position += editorState.zoomPos;
			rect = GUIScaleUtility.ScaleRect (rect, editorState.zoomPos, 
					editorState.parentEditor != null? new Vector2 (1/(editorState.parentEditor.zoom*editorState.zoom), 1/(editorState.parentEditor.zoom*editorState.zoom)) : 
														new Vector2 (1/editorState.zoom, 1/editorState.zoom));
			rect.position += editorState.canvasRect.position;
			return rect;
		}

		/// <summary>
		/// Transforms screen position pos (like mouse pos) to a point in current GUI space
		/// </summary>
		public static Vector2 ScreenToGUIPos (Vector2 pos) 
		{
			return ScreenToGUIPos (curEditorState, pos);
		}
		/// <summary>
		/// Transforms screen position pos (like mouse pos) to a point in specified GUI space
		/// </summary>
		public static Vector2 ScreenToGUIPos (NodeEditorState editorState, Vector2 pos) 
		{
			return Vector2.Scale (pos - editorState.zoomPos - editorState.canvasRect.position, new Vector2 (editorState.zoom, editorState.zoom));
		}

		/// <summary>
		/// Returns whether to account for input in curEditorState
		/// </summary>
		private static bool ignoreInput (Vector2 mousePos) 
		{
			// Account for any opened popups
			if (OverlayGUI.HasPopupControl ())
				return true;
			// Mouse outside of canvas rect or inside an ignoreInput rect
			if (!curEditorState.canvasRect.Contains (mousePos))
				return true;
			foreach (Rect ignoreRect in curEditorState.ignoreInput) 
			{
				if (ignoreRect.Contains (mousePos)) 
					return true;
			}
			return false;
		}
		
		#endregion
		
		#region Input Events

		/// <summary>
		/// Processes input events
		/// </summary>
		public static void InputEvents ()
		{
			Event e = Event.current;
			mousePos = e.mousePosition;

			bool leftClick = e.button == 0, rightClick = e.button == 1,
				mouseDown = e.type == EventType.MouseDown, mousUp = e.type == EventType.MouseUp;

			if (ignoreInput (mousePos))
				return;

			#region Change Node selection and focus
			// Choose focused and selected Node, accounting for focus changes
			curEditorState.focusedNode = null;


            EventType eventType = Event.current.type;
		    int button = Event.current.button;
            bool isAccepted = false;
//            Debug.Log(" event "+eventType+" button "+Event.current.button);
            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                if (eventType == EventType.DragPerform && DragAndDrop.objectReferences.Length > 0 &&
                    (DragAndDrop.objectReferences[0] is Texture2D))
                {
                    Debug.Log(" drag texture "+ DragAndDrop.objectReferences[0]);
                    UnityTextureInput node = (UnityTextureInput)Node.Create("UnityTextureInput", NodeEditor.ScreenToGUIPos(Event.current.mousePosition));
                    node.m_Input = DragAndDrop.objectReferences[0] as Texture2D;
                    

                }
                else if (eventType == EventType.DragPerform && DragAndDrop.GetGenericData("GenericDragColumnDragging") != null)
                {
                    DragAndDrop.AcceptDrag();
                    Debug.LogError("dragged generic data "+ DragAndDrop.GetGenericData("GenericDragColumnDragging"));
                    List<UnityEditor.IMGUI.Controls.TreeViewItem> _data =DragAndDrop.GetGenericData("GenericDragColumnDragging") as List<UnityEditor.IMGUI.Controls.TreeViewItem>;
                    var draggedElements = new List<TreeElement>();
                    foreach (var x in _data)
                        draggedElements.Add(((TreeViewItem<MyTreeElement>)x).data);

                    var srcItem = draggedElements[0] as MyTreeElement;
                    if (srcItem.m_Canvas)
                    {
                        SubTreeNode node = (SubTreeNode)Node.Create("SubTreeNode", NodeEditor.ScreenToGUIPos(Event.current.mousePosition));
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
                        Node node = Node.Create(srcItem.m_NodeID, NodeEditor.ScreenToGUIPos(Event.current.mousePosition));
                    }
                    
                    isAccepted = true;
                }
                else
                if (eventType == EventType.DragPerform && DragAndDrop.objectReferences.Length>0 && (DragAndDrop.objectReferences[0] is NodeCanvas))
                {
                    DragAndDrop.AcceptDrag();
//                    Debug.Log(" drag and drop " + DragAndDrop.objectReferences[0] + " co ord " + Event.current.mousePosition);
                    SubTreeNode node = (SubTreeNode)Node.Create("SubTreeNode", NodeEditor.ScreenToGUIPos(Event.current.mousePosition));
                    node.SubCanvas = (NodeCanvas)DragAndDrop.objectReferences[0];
                    string assetPath = AssetDatabase.GetAssetPath(node.SubCanvas);
//                    Debug.Log("drag and drop asset canvas path >" + assetPath + "<");
                    if (assetPath.Length > 0)
                    {
                        node.m_CanvasGuid = AssetDatabase.AssetPathToGUID(assetPath);
                        Debug.LogError(" set canvasGuid from asset >" + node.m_CanvasGuid + "<");
                        string fname=Path.GetFileName(assetPath);
                        fname = Path.ChangeExtension(fname, "");
                        node.name = "Sub:" + fname;
                    }
                    

                    node.OnLoadCanvas();
                    isAccepted = true;
                }
                Event.current.Use();
            }



//		    Debug.Log(" Event.current.type " + Event.current.type);
            if (mouseDown || mousUp)
			{
				curEditorState.focusedNode = NodeEditor.NodeAtPosition (mousePos,null,false);
				if (curEditorState.focusedNode != curEditorState.selectedNode)
					unfocusControls = true;
//                if(Event.current.type == EventType.Repaint) 
				if (mouseDown && leftClick) 
				{
					curEditorState.wantselectedNode = curEditorState.focusedNode;
				}
			}
			// Perform above mentioned focus changes in Repaint, which is the only suitable time to do this
			if (unfocusControls && Event.current.type == EventType.Repaint) 
			{
				GUIUtility.hotControl = 0;
				GUIUtility.keyboardControl = 0;
				unfocusControls = false;
			}
		#if UNITY_EDITOR
			if (curEditorState.focusedNode != null)
				UnityEditor.Selection.activeObject = curEditorState.focusedNode;
#endif
            #endregion


			switch (e.type) 
			{
			case EventType.MouseDown:

				curEditorState.dragNode = false;
				curEditorState.panWindow = false;

                    {
                        var ev = Event.current;

                        //if (wasclick && !wasused && ev.type == EventType.Used)

                        {
                            if ((lastClickPos - ev.mousePosition).sqrMagnitude <= 5 * 5 && ev.clickCount > 1)
                            {
                                curEditorState.selectedNode.OpenPreview();
                            }
                            else
                                lastClickPos = ev.mousePosition; // lastClickPos is class/global variable
                        }
                    }

                    if (curEditorState.focusedNode != null && ( !(curEditorState.focusedNode is GroupNode) ||!rightClick)) 
				{ // Clicked a Node






                        if (rightClick)
					{ // Node Context Click
                        var menu = new NodeEditorFramework.Utilities.GenericMenu();
						menu.AddItem (new GUIContent ("Delete Node"), false, ContextCallback, new NodeEditorMenuCallback ("deleteNode", curNodeCanvas, curEditorState));
						menu.AddItem (new GUIContent ("Duplicate Node"), false, ContextCallback, new NodeEditorMenuCallback ("duplicateNode", curNodeCanvas, curEditorState));
						if (curEditorState.focusedNode.AcceptsTranstitions)
						{
							menu.AddSeparator ("Seperator");
							menu.AddItem (new GUIContent ("Make Transition"), false, ContextCallback, new NodeEditorMenuCallback ("startTransition", curNodeCanvas, curEditorState));
						}
						menu.ShowAsContext ();
						e.Use ();
					}
					else if (leftClick)
					{ // Detect click on a connection knob
						if (!CanvasGUIToScreenRect (curEditorState.focusedNode.rect).Contains (mousePos))
						{ // Clicked NodeEdge, check Node Inputs and Outputs
							NodeOutput nodeOutput = curEditorState.focusedNode.GetOutputAtPos (e.mousePosition);
							if (nodeOutput != null)
							{ // Output clicked -> New Connection drawn from this
								curEditorState.connectOutput = nodeOutput;
								e.Use();
								return;
							}

							NodeInput nodeInput = curEditorState.focusedNode.GetInputAtPos (e.mousePosition);
							if (nodeInput != null && nodeInput.connection != null)
							{ // Input clicked -> Loose and edit Connection
								// TODO: Draw input from NodeInput
								curEditorState.connectOutput = nodeInput.connection;
								nodeInput.RemoveConnection ();
								e.Use();
							}
						}
                        else if (curEditorState.focusedNode.m_CanDragSize && CanvasGUIToScreenRect(curEditorState.focusedNode.GetDragRect()).Contains(mousePos))
                        {
                                curEditorState.dragNodeSize = true;
                        }
                        else if ( CanvasGUIToScreenRect(curEditorState.focusedNode.GetCloseRect()).Contains(mousePos))
                        {
                                if (curEditorState.focusedNode != null)
                                    curEditorState.focusedNode.Delete();

                         }

                   }
				}
				else
				{ // Clicked on canvas
					
					// NOTE: Panning is not done here but in LateEvents, so buttons on the canvas won't be blocked when clicking

					if (rightClick) 
					{ // Editor Context Click
						var menu = new NodeEditorFramework.Utilities.GenericMenu();
						if (curEditorState.connectOutput != null) 
						{ // A connection is drawn, so provide a context menu with apropriate nodes to auto-connect
							foreach (Node node in NodeTypes.nodes.Keys)
							{ // Iterate through all nodes and check for compability
								foreach (NodeInput input in node.Inputs)
								{
									if (input.type == curEditorState.connectOutput.type)
									{

                                                menu.AddItem (new GUIContent ("Add " + NodeTypes.nodes[node].adress), false, ContextCallback, new NodeEditorMenuCallback (node.GetID, curNodeCanvas, curEditorState));

										break;
									}
								}
							}
						}
						else if (curEditorState.makeTransition != null && curEditorState.makeTransition.AcceptsTranstitions) 
						{ // A transition is drawn, so provide a context menu with nodes to auto-connect
							foreach (Node node in NodeTypes.nodes.Keys)
							{ // Iterate through all nodes and check for compability
								if (node.AcceptsTranstitions)
									menu.AddItem (new GUIContent ("Add " + NodeTypes.nodes[node].adress), false, ContextCallback, new NodeEditorMenuCallback (node.GetID, curNodeCanvas, curEditorState));
							}
						}
						else 
						{ // Ordinary context click, add all nodes to add
							foreach (Node node in NodeTypes.nodes.Keys)
								menu.AddItem (new GUIContent ("Add " + NodeTypes.nodes [node].adress), false, ContextCallback, new NodeEditorMenuCallback (node.GetID, curNodeCanvas, curEditorState));
						}
						menu.ShowAsContext ();
						e.Use ();
					}
				}
				
				break;
				
			case EventType.MouseUp:
			        if (curEditorState.focusedNode is GroupNode && (curEditorState.dragNode || curEditorState.dragNodeSize))
			        {
			            GroupNode gNode = curEditorState.focusedNode as GroupNode;
			            //Check children are still inside

			            for (int index = gNode.m_Children.Count - 1; index >= 0; index--)
			            {
			                Node n = gNode.m_Children[index];
			                if (n == null)
			                {
                                gNode.m_Children.RemoveAt(index);
			                    continue;
			                }
			                if (!gNode.rect.Overlaps(n.rect)) // Node Body
			                {
			                    gNode.m_Children.RemoveAt(index);
			                    n.m_Parent = null;
			                }
			            }
                        //Do any new nodes now overlap
			            foreach (Node n in curNodeCanvas.nodes)
			            {
			                if (n.m_Parent || n is GroupNode)
			                    continue;
			                if (gNode.rect.Overlaps(n.rect)) // Node Body
			                {
			                    gNode.m_Children.Add(n);
			                    n.m_Parent = gNode;
			                }
			            }

			        }

			        if (curEditorState.focusedNode != null && curEditorState.connectOutput != null) 
				{ // Apply Drawn connections on node if theres a clicked input
					if (!curEditorState.focusedNode.Outputs.Contains (curEditorState.connectOutput)) 
					{ // An input was clicked, it'll will now be connected
						NodeInput clickedInput = curEditorState.focusedNode.GetInputAtPos (e.mousePosition);
						if (clickedInput!=null && clickedInput.CanApplyConnection (curEditorState.connectOutput)) 
						{ // It can connect (type is equals, it does not cause recursion, ...)
							clickedInput.ApplyConnection (curEditorState.connectOutput);
						}
					}
					e.Use ();
				}
			    if (curEditorState.dragNode)
			    {
                        //we were dragging did we drag it onto a group node
                    GroupNode under =NodeEditor.NodeAtPosition(mousePos, curEditorState.focusedNode,true) as GroupNode;
			        if (under != null )
			        {
			            if (curEditorState.focusedNode && curEditorState.focusedNode.m_Parent != under)
			            {
			                if (curEditorState.focusedNode.m_Parent)
			                    (curEditorState.focusedNode.m_Parent as GroupNode).m_Children.Remove(curEditorState.focusedNode);

			                Debug.Log("reparent " + curEditorState.focusedNode + " to " + under);
			                under.m_Children.Add(curEditorState.focusedNode);
			                curEditorState.focusedNode.m_Parent = under;
			            }
                        // else it was just moved inside its own parent

			        }
			        else
			        {
			            if (curEditorState.focusedNode && curEditorState.focusedNode.m_Parent)
			            {
                                Debug.Log("unparent "+ curEditorState.focusedNode+" from "+ curEditorState.focusedNode.m_Parent);
                                (curEditorState.focusedNode.m_Parent as GroupNode).m_Children.Remove(curEditorState.focusedNode);
			                curEditorState.focusedNode.m_Parent = null;
                                
			            }
			        }

                }

                curEditorState.dragNodeSize = false;
                curEditorState.makeTransition = null;
				curEditorState.connectOutput = null;
				curEditorState.dragNode = false;
				curEditorState.panWindow = false;

                    
				
				break;
				
			case EventType.ScrollWheel:

				// Apply Zoom
				curEditorState.zoom = (float)Math.Round (Math.Min (2.0f, Math.Max (0.1f, curEditorState.zoom + e.delta.y / 15)), 2);

				RepaintClients ();
				break;
				
			case EventType.KeyDown:

				// TODO: Node Editor: Shortcuts

				if (e.keyCode == KeyCode.N) // Start Navigating (curve to origin / active Node)
					curEditorState.navigate = true;
                    if (e.keyCode == KeyCode.D && curEditorState.selectedNode!=null) // Stop Navigating
                        curEditorState.selectedNode.OpenPreview();

                    if (e.keyCode == KeyCode.LeftControl && curEditorState.selectedNode != null)
				{ // Snap selected Node's position to multiples of 10
					Vector2 pos = curEditorState.selectedNode.rect.position;
					pos = (pos - curEditorState.panOffset) / 10;
					pos = new Vector2 (Mathf.RoundToInt (pos.x), Mathf.RoundToInt (pos.y));
					curEditorState.selectedNode.rect.position = pos * 10 + curEditorState.panOffset;
				}

				RepaintClients ();
				break;
				
			case EventType.KeyUp:
				
				if (e.keyCode == KeyCode.N) // Stop Navigating
					curEditorState.navigate = false;
				
				RepaintClients ();
				break;
			
			case EventType.MouseDrag:
                    RepaintClients();
                    if (curEditorState.panWindow && button==2) 
				{ // Scroll everything with the current mouse delta
					curEditorState.panOffset += e.delta * curEditorState.zoom;
				    foreach (Node node in curNodeCanvas.nodes)
				    {
                        if(node!=null)
				            node.rect.position += e.delta*curEditorState.zoom;
				    }
				    e.delta = Vector2.zero;
					RepaintClients ();
				}
			    if (curEditorState.dragNodeSize)
			    {
                    Vector2 delta = e.delta * curEditorState.zoom;
                    curEditorState.selectedNode.rect.size += delta;
                }
				if (curEditorState.dragNode && curEditorState.selectedNode != null && GUIUtility.hotControl == 0 && button==0) 
				{ // Drag the active node with the current mouse delta
				    Vector2 delta = e.delta*curEditorState.zoom;
                    curEditorState.selectedNode.rect.position += delta;
				    if (curEditorState.selectedNode is GroupNode)
				    {
				        foreach (var n in (curEditorState.selectedNode as GroupNode).m_Children)
                        {
                            if(n)
				                n.rect.position += delta;
                        }
				    }
				    if (e.alt)
				    {
                        Dictionary<Node, int> d = new Dictionary<Node, int>();
                        d[curEditorState.selectedNode] = 1;

				        MoveChildren(ref d, curEditorState.selectedNode, delta);
				    }
				    NodeEditorCallbacks.IssueOnMoveNode(curEditorState.selectedNode);
                    e.delta = Vector2.zero;
					RepaintClients ();
				} 
				else
					curEditorState.dragNode = false;

				break;
			}
		}

	    static void MoveChildren(ref Dictionary<Node,int> _dic,Node _n,Vector2 _delta)
	    {
	        foreach (var input in _n.Inputs)
	        {
                Node cn = input.connection.body;
	            if (!_dic.ContainsKey(cn))
	            {
                    
                    cn.rect.position += _delta;
	                NodeEditorCallbacks.IssueOnMoveNode(cn);
	                MoveChildren(ref _dic, cn, _delta);
	                _dic[cn] = 1;
	            }
	        }

	    }
        /// <summary>
        /// Proccesses late events. Called after GUI Functions, when they have higher priority in focus
        /// </summary>
        public static void LateEvents () 
		{
			Event e = Event.current;

			if (ignoreInput (mousePos))
				return;

			if (e.type == EventType.MouseDown )
			{ // Left click
				if (GUIUtility.hotControl <= 0)
				{ // Did not click on a GUI Element
				    Rect titleRect=Rect.zero;
				    if (curEditorState.selectedNode != null)
				    {
				        titleRect = curEditorState.selectedNode.rect;
				        titleRect.yMax = titleRect.yMin + 20;
				    }
				    if (e.button == 0 && curEditorState.selectedNode != null && CanvasGUIToScreenRect (titleRect).Contains (e.mousePosition)) 
					{ // Clicked inside the selected Node, so start dragging it
						curEditorState.dragNode = true;
						e.delta = Vector2.zero;
                        RepaintClients();
                    }
					else if (e.button == 2 )//&& (curEditorState.focusedNode == null|| curEditorState.focusedNode is GroupNode)) 
					{ // Clicked on the empty canvas
						//if (e.button == 0 || e.button == 2)
						{ // Start panning
							curEditorState.panWindow = true;
							e.delta = Vector2.zero;
						}
					}
                    
                }
			}
		}

		/// <summary>
		/// Evaluates context callbacks previously registered
		/// </summary>
		public static void ContextCallback (object obj)
		{
			NodeEditorMenuCallback callback = obj as NodeEditorMenuCallback;
			if (callback == null)
				throw new UnityException ("Callback Object passed by context is not of type NodeEditorMenuCallback!");
			curNodeCanvas = callback.canvas;
			curEditorState = callback.editor;

			switch (callback.message)
			{
			case "deleteNode": // Delete request
				if (curEditorState.focusedNode != null) 
					curEditorState.focusedNode.Delete ();
				break;
				
			case "duplicateNode": // Duplicate request
				if (curEditorState.focusedNode != null) 
				{
					ContextCallback (new NodeEditorMenuCallback (curEditorState.focusedNode.GetID, curNodeCanvas, curEditorState));
					Node duplicatedNode = curNodeCanvas.nodes [curNodeCanvas.nodes.Count-1];
				    duplicatedNode.rect = curEditorState.focusedNode.rect;
				    duplicatedNode.rect.x += 50;
                    duplicatedNode.rect.y += 50;

                        curEditorState.focusedNode = duplicatedNode;
					curEditorState.dragNode = true;
					curEditorState.makeTransition = null;
					curEditorState.connectOutput = null;
					curEditorState.panWindow = false;
				}
				break;

			case "startTransition": // Starting a new transition
				if (curEditorState.focusedNode != null) 
				{
					curEditorState.makeTransition = curEditorState.focusedNode;
					curEditorState.connectOutput = null;
				}
				curEditorState.dragNode = false;
				curEditorState.panWindow = false;

				break;

			default: // Node creation request
				    Node node = Node.Create (callback.message, ScreenToGUIPos (callback.contextClickPos));

                    GroupNode under = NodeEditor.NodeAtPosition(mousePos, node, true) as GroupNode;
                    if (under != null)
                    {
                        Debug.Log("reparent " + curEditorState.focusedNode + " to " + under);
                        under.m_Children.Add(node);
                        node.m_Parent = under;
                    }


                    // Handle auto-connection
                    if (curEditorState.connectOutput != null)
				{ // If nodeOutput is defined, link it to the first input of the same type
					foreach (NodeInput input in node.Inputs)
					{
						if (input.CanApplyConnection (curEditorState.connectOutput))
						{ // If it can connect (type is equals, it does not cause recursion, ...)
							input.ApplyConnection (curEditorState.connectOutput);
							break;
						}
					}
				}

				curEditorState.makeTransition = null;
				curEditorState.connectOutput = null;
				curEditorState.dragNode = false;
				curEditorState.panWindow = false;

				break;
			}
			RepaintClients ();
		}

		public class NodeEditorMenuCallback
		{
			public string message;
			public NodeCanvas canvas;
			public NodeEditorState editor;
			public Vector2 contextClickPos;

			public NodeEditorMenuCallback (string Message, NodeCanvas nodecanvas, NodeEditorState editorState) 
			{
				message = Message;
				canvas = nodecanvas;
				editor = editorState;
				contextClickPos = Event.current.mousePosition;
			}
		}
		
		#endregion

		#region Calculation
		
		// A list of Nodes from which calculation originates -> Call StartCalculation
		public static List<Node> workList;
		private static int calculationCount;

		/// <summary>
		/// Recalculate from every Input Node.
		/// Usually does not need to be called at all, the smart calculation system is doing the job just fine
		/// </summary>
		public static void RecalculateAll (NodeCanvas nodeCanvas) 
		{
			workList = new List<Node> ();
			foreach (Node node in nodeCanvas.nodes) 
			{
                if (node != null)
                    node.ClearCalculation();
                if (node!=null && node.isInput ())
				{ // Add all Inputs
					
					workList.Add (node);
				}
			}
			StartCalculation ();
		}
        /// <summary>
        /// Recalculate from every Input Node.
        /// Usually does not need to be called at all, the smart calculation system is doing the job just fine
        /// </summary>
        public static void RecalculateAllAnd(NodeCanvas nodeCanvas,List<Node> _workList )
        {
            workList = new List<Node>();
            foreach(var x in _workList)
                workList.Add(x);
            foreach (Node node in nodeCanvas.nodes)
            {
                if (node.isInput())
                { // Add all Inputs
                    node.ClearCalculation();
                    workList.Add(node);
                }
            }
            StartCalculation();
        }
        /// <summary>
        /// Recalculate from this node. 
        /// Usually does not need to be called manually
        /// </summary>
        public static void RecalculateFrom (Node node) 
		{
			node.ClearCalculation ();
			workList = new List<Node> { node };
			StartCalculation ();
		}
		
		/// <summary>
		/// Iterates through workList and calculates everything, including children
		/// </summary>
		public static void StartCalculation () 
		{
			if (workList == null || workList.Count == 0)
				return;
		    timer = null;
			// this blocks iterates through the worklist and starts calculating
			// if a node returns false, it stops and adds the node to the worklist
			// this workList is worked on until it's empty or a limit is reached
			calculationCount = 0;
			bool limitReached = false;
			for (int roundCnt = 0; !limitReached; roundCnt++)
			{ // Runs until every node possible is calculated
				limitReached = true;
				for (int workCnt = 0; workCnt < workList.Count; workCnt++)
				{
					if (ContinueCalculation (workList[workCnt]))
						limitReached = false;
				}
				if (roundCnt > 1000)
					limitReached = true;
			}
		}

	    static private System.Diagnostics.Stopwatch timer;
		
		/// <summary>
		/// Recursive function which continues calculation on this node and all the child nodes
		/// Usually does not need to be called manually
		/// Returns success/failure of this node only
		/// </summary>
		public static bool ContinueCalculation (Node node) 
		{
			if (node.calculated)
				return false;
		    if (timer == null)
		    {
		        timer = new System.Diagnostics.Stopwatch();
		        timer.Start();
//		        Debug.Log(" start timer");
		    }
		    bool InputsCalculated = node.descendantsCalculated();
            if ((InputsCalculated || node.isInLoop()) && node.Calculate())
		    {
//                Debug.Log("Calculated "+node);
                node.PostCalculate();
		        // finished Calculating, continue with the children
		        node.calculated = true;
//                Debug.Log("calculated "+ node.name+" time "+ timer.ElapsedMilliseconds);
		        if (node.name.Contains("::"))
		            node.name = node.name.Substring(0, node.name.IndexOf("::")) + "::" + timer.ElapsedMilliseconds;
		        else
		            node.name += "::" + timer.ElapsedMilliseconds;

		        calculationCount++;
		        workList.Remove(node);
		        if (node.ContinueCalculation && calculationCount < 30000)
		        {
		            foreach (NodeOutput output in node.Outputs)
		            {
		                for (int index = output.connections.Count - 1; index >= 0; index--)
		                {
		                    NodeInput connection = output.connections[index];
                            if(connection!=null && connection.body!=null)
		                        ContinueCalculation(connection.body);
		                }
		            }
		        }
		        else if (calculationCount >= 30000)
		            Debug.LogError(
		                "Stopped calculation because of suspected Recursion. Maximum calculation iteration is currently at 1000!");
		        return true;
		    }
		    else
		    {
		        if (!InputsCalculated)
		        {
                    //add inputs 
                    foreach (NodeInput input in node.Inputs)
                    {
                        if (input!=null && input.connection != null && !input.connection.body.calculated)
                        {
                            if (!workList.Contains(input.connection.body))
                            {
                                
                                workList.Add(input.connection.body);
                            }
                        }
                    }
                }
		        if (!workList.Contains(node))
		        {
		            // failed to calculate, add it to check later
		            workList.Add(node);
		        }
		        if (node.AllowRecursion)
		            return true;
		    }
		    return false;
		}
		
		#endregion
	}
}