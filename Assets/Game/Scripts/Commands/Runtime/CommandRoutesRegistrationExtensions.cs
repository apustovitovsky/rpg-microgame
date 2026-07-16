using System;
using VContainer;

namespace Game.Commands
{
    public static class CommandRoutesRegistrationExtensions
    {
        public static void RegisterCommandRoutes<TRoutes>(
            this IContainerBuilder builder)
            where TRoutes : class, ICommandRoutes
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Register<TRoutes>(Lifetime.Scoped)
                .AsImplementedInterfaces()
                .AsSelf();
        }

        public static void RegisterCommandRoute<
            TRoutes,
            TCommand>(
            this IContainerBuilder builder)
            where TRoutes : class,
                ICommandRoutes,
                ICommandHandler<TCommand>
            where TCommand : ICommand
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Register<
                    CommandRoutesHandlerAdapter<
                        TRoutes,
                        TCommand>>(
                    Lifetime.Scoped)
                .As<ICommandHandlerAdapter>();
        }

        public static void RegisterCommandRoute<
            TRoutes,
            TCommand,
            TResult>(
            this IContainerBuilder builder)
            where TRoutes : class,
                ICommandRoutes,
                ICommandHandler<TCommand, TResult>
            where TCommand : ICommand<TResult>
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Register<
                    CommandRoutesHandlerAdapter<
                        TRoutes,
                        TCommand,
                        TResult>>(
                    Lifetime.Scoped)
                .As<ICommandHandlerAdapter>();
        }
    }
}