﻿using Entropy.Attributes;
using Entropy.Helpers;
using HarmonyLib;
using System.Reflection;
using AccessTools = HarmonyLib.AccessTools;

namespace Entropy.Patches;

/// <summary>
/// Represents a patch for a method.
/// </summary>
public class HarmonyPatchInfo
{
	private readonly Action? _unpatch;
	/// <summary>
	/// The category of the patch.
	/// </summary>
	public PatchCategory Category { get; }
	/// <summary>
	/// The original method that is being patched.
	/// </summary>
	public MethodBase OriginalMethod { get; }
	/// <summary>
	/// The type in which the patch is defined.
	/// </summary>
	public Type DeclaringType { get; }

	/// <summary>
	/// Flag indicating if the patch is currently applied.
	/// </summary>
	public bool IsPatched { get; private set; }

	/// <summary>
	/// Flag indicating if the patch supports reversal.
	/// </summary>
	public bool Unpatchable { get; private set; }

	/// <summary>
	/// Creates a new instance of <see cref="HarmonyPatchInfo"/> from the specified type that defines the patch.
	/// </summary>
	/// <param name="declaringType">The type to try to create a patch from.</param>
	/// <returns>Returns a new instance of <see cref="HarmonyPatchInfo"/> if the type contains harmony patch attributes, otherwise null.</returns>
	public static HarmonyPatchInfo? Create(Type declaringType)
	{
		var methods = HarmonyMethodExtensions.GetFromType(declaringType);
		return methods is not { Count: > 0 } ? null : new HarmonyPatchInfo(declaringType, methods);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HarmonyPatchInfo"/> class with the specified declaring patch type and methods.
	/// </summary>
	/// <param name="declaringType">The type that declares the patch.</param>
	/// <param name="methods">List of <see cref="HarmonyMethod"/> attributes that define the patch.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="methods"/> is null.</exception>
	/// <exception cref="ApplicationException">Thrown when <see cref="OriginalMethod"/> for patching could not be found or is ambiguous.</exception>
	private HarmonyPatchInfo(Type declaringType, List<HarmonyMethod> methods)
	{
		if (methods == null)
			throw new ArgumentNullException(nameof(methods));
		DeclaringType = declaringType;
		var method = HarmonyMethod.Merge(methods);
		Category = declaringType.GetCategory();
		OriginalMethod = GetOriginalMethod(declaringType, method);
		Unpatchable = declaringType.GetCustomAttribute<Unpatchable>() == null;
		var unpatchMethodInfo = AccessTools.Method(declaringType, "Unpatch");
		Unpatchable = Unpatchable && unpatchMethodInfo == null;
		if(unpatchMethodInfo != null)
			this._unpatch = AccessTools.MethodDelegate<Action>(unpatchMethodInfo);
		if (OriginalMethod == null)
			throw new ApplicationException("OriginalMethod is null!");
	}

	/// <summary>
	/// Applies the patch using specified <see cref="Harmony"/> instance.
	/// </summary>
	/// <param name="harmony">Harmony instance to use for patching.</param>
	/// <returns><see langword="true"/> when the patch was applied successfully, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="harmony"/> is null.</exception>
	public bool Patch(Harmony harmony)
	{
		if (harmony == null)
			throw new ArgumentNullException(nameof(harmony));
		// Check if the method is already patched.
		var patched = Harmony.GetPatchInfo(OriginalMethod)?.Owners?.Any(harmony.Id.Equals) ?? false;

		if (!patched)
		{
			var processor = new PatchClassProcessor(harmony, DeclaringType);
			processor.Patch();
			IsPatched = true;
			return true;
		}
		return false;
	}

	/// <summary>
	/// Revert the patch using specified <see cref="Harmony"/> instance.
	/// </summary>
	/// <param name="harmony">Harmony instance to use for unpatching.</param>
	/// <returns><see langword="true"/> if unpatching was successful, otherwise <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="harmony"/> is null.</exception>
	public bool Unpatch(Harmony harmony)
	{
		if (harmony == null)
			throw new ArgumentNullException(nameof(harmony));
		if(!IsPatched || Unpatchable)
			return false;
		try
		{
			// Check if the method is actually patched (though it might be not by this patch).
			if(Harmony.GetPatchInfo(OriginalMethod)?.Owners?.Any(harmony.Id.Equals) ?? false)
				harmony.CreateProcessor(OriginalMethod).Unpatch(HarmonyPatchType.All, harmony.Id);
			if(this._unpatch is not null)
				this._unpatch();
			IsPatched = false;
			return true;
		}
		catch(Exception e)
		{
			EntropyPlugin.LogError($"Error during unpatching: {e}");
			Unpatchable = true;
			return false;
		}
	}

	private static MethodBase GetOriginalMethod(Type declaringType, HarmonyMethod attr)
	{
		MethodBase? result = null;
		try
		{
			switch (attr.methodType.GetValueOrDefault())
			{
				default:
				case MethodType.Normal:
					if (attr.methodName != null)
						result = AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes);
					break;
				case MethodType.Getter:
					if (attr.methodName != null)
						result = AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetGetMethod(true);
					break;
				case MethodType.Setter:
					if (attr.methodName != null)
						result = AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetSetMethod(true);
					break;
				case MethodType.Constructor:
					result = AccessTools.DeclaredConstructor(attr.declaringType, attr.argumentTypes);
					break;
				case MethodType.StaticConstructor:
					result = AccessTools.GetDeclaredConstructors(attr.declaringType).FirstOrDefault(c => c.IsStatic);
					break;
				case MethodType.Enumerator:
					if (attr.methodName != null)
						result = AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes));
					break;
			}
		}
		catch (AmbiguousMatchException ex)
		{
			throw new ApplicationException($"Ambiguous match for HarmonyMethod[{attr}]", ex.InnerException ?? ex);
		}
		if (result != null)
			return result;
		throw new ApplicationException($"Unknown method for HarmonyMethod[{attr}] declared in {declaringType}");
	}
}