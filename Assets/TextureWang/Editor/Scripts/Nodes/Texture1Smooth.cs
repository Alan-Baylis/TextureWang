using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

[Node (false, "OneInput/Smooth")]
public class Texture1Smooth : TextureMathOp
{
    public const string ID = "Texture1Smooth";
    public override string GetID { get { return ID; } }

    public override Node Create (Vector2 pos) 
    {

        Texture1Smooth node = CreateInstance<Texture1Smooth> ();
        
        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "Smooth";
        node.CreateInputOutputs();
        node.m_OpType=MathOp.Smooth;
        return node;
    }
    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();
        m_Value1.SliderLabelInt(this,"Dist");//,m_Value1, 1, 50);//,new GUIContent("Red", "Float"), m_R);



    }

}