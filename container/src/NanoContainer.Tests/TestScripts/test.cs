using PicoContainer;
using PicoContainer.Defaults;

public class Test
{
	private IPicoContainer parent;

	public IPicoContainer Parent
	{
		set { parent = value; }
	}

	public IMutablePicoContainer Compose()
	{
		DefaultPicoContainer p = new DefaultPicoContainer(parent);

		p.RegisterComponentInstance("hello", "C#");
		return p;
	}
}