using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using NanoContainer;
using NanoContainer.IntegrationKit;
using NanoContainer.Script.Xml;
using NanoContainer.Test.TestModel;
using NanoContainer.Tests.TestModel;
using NUnit.Framework;
using PicoContainer;
using PicoContainer.Defaults;

namespace Test.Script.Xml
{
	[TestFixture]
	public class XmlScriptsTestCase : AbstractScriptedContainerBuilderTestCase
	{
		[Test]
		public void SimpleContent()
		{
			string xmlScript = @"
				<container>
					<component-instance key='Hello'>XML</component-instance>
					<component-instance key='Hei'>XMLContinerBuilder</component-instance>
				</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));

			IMutablePicoContainer parent = new DefaultPicoContainer();
			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			IPicoContainer pico = cbf.Build(parent, new ArrayList());

			Assert.AreEqual("XML", pico.GetComponentInstance("Hello"));
			Assert.AreEqual("XMLContinerBuilder", pico.GetComponentInstance("Hei"));
		}

		[Test]
		public void CreateSimpleContainer()
		{
			string xmlScript = @"
				<container>
					<assemblies>
						<element file='NanoContainer.Tests.dll'/>
					</assemblies>
					<component-implementation type='System.Text.StringBuilder'/>
					<component-implementation key='typeof(NanoContainer.Test.TestModel.WebServerConfig)' type='NanoContainer.Test.TestModel.DefaultWebServerConfig'/>
					<component-implementation key='NanoContainer.Test.TestModel.WebServer' type='NanoContainer.Test.TestModel.DefaultWebServer'/>
				</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));
			IMutablePicoContainer parent = new DefaultPicoContainer();
			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			IPicoContainer pico = cbf.Build(parent, new ArrayList());

			Assert.IsNotNull(pico.GetComponentInstance(typeof (StringBuilder)));
			Assert.IsNotNull(pico.GetComponentInstance(typeof (WebServerConfig)));
			Assert.IsNotNull(pico.GetComponentInstance("NanoContainer.Test.TestModel.WebServer"));
		}

		[Test]
		public void CreateContainerAndUseConstantParameters()
		{
			string xmlScript = @"
					<container>
						<assemblies>
							<element file='NanoContainer.Tests.dll'/>
						</assemblies>
						<component-implementation key='fooBar' type='NanoContainer.Tests.TestModel.DependentOnStrings'>
				 				<parameter>""ONE""</parameter>
				 				<parameter>""TWO""</parameter>
						</component-implementation>
					</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));

			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			StringCollection assemblies = new StringCollection();
			assemblies.Add("NanoContainer.Tests.dll");
			IPicoContainer pico = cbf.Build(assemblies);

			Assert.AreEqual(1, pico.ComponentInstances.Count);

			DependentOnStrings dependentOnStrings = pico.GetComponentInstance("fooBar") as DependentOnStrings;

			Assert.AreEqual("ONE", dependentOnStrings.One);
			Assert.AreEqual("TWO", dependentOnStrings.Two);
		}

		[Test]
		[ExpectedException(typeof(PicoCompositionException))]
		public void InvalidConstantParameterThrowsException()
		{
			string xmlScript = @"
					<container>
						<assemblies>
							<element file='NanoContainer.Tests.dll'/>
						</assemblies>
						<component-implementation type='NanoContainer.Tests.TestModel.DependentOnStrings'>
				 				<parameter>this should cause the test to fail</parameter>
				 				<parameter>""TWO""</parameter>
						</component-implementation>
					</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));

			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			cbf.Build(new ArrayList());
		}

		[Test]
		public void CreateSimpleContainerWithExplicitKeysAndParameters()
		{
			string xmlScript = @"
					<container>
						<assemblies>
							<element file='NanoContainer.Tests.dll'/>
						</assemblies>
						<component-implementation key='aBuffer' type='System.Text.StringBuilder'/>
						<component-implementation key='NanoContainer.Test.TestModel.WebServerConfig' type='NanoContainer.Test.TestModel.DefaultWebServerConfig'/>
						<component-implementation key='NanoContainer.Test.TestModel.WebServer' type='NanoContainer.Test.TestModel.DefaultWebServer'>
				 				<parameter key='NanoContainer.Test.TestModel.WebServerConfig'/>
				 				<parameter key='aBuffer'/>
						</component-implementation>
					</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));

			IMutablePicoContainer parent = new DefaultPicoContainer();
			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			IPicoContainer pico = cbf.Build(parent, new ArrayList());

			Assert.AreEqual(3, pico.ComponentInstances.Count);
			Assert.IsNotNull(pico.GetComponentInstance("aBuffer"));
			Assert.IsNotNull(pico.GetComponentInstance("NanoContainer.Test.TestModel.WebServerConfig"));
			Assert.IsNotNull(pico.GetComponentInstance("NanoContainer.Test.TestModel.WebServer"));
		}

		[Test]
		public void NonParameterElementsAreIgnoredInComponentImplementation()
		{
			string xmlScript = @"
					<container>
						<assemblies>
							<element file='NanoContainer.Tests.dll'/>
						</assemblies>
						<component-implementation key='aBuffer' type='System.Text.StringBuilder'/>
						<component-implementation key='NanoContainer.Test.TestModel.WebServerConfig' type='NanoContainer.Test.TestModel.DefaultWebServerConfig'/>
						<component-implementation key='NanoContainer.Test.TestModel.WebServer' type='NanoContainer.Test.TestModel.DefaultWebServer'>
				 				<parameter key='NanoContainer.Test.TestModel.WebServerConfig'/>
				 				<parameter key='aBuffer'/>
								<any-old-stuff/>
						</component-implementation>
					</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));

			IMutablePicoContainer parent = new DefaultPicoContainer();
			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			IPicoContainer pico = cbf.Build(parent, new ArrayList());

			Assert.AreEqual(3, pico.ComponentInstances.Count);
			Assert.IsNotNull(pico.GetComponentInstance("aBuffer"));
			Assert.IsNotNull(pico.GetComponentInstance("NanoContainer.Test.TestModel.WebServerConfig"));
			Assert.IsNotNull(pico.GetComponentInstance("NanoContainer.Test.TestModel.WebServer"));
		}

		[Test]
		public void ContainerCanHostAChild()
		{
			string xmlScript = @"
					<container>
						<assemblies>
							<element file='NanoContainer.Tests.dll'/>
						</assemblies>
						<component-implementation key='NanoContainer.Test.TestModel.WebServerConfig' type='NanoContainer.Test.TestModel.DefaultWebServerConfig'/>
						<component-implementation type='System.Text.StringBuilder'/>
						<container>
							<component-implementation key='NanoContainer.Test.TestModel.WebServer' type='NanoContainer.Test.TestModel.DefaultWebServer'/>
						</container>
					</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));

			IMutablePicoContainer parent = new DefaultPicoContainer();
			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			IPicoContainer pico = cbf.Build(parent, new ArrayList());

			Assert.IsNotNull(pico.GetComponentInstance("NanoContainer.Test.TestModel.WebServerConfig"));

			IPicoContainer childcontainer = (DefaultPicoContainer) pico.GetComponentInstance(typeof (DefaultPicoContainer));

			Assert.IsNotNull(childcontainer);
			Assert.IsNotNull(childcontainer.GetComponentInstance("NanoContainer.Test.TestModel.WebServer"));

			StringBuilder sb = (StringBuilder) pico.GetComponentInstance(typeof (StringBuilder));
			Assert.IsTrue(sb.ToString().IndexOf("-WebServer") != -1);
		}

		[Test]
		public void LoadFromAnExternalAssembly()
		{
			FileInfo testCompDll = new FileInfo("../../../TestComp/bin/Debug/TestComp.dll");
			Assert.IsTrue(testCompDll.Exists);
			
			string xmlScript = @"
				<container>
				  <assemblies>
				    <element file='" + testCompDll.FullName + @"'/>
				  </assemblies>
				  <component-implementation key='foo' type='TestComp'/>
				</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(xmlScript)));

			IMutablePicoContainer parent = new DefaultPicoContainer();
			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			IPicoContainer pico = cbf.Build(parent, new ArrayList());

			object fooTestComp = pico.GetComponentInstance("foo");
			Assert.IsNotNull(fooTestComp, "Container should have a 'foo' component");
		}

		[Test]
		public void TypeLoaderHierarchy()
		{
			FileInfo testCompDll = new FileInfo("../../../TestComp/bin/Debug/TestComp.dll");
			FileInfo testCompDll2 = new FileInfo("../../../TestComp2/bin/Debug/TestComp2.dll");
			FileInfo notStartableDll = new FileInfo("../../../NotStartable/bin/Debug/NotStartable.dll");

			Assert.IsTrue(testCompDll.Exists);
			Assert.IsTrue(testCompDll2.Exists);
			Assert.IsTrue(notStartableDll.Exists);
			
			string xmlScript = @"
					<container>
					  <assemblies>
					    <element file='" + testCompDll.FullName + @"'/>
					  </assemblies>
					  <component-implementation key='foo' type='TestComp'/>
					  <container>
					    <assemblies>
					      <element file='" + testCompDll2.FullName + @"'/>
					      <element file='" + notStartableDll.FullName + @"'/>
					    </assemblies>
					    <component-implementation key='bar' type='TestComp2'/>
					    <component-implementation key='phony' type='NotStartable'/>
					  </container>
					  <component-implementation type='System.Text.StringBuilder'/>
					</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(xmlScript)));

			IMutablePicoContainer parent = new DefaultPicoContainer();
			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			IPicoContainer pico = cbf.Build(parent, new ArrayList());

			object fooTestComp = pico.GetComponentInstance("foo");
			Assert.IsNotNull(fooTestComp, "Container should have a 'foo' component");

			StringBuilder sb = (StringBuilder) pico.GetComponentInstance(typeof(StringBuilder));
			Assert.IsTrue(sb.ToString().IndexOf("-TestComp2") != -1, "Container should have instantiated a 'TestComp2' component because it is Startable");
			// We are using the DefaultLifecycleManager, which only instantiates Startable components, and not non-Startable components.
			Assert.IsTrue(sb.ToString().IndexOf("-NotStartable") == -1, "Container should NOT have instantiated a 'NotStartable' component because it is NOT Startable");
		}

		[Test]
		[ExpectedException(typeof (PicoCompositionException))]
		public void UnknownClassThrowsPicoCompositionException()
		{
			string xmlScript = @"
					<container>
						<component-implementation type='Foo'/>
					</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));
			IMutablePicoContainer parent = new DefaultPicoContainer();
			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			cbf.Build(parent, new ArrayList());
		}

		[Test]
		[ExpectedException(typeof (PicoCompositionException))]
		public void ConstantParameterWithNoChildElementThrowsPicoCompositionException()
		{
			string xmlScript = @"
					<container>
						<component-implementation key='NanoContainer.Test.TestModel.WebServer' type='NanoContainer.Test.TestModel.DefaultWebServer'>
				 			<parameter/>
				 			<parameter/>
						</component-implementation>
					</container>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));
			IMutablePicoContainer parent = new DefaultPicoContainer();

			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			cbf.Build(parent, new ArrayList());
		}

		[Test]
		public void EmptyScriptDoesNotThrowsEmptyCompositionException()
		{
			string xmlScript = @"<container/>";

			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));
			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			cbf.Build(null, new ArrayList());
		}

		[Test]
		[ExpectedException(typeof (ArgumentNullException))]
		public void CreateContainerFromNullScriptThrowsArgumentNullException()
		{
			string xmlScript = null;
			StreamReader scriptStream = new StreamReader(new MemoryStream(new ASCIIEncoding().GetBytes(xmlScript)));
			ContainerBuilderFacade cbf = new XmlContainerBuilderFacade(scriptStream);
			cbf.Build(null, new ArrayList());
		}
	}
}