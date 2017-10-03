using UnityEngine;
using System.Collections.Generic;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using System;

[Node(false, "Source/Bricks")]
public class CreateOpGrid : CreateOp
{


    public const string ID = "CreateOpGrid";
    public override string GetID { get { return ID; } }

    //public Texture m_Cached;



    public override Node Create(Vector2 pos)
    {

        CreateOpGrid node = CreateInstance<CreateOpGrid>();
        
        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "CreateOpGrid";
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

        node.m_Value1 = new FloatRemap(10.0f,0,100);
        node.m_Value2 = new FloatRemap(10.0f,0,100);

        node.m_ShaderOp = ShaderOp.Grid;
        node.m_TexMode=TexMode.Greyscale;

        return node;
    }
    protected internal override void InspectorNodeGUI()
    {

    }
    public override void SetUniqueVars(Material _mat)
    {
        _mat.SetVector("_Multiply2", new Vector4(m_Value5, m_Value6, m_Value7 , m_Value8 ));
    }
    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();

        {
            float scale = 10.0f;
            m_Value1.SliderLabel(this,"Columns");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value2.SliderLabel(this,"Rows");//, 0.0f, 100.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value3.SliderLabel(this,"FillSize");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value4.SliderLabel(this,"MortarSize");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value5.SliderLabel(this,"OddRowOffset");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value6.SliderLabel(this,"YawScale");//, -scale, scale);//,new GUIContent("Red", "Float"), m_R);
            m_Value7.SliderLabel(this,"Color Variation");//, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
            m_Value8.SliderLabel(this,"Seed");//, -1.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
        }




    }

}
