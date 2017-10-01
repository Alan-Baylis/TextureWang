using NodeEditorFramework;
using NodeEditorFramework.Utilities;
using UnityEngine;

[Node (false, "TwoInput/Distort")]
public class Texture2Distort : Texture2OpBase
{
    public const string ID = "Texture2Distort";
    public override string GetID { get { return ID; } }



    public override Node Create (Vector2 pos) 
    {

        Texture2Distort node = CreateInstance<Texture2Distort> ();
        
        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "2Distort";
        node.m_Value =  new FloatRemap(10.0f);
        node.m_Value2 = new FloatRemap(1.0f);
        node.CreateInput("Src", "TextureParam", NodeSide.Left, 50);
        node.CreateInput("Distort", "TextureParam", NodeSide.Left, 70);
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);

        node.m_OpType=TexOp.Distort;
        return node;
    }

    public bool m_Uniform = true;
    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();
        //m_OpType = (TexOp)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
        //if(m_OpType == TexOP.Blend)
        m_Uniform = GUILayout.Toggle(m_Uniform, "Uniform:");
        m_Value.SliderLabel(this, m_Uniform?"Dist":"DistX");//,m_Value, 0, 1000.0f);//,new GUIContent("Red", "Float"), m_R);

        if (m_Uniform)
        {
            m_Value1 = m_Value;
        }
        else
        {
            m_Value1.SliderLabel(this, "DistY");//, m_Value1, 0, 1000.0f);
        }
//        if (m_OpType == TexOp.DirectionalWarp)
            m_Value2.SliderLabel(this,"BlurRadius");//,m_Value2, 0.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);        

        PostDrawNodePropertyEditor();
/*
        Texture tex = CreateTextureIcon(256);

        GUILayout.Label(tex);
*/

    }
}