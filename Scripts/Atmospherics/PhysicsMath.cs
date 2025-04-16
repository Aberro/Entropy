using Assets.Scripts.Atmospherics;

namespace Entropy.Scripts.Atmospherics
{
	public static class PhysicsMath
	{
		public static TemperatureKelvin Min(TemperatureKelvin val1, TemperatureKelvin val2)
		{
			return val1 < val2 || double.IsNaN(val1.ToDouble()) ? val1 : val2;
		}

		public static TemperatureKelvin Max(TemperatureKelvin val1, TemperatureKelvin val2)
		{
			return val1 > val2 || double.IsNaN(val1.ToDouble()) ? val1 : val2;
		}

		public static PressurekPa Min(PressurekPa val1, PressurekPa val2)
		{
			return val1 < val2 || double.IsNaN(val1.ToDouble()) ? val1 : val2;
		}

		public static PressurekPa Max(PressurekPa val1, PressurekPa val2)
		{
			return val1 > val2 || double.IsNaN(val1.ToDouble()) ? val1 : val2;
		}

		public static VolumeLitres Min(VolumeLitres val1, VolumeLitres val2)
		{
			return val1 < val2 || double.IsNaN(val1.ToDouble()) ? val1 : val2;
		}

		public static VolumeLitres Max(VolumeLitres val1, VolumeLitres val2)
		{
			return val1 > val2 || double.IsNaN(val1.ToDouble()) ? val1 : val2;
		}

		public static MoleQuantity Min(MoleQuantity val1, MoleQuantity val2)
		{
			return val1 < val2 || double.IsNaN(val1.ToDouble()) ? val1 : val2;
		}

		public static MoleQuantity Max(MoleQuantity val1, MoleQuantity val2)
		{
			return val1 > val2 || double.IsNaN(val1.ToDouble()) ? val1 : val2;
		}
	}
}
