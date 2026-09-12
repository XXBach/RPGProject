using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    [SerializeField] private GameObject _mainChar_PrinceEldric_Prefab;
    [SerializeField] private TurnManager _turnManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        Vector3 wannaSpawnPosition = new Vector3(5, 5);
        Vector3 instantiatePosition = new Vector3(wannaSpawnPosition.x + 0.5f, wannaSpawnPosition.y + 0.5f);
        GameObject mainChar = Instantiate(_mainChar_PrinceEldric_Prefab, instantiatePosition, Quaternion.identity);
        ICharacter character = mainChar.GetComponent<ICharacter>();
        _turnManager.RegisterCharacter(character);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
