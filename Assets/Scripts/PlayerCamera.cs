using Unity.Netcode;
using UnityEngine;

public class PlayerCamera : NetworkBehaviour
{
    [SerializeField] private Camera _playerCamera;

    public override void OnNetworkSpawn()
    {
        // Включаем камеру только для локального игрока
        if (_playerCamera != null)
        {
            _playerCamera.enabled = IsOwner;

            // Отключаем Audio Listener у не-владельцев
            AudioListener audioListener = _playerCamera.GetComponent<AudioListener>();
            if (audioListener != null)
            {
                audioListener.enabled = IsOwner;
            }
        }
    }
}