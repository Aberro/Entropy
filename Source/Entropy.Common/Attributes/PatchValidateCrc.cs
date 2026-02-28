namespace Entropy.Common.Attributes;

/// <summary>
/// Ensures that the patch target method has the given CRC, otherwise patching warning prompt appears.
/// </summary>
/// <param name="crc"></param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public class PatchValidateCrcAttribute(ulong crc) : Attribute 
{
	public ulong CRC { get; } = crc;
}