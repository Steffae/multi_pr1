using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    // Ник должен быть виден всем клиентам, но менять его может только сервер.
    public NetworkVariable<FixedString32Bytes> Nickname = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // HP тоже читает каждый клиент, но изменяется только на сервере.
    public NetworkVariable<int> HP = new(
        100,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public override void OnNetworkSpawn()
    {
        Renderer renderer = GetComponent<Renderer>();
        // Случайная позиция при спавне (только для сервера)
        if (NetworkObjectId==1)
        {
            Vector3 randomPos = new Vector3(Random.Range(1f, 5f), 1.1f, Random.Range(1f, 5f));
            transform.position = randomPos;
            renderer.material.color = Color.pink;
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        if (IsOwner)
        {
            // Только владелец отправляет на сервер свой локально введенный ник.
            SubmitNicknameServerRpc(ConnectionUI.PlayerNickname);
        }
    }

#pragma warning disable CS0618 // Тип или член устарел
    [ServerRpc(RequireOwnership = false)]
#pragma warning restore CS0618 // Тип или член устарел
    private void SubmitNicknameServerRpc(string nickname)
    {
        // Сервер нормализует ник и записывает итоговое значение в NetworkVariable.
        string safeValue = string.IsNullOrWhiteSpace(nickname) ? $"Player_{OwnerClientId}" : nickname.Trim();
        Nickname.Value = safeValue;
    }

}