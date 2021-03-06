using NanoContainer.Attributes;

namespace NanoContainer.Tests.TestModel
{
	[RegisterWithContainer,
		ConstantParameter(0, "ONE"),
		ConstantParameter(1, "TWO")]
	public class DependentOnStrings
	{
		private string one;
		private string two;

		public DependentOnStrings(string one, string two)
		{
			this.one = one;
			this.two = two;
		}

		public string One
		{
			get { return one; }
		}

		public string Two
		{
			get { return two; }
		}
	}
}