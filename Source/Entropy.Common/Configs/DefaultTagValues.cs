using System;
using System.Collections.Generic;
using System.Text;

namespace Entropy.Common.Configs;

public static class DefaultTagValues
{
	public const int Order = 0;
	public const string? DisplayName = null;
	public const bool Visible = true;
	public const bool Disabled = false;
	public const Func<ConfigEntryBase, bool>? CustomDrawer = null;
	public const bool RequireRestart = false;
	public const string Format = "%.3f";
}