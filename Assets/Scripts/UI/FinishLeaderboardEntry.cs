using UnityEngine;

public class FinishLeaderboardEntry : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI _position;
    [SerializeField] private TMPro.TextMeshProUGUI _playerName;
    [SerializeField] private TMPro.TextMeshProUGUI _time;

    internal void SetData(int position, string playerName, string time)
    {
        _position.text = position.ToString();
        _playerName.text = playerName;
        _time.text = time;
    }
}
