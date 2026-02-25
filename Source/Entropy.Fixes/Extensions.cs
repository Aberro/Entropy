using Assets.Scripts.Objects.Electrical;
using System.Xml.Serialization;
using Entropy.Common.Utils;


namespace Entropy.Fixes;

public static class Extensions
{
	extension(VendingMachine machine)
	{
		public VendingMachineExtension Extension
		{
			get => machine.Extensions.GetOrCreate(() => new VendingMachineExtension());
			set => machine.Extensions.Set(value);
		}
		public double? Setting
		{
			get => machine.Extension.Setting;
			set => machine.Extension.Setting = value;
		}
	}
	[XmlRoot]
	[XmlInclude(typeof(VendingMachineExtension))]
	public class VendingMachineExtension
	{
		[XmlElement]
		public double? Setting { get; set; }
	}
}
