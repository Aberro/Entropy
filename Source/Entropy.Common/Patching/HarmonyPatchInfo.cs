using Assets.Scripts.UI;
using BepInEx.Harmony;
using Cysharp.Threading.Tasks;
using Entropy.Common.Attributes;
using Entropy.Common.Configs;
using Entropy.Common.Mods;
using Entropy.Common.Utils;
using HarmonyLib;
using HarmonyLib.Tools;
using LaunchPadBooster;
using LaunchPadBooster.Patching;
using SimpleSpritePacker;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using AccessTools = HarmonyLib.AccessTools;

namespace Entropy.Common.Patching;

/// <summary>
/// Represents a patch for a method.
/// </summary>
public class HarmonyPatchInfo
{
	private HarmonyMethod _harmonyMethod;
	private HarmonyPatchType _patchKind;
	private MethodInfo? _patch;
	private readonly Action? _unpatch;
	/// <summary>
	/// The category of the patch.
	/// </summary>
	public ConfigCategory Category { get; }

	/// <summary>
	/// The configuration entry of the patch, used to enable/disable the patch.
	/// </summary>
	public PatchConfigEntry? ConfigEntry { get; }

	/// <summary>
	/// The original method that is being patched.
	/// </summary>
	public MethodBase OriginalMethod { get; }

	/// <summary>
	/// The type in which the patch is defined.
	/// </summary>
	public Type DeclaringType { get; }

	/// <summary>
	/// The method in the patch class that declares the patch.
	/// </summary>
	public MethodInfo DeclaringMethod { get; }

	/// <summary>
	/// Flag indicating if the patch is currently applied.
	/// </summary>
	public bool IsPatched => _patch != null;

	/// <summary>
	/// Flag indicating if the patch supports reversal.
	/// </summary>
	public bool Unpatchable { get; private set; }

	/// <summary>
	/// Prepare method to be called before patching. If it returns false, the patch will not be applied.
	/// </summary>
	public Func<bool>? Prepare { get; }

	/// <summary>
	/// Cleanup method to be called after patching.
	/// </summary>
	public Action? Cleanup { get; }

	/// <summary>
	/// Prepare method to be called before unpatching. If it returns false, unpatching won't be done.
	/// </summary>
	public Func<bool>? PrepareUnpatch { get; }

	/// <summary>
	/// Cleanup method to be called after patching.
	/// </summary>
	public Action? CleanupUnpatch { get; }

