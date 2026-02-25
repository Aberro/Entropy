

using Assets.Scripts.Objects.Electrical;
using Assets.Scripts.Objects.Motherboards;
using Entropy.Common.Utils;
using System.Xml.Serialization;

namespace Entropy.CodeEditor;

public static class Extensions
{
	[XmlRoot]
	public struct ProgrammableChipMotherboardExtension
	{
		[XmlElement]
		public bool UnsavedChanges { get; set; }
		[XmlElement]
		public string? ProgramName { get; set; }
	}

	extension(ProgrammableChipMotherboard motherboard)
	{
		public ProgrammableChipMotherboardExtension ProgrammableChipMotherboardExtension
		{
			get => motherboard.Extensions.GetOrCreate(() => new ProgrammableChipMotherboardExtension());
			set => motherboard.Extensions.Set(value);
		}
		public bool UnsavedChanges
		{
			get => motherboard.ProgrammableChipMotherboardExtension.UnsavedChanges;
			set => motherboard.ProgrammableChipMotherboardExtension = motherboard.ProgrammableChipMotherboardExtension with { UnsavedChanges = value };
		}
		public string? ProgramName
		{
			get => motherboard.ProgrammableChipMotherboardExtension.ProgramName;
			set => motherboard.ProgrammableChipMotherboardExtension = motherboard.ProgrammableChipMotherboardExtension with { ProgramName = value };
		}
	}
	// A bit later, maybe...
	//[XmlRoot]
	//public struct ProgrammableChipExtension
	//{
	//	[XmlElement]
	//	public string? ProgramName { get; set; }
	//}
	//extension(ProgrammableChip chip)
	//{
	//}
}