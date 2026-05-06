namespace Etheria.Features.Targeting
{
    public sealed class TargetCandidateSnapshotProvider : ITargetCandidateSnapshotProvider
    {
        private readonly ITargetCandidateProvider _candidateProvider;
        private readonly TargetCandidate[] _buffer;

        public TargetCandidateSnapshotProvider(
            ITargetCandidateProvider candidateProvider,
            TargetingSettingsSO settings)
        {
            _candidateProvider = candidateProvider;
            _buffer = new TargetCandidate[settings.MaxTargetCandidates];
        }

        public TargetCandidateSnapshot Capture()
        {
            var count = _candidateProvider.GetCandidates(_buffer);
            return new TargetCandidateSnapshot(_buffer, count);
        }
    }
}
