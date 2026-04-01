using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork _playerNetwork;
    [SerializeField] private int _damage = 10;
    [SerializeField] private float _attackRange = 3f;

    private PlayerInputActions _inputActions;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (_inputActions != null)
        {
            _inputActions.Player.Attack.performed += OnAttack;
            _inputActions.Player.Enable();
        }
    }

    private void OnDisable()
    {
        if (_inputActions != null)
        {
            _inputActions.Player.Attack.performed -= OnAttack;
            _inputActions.Player.Disable();
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        // Только локальный игрок может инициировать атаку
        if (!IsOwner) return;

        TryFindAndAttack();
    }

    private void TryFindAndAttack()
    {
        // Находим всех игроков в сцене
        PlayerNetwork[] allPlayers = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);

        PlayerNetwork closestTarget = null;

        foreach (PlayerNetwork player in allPlayers)
        {
            // Не атакуем себя
            if (player == _playerNetwork) continue;

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= _attackRange)
            {
                closestTarget = player;
                break; // Берем первого попавшегося в радиусе
            }
        }

        if (closestTarget != null)
        {
            TryAttack(closestTarget);
        }
    }

    public void TryAttack(PlayerNetwork target)
    {
        // Атаку инициирует только локальный владелец объекта
        if (!IsOwner || target == null)
            return;

        DealDamageServerRpc(target.NetworkObjectId, _damage);
    }

    [ServerRpc]
    private void DealDamageServerRpc(ulong targetObjectId, int damage)
    {
        // Сервер проверяет, существует ли цель среди заспавненных сетевых объектов
        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetObjectId, out NetworkObject targetObject))
            return;

        PlayerNetwork targetPlayer = targetObject.GetComponent<PlayerNetwork>();

        // Запрещаем урон самому себе и удары по некорректной цели
        if (targetPlayer == null || targetPlayer == _playerNetwork)
            return;

        // Проверка дистанции на сервере (защита от читов)
        float distance = Vector3.Distance(transform.position, targetPlayer.transform.position);
        if (distance > _attackRange)
            return;

        // Итоговое значение HP ограничиваем снизу нулем
        int nextHp = Mathf.Max(0, targetPlayer.HP.Value - damage);
        targetPlayer.HP.Value = nextHp;

        // Лог в консоль для отладки
        Debug.Log($"[Server] {_playerNetwork.Nickname.Value} attacked {targetPlayer.Nickname.Value} for {damage} damage. {targetPlayer.Nickname.Value} HP: {nextHp}");
    }
}