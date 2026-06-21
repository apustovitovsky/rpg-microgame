using VContainer;
using VContainer.Unity;

namespace Etheria.Core.DI
{
    public static class ScopeHierarchyRegistrationExtensions
    {
        public static ComponentRegistrationBuilder UnderScopeRoot(
            this ComponentRegistrationBuilder registration)
        {
            return registration.UnderTransform(
                resolver => resolver.Resolve<ScopeHierarchy>().ContentRoot);
        }
    }
}
