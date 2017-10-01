using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

[Node (false, "OneInput/Mod")]
public class Texture1Mod : TextureMathOp
{
    public const string ID = "Texture1Mod";
    public override string GetID { get { return ID; } }

    public override Node Create (Vector2 pos) 
    {

        Texture1Mod node = CreateInstance<Texture1Mod> ();
        
        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "Mod";
        node.CreateInputOutputs();
        node.m_OpType=MathOp.Stepify;
        return node;
    }
    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();
        m_Value1.SliderLabel(this,"StepSize");//,m_Value1, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);




    }

}