using NodeEditorFramework;
using UnityEngine;

[Node (false, "OneInput/EdgeDetectSobel")]
public class Texture1Sobel : TextureMathOp
{
    public const string ID = "Texture1Sobel";
    public override string GetID { get { return ID; } }

    private bool m_UseNearestColor=false;

    public override Node Create (Vector2 pos) 
    {

        Texture1Sobel node = CreateInstance<Texture1Sobel> ();
        
        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "EdgeDetectSobel";
        node.CreateInputOutputs();
        node.m_OpType=MathOp.Sobel;
        node.m_Value1 = new FloatRemap(10.0f);
        node.m_Value2 = new FloatRemap(0.5f);

        return node;
    }
    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();
        m_Value1.SliderLabelInt(this,"Dist");//,m_Value1, 0, 100);//,new GUIContent("Red", "Float"), m_R);
                                                          //        m_Value3 = (int)FloatRemap.SliderLabel(this,"DistY", m_Value3, 0, 100);//,new GUIContent("Red", "Float"), m_R);
        m_Value2.SliderLabel(this,"Min Threshold");//, m_Value2, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
        m_UseNearestColor = GUILayout.Toggle(m_UseNearestColor, "Use Nearest Color");


    }

    public override void SetUniqueVars(Material _mat)
    {
        _mat.SetVector("m_GeneralInts", new Vector4(m_UseNearestColor ? 1 : 0,0,0,0));
    }

}