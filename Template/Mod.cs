using StationeersLibrary;
using StationeersLibrary.Modding;
using System.Reflection;
using Entropy.Common;
using UnityEngine;

namespace Template;

public class TemplateMod : Mod<TemplateMod>
{
	public static TemplateMod? Instance { get; private set; }

	public override bool UseLogger => true;
	public override bool UseConfig => true;
	public override bool UseHarmony => true;

	public override ModInfo Data => AssemblyUtils.GetModInfo(this);

	public TemplateMod() : base() => Instance = this;

	public override void OnLoaded(List<GameObject> prefabs) => base.OnLoaded(prefabs);

	public override void OnStart() { }
}

public class TemplateModEx : ModEx<TemplateModEx>
{
	public static TemplateModEx? Instance { get; private set; }

	public TemplateModEx() : base() => Instance = this;
}