using System;
using Game.Commands;

namespace Game.Interaction
{
    public readonly struct InteractCommand :
        ICommand
    {
        public InteractCommand(
            Guid targetInstanceId)
        {
            TargetInstanceId = targetInstanceId;
        }

        public Guid TargetInstanceId { get; }
    }
}