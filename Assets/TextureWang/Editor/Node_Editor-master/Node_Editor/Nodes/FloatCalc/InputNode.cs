using UnityEngine;
using System.Collections;
using NodeEditorFramework;
using NodeEditorFramework.Utilities;

[System.Serializable]
[Node (false, "Float/Input")]
public class InputNode : Node 
{
	public const string ID = "inputNode";
	public override string GetID { get { return ID; } }

	
    public FloatRemap m_Value;

	public override Node Create (Vector2 pos) 
	{ // This function has to be registered in Node_Editor.ContextCallback
		InputNode node = CreateInstance <InputNode> ();
		
		node.name = "Input Node";
		node.rect = new Rect (pos.x, pos.y, 200, 100);;
		node.m_Value=new FloatRemap(1,-1,1);
		NodeOutput.Create (node, "Value", "Float");

		return node;
	}


    protected internal override void NodeGUI () 
	{
        //value = RTEditorGUI.FloatField (new GUIContent ("Value", "The input value of type float"), value);
        GUILayout.Label("Value:" + (float)m_Value);
        OutputKnob (0);

	}

    public override void DrawNodePropertyEditor()
    {
        m_Value.SliderLabel(this, "Value");

    }
    public override bool Calculate () 
	{
		Outputs[0].SetValue<float> (m_Value);
		return true;
	}
}