	/// <summary>
	/// Creates a new instance of <see cref="HarmonyPatchInfo"/> from the specified type that defines the patch.
	/// </summary>
	/// <param name="mod">The mod that this patch belongs to.</param>
	/// <param name="declaringType">The type to try to create a patch from.</param>
	/// <returns>Returns a new instance of <see cref="HarmonyPatchInfo"/> if the type contains harmony patch attributes, otherwise null.</returns>
	public static IEnumerable<HarmonyPatchInfo?> Create(EntropyModBase mod, Harmony harmony, Type declaringType)
	{
		ArgumentNullException.ThrowIfNull(mod);
		ArgumentNullException.ThrowIfNull(declaringType);
		var processor = new PatchClassProcessor(harmony, declaringType);
		var attributePatches = Traverse.Create(processor).Field("patchMethods")?.GetValue<IList>();
		var auxillaryMethods = Traverse.Create(processor).Field("auxilaryMethods")?.GetValue<Dictionary<Type, MethodInfo>>();
		if(attributePatches is null && auxillaryMethods is null)
			yield break;

		Func<bool>? prepareMethodInvocation = null;
		Action? cleanupMethodInvocation = null;
		List<MethodInfo> targetMethods = new();

		if (auxillaryMethods is not null)
		{
			foreach (var pair in auxillaryMethods)
			{
				switch (pair.Key)
				{
					case Type t when t == typeof(HarmonyPrepare):
						var prepareMethod = pair.Value;
						var prepareMethodParameters = AccessTools.ActualParameters(prepareMethod, [harmony]);
						prepareMethodInvocation = () => (prepareMethod.Invoke(null, prepareMethodParameters) is bool b) ? b : true;
						break;
					case Type t when t == typeof(HarmonyCleanup):
						var cleanupMethod = pair.Value;
						var cleanupMethodParameters = AccessTools.ActualParameters(cleanupMethod, [harmony]);
						cleanupMethodInvocation = () => { cleanupMethod.Invoke(null, cleanupMethodParameters); };
						break;
					case Type t when t == typeof(HarmonyTargetMethod):
						var targetMethodMethod = pair.Value;
						var targetMethodMethodParameters = AccessTools.ActualParameters(targetMethodMethod, [harmony]);
						MethodInfo? targetMethodResult;
						try
						{
							targetMethodResult = targetMethodMethod.Invoke(null, targetMethodMethodParameters) as MethodInfo;
						}
						catch (Exception ex)
						{
							mod.Logger.LogError($"Error invoking {declaringType.FullName}.TargetMethod!");
							mod.Logger.LogException(ex);
							break;
						}
						if (targetMethodResult is null)
						{
							mod.Logger.LogWarning($"{declaringType.FullName}.TargetMethod returned nothing!");
							break;
						} else
						{
							targetMethods.Add(targetMethodResult);
						}
						break;
					case Type t when t == typeof(HarmonyTargetMethods):
						var targetMethodsMethod = pair.Value;
						var targetMethodsMethodParameters = AccessTools.ActualParameters(targetMethodsMethod, [harmony]);
						IEnumerable? targetMethodsResult;
						try
						{
							targetMethodsResult = targetMethodsMethod.Invoke(null, targetMethodsMethodParameters) as IEnumerable;
						}
						catch (Exception ex)
						{
							mod.Logger.LogError($"Error invoking {declaringType.FullName}.TargetMethods!");
							mod.Logger.LogException(ex);
							break;
						}
						if (targetMethodsResult is null)
						{
							mod.Logger.LogWarning($"{declaringType.FullName}.TargetMethods returned nothing!");
							break;
						} else
						{
							foreach ( var targetMethod in targetMethodsResult)
							{
								if (targetMethod is not MethodInfo methodInfo)
								{
									mod.Logger.LogWarning($"Target method {targetMethod?.ToString()} is not a MethodBase, skipping!");
									continue;
								}
								targetMethods.Add(methodInfo);
							}
						}
						break;
				}
			}
		}
		// Our own auxilary methods:
		Func<bool>? prepareUnpatchInvocation = null;
		Action? cleanupUnpatchInvocation = null;
		{
			var prepareUnpatch = AccessTools.GetDeclaredMethods(declaringType).FirstOrDefault(x => x.Name == "PrepareUnpatch");
			var cleanupUnpatch = AccessTools.GetDeclaredMethods(declaringType).FirstOrDefault(x => x.Name == "CleanupUnpatch");
			if (prepareUnpatch is not null)
			{
				var prepareUnpatchParameters = AccessTools.ActualParameters(prepareUnpatch, [harmony]);
				prepareUnpatchInvocation = () => prepareUnpatch.Invoke(null, prepareUnpatchParameters) is bool b ? b : true;
			}
			if (cleanupUnpatch is not null)
			{
				var cleanupUnpatchParameters = AccessTools.ActualParameters(cleanupUnpatch, [harmony]);
				cleanupUnpatchInvocation = () => cleanupUnpatch.Invoke(null, cleanupUnpatchParameters);
			}
		}
		if (attributePatches is not null)
		{
			foreach (var attributePatch in attributePatches)
			{
				var harmonyMethod = Traverse.Create(attributePatch).Field("info").GetValue<HarmonyMethod>();
				var harmonyPatchType = Traverse.Create(attributePatch).Field("type").GetValue<HarmonyPatchType>();

				if (targetMethods.Count > 0)
				{
					foreach (var methodInfo in targetMethods)
					{
						var newHarmonyMethod = new HarmonyMethod();
						harmonyMethod.CopyTo(newHarmonyMethod);
						newHarmonyMethod.method = harmonyMethod.method;
						newHarmonyMethod.declaringType = methodInfo.DeclaringType;
						newHarmonyMethod.methodName = methodInfo.Name;
						yield return new HarmonyPatchInfo(
							mod,
							declaringType,
							newHarmonyMethod,
							harmonyPatchType,
							prepareMethodInvocation,
							cleanupMethodInvocation,
							prepareUnpatchInvocation,
							cleanupUnpatchInvocation);
					}
				} else
				{
					if (harmonyMethod.method is null)
					{
						CommonMod.Instance.Logger.LogWarning($"Could not find patch method(s) in {declaringType.FullName} marked with [HarmonyPatch] attribute!");
						continue; // target method is undefined, this is probably TargetMethod(s) patch
					}
					var declaringMethod = harmonyMethod.method;
					var validateCrc = declaringMethod.GetCustomAttribute<PatchValidateCrcAttribute>();
					if (validateCrc != null)
					{
						var originalMethod = GetOriginalMethod(declaringType, harmonyMethod);
						var crc = Hash64.HashToUInt64(originalMethod.GetMethodBody().GetILAsByteArray());
						if (validateCrc.CRC != crc)
						{
							CommonMod.Instance.Logger.LogError($"Target method `{originalMethod.DeclaringType.FullName}.{originalMethod.Name}` for patch {declaringType.Name}.{declaringMethod} CRC validation failed: expected {validateCrc.CRC}, actual {crc}! The patch needs to be updated!");
							// This task CANNOT be run at this moment, because PromptPanel.Instance is null!
							// it will be ran when PromptPanel.Instance would become initialized later on.
							yield return new Lazy<Task<HarmonyPatchInfo?>>(async () =>
							{
								var cancel = false;
								await PromptPanel.Instance.AwaitShowPrompt(
									@"Patch CRC validation failed!",
									$"The target method `{originalMethod.DeclaringType.FullName}.{originalMethod.Name}` for patch `{declaringType.Name}.{declaringMethod.Name}` CRC validation failed:" +
									$"expected {validateCrc.CRC}, actual {crc}!\r\nThe patch needs to be updated." +
									"\r\n\r\nDo you wish to proceed with patching anyway (could result in incorrect mod behavior or break your save in worst case)?",
									"Proceed",
									() => cancel = false,
									"Cancel",
									() => cancel = true);
								if (cancel)
									return null;
								return new HarmonyPatchInfo(
									mod,
									declaringType,
									harmonyMethod,
									harmonyPatchType,
									prepareMethodInvocation,
									cleanupMethodInvocation,
									prepareUnpatchInvocation,
									cleanupUnpatchInvocation);
							});
						}
					}
					yield return new HarmonyPatchInfo(
						mod,
						declaringType,
						harmonyMethod,
						harmonyPatchType,
						prepareMethodInvocation,
						cleanupMethodInvocation,
						prepareUnpatchInvocation,
						cleanupUnpatchInvocation)));
				}
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="HarmonyPatchInfo"/> class with the specified declaring patch type and methods.
	/// </summary>
	/// <param name="mod">The mod that this patch belongs to.</param>
	/// <param name="declaringType">The type that declares the patch.</param>
	/// <param name="methods">List of <see cref="HarmonyMethod"/> attributes that define the patch.</param>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="methods"/> is null.</exception>
	/// <exception cref="ApplicationException">Thrown when <see cref="OriginalMethod"/> for patching could not be found or is ambiguous.</exception>
	private HarmonyPatchInfo(EntropyModBase mod, Type declaringType, HarmonyMethod method, HarmonyPatchType patchType, Func<bool>? prepare, Action? cleanup, Func<bool>? prepareUnpatch, Action? cleanupUnpatch)
	{
		_harmonyMethod = method;
		DeclaringType = declaringType;
		DeclaringMethod = method.method;
		_patchKind = patchType;
		this.Prepare = prepare;
		this.Cleanup = cleanup;
		this.PrepareUnpatch = prepareUnpatch;
		this.CleanupUnpatch = cleanupUnpatch;
		Category = method.method.GetCategory();
		if(Category == ConfigCategory.GetDefault(mod))
			Category = declaringType.GetCategory();
		ConfigEntry = method.method.GetPatchConfigEntry() ?? declaringType.GetPatchConfigEntry();
		OriginalMethod = GetOriginalMethod(declaringType, method);
		Unpatchable = declaringType.GetCustomAttribute<UnpatchableAttribute>() != null || _patchKind is HarmonyPatchType.ReversePatch;
		var unpatchMethodInfo = AccessTools.GetDeclaredMethods(declaringType).FirstOrDefault(x => x.Name == "Unpatch");
		if (unpatchMethodInfo != null)
		{
			if (Unpatchable)
				throw new ApplicationException("Patch cannot be marked as unpatchable and have an Unpatch method at the same time!");
			this._unpatch = AccessTools.MethodDelegate<Action>(unpatchMethodInfo);
		}
		if (OriginalMethod == null)
			throw new InvalidOperationException("OriginalMethod is null!");
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
		var isPatched = Harmony.GetPatchInfo(OriginalMethod)?.Owners?.Any(harmony.Id.Equals) ?? false;

		if (!IsPatched)
		{
			try
			{
				if(Prepare is not null && !Prepare())
				{
					this.Category.Mod.Logger.LogInfo($"Skipped patch {this.DeclaringType.FullName}.{this.DeclaringMethod.Name} because Prepare returned false.");
					return false;
				}
				if (_patchKind == HarmonyPatchType.ReversePatch)
				{
					var reversePatch = harmony.CreateReversePatcher(OriginalMethod, _harmonyMethod);
					_patch = reversePatch.Patch(HarmonyReversePatchType.Original);
				} else
				{
					HarmonyFileLog.Enabled = true;
					var patchInfo = Harmony.GetPatchInfo(OriginalMethod);
					if (patchInfo != null && patchInfo.Owners.Count > 0)
					{
						CommonMod.Instance.Logger.LogWarning($"Method {OriginalMethod.DeclaringType?.FullName}.{OriginalMethod.Name} already has patches applied, patching it again may cause unexpected behavior!");
					}
					_patch = harmony.Patch(OriginalMethod,
						_patchKind == HarmonyPatchType.Prefix ? _harmonyMethod : null,
						_patchKind == HarmonyPatchType.Postfix ? _harmonyMethod : null,
						_patchKind == HarmonyPatchType.Transpiler ? _harmonyMethod : null,
						_patchKind == HarmonyPatchType.Finalizer ? _harmonyMethod : null,
						_patchKind == HarmonyPatchType.ILManipulator ? _harmonyMethod : null);
				}
				Cleanup?.Invoke();
				return IsPatched;
			}
			catch(Exception e)
			{
				this.Category.Mod.Logger.LogError($"Error during patching {this.DeclaringType.FullName}.{this.DeclaringMethod.Name}");
				this.Category.Mod.Logger.LogException(e);
				HarmonyFileLog.Writer.Flush();
				return false;
			}
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
		if(_patchKind is HarmonyPatchType.ReversePatch)
		{
			this.Category.Mod.Logger.LogWarning($"Reverse patch {this.DeclaringType.FullName}.{this.DeclaringMethod.Name} cannot be unpatched!");
			return false;
		}
		if(DeclaringMethod is null)
		{
			this.Category.Mod.Logger.LogWarning($"Declaring method in patch class {this.DeclaringType.FullName} is null, cannot unpatch!");
			return false;
		}
		if (PrepareUnpatch is not null && !PrepareUnpatch())
		{
			this.Category.Mod.Logger.LogInfo($"Skipped unpatch {this.DeclaringType.FullName}.{this.DeclaringMethod.Name} because PrepareUnpatch returned false.");
			return false;
		}
		try
		{
			harmony.Unpatch(OriginalMethod, DeclaringMethod);
			_patch = null;
			if(this._unpatch is not null)
				this._unpatch();
			CleanupUnpatch?.Invoke();
			return !IsPatched;
		}
		catch(Exception e)
		{
			this.Category.Mod.Logger.LogError($"Error during unpatching {this.DeclaringType.FullName}.{this.DeclaringMethod.Name}");
			this.Category.Mod.Logger.LogException(e);
			Unpatchable = true;
			HarmonyFileLog.Writer.Flush();
			return false;
		}
	}

	private static MethodBase GetOriginalMethod(Type declaringType, HarmonyMethod attr)
	{
		try
		{
			switch (attr.methodType.GetValueOrDefault())
			{
				default:
				case MethodType.Normal:
					if (attr.methodName != null)
						return AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes);
					break;
				case MethodType.Getter:
					if (attr.methodName != null)
						return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetGetMethod(true);
					break;
				case MethodType.Setter:
					if (attr.methodName != null)
						return AccessTools.DeclaredProperty(attr.declaringType, attr.methodName).GetSetMethod(true);
					break;
				case MethodType.Constructor:
					return AccessTools.DeclaredConstructor(attr.declaringType, attr.argumentTypes);
				case MethodType.StaticConstructor:
					return AccessTools.GetDeclaredConstructors(attr.declaringType).FirstOrDefault(c => c.IsStatic);
				case MethodType.Enumerator:
					if (attr.methodName != null)
						return AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(attr.declaringType, attr.methodName, attr.argumentTypes));
					break;
			}
		}
		catch (AmbiguousMatchException ex)
		{
			throw new AmbiguousMatchException($"Ambiguous match for HarmonyMethod[{attr}]", ex.InnerException ?? ex);
		}
		throw new InvalidOperationException($"Unknown method for HarmonyMethod[{attr}] declared in {declaringType}");
	}
}