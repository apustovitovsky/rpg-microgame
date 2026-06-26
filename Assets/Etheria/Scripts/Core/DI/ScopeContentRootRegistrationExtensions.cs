using VContainer;
using VContainer.Unity;

namespace Etheria.Core.DI
{
    public static class ScopeContentRootRegistrationExtensions
    {
        public static ComponentRegistrationBuilder UnderScopeRoot(
            this ComponentRegistrationBuilder registration)
        {
            return registration.UnderTransform(
                resolver => resolver.Resolve<ScopeRoot>().Transform);
        }
    }
}
