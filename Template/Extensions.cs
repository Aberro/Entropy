using Assets.Scripts.Objects.Electrical;
using System.Xml.Serialization;
using Entropy.Common.Utils;

namespace Template;

public static class Extensions
{
	extension(VendingMachine machine)
	{
		// A C#14 extension properties. This is not necessary, but just a more convenient access to the modded extension instance
		public VendingMachineExtension Extension
		{
			get => machine.Extensions.GetOrCreate(() => new VendingMachineExtension());
			set => machine.Extensions.Set(value);
		}
		// Similarly to the previous extension, but this is convenient access to the extension property itself, making it appear like part of the instance being extended.
		public double? Setting
		{
			get => machine.Extension.Setting;
			set => machine.Extension.Setting = value;
		}
	}

	// XmlRoot, XmlInclude and XmlElement make this object serializable, so it will be automatically saved and restored during saving and loading game.
	[XmlRoot]
	[XmlInclude(typeof(VendingMachineExtension))]
	public class VendingMachineExtension
	{
		// Primitive types could have only XmlElement, complex types need to be serializable as well to be serialized. And have XmlElement attribute.
		[XmlElement]
		public double? Setting { get; set; }
	}
}