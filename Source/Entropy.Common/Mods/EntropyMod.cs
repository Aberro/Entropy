using BepInEx.Configuration;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Entropy.Common.Mods;


/// <summary>
/// An extended base mod class that automatically enables Harmony, Config, and Logger support,
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class EntropyMod<T> : EntropyModBase where T : EntropyMod<T>
{
	private static T _instance = null!;

	public static T Instance
	{
		get => _instance;
		private set
		{
			if (_instance != null)
				throw new InvalidOperationException("Instance already set.");
			_instance = value;
		}
	}

	protected EntropyMod() : base()
	{
		if (this is T)
			_instance = (T) this;
	}
}