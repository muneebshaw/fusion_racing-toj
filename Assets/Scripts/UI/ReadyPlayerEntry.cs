using UnityEngine;
using UnityEngine.UI;

public class ReadyPlayerEntry : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _playerName;
    [SerializeField] private TMPro.TextMeshProUGUI _playerReadyStatus;
    [SerializeField] private Button _readyButton;
    //private bool isReady = false;

    private NetworkPlayer owner;

    internal void Setup(/*string playerName, bool _isReady*//*, bool isLocal,*/ NetworkPlayer _owner)
    {
        owner = _owner;
        _playerName.text = owner.PlayerName.ToString();
        _playerName.color = owner.HasStateAuthority ? Color.red : Color.white;
        //isReady = _isReady;
        _playerReadyStatus.text = owner.IsReady ? "Ready" : "Not Ready";
        _readyButton.interactable = owner.HasStateAuthority;


        //_readyButton.onClick.RemoveAllListeners();
        //_readyButton.onClick.AddListener(() => 
        //{
        //    isReady = !isReady;
        //    _playerReadyStatus.text = isReady ? "Ready" : "Not Ready";
        //    buttonCallback.Invoke(isReady);
        //});
    }

    internal void UpdateReadyStatus()
    {
        //isReady = _isReady;
        _playerReadyStatus.text = owner.IsReady ? "Ready" : "Not Ready";
    }

    public void HandleReadyButton() // called by UI button
    {
        if (!owner.HasStateAuthority) return;

        //isReady = !isReady;
        //_playerReadyStatus.text = isReady ? "Ready" : "Not Ready";
        owner.SetReady(!owner.IsReady);
    }
}
