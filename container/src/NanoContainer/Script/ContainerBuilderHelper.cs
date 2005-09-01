using System.Collections;
using NanoContainer.IntegrationKit;
using PicoContainer;
using PicoContainer.Defaults;

namespace NanoContainer.Script
{
	public class ContainerBuilderHelper
	{
		private ContainerBuilder containerBuilder;

		public ContainerBuilderHelper(ContainerBuilder containerBuilder)
		{
			this.containerBuilder = containerBuilder;
		}

		public IMutablePicoContainer Build(IList assemblies)
		{
			return this.Build(new DefaultPicoContainer(), assemblies);
		}

		public IMutablePicoContainer Build(IMutablePicoContainer parent, IList assemblies)
		{
			SimpleReference simpleReference = new SimpleReference();
			SimpleReference parentReference = new SimpleReference();
			parentReference.Set(parent);

			containerBuilder.BuildContainer(simpleReference, parentReference, assemblies);
			return simpleReference.Get() as IMutablePicoContainer;
		}
	}
}
