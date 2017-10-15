using NodeEditorFramework;
using UnityEngine;

[Node(false, "Source/Ripples")]
public class CreateOpRipples : CreateOp
{


    public const string ID = "CreateOpRipples";
    public override string GetID { get { return ID; } }

    //public Texture m_Cached;



    public override Node Create(Vector2 pos)
    {

        CreateOpRipples node = CreateInstance<CreateOpRipples>();
        
        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "CreateOpRipples";
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

        node.m_Value1 = new FloatRemap(8.0f,0,20);
        node.m_Value2 = new FloatRemap(1.0f,0,20);
        node.m_Value3 = new FloatRemap(4.0f,0,20);
        node.m_Value4 = new FloatRemap(0.0f);
        node.m_Value5 = new FloatRemap(0.5f,-1,1);
        node.m_Value6 = new FloatRemap(0.5f, -1, 1);
        

        node.m_ShaderOp = ShaderOp.Ripples;
        node.m_TexMode = TexMode.Greyscale;
        return node;
    }
    protected internal override void InspectorNodeGUI()
    {

    }

    private bool m_AbsResult=false;
    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();
        {
            m_Value1.SliderLabel(this,"Width");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"Height");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabel(this, "Freq");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value4.SliderLabel(this, "Freq Offset");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value5.SliderLabel(this, "OffsetX");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value6.SliderLabel(this, "OffsetY");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            m_AbsResult = GUILayout.Toggle(m_AbsResult, "Abs Result");
            m_gain.Set( m_AbsResult ? 1.0f : 0.0f);

        }

    }



}