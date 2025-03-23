using Fusion;

public class NetworkPlayer : NetworkBehaviour
{
    private PrometeoCarController _cc;

    public override void Spawned()
    {
        _cc = GetComponent<PrometeoCarController>();
        // if I'm the local player
        if (Object.HasInputAuthority)
        {
            BasicSpawner.LocalPlayerSpawned?.Invoke(_cc);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            //data.direction.Normalize();
            _cc.input = data;
        }
    }
}
