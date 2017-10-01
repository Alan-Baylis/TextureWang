using UnityEngine;

namespace NodeEditorFramework
{
    /// <summary>
    /// NodeInput accepts one connection to a NodeOutput by default
    /// </summary>
    public class NodeButton : NodeKnob
    {
        // NodeKnob Members
        protected override NodeSide defaultSide
        {
            get { return NodeSide.Left; }
        }


        #region Contructors



        /// <summary>
        /// Creates a new NodeInput in NodeBody of specified type at the specified NodeSide and position
        /// </summary>
        public static NodeButton Create(Node nodeBody, Vector2 _offset)
        {
            NodeButton input = CreateInstance<NodeButton>();
            return input;
        }

        #endregion

        #region Additional Serialization

        protected internal override void CopyScriptableObjects(
            System.Func<ScriptableObject, ScriptableObject> replaceSerializableObject)
        {
//            connection = replaceSerializableObject.Invoke (connection) as NodeOutput;
        }

        #endregion

        #region KnobType

        protected override void ReloadTexture()
        {

            //knobTexture = typeData.InKnobTex;
        }



        #endregion



    }
}