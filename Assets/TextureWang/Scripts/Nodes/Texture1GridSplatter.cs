using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

[Node (false, "OneInput/GridSplatter")]
public class Texture1GridSplatter : TextureMathOp
{
    public FloatRemap m_OffsetX;
    public FloatRemap m_OffsetY;
    public FloatRemap m_OffsetRow;

    public const string ID = "GridSplatter";
    public override string GetID { get { return ID; } }

    public override Node Create (Vector2 pos) 
    {

        Texture1GridSplatter node = CreateInstance<Texture1GridSplatter> ();
        
        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "GridSplatter";
        node.CreateInputOutputs();
        node.m_OpType=MathOp.SplatterGrid;
        node.m_Value1 = new FloatRemap(1.0f);
        node.m_Value2 = new FloatRemap(1.0f);
        node.m_Value3 = new FloatRemap(1.0f);
        node.m_OffsetX = new FloatRemap(0.0f, -1, 1);
        node.m_OffsetY = new FloatRemap(0.0f, -1, 1);
        node.m_OffsetRow = new FloatRemap(0.0f,-1,1);
        
        return node;
    }
    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();
        m_Value1.SliderLabel(this,"Seed");//, -10000.0f, 10000.0f) ;//,new GUIContent("Red", "Float"), m_R);
        m_Value2.SliderLabel(this,"Randomize");//, 0.001f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
        m_Value3.SliderLabelInt(this,"Repeat");//, 1, 100);//,new GUIContent("Red", "Float"), m_R);

        m_OffsetX.SliderLabel(this,"OffsetX");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
        m_OffsetY.SliderLabel(this,"OffsetY");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);

        m_OffsetRow.SliderLabel(this, "OffsetRow");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);



    }
    public override void SetUniqueVars(Material _mat)
    {
        _mat.SetVector("_Multiply2", new Vector4(m_OffsetX, m_OffsetY, m_OffsetRow, 0));
    }
}