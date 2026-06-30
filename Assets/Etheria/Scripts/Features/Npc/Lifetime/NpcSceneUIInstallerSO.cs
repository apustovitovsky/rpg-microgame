using Etheria.Core.DI;
using Etheria.Game.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Etheria.Npc
{
    [CreateAssetMenu(
        fileName = "NpcSceneUIInstaller",
        menuName = "Etheria/Npc/Scene UI Installer")]
    public sealed class NpcSceneUIInstallerSO : InstallerSO
    {
        [SerializeField]
        private NpcNameLabelView _labelPrefab;

        public override void Install(IContainerBuilder builder)
        {
            if (_labelPrefab == null)
            {
                Debug.LogError(
                    $"{nameof(NpcSceneUIInstallerSO)} requires assigned NPC name label prefab.");

                return;
            }

            builder.RegisterComponentInHierarchy<CharacterUiPoolHost>();

            builder.RegisterInstance(_labelPrefab);

            builder.Register<INpcNameLabelPoolRoots>(
                resolver => resolver.Resolve<CharacterUiPoolHost>().Labels,
                Lifetime.Singleton);

            builder.Register<NpcNameLabelPool>(Lifetime.Singleton);
        }
    }
}