#nullable enable

namespace Entropy.Scripts.UI.TextEditor
{
	public struct Breakpoint
	{
		public int mLine;
		public bool mEnabled;
		public string? mCondition;

		public static Breakpoint Create() => new(0);
		private Breakpoint(int _)
		{
			this.mLine = -1;
			this.mEnabled = false;
			this.mCondition = null;
		}
	}
}