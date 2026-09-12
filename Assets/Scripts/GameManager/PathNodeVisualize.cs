using UnityEngine;

public class PathNodeVisualize : MonoBehaviour {
    [SerializeField] private TextMesh gCostTextMesh;
    [SerializeField] private TextMesh hCostTextMesh;
    [SerializeField] private TextMesh fCostTextMesh;

    public PathNode PathNode { get; set; }

    private void Update()
    {
        if (PathNode == null) return;

        gCostTextMesh.text = PathNode.GCost <= 0 ? "" : PathNode.GCost.ToString();
        hCostTextMesh.text = PathNode.HCost <= 0 ? "" : PathNode.HCost.ToString();
        fCostTextMesh.text = PathNode.FCost <= 0 ? "" : PathNode.FCost.ToString();
    }
}
