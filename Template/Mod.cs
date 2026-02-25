using BepInEx.Configuration;
using Entropy.Common.Mods;
using UnityEngine;

namespace Template;

// A minimal mod declaration.
public class TemplateMod : EntropyMod<TemplateMod>
{
	// Could also use this overload for getting the list of prefabs or config file,
	// though the latter is not recommended - config file is managed by Entropy framework.
	// public override void OnLoaded(List<GameObject> prefabs, ConfigFile config) => base.OnLoaded(prefabs, config);
}