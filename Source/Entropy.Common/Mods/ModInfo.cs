using System;
using System.Collections.Generic;
using System.Text;

namespace Entropy.Common.Mods;

public record ModInfo
{
	public string? ModID { get; set; }
	public string? Name { get; set; }
	public Version? Version { get; set; }
	public string? VersionString => this.Version?.ToString();
	public ulong WorkshopId { get; set; }
}