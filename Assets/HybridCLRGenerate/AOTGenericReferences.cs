using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"GameFramework.dll",
		"System.dll",
		"UnityGameFramework.Runtime.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// GameFramework.GameFrameworkLinkedList.Enumerator<object>
	// GameFramework.GameFrameworkLinkedList<object>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.LinkedList.Enumerator<object>
	// System.Collections.Generic.LinkedList<object>
	// System.Collections.Generic.LinkedListNode<object>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.Queue.Enumerator<object>
	// System.Collections.Generic.Queue<object>
	// }}

	public void RefMethods()
	{
		// object[] System.Array.Empty<object>()
		// object UnityGameFramework.Runtime.GameEntry.GetComponent<object>()
	}
}