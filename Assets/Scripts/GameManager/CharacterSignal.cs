using System;

/// <summary>
/// Kênh sự kiện tĩnh: báo cho bất kỳ hệ thống nào quan tâm rằng một character vừa chết.
/// Cùng pattern với CameraSignals / CombatSignal / MovementSignal.
/// </summary>
public static class CharacterSignal
{
    public static event Action<ICharacter> CharacterDied;

    public static void FireCharacterDied(ICharacter character)
    {
        CharacterDied?.Invoke(character);
    }
}
