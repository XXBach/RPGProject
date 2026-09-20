using UnityEngine;

[CreateAssetMenu(fileName = "SpawningPosition", menuName = "Scriptable Objects/SpawningPosition")]
public class SpawningPosition : ScriptableObject
{
    [SerializeField] private int _xCoordinate;
    [SerializeField] private int _yCoordinate;

    public int XCoordinate
    {
        get { return _xCoordinate; }
        set { _xCoordinate = value; }
    }
    public int YCoordinate
    {
        get { return _yCoordinate; }
        set { _yCoordinate = value; }
    }
}
