using NodeEditorFramework;
using NodeEditorFramework.Utilities;

using UnityEngine;

[Node (false, "OneInput/Add")]
public class Texture1Add : TextureMathOp
{
    public const string ID = "Texture1Add";
    public override string GetID { get { return ID; } }

    public FloatRemap m_AddConst;

    public override Node Create (Vector2 pos) 
    {

        Texture1Add node = CreateInstance<Texture1Add> ();
        
        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "Add";
        node.CreateInputOutputs();

        node.m_OpType=MathOp.Add;
        return node;
    }
    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();

        m_Value1.SliderLabel(this,"Red");//,  -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
        if (m_TexMode == TexMode.ColorRGB)
        {
            m_Value2.SliderLabel(this,"Green");//,  -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabel(this,"Blue");//,  -1.0f, 1.0f); //,new GUIContent("Red", "Float"), m_R);
        }



    }

}