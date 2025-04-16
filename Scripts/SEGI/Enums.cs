using JetBrains.Annotations;

namespace Entropy.Scripts.SEGI;

[Serializable]
public enum VoxelResolution
{
	[PublicAPI]
	veryLow = 64,

	[PublicAPI]
	low = 128,

	[PublicAPI]
	medium = 256,

	[PublicAPI]
	high = 512,

	[PublicAPI]
	veryHigh = 1024,

	[PublicAPI]
	ultra = 2048,
}

[Serializable]
public enum ShadowMapResolution
{
	[PublicAPI]
	veryLow = 64,
	[PublicAPI]
	low = 128,
	[PublicAPI]
	medium = 256,
	[PublicAPI]
	high = 512,
	[PublicAPI]
	veryHigh = 1024,
	[PublicAPI]
	ultra = 2048,
}
