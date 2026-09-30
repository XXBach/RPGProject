using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System;
using Random = UnityEngine.Random;
using TMPro;
using Unity.VisualScripting;
/*<Summary>
    TurnManager quản lý thứ tự lượt của các unit trong game.
    TurnManager sẽ gọi PlayerActionMenu.ShowMenuFor(unit) mỗi khi tới lượt 1 unit
    PlayerActionMenu sẽ lo việc còn lại
    Khi Player chọn hết lượt, PlayerActionMenu sẽ gọi TurnManager.EndTurn() để chuyển lượt sang unit tiếp theo
    Khi toàn bộ List<ICharacter> đã hết lượt, TurnManager sẽ gọi StartNewRound() để reset lượt cho tất cả unit
</Summary>*/
public enum TurnState
{
    PLAYER_TURN = 0,
    ENEMY_TURN = 1,
}
public enum TurnManagerPhase
{
    STARTTURN = 0,
    EXECUTETURN = 1,
    WAITINGFORENDTURNSIGNAL = 2,
    ENDTURN = 3,
}
public class TurnManager : MonoBehaviour
{
    public int _turnNumber;
    [SerializeField] private PlayerActionMenu _actionMenu;
    [SerializeField] private StatsMenuManager _statsMenu;
    [SerializeField] private SpawningScript _spawningScript;
    public List<ICharacter> _characterList;
    private TurnManagerPhase _currentPhase;
    private UnityEvent _endTurnEvent;
    private int currentCharacterIndex = 0;
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    private List<ICharacter> OrderedTurns;
    private bool _isTurnEnded = false;
    private ICharacter _currentTurnCharacter;
    public void SetIsTurnEnded()
    {
        _isTurnEnded = true;
    }
    private void Awake()
    {
        _turnNumber = 0;
        _characterList = new List<ICharacter>();
        OrderedTurns = new List<ICharacter>();
    }
    void Start()
    {
        _characterList.AddRange(_spawningScript.HandleSpawn(_turnNumber));
        _currentPhase = TurnManagerPhase.STARTTURN;
        _turnNumber = 0;
    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentPhase)
        {
            case TurnManagerPhase.STARTTURN:
                HandleStartTurn();
                break;
            case TurnManagerPhase.EXECUTETURN:
                HandleTurnExecution();
                break;
            case TurnManagerPhase.WAITINGFORENDTURNSIGNAL:
                if (_isTurnEnded)
                {
                    _isTurnEnded = false;
                    HandleEndTurnSignal();
                    _currentPhase = TurnManagerPhase.ENDTURN;
                }
                else if (_currentTurnCharacter is Player) // FIX: dùng reference thay vì OrderedTurns[currentCharacterIndex]
                {
                    HandleTurnExecution();
                }
                break;
            case TurnManagerPhase.ENDTURN:
                HandleEndTurn();
                break;
        }
    }
    //Khi state đang ở startturn, hàm này sẽ sắp xếp danh sách các unit theo thứ tự speed có sẵn, nếu bằng thì gọi đến thanh MP, nếu bằng nữa thì random, và gọi ShowMenuFor(unit) cho unit đầu tiên trong danh sách
    public void HandleStartTurn()
    {
        OrderedTurns = OrderingList();
        currentCharacterIndex = 0;
        _currentTurnCharacter = OrderedTurns.Count > 0 ? OrderedTurns[0] : null; // FIX
        StartTurnFor(_currentTurnCharacter);
        SetTurnNumber();
        _currentPhase = TurnManagerPhase.WAITINGFORENDTURNSIGNAL;
    }

    public void HandleTurnExecution()
    {
        StartTurnFor(_currentTurnCharacter); // FIX: luôn dùng đúng reference hiện tại
        _currentPhase = TurnManagerPhase.WAITINGFORENDTURNSIGNAL;
    }
    // TurnManager.cs
    private void StartTurnFor(ICharacter character)
    {
        if (character == null) return;

        CameraSignals.RequestMove(character.GetCharWorldPosition(), smooth: true); // FIX: lia camera tới unit đang đến lượt, dù là Player hay Enemy

        if (character is Player)
        {
            this._actionMenu.ShowMenuFor(character);
        }
        else if (character is Enemy)
        {
            character.GetEnemyAI()._characterCurrentState = CharacterState.CALCULATING;
        }
        this._statsMenu.ShowStatMenuFor(character);
    }

    public void HandleEndTurnSignal()
    {
        // FIX: tra lại vị trí THỰC của _currentTurnCharacter trong OrderedTurns ngay lúc này,
        // thay vì cộng dồn currentCharacterIndex mù quáng. Nhờ vậy dù bao nhiêu character đã bị
        // xoá ở bất kỳ vị trí nào trong list (trước/sau nhân vật hiện tại), lượt kế tiếp vẫn luôn đúng.
        int idx = OrderedTurns.IndexOf(_currentTurnCharacter);
        currentCharacterIndex = idx + 1;

        if (currentCharacterIndex >= OrderedTurns.Count)
        {
            _currentTurnCharacter = null;
        }
        else
        {
            _currentTurnCharacter = OrderedTurns[currentCharacterIndex];
        }
    }
    public void HandleEndTurn()
    {
        _endTurnEvent?.Invoke();
        if (currentCharacterIndex >= OrderedTurns.Count) // FIX: so sánh với OrderedTurns cho nhất quán với HandleEndTurnSignal
        {
            this._turnNumber++;
            List<ICharacter> spawnCharacters = _spawningScript.HandleSpawn(_turnNumber);
            foreach (ICharacter character in spawnCharacters)
            {
                this.RegisterCharacter(character);
            }
            _currentPhase = TurnManagerPhase.STARTTURN;
        }
        else
        {
            _currentPhase = TurnManagerPhase.EXECUTETURN;
        }
    }
    public List<ICharacter> OrderingList()
    {
        List<ICharacter> orderedList = new List<ICharacter>();
        int count = _characterList.Count; // chốt trước, không phụ thuộc list đang bị xóa dần
        for (int i = 0; i < count; i++)
        {
            ICharacter characterRef = GetHighestSpeedCharRef();
            orderedList.Add(characterRef);
            _characterList.Remove(characterRef);
        }
        _characterList = orderedList;
        return orderedList;
    }
    public ICharacter GetHighestSpeedCharRef()
    {
        ICharacter CurrentCharRef = _characterList[0];

        foreach (var characterRef in _characterList)
        {
            if (characterRef == CurrentCharRef) continue;

            if (characterRef.GetCurrentSpeed() > CurrentCharRef.GetCurrentSpeed())
            {
                CurrentCharRef = characterRef;
            }
            else if (characterRef.GetCurrentSpeed() == CurrentCharRef.GetCurrentSpeed())
            {
                CurrentCharRef = GetHigherMPCharRef(CurrentCharRef, characterRef);
            }
        }
        return CurrentCharRef;
    }
    public ICharacter GetHigherMPCharRef(ICharacter characterA, ICharacter characterB)
    {
        if (characterA.GetCurrentMP() > characterB.GetCurrentMP()) return characterA;
        else if (characterA.GetCurrentMP() < characterB.GetCurrentMP()) return characterB;
        else
        {
            bool isCharA = Random.value < 0.5f;             
            return isCharA ? characterA : characterB;
        }
    }
    public void RegisterCharacter(ICharacter character)
    {
        if (!_characterList.Contains(character))
        {
            _characterList.Add(character);
        }
    }

    // FIX: thay hoàn toàn logic cộng/trừ index bằng thao tác đơn giản trên reference — không còn gì để tính sai
    public void RemoveCharacter(ICharacter character)
    {
        if (character == null) return;

        OrderedTurns.Remove(character);
        _characterList.Remove(character);

        if (character == _currentTurnCharacter)
        {
            // Trường hợp hiếm: nhân vật đang thi triển lượt của chính mình bị loại giữa chừng
            // (vd sau này có counter-attack) -> kết thúc lượt ngay, tránh treo turn queue mãi mãi
            _currentTurnCharacter = null;
            SetIsTurnEnded();
        }
    }

    public void SetTurnNumber()
    {
        _textMeshPro.text = "Turn: " + _turnNumber;
    }

    public ICharacter FindCharAtPosition(Vector3 worldposition)
    {
        Vector2Int targetGridPos = GridSetup.Grid.GetGridPosition(worldposition);
        foreach (ICharacter character in this._characterList)
        {
            Vector2Int charGridPos = GridSetup.Grid.GetGridPosition(character.GetCharWorldPosition());
            if (charGridPos == targetGridPos) return character;
        }
        return null;
    }
}
