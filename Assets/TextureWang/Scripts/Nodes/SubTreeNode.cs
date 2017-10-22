using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[Node (false, "SubTreeNode")]
public class SubTreeNode : TextureNode
{
    
    public const string ID = "SubTreeNode";
    public override string GetID { get { return ID; } }

    public NodeCanvas SubCanvas
    {
        get { return m_SubCanvas; }
        set { m_SubCanvas = value; }
    }

//    [SerializeField]
    private NodeCanvas m_SubCanvas;
    public string m_CanvasGuid;
    private bool m_WasCloned;

    protected internal override void InspectorNodeGUI()
    {
        
    }

    public override Node Create (Vector2 _pos) 
    {

        SubTreeNode node = CreateInstance<SubTreeNode> ();
        
        node.rect = new Rect(_pos.x, _pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "SubTreeNode";
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

        return node;
    }

    private void OnGUI()
    {
        NodeGUI();
    }

    public override void DrawNodePropertyEditor() 
    {
        // m_OpType = (TexOP)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
#if UNITY_EDITOR
/*
        if (m_SubCanvas != null)
        {

            string assetPath = AssetDatabase.GetAssetPath(m_SubCanvas);
            Debug.Log(" canvas path >"+assetPath+"<");
            if (assetPath.Length > 0)
            {
                m_CanvasGuid = AssetDatabase.AssetPathToGUID(assetPath);
                Debug.LogError(" set canvasGuid from asset >" + m_CanvasGuid+"<");
            }


        }
*/
        if (m_CanvasGuid != null)
        {
            m_CanvasGuid = GUILayout.TextField(m_CanvasGuid);
        }
        else
        {
            m_CanvasGuid = GUILayout.TextField("");
            m_CanvasGuid = "";

        }
        //Debug.LogError(" set canvasGuid textfiled " + m_CanvasGuid);
#endif
//        m_SubCanvas = (NodeCanvas)EditorGUI.ObjectField(new Rect(0, 250, 250, 50), m_SubCanvas, typeof(NodeCanvas), false);


    }

    bool FixupForSubCanvas()
    {
        if (!string.IsNullOrEmpty(m_CanvasGuid) && m_SubCanvas == null)
        {

            string NodeCanvasPath = AssetDatabase.GUIDToAssetPath(m_CanvasGuid);

            m_SubCanvas = NodeEditorSaveManager.LoadNodeCanvas(NodeCanvasPath,false); 
            m_WasCloned = true;

        }

        if (m_SubCanvas != null)
        {
            if (!m_WasCloned)
            {
                NodeEditorSaveManager.CreateWorkingCopy(m_SubCanvas, false);//miked remove ref
                m_WasCloned = true;
            }
            
            List<NodeInput> needsInput = new List<NodeInput>();
            List<UnityTextureOutput> needsOutput = new List<UnityTextureOutput>();
            foreach (Node n in m_SubCanvas.nodes)
            {

                if (n.Inputs.Count > 0)
                {
                    if (n is UnityTextureOutput && n.Inputs[0].connection != null)
                    {
                        needsOutput.Add(n as UnityTextureOutput);

                    }
                    for (int i = 0; i < n.Inputs.Count; i++)
                    {
                        if (n.Inputs[i].connection == null)
                        {
                            //this node has no input so we will wire it up to ours
                            needsInput.Add(n.Inputs[i]);
                            //                            Debug.Log(" missing input for node "+n+" name "+n.name);
                        }
                    }
                }
            }
            if (needsOutput.Count > Outputs.Count)
            {
                
                while (needsOutput.Count > Outputs.Count)
                {
                    //                    Debug.Log(" create input "+Inputs.Count);
                    CreateOutput("Texture" + Outputs.Count+" "+ needsOutput[needsOutput.Count - 1].m_TexName, needsOutput[needsOutput.Count - 1].Inputs[0].connection.typeID, NodeSide.Right, 50 + Outputs.Count * 20);
                }
            }
            if(needsOutput.Count>0)
                Outputs[0].name = "Texture0" + " " + needsOutput[0].m_TexName;

            if (needsInput.Count > Inputs.Count)
            {
              //  while (needsInput.Count > Inputs.Count)
                int startInputCount = Inputs.Count;
//                for(int index= needsInput.Count-1;index>= startInputCount; index--)
                for (int index = Inputs.Count ; index < needsInput.Count; index++)
                {
                    string name = needsInput[index].name;
                    //                    Debug.Log(" create input "+Inputs.Count);
                    CreateInput(name, needsInput[index].typeID, NodeSide.Left, 30 + Inputs.Count*20);
                }

                return false;
            }

        }
        return true;
    }

    public override void OnLoadCanvas()
    {
        base.OnLoadCanvas();
        FixupForSubCanvas();
    }

    protected internal override void DrawConnections()
    {
        base.DrawConnections();

        if (Event.current.type != EventType.Repaint)
            return;
        foreach (NodeOutput output in Outputs)
        {
            if (output == null)
                continue;
            Vector2 startPos = output.GetGUIKnob().center;
            Vector2 startDir = output.GetDirection();

//            foreach (NodeInput input in output.connections)
            {
//                if (input != null)
                {


                    EditorGUI.LabelField(new Rect(startPos - new Vector2(0, 20), new Vector2(200, 50)), output.name);

                }
            }
        }
    }

    public override bool Calculate()
    {
        if (!allInputsReady())
        {
            //Debug.LogError(" input no ready");
            return false;
        }
        if (!FixupForSubCanvas())
            return false;
        if (m_SubCanvas != null)
        {
            List<Node> workList = new List<Node>();

            List<NodeInput> needsRemoval = new List<NodeInput>();
            //connect each of our inputs to the internal inputs of the sub canvas
            int count = 0;
            foreach (Node n in m_SubCanvas.nodes)
            {
                if (n.Inputs.Count > 0)
                {
                    for (int i = 0; i < n.Inputs.Count; i++)
                    {
                        if (n.Inputs[i].connection == null)
                        {
                            //this node has no input so we will wire it up to ours
//                            Debug.Log(" connect input " + Inputs.Count+" count "+count);
                            if (Inputs.Count > count)
                            {
                                workList.Add(n);
                                n.calculated = false;
                                n.Inputs[i].ApplyConnection(Inputs[count].connection,false);
                                needsRemoval.Add(n.Inputs[i]);
                            }
                            count++;
                        }
                    }
                }
            }


            NodeEditor.RecalculateAllAndWorkList(m_SubCanvas,workList);
            int countOut = 0;
            foreach (Node n in m_SubCanvas.nodes)
            {
                if (n is UnityTextureOutput)
                {
                    m_Param = n.Inputs[0].GetValue<TextureParam>();
                    Outputs[countOut++].SetValue<TextureParam>(m_Param);
                }
                else
                if (n.Outputs.Count>0 && n.Outputs[0].connections.Count == 0)
                {
                    //this node has no output so it must be the final destination
                    m_Param = n.Outputs[0].GetValue<TextureParam>();
                    Outputs[countOut++].SetValue<TextureParam>(m_Param);
                }
                if (countOut >= Outputs.Count)
                    break;

            }

            foreach (var x in needsRemoval)
            {
                x.RemoveConnection(false);
            }

            CreateCachedTextureIcon();
            //m_Cached = m_Param.GetHWSourceTexture();
            
        }

        return true;
    }
}