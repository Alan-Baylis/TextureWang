using UnityEngine;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using NodeEditorFramework;

namespace NodeEditorFramework 
{
	/// <summary>
	/// Handles fetching and storing of all NodeDeclarations
	/// </summary>
	public static class NodeTypes
	{
		public static Dictionary<Node, NodeData> nodes;

		/// <summary>
		/// Fetches every Node Declaration in the assembly and stores them in the nodes List.
		/// nodes List contains a default instance of each node type in the key and editor specific NodeData in the value
		/// </summary>
		public static void FetchNodes() 
		{
			nodes = new Dictionary<Node, NodeData> ();

			List<Assembly> scriptAssemblies = AppDomain.CurrentDomain.GetAssemblies ().Where ((Assembly assembly) => assembly.FullName.Contains ("Assembly")).ToList ();
			if (!scriptAssemblies.Contains (Assembly.GetExecutingAssembly ()))
				scriptAssemblies.Add (Assembly.GetExecutingAssembly ());
			foreach (Assembly assembly in scriptAssemblies) 
			{
				foreach (Type type in assembly.GetTypes ().Where (T => T.IsClass && !T.IsAbstract && T.IsSubclassOf (typeof (Node)))) 
				{
					object[] nodeAttributes = type.GetCustomAttributes (typeof (NodeAttribute), false);
					NodeAttribute attr = nodeAttributes [0] as NodeAttribute;
					if (attr == null || !attr.hide)
					{
						Node node = ScriptableObject.CreateInstance (type.Name) as Node; // Create a 'raw' instance (not setup using the appropriate Create function)
                        
                        node = node.Create (Vector2.zero); // From that, call the appropriate Create Method to init the previously 'raw' instance
						nodes.Add (node, new NodeData (attr == null? node.name : attr.contextText));
                        node.hideFlags = HideFlags.HideAndDontSave;
//                        Debug.LogError("add node "+node);
					}
				}
			}
		}

		/// <summary>
		/// Returns the NodeData for the given Node
		/// </summary>
		public static NodeData getNodeData (Node node)
		{
			return nodes [getDefaultNode (node.GetID)];
		}

		/// <summary>
		/// Returns the default node from the given nodeID. 
		/// The default node is a dummy used to create other nodes (Due to various limitations creation has to be performed on Node instances)
		/// </summary>
		public static Node getDefaultNode (string nodeID)
		{
//            Debug.Log(" getDefaultNode "+nodeID);
			var ret= nodes.Keys.Single<Node> ((Node node) => node.GetID == nodeID);
		    foreach (var x in nodes.Keys)
		    {
		        if (x == null)
		        {
		            Debug.LogError(" x is null "+x+" but has id "+x.GetID);
		        }
		        else
		        {
//                    Debug.LogError(" x is not null " + x + " has id " + x.GetID);
                }
		    }

		    if (ret == null)
		    {
		        foreach (var x in nodes.Keys)
		        {
		            Debug.Log("Cant find ID "+nodeID+" but found "+x.GetID);
		            if (x.GetID == nodeID)
		            {
                        Debug.LogError("Cant find ID BUT FOUND IT ANYWAY USING FOREACH!!!" + nodeID + " but found " + x.GetID+" x "+x);
                        return x;
		            }
		        }
		    }
		    return ret;
		}

		/// <summary>
		/// Returns the default node from the node type. 
		/// The default node is a dummy used to create other nodes (Due to various limitations creation has to be performed on Node instances)
		/// </summary>
		public static T getDefaultNode<T> () where T : Node
		{
			return nodes.Keys.Single<Node> ((Node node) => node.GetType () == typeof (T)) as T;
		}
	}

	/// <summary>
	/// The NodeData contains the additional, editor specific data of a node type
	/// </summary>
	public struct NodeData 
	{
		public string adress;

		public NodeData (string name) 
		{
			adress = name;
		}
	}

	/// <summary>
	/// The NodeAttribute is used to specify editor specific data for a node type, later stored using a NodeData
	/// </summary>
	public class NodeAttribute : Attribute 
	{
		public bool hide { get; private set; }
		public string contextText { get; private set; }

		public NodeAttribute (bool HideNode, string ReplacedContextText) 
		{
			hide = HideNode;
			contextText = ReplacedContextText;
		}
	}
}