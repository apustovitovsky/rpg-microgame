using Cysharp.Threading.Tasks;

namespace Game.Dialogue
{
    public interface IDialogueParticipantLease
    {
        UniTask DisposeAsync();
    }
}