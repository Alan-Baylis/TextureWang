using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node (false, "TwoInput/Gradient")]
public class Texture2Gradient : Texture2OpBase
{
    public const string ID = "Texture2Gradient";
	public override string GetID { get { return ID; } }



    public override Node Create (Vector2 pos) 
	{

        Texture2Gradient node = CreateInstance<Texture2Gradient> ();
        
        node.rect = new Rect(pos.x, pos.y, m_NodeWidth, m_NodeHeight);
        node.name = "2Gradient";
        node.CreateInput("TextureIndex", "TextureParam", NodeSide.Left, 50);
        node.CreateInput("1DGradient", "TextureParam", NodeSide.Left, 70);
        node.CreateOutput("Texture", "TextureParam", NodeSide.Right, 50);
        node.m_OpType=TexOp.Gradient;
        return node;
	}

    public override void DrawNodePropertyEditor()
    {
        base.DrawNodePropertyEditor();
        //m_OpType = (TexOp)UnityEditor.EditorGUILayout.EnumPopup(new GUIContent("Type", "The type of calculation performed on Input 1"), m_OpType, GUILayout.MaxWidth(200));
        //if(m_OpType == TexOP.Blend)
/*
        m_Value.SliderLabel(this,"Dist");//, -50.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);
        if (m_OpType == TexOp.DirectionalWarp)
            m_Value2.SliderLabel(this,"Angle");//, -50.0f, 50.0f);//,new GUIContent("Red", "Float"), m_R);        
*/
        PostDrawNodePropertyEditor();
/*
        Texture tex = CreateTextureIcon(256);

        GUILayout.Label(tex);
*/

    }
}
