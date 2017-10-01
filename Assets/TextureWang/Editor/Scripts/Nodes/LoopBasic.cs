using System.Runtime.Remoting.Messaging;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

[Node(false, "FlowControl/LoopBasic")]
public class LoopBasic : Node
{
    public const string ID = "LoopBasic";

    public  FloatRemap m_LoopCount;

    public override string GetID
    {
        get { return ID; }
    }

    public override Node Create(Vector2 pos)
    {
        LoopBasic node = CreateInstance<LoopBasic>();

        node.rect = new Rect(pos.x, pos.y, 150, 120);
        node.name = "Loop Basic";
        node.m_LoopCount = new FloatRemap(10,1,100);

        node.CreateInput("First Input", "TextureParam", NodeSide.Left, 50);
        node.CreateInput("Loop Input", "TextureParam", NodeSide.Left, 100);
        node.CreateOutput("Out Loop", "TextureParam", NodeSide.Right, 50);
        node.CreateOutput("Out Final", "TextureParam", NodeSide.Right, 100);
        node.CreateOutput("Percentage", "Float", NodeSide.Right, 150);
        
        return node;
    }

    protected internal override void NodeGUI()
    {
        GUILayout.Label("Loop Count: "+ (float)m_LoopCount);
        //m_LoopCount = RTEditorGUI.IntSlider(m_LoopCount, 1, 100);
        GUILayout.BeginHorizontal();
        
        GUILayout.BeginVertical();

        Inputs[0].DisplayLayout();
        Inputs[1].DisplayLayout();

        GUILayout.EndVertical();
        GUILayout.BeginVertical();

        Outputs[0].DisplayLayout();
        Outputs[1].DisplayLayout();
        Outputs[2].DisplayLayout();

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

    }

    public override void DrawNodePropertyEditor()
    {
        m_LoopCount.SliderLabelInt(this,"LoopCount");// = RTEditorGUI.IntSlider(m_LoopCount, 1, 100);

        //NodeGUI();
    }

    public override bool AllowRecursion { get { return true; } }

    private int m_Loops;
    public override void InputChanged()
    {
        //Debug.LogError(" m_LoopCount set to 0");
        //m_Loops = 0;
    }
    public override void OnClearCalculation()
    {
//        Debug.LogError(" OnClearCalculation  m_Loops set to 0");

        m_Loops = 0;
    }


    public void SetDirty(Node n)
    {
        n.m_DirtyID = Node.ms_GlobalDirtyID;
        foreach (var x in n.Outputs)
        {
            foreach (var c in x.connections)
            {
//                Debug.Log("set dirty " + c.body + " from " + n);
                if (c == null)
                    continue;
                c.body.calculated = false;
                if (c.body.m_DirtyID != Node.ms_GlobalDirtyID)
                    SetDirty(c.body);
            }
        }
    }

    private TextureParam m_Temp;
    public override bool Calculate ()
    {
        allInputsReady();
        
        if (m_Loops == 0 && (Inputs[0].connection==null || Inputs[0].connection.IsValueNull) )//!allInputsReady())
        {
//            Debug.LogError(" m_LoopCount set to 0 input 0 is null");
            m_Loops = 0;
            return false;
        }
        if (m_Loops > 0 && (Inputs[1].connection == null || Inputs[1].connection.IsValueNull))//!allInputsReady())
        {
//            Debug.LogError(" m_LoopCount set to 1 input 1 is null");
            m_Loops = 1;
            return false;
        }
        if (m_Loops == 0)
            Outputs[0].SetValue<TextureParam>(Inputs[0].GetValue<TextureParam>());
        else
        {
            //check if any of our connects to Output Looped, output straight back to us
            //if so that creates a render case where the same renderTexture is the input and the output, so make a copy 
            bool inputIsOutput = false;
            foreach (var c in Outputs[0].connections)
            {
                foreach (var o in c.body.Outputs)
                {
                    foreach (var c2 in o.connections)
                    {
                        if (c2.body == this)
                        {
//                            inputIsOutput = true;
                            Debug.LogError("found an in out is the same from "+o.body);
                        }
                    }

                }
            }
            if (inputIsOutput)
            {
                if (m_Temp == null)
                    m_Temp = new TextureParam(Inputs[1].GetValue<TextureParam>());
                Material m = TextureNode.GetMaterial("TextureOps");
                Graphics.Blit(Inputs[1].GetValue<TextureParam>().GetHWSourceTexture(), m_Temp.m_Destination, m,
                    (int) ShaderOp.CopyColorAndAlpha);

                Outputs[0].SetValue<TextureParam>(m_Temp);
            }
            else
            {
                Outputs[0].SetValue<TextureParam>(Inputs[1].GetValue<TextureParam>());
            }
        }
        Outputs[2].SetValue<float>((float)m_Loops/ (float)m_LoopCount);
        m_Loops++;
//        Debug.LogError("Loop Count Inc" + m_Loops + " / " + m_LoopCount);
        if (m_Loops >= m_LoopCount)
        {
            
            Outputs[1].SetValue<TextureParam>(Inputs[1].GetValue<TextureParam>());
        }
        else
        {
            if (Outputs[2] != null)
            {
                Node.ms_GlobalDirtyID++;
                SetDirty(this);
                calculated = true; //so the descendant can calculate that uses output 0
                foreach (var c in Outputs[2].connections)
                {
                    if (c != null)
                    {
                        
                        NodeEditor.ContinueCalculation(c.body);
                    }
                }
                calculated = false;
            }
            if (Outputs[0] != null)
            {
                Node.ms_GlobalDirtyID++;
                SetDirty(this);
                calculated = true; //so the descendant can calculate that uses output 0
                foreach (var c in Outputs[0].connections)
                {
                    if (c != null)
                    {
                        
                        NodeEditor.ContinueCalculation(c.body);
                    }
                }
                calculated = false;
            }
        }
        return  m_Loops>=m_LoopCount;
    }
}