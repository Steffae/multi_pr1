using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private TMP_Text _ammoText;
    [SerializeField] private TMP_Text _respawnTimerText;

    private PlayerShooting _playerShooting;
    private PlayerNetwork _playerNetwork;
    private float _respawnTimer;
    private bool _isDead;

    private void Awake()
    {
        _playerShooting = GetComponent<PlayerShooting>();
        _playerNetwork = GetComponent<PlayerNetwork>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // Скрываем UI для чужих игроков
            gameObject.SetActive(false);
            return;
        }

        // Подписываемся на изменения
        _playerShooting.CurrentAmmo.OnValueChanged += OnAmmoChanged;
        _playerNetwork.IsAlive.OnValueChanged += OnIsAliveChanged;

        // Начальные значения
        OnAmmoChanged(0, _playerShooting.CurrentAmmo.Value);
        OnIsAliveChanged(true, _playerNetwork.IsAlive.Value);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsOwner) return;

        _playerShooting.CurrentAmmo.OnValueChanged -= OnAmmoChanged;
        _playerNetwork.IsAlive.OnValueChanged -= OnIsAliveChanged;
    }

    private void OnAmmoChanged(int oldValue, int newValue)
    {
        if (_ammoText != null)
        {
            _ammoText.text = $"Ammo: {newValue}";
        }
    }

    private void OnIsAliveChanged(bool oldValue, bool newValue)
    {
        _isDead = !newValue;

        if (_respawnTimerText != null)
        {
            _respawnTimerText.gameObject.SetActive(_isDead);
        }

        if (_isDead)
        {
            StartCoroutine(RespawnTimerCoroutine());
        }
    }

    private System.Collections.IEnumerator RespawnTimerCoroutine()
    {
        float timer = 5f;

        while (timer > 0 && _isDead)
        {
            if (_respawnTimerText != null)
            {
                _respawnTimerText.text = $"Respawning in: {Mathf.CeilToInt(timer)}";
            }
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        if (_respawnTimerText != null)
        {
            _respawnTimerText.text = "";
        }
    }
}