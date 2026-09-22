using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SpawningScript : MonoBehaviour
{
    [SerializeField] private List<GameObject> _characterPrefabs;
    [SerializeField] private TurnManager _currentTurnManager;

    private void Awake()
    {
        StartSceneSpawning();
    }

    public void StartSceneSpawning()
    {
        foreach(GameObject character in _characterPrefabs)
        {
            ICharacter ICharHolder = character.GetComponent<ICharacter>();
            SpawningPosition CharSpawningPosition = ICharHolder.GetSpawningPosition();
            Vector3 CharSpawningWorldPos = GridSetup.Grid.GetCellWorldPosition(CharSpawningPosition.XCoordinate, CharSpawningPosition.YCoordinate);
            CharSpawningWorldPos = CharSpawningWorldPos + new Vector3(0.5f, 0.5f);
            GameObject characterSpawn = Instantiate(character, CharSpawningWorldPos, Quaternion.identity);
            Debug.Log(characterSpawn.GetComponent<ICharacter>());
            _currentTurnManager.RegisterCharacter(characterSpawn.GetComponent<ICharacter>());
        }
    }
    public void TurnBasedSpawning(int TurnsBetweenWaves, GameObject _characterSpawn)
    {
        if(_currentTurnManager._turnNumber % TurnsBetweenWaves == 0)
        {
            ICharacter ICharHolder = _characterSpawn.GetComponent<ICharacter>();
            SpawningPosition CharSpawningPosition = ICharHolder.GetSpawningPosition();
            Vector3 CharSpawningWorldPos = GridSetup.Grid.GetCellWorldPosition(CharSpawningPosition.XCoordinate, CharSpawningPosition.YCoordinate);
            CharSpawningWorldPos = CharSpawningWorldPos + new Vector3(0.5f, 0.5f);
            GameObject characterSpawn = Instantiate(_characterSpawn, CharSpawningWorldPos, Quaternion.identity);
            _currentTurnManager.RegisterCharacter(characterSpawn.GetComponent<ICharacter>());
            _characterPrefabs.Add(_characterSpawn);
        }
    }
}
