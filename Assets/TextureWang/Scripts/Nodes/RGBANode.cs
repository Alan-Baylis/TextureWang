using UnityEngine;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[Node (false, "Standard/Example/RGBANode")]
public class RGBANode : TextureNode
{
	public const string ID = "RGBANode";
	public override string GetID { get { return ID; } }

    public float m_R;
    public float m_G;
    public float m_B;
    public float m_A;

    public TextureParam m_Param;

    public override Node Create (Vector2 pos) 
	{
		RGBANode node = CreateInstance<RGBANode> ();
		
		node.rect = new Rect (pos.x, pos.y, 150, 140);
		node.name = "RGBA constant";
		
        node.CreateOutput ("R", "Float", NodeSide.Right, 10);
        node.CreateOutput("G", "Float", NodeSide.Right, 20);
        node.CreateOutput("B", "Float", NodeSide.Right, 30);
        node.CreateOutput("A", "Float", NodeSide.Right, 40);
        node.CreateOutput("Texture", "TextureParam", NodeSide.Bottom, 50);

        return node;
	}
	
	protected internal override void InspectorNodeGUI() 
	{
        m_R = RTEditorGUI.Slider(m_R, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
        OutputKnob(0);
        m_G = RTEditorGUI.Slider(m_G, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
        OutputKnob(1);
        m_B = RTEditorGUI.Slider(m_B, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
        OutputKnob(2);
        m_A = RTEditorGUI.Slider(m_A, 0.0f, 1.0f);//,new GUIContent("Red", "Float"), m_R);
        OutputKnob(3);


		
	}
	
	public override bool Calculate () 
	{
		if (!allInputsReady ())
			return false;
		Outputs[0].SetValue<float> (m_R);
        Outputs[1].SetValue<float>(m_G);
        Outputs[2].SetValue<float>(m_B);
        Outputs[3].SetValue<float>(m_A);
        return true;
	}
}
