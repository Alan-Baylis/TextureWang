using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEditor;

namespace NodeEditorFramework
{
	public abstract class Node : ScriptableObject //MonoBehaviour
    {
        
        public Rect rect = new Rect ();
		internal Vector2 contentOffset = Vector2.zero;
		[SerializeField]
		public List<NodeKnob> nodeKnobs = new List<NodeKnob> ();

		// Calculation graph
//		[NonSerialized]
		[SerializeField, HideInInspector]
		public List<NodeInput> Inputs = new List<NodeInput>();
//		[NonSerialized]
		[SerializeField, HideInInspector]
		public List<NodeOutput> Outputs = new List<NodeOutput>();
		[HideInInspector]
		[NonSerialized]
		public bool calculated = true;

	    public string m_Doc="";

	    public Node m_Parent;

	    public Color m_TitleBoxColor=Color.red;
        public Color m_MainBoxColor=Color.white ;

	    public bool m_CanDragSize=false ;
	    private Texture2D m_CloseTex;
        private Texture2D m_DragTex;

        public int m_DirtyID;
        public static int ms_GlobalDirtyID;

        #region General

        public virtual void OpenPreview()
	    {
	        
	    }
        public Rect GetDragRect()
	    {
	        Rect ret=new Rect(rect.position + rect.size - new Vector2(10, 10), new Vector2(10, 10));
	        return ret;
	    }
        public Rect GetCloseRect()
        {
            Rect ret = new Rect(rect.position + new Vector2(4, 4), new Vector2(12, 12));
            return ret;
        }

        /// <summary>
        /// Init the Node Base after the Node has been created. This includes adding to canvas, and to calculate for the first time
        /// </summary>
        protected virtual void InitBase () 
		{
			Calculate ();
            PostCalculate();
            m_CloseTex = ResourceManager.GetTintedTexture("Textures/close.png", Color.white);
            m_DragTex = ResourceManager.GetTintedTexture("Textures/drag.png", Color.white);
            if (!NodeEditor.curNodeCanvas.nodes.Contains (this))
				NodeEditor.curNodeCanvas.nodes.Add (this);
			#if UNITY_EDITOR
			if (name == "")
				name = UnityEditor.ObjectNames.NicifyVariableName (GetID);
			#endif
		}

		/// <summary>
		/// Deletes this Node from curNodeCanvas and the save file
		/// </summary>
		public void Delete () 
		{
			if (!NodeEditor.curNodeCanvas.nodes.Contains (this))
				throw new UnityException ("The Node " + name + " does not exist on the Canvas " + NodeEditor.curNodeCanvas.name + "!");
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("Delete Node");
            Undo.RecordObject(NodeEditor.curNodeCanvas, "Delete Object");
		    Undo.RecordObject(this, "node data");

            NodeEditorCallbacks.IssueOnDeleteNode (this);
			NodeEditor.curNodeCanvas.nodes.Remove (this);
			for (int outCnt = 0; outCnt < Outputs.Count; outCnt++) 
			{
				NodeOutput output = Outputs [outCnt];
				while (output.connections.Count != 0)
					output.connections[0].RemoveConnection ();
				Undo.DestroyObjectImmediate(output);//, true);
			}
			for (int inCnt = 0; inCnt < Inputs.Count; inCnt++) 
			{
				NodeInput input = Inputs [inCnt];
				if (input.connection != null)
					input.connection.connections.Remove (input);
                Undo.DestroyObjectImmediate(input);//, true);
            }
			for (int knobCnt = 0; knobCnt < nodeKnobs.Count; knobCnt++) 
			{ // Inputs/Outputs need specific treatment, unfortunately
				if (nodeKnobs[knobCnt] != null)
                    Undo.DestroyObjectImmediate(nodeKnobs[knobCnt]);//, true);
            }
			Undo.DestroyObjectImmediate (this);
		}

		public static Node Create (string nodeID, Vector2 position) 
		{
			Node node = NodeTypes.getDefaultNode (nodeID);
			if (node == null)
				throw new UnityException ("Cannot create Node with id " + nodeID + " as no such Node type is registered!");

			node = node.Create (position);
			node.InitBase ();

			NodeEditorCallbacks.IssueOnAddNode (node);
			return node;
		}

