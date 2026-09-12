using UnityEngine;
using UnityEngine.TextCore.Text;

public class TextUtils
{
    public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localposition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, Font font)
    {
        // Implementation for creating world text
        GameObject gameObject = new GameObject("WorldText", typeof(TextMesh));
        Transform transform = gameObject.transform;
        transform.SetParent(parent, false);
        transform.localPosition = localposition;
        TextMesh textMesh = gameObject.GetComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = fontSize;
        textMesh.color = color;
        textMesh.anchor = textAnchor;
        textMesh.alignment = textAlignment;
        textMesh.characterSize = 0.1f;
        textMesh.font = font;
        return textMesh;
    }
    public static void CreateEmptyMeshArray(int quadCount, out Vector3[] vertices, out Vector2[] uvs, out int[] triangle)
    {
        vertices = new Vector3[4 * quadCount];
        uvs = new Vector2[4 * quadCount];
        triangle = new int[6 * quadCount];
    }
}
