using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class SpawningScript : MonoBehaviour
{
    [SerializeField] private List<GameObject> _characterPrefabs;
    [SerializeField] private TurnManager _currentTurnManager;

    private void Start()
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
            Instantiate(character, CharSpawningWorldPos, Quaternion.identity);
            _currentTurnManager.RegisterCharacter(ICharHolder);
        }
    }
    public void TurnBasedSpawning(int TurnsBetweenWaves, GameObject _characterSpawn)
    {
        if(_currentTurnManager._turnNumber % TurnsBetweenWaves == 0)
        {
            ICharacter ICharHolder = _characterSpawn.GetComponent<ICharacter>();
            SpawningPosition CharSpawningPosition = ICharHolder.GetSpawningPosition();
            Vector3 CharSpawningWorldPos = GridSetup.Grid.GetCellWorldPosition(CharSpawningPosition.XCoordinate, CharSpawningPosition.YCoordinate);
            Instantiate(_characterSpawn, CharSpawningWorldPos, Quaternion.identity);
            _currentTurnManager.RegisterCharacter(ICharHolder);
            _characterPrefabs.Add(_characterSpawn);
        }
    }
}
