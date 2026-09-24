using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;


[DefaultExecutionOrder(-200)]
[System.Serializable]
public class SpawnData
{
    public GameObject CharacterPrefab;
    public Vector2Int SpawningPosition;
    public int SpawningTurn;
}
[DefaultExecutionOrder(-150)]
public class SpawningScript : MonoBehaviour
{
    [SerializeField] private SpawnScenario _currentSpawnScenario;

    /*
     Giải thuật: Có 2 hàm - Spawn khi bắt đầu và Spawn khi gọi hết hàm
     *Spawn khi bắt đầu trận: 
     *Lọc danh sách và lấy toàn bộ những SpawnData có SpawningTurn = TurnNumber
     *Tiến hành Handle Spawn toàn bộ các character đó và register chúng vào danh sách nhân vật trong turn
     */
    public List<ICharacter> HandleSpawn(int TurnNum)
    {
        List<ICharacter> SpawnedCharacter = new List<ICharacter>();
        foreach(SpawnData data in _currentSpawnScenario.Scenarios)
        {
            if (data.SpawningTurn == TurnNum)
            {
                GameObject prefab = data.CharacterPrefab;
                Vector2Int spawningCoodinates = data.SpawningPosition;
                Vector3 spawningWorldPos = GridSetup.Grid.GetCellWorldPosition(spawningCoodinates.x, spawningCoodinates.y);
                spawningWorldPos = spawningWorldPos + new Vector3(.5f, .5f);
                GameObject spawned = Instantiate(prefab, spawningWorldPos, Quaternion.identity);
                SpawnedCharacter.Add(spawned.GetComponent<ICharacter>());
            }
            else continue;
        }
        return SpawnedCharacter;
    }
}
