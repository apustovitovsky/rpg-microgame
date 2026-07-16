using System;
using VContainer;

namespace Game.Commands
{
    public static class CommandExecutionRegistrationExtensions
    {
        public static void RegisterCommandExecutionGroup<TExecutionGroup>(
            this IContainerBuilder builder)
            where TExecutionGroup : class, ICommandExecutionGroup
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Register<TExecutionGroup>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();
        }

        public static void RegisterCommandExecution<
            TExecutionGroup,
            TCommand>(
            this IContainerBuilder builder)
            where TExecutionGroup : class,
                ICommandExecutionGroup,
                ICommandExecution<TCommand>
            where TCommand : ICommand
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Register<
                    CommandExecutionEntry<
                        TExecutionGroup,
                        TCommand>>(
                    Lifetime.Scoped)
                .As<ICommandExecutionEntry>();
        }

        public static void RegisterCommandExecution<
            TExecutionGroup,
            TCommand,
            TResult>(
            this IContainerBuilder builder)
            where TExecutionGroup : class,
                ICommandExecutionGroup,
                ICommandExecution<TCommand, TResult>
            where TCommand : ICommand<TResult>
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Register<
                    CommandExecutionAdapter<
                        TExecutionGroup,
                        TCommand,
                        TResult>>(
                    Lifetime.Scoped)
                .As<ICommandExecutionEntry>();
        }
    }
}