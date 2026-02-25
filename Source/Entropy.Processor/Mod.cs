using BepInEx.Configuration;
using Entropy.Common.Mods;
using Entropy.Common.Services;
using System;
using UnityEngine;

namespace Entropy.Processor;

public class ProcessorMod : EntropyMod<ProcessorMod>
{
	public override void OnLoaded(List<GameObject> prefabs, ConfigFile config)
	{
		base.OnLoaded(prefabs, config);
		ProcessorProvider.SetListOfCommandsProvider(ChipProcessor.GetInstructions);
		ProcessorProvider.SetListOfIdentifiersProvider(ChipProcessor.GetEnumerationValues);
	}
}