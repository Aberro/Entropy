using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
public sealed class InlineArrayAttribute(int length) : Attribute
{
	public int Length { get; } = length;
}