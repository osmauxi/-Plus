using Unity.Netcode.Components;

public class ClientMovePermission : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