		/// <summary>
		/// Makes sure this Node has migrated from the previous save version of NodeKnobs to the current mixed and generic one
		/// </summary>
		internal void CheckNodeKnobMigration () 
		{ // TODO: Migration from previous NodeKnob system; Remove later on
			if (nodeKnobs.Count == 0 && (Inputs.Count != 0 || Outputs.Count != 0)) 
			{
				nodeKnobs.AddRange (Inputs.Cast<NodeKnob> ());
				nodeKnobs.AddRange (Outputs.Cast<NodeKnob> ());
			}
		}

		#endregion

		#region Node Type methods (abstract)

		/// <summary>
		/// Get the ID of the Node
		/// </summary>
		public abstract string GetID { get; }

		/// <summary>
		/// Create an instance of this Node at the given position
		/// </summary>
		public abstract Node Create (Vector2 pos);
		
		/// <summary>
		/// Draw the Node immediately
		/// </summary>
		protected internal abstract void NodeGUI ();

        /// <summary>
        /// Used to display a custom node property editor in the side window of the NodeEditorWindow
        /// Optionally override this to implement
        /// </summary>
        public virtual void DrawNodePropertyEditor() { }

        public virtual void OnLoadCanvas() { }

        /// <summary>
        /// Calculate the outputs of this Node
        /// Return Success/Fail
        /// Might be dependant on previous nodes
        /// </summary>
        public abstract bool Calculate ();

	    public virtual void InputChanged()
	    {
	        
	    }

        public virtual void PostCalculate()
	    {
	        foreach (var o in Outputs)
	        {
	            foreach (var c in o.connections)
	            {
                    if(c!=null && c.connection!=null && c.connection.body!=null)
	                    c.connection.body.InputChanged();
	            }
	        }
	    }

        #endregion

        #region Node Type Properties

        /// <summary>
        /// Does this node allow recursion? Recursion is allowed if atleast a single Node in the loop allows for recursion
        /// </summary>
        public virtual bool AllowRecursion { get { return false; } }

		/// <summary>
		/// Should the following Nodes be calculated after finishing the Calculation function of this node?
		/// </summary>
		public virtual bool ContinueCalculation { get { return true; } }

		/// <summary>
		/// Does this Node accepts Transitions?
		/// </summary>
		public virtual bool AcceptsTranstitions { get { return false; } }

        #endregion

		#region Protected Callbacks

		/// <summary>
		/// Callback when the node is deleted
		/// </summary>
		protected internal virtual void OnDelete () {}

		/// <summary>
		/// Callback when the NodeInput was assigned a new connection
		/// </summary>
		protected internal virtual void OnAddInputConnection (NodeInput input) {}

		/// <summary>
		/// Callback when the NodeOutput was assigned a new connection (the last in the list)
		/// </summary>
		protected internal virtual void OnAddOutputConnection (NodeOutput output) {}

		#endregion

		#region Additional Serialization

		/// <summary>
		/// Returns all additional ScriptableObjects this Node holds. 
		/// That means only the actual SOURCES, simple REFERENCES will not be returned
		/// This means all SciptableObjects returned here do not have it's source elsewhere
		/// </summary>
		protected internal virtual ScriptableObject[] GetScriptableObjects () { return new ScriptableObject[0]; }

		/// <summary>
		/// Replaces all REFERENCES aswell as SOURCES of any ScriptableObjects this Node holds with the cloned versions in the serialization process.
		/// </summary>
		protected internal virtual void CopyScriptableObjects (System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject) {}

        #endregion

	    protected internal virtual void FixInternalKnobs(Dictionary<string, NodeKnob> _knobMap)
	    {
	    }

	    #region Node and Knob Drawing

        private static Texture2D _staticRectTexture;
        private static GUIStyle _staticRectStyle;
        private static Color _staticRectColor;

        // Note that this function is only meant to be called from OnGUI() functions.
        public static void GUIDrawRect(Rect position, Color color)
        {
            if (_staticRectTexture == null)
            {
                _staticRectTexture = new Texture2D(1, 1);
            }

            if (_staticRectStyle == null)
            {
                _staticRectStyle = new GUIStyle();
            }
            if (_staticRectColor != color)
            {
                _staticRectTexture.SetPixel(0, 0, color);
                _staticRectTexture.Apply();

                _staticRectColor = color;
            }

            _staticRectStyle.normal.background = _staticRectTexture;

            GUI.Box(position, GUIContent.none, _staticRectStyle);


        }

