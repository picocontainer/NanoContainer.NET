using System.Collections;
using PicoContainer;
using PicoContainer.Defaults;

namespace NanoContainer.IntegrationKit
{
	public abstract class LifeCycleContainerBuilder : ContainerBuilder
	{
		private IMutablePicoContainer GetInstanceFromReference(IObjectReference objectReference)
		{
			return objectReference == null ? null : objectReference.Get() as IMutablePicoContainer;
		}

		public void BuildContainer(IObjectReference containerRef,
		                           IObjectReference parentContainerRef,
		                           IList assemblies)
		{
			IMutablePicoContainer parent = GetInstanceFromReference(parentContainerRef);
			IMutablePicoContainer container = CreateContainer(parent, assemblies);

			// register the child in the parent so that lifecycle can be propagated down the hierarchy
			if (parent != null)
			{
				parent.UnregisterComponentByInstance(container);
				parent.RegisterComponentInstance(container, container);
			}

			ComposeContainer(container, assemblies);
			container.Start();
			containerRef.Set(container);
		}

		public void KillContainer(IObjectReference containerRef)
		{
			IMutablePicoContainer pico = GetInstanceFromReference(containerRef);

			try
			{
				if (pico != null)
				{
					pico.Stop();
					pico.Dispose();

					IMutablePicoContainer parent = pico.Parent as IMutablePicoContainer;
					if (parent != null)
					{
						parent.UnregisterComponentByInstance(pico);
					}
				}
			}
			finally
			{
				pico = null;
			}
		}

		protected abstract void ComposeContainer(IMutablePicoContainer container, IList assemblies);

		protected abstract IMutablePicoContainer CreateContainer(IPicoContainer parentContainer, IList assemblies);
	}
}