        /// <summary>
        /// Draws the node frame and calls NodeGUI. Can be overridden to customize drawing.
        /// </summary>
        protected internal virtual void DrawNode (bool _fromParent)
        {
            if(m_CloseTex==null)
                m_CloseTex = ResourceManager.GetTintedTexture("Textures/close.png", Color.white);
            if (m_DragTex == null)
                m_DragTex = ResourceManager.GetTintedTexture("Textures/drag.png", Color.white);

            //            if (m_Parent != null && !_fromParent)
            //                return;
            // TODO: Node Editor Feature: Custom Windowing System
            // Create a rect that is adjusted to the editor zoom
            Rect nodeRect = rect;
			nodeRect.position += NodeEditor.curEditorState.zoomPanAdjust;
			contentOffset = new Vector2 (0, 20);

			// Create a headerRect out of the previous rect and draw it, marking the selected node as such by making the header bold
			Rect headerRectBig = new Rect (nodeRect.x, nodeRect.y, nodeRect.width, contentOffset.y);
            Rect headerRectSmall = new Rect(nodeRect.x+18, nodeRect.y, nodeRect.width-18, contentOffset.y);
            GUI.color = m_TitleBoxColor;
            //GUIDrawRect(nodeRect,Color.red);
            GUI.Label (headerRectBig, "", NodeEditor.curEditorState.selectedNode == this? NodeEditorGUI.nodeBoxBold : NodeEditorGUI.nodeBox);
            GUI.Label(headerRectSmall, name, NodeEditor.curEditorState.selectedNode == this ? NodeEditorGUI.nodeBoxBold : NodeEditorGUI.nodeBox);
            if (this is SubTreeNode)
            {
                Rect boxRect = new Rect(nodeRect.x+2, nodeRect.y+2, 8, contentOffset.y-2);
                
                GUIDrawRect(boxRect, Color.red);
                
            }
            GUI.DrawTexture(new Rect(nodeRect.x+2 , nodeRect.y+2 , 16, 16), m_CloseTex);

            // Begin the body frame around the NodeGUI
            Rect bodyRect = new Rect (nodeRect.x, nodeRect.y + contentOffset.y, nodeRect.width, nodeRect.height - contentOffset.y);

            GUI.color = m_MainBoxColor;
            GUI.BeginGroup (bodyRect, GUI.skin.box);
			bodyRect.position = Vector2.zero;
            GUILayout.BeginArea(bodyRect, GUI.skin.box);
            // Call NodeGUI
//            GUI.changed = false;
            NodeGUI();

            

            // End NodeGUI frame
            GUILayout.EndArea();
            if (m_CanDragSize)
            {
                //Rect boxRect = new Rect(nodeRect.width-10, nodeRect.height-10, 10,10);

                //GUIDrawRect(boxRect, Color.black);
                GUI.DrawTexture(new Rect(bodyRect.width - 16, bodyRect.height - 16, 16, 16), m_DragTex);
            }
            GUI.EndGroup ();
            GUI.color = Color.white;
        }

		/// <summary>
		/// Draws the nodeKnobs
		/// </summary>
		protected internal virtual void DrawKnobs () 
		{
			CheckNodeKnobMigration ();
			foreach (NodeKnob knob in nodeKnobs)
                if(knob!=null)
				    knob.DrawKnob ();
		}

		/// <summary>
		/// Draws the node curves
		/// </summary>
		protected internal virtual void DrawConnections () 
		{
			CheckNodeKnobMigration ();
			if (Event.current.type != EventType.Repaint)
				return;
			foreach (NodeOutput output in Outputs)
			{
				Vector2 startPos = output.GetGUIKnob ().center;
				Vector2 startDir = output.GetDirection ();

				foreach (NodeInput input in output.connections) 
				{
				    if (input != null)
				    {
				        NodeEditorGUI.DrawConnection(startPos,
				            startDir,
				            input.GetGUIKnob().center,
				            input.GetDirection(),
				            ConnectionTypes.GetTypeData(output.type, true).Color);

                        //EditorGUI.LabelField(new Rect(input.GetGUIKnob().center-new Vector2(50,20), new Vector2(200, 50)), input.name);

                    }
                }
			}
            foreach (var input in Inputs)
		    {
                if(input!=null)
                    EditorGUI.LabelField(new Rect(input.GetGUIKnob().center - new Vector2(50, 20), new Vector2(200, 50)), input.name);
            }


		}

        #endregion

            #region Node Calculation Utility

            /// <summary>
            /// Checks if there are no unassigned and no null-value inputs.
            /// </summary>
        protected internal bool allInputsReady ()
		{
			foreach (NodeInput input in Inputs)
			{
				if (input.Optional==false && (input.connection == null || input.connection.IsValueNull))
					return false;
			}
			return true;
		}
		/// <summary>
		/// Checks if there are any unassigned inputs.
		/// </summary>
		protected internal bool hasUnassignedInputs () 
		{
			foreach (NodeInput input in Inputs)
			{
				if (input.connection == null)
					return true;
			}
			return false;
		}
		
		/// <summary>
		/// Returns whether every direct dexcendant has been calculated
		/// </summary>
		protected internal bool descendantsCalculated () 
		{
			foreach (NodeInput input in Inputs)
			{
				if (input!=null && input.connection != null && !input.connection.body.calculated)
					return false;
			}
			return true;
		}

		/// <summary>
		/// Returns whether the node acts as an input (no inputs or no inputs assigned)
		/// </summary>
		protected internal bool isInput () 
		{
			foreach (NodeInput input in Inputs)
			{
                if(input==null)
                    Debug.LogError("null input in "+this);
                else
				if (input.connection != null)
					return false;
			}
			return true;
		}

		#endregion

		#region Node Knob Utility

		// -- OUTPUTS --

		/// <summary>
		/// Creates and output on your Node of the given type.
		/// </summary>
		public void CreateOutput (string outputName, string outputType)
		{
			NodeOutput.Create (this, outputName, outputType);
		}
		/// <summary>
		/// Creates and output on this Node of the given type at the specified NodeSide.
		/// </summary>
		public void CreateOutput (string outputName, string outputType, NodeSide nodeSide)
		{
			NodeOutput.Create (this, outputName, outputType, nodeSide);
		}
		/// <summary>
		/// Creates and output on this Node of the given type at the specified NodeSide and position.
		/// </summary>
		public void CreateOutput (string outputName, string outputType, NodeSide nodeSide, float sidePosition)
		{
			NodeOutput.Create (this, outputName, outputType, nodeSide, sidePosition);
		}

		/// <summary>
		/// Aligns the OutputKnob on it's NodeSide with the last GUILayout control drawn.
		/// </summary>
		/// <param name="outputIdx">The index of the output in the Node's Outputs list</param>
		protected void OutputKnob (int outputIdx)
		{
			if (Event.current.type == EventType.Repaint)
				Outputs[outputIdx].SetPosition ();
		}

		/// <summary>
		/// Returns the output knob that is at the position on this node or null
		/// </summary>
		public NodeOutput GetOutputAtPos (Vector2 pos) 
		{
			foreach (NodeOutput output in Outputs)
			{ // Search for an output at the position
				if (output.GetScreenKnob ().Contains (new Vector3 (pos.x, pos.y)))
					return output;
			}
			return null;
		}


		// -- INPUTS --

		/// <summary>
		/// Creates and input on your Node of the given type.
		/// </summary>
		public NodeInput CreateInput (string inputName, string inputType)
		{
		    foreach (var x in Inputs)
		    {
                if(x==null)
                    Debug.LogError("found null input in "+this+" inputName "+inputName+" "+inputType);
                else
		        if (x.name == inputName)
		        {
//                    Debug.Log("create input that exists use existing "+inputName+" type "+inputType);
		            return x;
		        }
		    }
		    var ret = NodeInput.Create (this, inputName, inputType);

		    float step = (rect.height-30)/Inputs.Count;
		    float pos = 30;
		    foreach (var x in Inputs)
		    {
		        if (x == null)
		            continue;
		        x.sidePosition = pos;
		        pos += step;
		    }
		    return ret;
		}

	    public void RemoveInput(NodeInput _in)
	    {
            if (_in.connection != null)
                _in.connection.connections.Remove(_in);

            Inputs.Remove(_in);
	        nodeKnobs.Remove(_in);
            DestroyImmediate(_in);
	    }
		/// <summary>
		/// Creates and input on this Node of the given type at the specified NodeSide.
		/// </summary>
		public NodeInput CreateInput (string inputName, string inputType, NodeSide nodeSide)
		{
            return NodeInput.Create (this, inputName, inputType, nodeSide);
		}
		/// <summary>
		/// Creates and input on this Node of the given type at the specified NodeSide and position.
		/// </summary>
		public NodeInput CreateInput (string inputName, string inputType, NodeSide nodeSide, float sidePosition)
		{
            return NodeInput.Create (this, inputName, inputType, nodeSide, sidePosition);
		}

		/// <summary>
		/// Aligns the InputKnob on it's NodeSide with the last GUILayout control drawn.
		/// </summary>
		/// <param name="inputIdx">The index of the input in the Node's Inputs list</param>
		protected void InputKnob (int inputIdx)
		{
			if (Event.current.type == EventType.Repaint)
				Inputs[inputIdx].SetPosition ();
		}

		/// <summary>
		/// Returns the input knob that is at the position on this node or null
		/// </summary>
		public NodeInput GetInputAtPos (Vector2 pos) 
		{
			foreach (NodeInput input in Inputs)
			{ // Search for an input at the position
				if (input!=null && input.GetScreenKnob ().Contains (new Vector3 (pos.x, pos.y)))
					return input;
			}
			return null;
		}

		#endregion

		#region Recursive Search Utility

		/// <summary>
		/// Recursively checks whether this node is a child of the other node
		/// </summary>
		public bool isChildOf (Node otherNode)
		{
			if (otherNode == null || otherNode == this)
				return false;
			if (BeginRecursiveSearchLoop ()) return false;
			foreach (NodeInput input in Inputs)
			{
				NodeOutput connection = input.connection;
				if (connection != null && connection.body != startRecursiveSearchNode)
				{
					if (connection.body == otherNode || connection.body.isChildOf (otherNode))
					{
						StopRecursiveSearchLoop ();
						return true;
					}
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// Recursively checks whether this node is in a loop
		/// </summary>
		internal bool isInLoop ()
		{
			if (BeginRecursiveSearchLoop ()) return this == startRecursiveSearchNode;
			foreach (NodeInput input in Inputs)
			{
			    if (input == null)
			        continue;
				NodeOutput connection = input.connection;
				if (connection != null && connection.body.isInLoop ())
				{
					StopRecursiveSearchLoop ();
					return true;
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// Recursively checks whether any node in the loop to be made allows recursion.
		/// Other node is the node this node needs connect to in order to fill the loop (other node being the node coming AFTER this node).
		/// That means isChildOf has to be confirmed before calling this!
		/// </summary>
		internal bool allowsLoopRecursion (Node otherNode)
		{
			if (AllowRecursion)
				return true;
			if (otherNode == null)
				return false;
			if (BeginRecursiveSearchLoop ()) return false;
			foreach (NodeInput input in Inputs)
			{
				NodeOutput connection = input.connection;
				if (connection != null && connection.body != startRecursiveSearchNode)
				{
					if (connection.body.allowsLoopRecursion (otherNode))
					{
						StopRecursiveSearchLoop ();
						return true;
					}
				}
			}
			EndRecursiveSearchLoop ();
			return false;
		}

		/// <summary>
		/// A recursive function to clear all calculations depending on this node.
		/// Usually does not need to be called manually
		/// </summary>
		public void ClearCalculation () 
		{
			if (BeginRecursiveSearchLoop ()) return;
			calculated = false;
		    OnClearCalculation();
            foreach (NodeOutput output in Outputs)
			{
			    if (output != null)
			    {
			        foreach (NodeInput input in output.connections)
			        {
                        if(input && input.body)
			                input.body.ClearCalculation();
			        }
			    }
			}
			EndRecursiveSearchLoop ();
		}

	    public virtual void OnClearCalculation()
	    {

	    }

	    #region Recursive Search Helpers

		private List<Node> recursiveSearchSurpassed;
		private Node startRecursiveSearchNode; // Temporary start node for recursive searches

		/// <summary>
		/// Begins the recursive search loop and returns whether this node has already been searched
		/// </summary>
		internal bool BeginRecursiveSearchLoop ()
		{
			if (startRecursiveSearchNode == null || recursiveSearchSurpassed == null) 
			{ // Start search
				recursiveSearchSurpassed = new List<Node> ();
				startRecursiveSearchNode = this;
			}

			if (recursiveSearchSurpassed.Contains (this))
				return true;
			recursiveSearchSurpassed.Add (this);
			return false;
		}

		/// <summary>
		/// Ends the recursive search loop if this was the start node
		/// </summary>
		internal void EndRecursiveSearchLoop () 
		{
			if (startRecursiveSearchNode == this) 
			{ // End search
				recursiveSearchSurpassed = null;
				startRecursiveSearchNode = null;
			}
		}

		/// <summary>
		/// Stops the recursive search loop immediately. Call when you found what you needed.
		/// </summary>
		internal void StopRecursiveSearchLoop () 
		{
			recursiveSearchSurpassed = null;
			startRecursiveSearchNode = null;
		}

		#endregion

		#endregion
	}
}
