using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Objects;
using Assets.Scripts.Objects.Items;
using Assets.Scripts.UI.CustomScrollPanel;
using HarmonyLib;
using Assets.Scripts.UI;
using System.Threading.Tasks;
using Entropy.Scripts.Cartridges;

namespace Entropy.Scripts.Utilities;

public static class PrefabCopier
{
	public static void EnsureGameObjectSimilarity(GameObject copyTo, GameObject copyFrom)
	{
		if (copyTo.GetComponents(typeof(Component)).Length != copyFrom.GetComponents(typeof(Component)).Length)
			throw new Exception("Component count mismatch! Mod needs to be updated");
		if (copyTo.transform.childCount != copyFrom.transform.childCount)
			throw new Exception("Child count mismatch! Mod needs to be updated");
		for (var i = 0; i < copyFrom.transform.childCount; i++)
		{
			var childCopyTo = copyTo.transform.GetChild(i).gameObject;
			var childCopyFrom = copyFrom.transform.GetChild(i).gameObject;
			if (childCopyTo.name != childCopyFrom.name)
				throw new Exception("Child name mismatch! Mod needs to be updated");
			EnsureGameObjectSimilarity(childCopyTo, childCopyFrom);
		}
	}
	public static GameObject CopyGameObject(GameObject copyFrom, bool ignoreUnknown = false)
	{
		if(!copyFrom.IsValid())
			throw new ArgumentNullException(nameof(copyFrom));

		var result = new GameObject(copyFrom.name);
		var copyFromComponents = copyFrom.GetComponents<Component>();
		foreach (var component in copyFromComponents)
		{
			switch(component)
			{
			case Cartridge cartridge:
				CopyCartridge(result, cartridge);
				break;
			case RectTransform rectTransform:
				CopyRectTransform(result, rectTransform);
				break;
			case CanvasRenderer canvasRenderer:
				CopyCanvasRenderer(result, canvasRenderer);
				break;
			case MeshRenderer meshRenderer:
				CopyMeshRenderer(result, meshRenderer);
				break;
			case MeshFilter meshFilter:
				CopyMeshFilter(result, meshFilter);
				break;
			case Rigidbody rigidbody:
				CopyRigidbody(result, rigidbody);
				break;
			case Image image:
				CopyImage(result, image);
				break;
			case Text text:
				CopyText(result, text);
				break;
			case TextMeshProUGUI textMesh:
				CopyTextMeshPro(result, textMesh);
				break;
			case ScrollPanel scrollPanel:
				CopyScrollPanel(result, scrollPanel);
				break;
			case UnityEngine.UI.Mask mask:
				CopyMask(result, mask);
				break;
			case BoxCollider boxCollider:
				CopyBoxCollider(result, boxCollider);
				break;
			default:
				if(ignoreUnknown)
					throw new ApplicationException($"Unknown component type: {component.GetType().FullName}");
				continue;
			}
			result.SetActive(copyFrom.activeSelf);
		}
		for(var i = 0; i < copyFrom.transform.childCount; i++)
		{
			var childToCopy = copyFrom.transform.GetChild(i).gameObject;
			var child = CopyGameObject(childToCopy);
			child.transform.parent = result.transform;
		}
		return result;
	}
	private static AnimationTriggers CopyAnimationTriggers(AnimationTriggers animationTriggers)
	{
		var result = new AnimationTriggers
		{
			normalTrigger = animationTriggers.normalTrigger,
			selectedTrigger = animationTriggers.selectedTrigger,
			disabledTrigger = animationTriggers.disabledTrigger,
			highlightedTrigger = animationTriggers.highlightedTrigger,
			pressedTrigger = animationTriggers.pressedTrigger
		};
		return result;
	}
	private static void CopyCartridge(GameObject copyToObject, Cartridge copyFrom)
	{
		var copyTo = copyToObject.AddComponent<DebugCartridge>();
		var cartridgeTraverse = Traverse.Create(copyTo);
		var copyFromTraverse = Traverse.Create(copyFrom);
		copyTo.Forward = copyFrom.Forward;
		copyTo.HideInStationpedia = copyFrom.HideInStationpedia;
		copyTo.IsEmergency = copyFrom.IsEmergency;
		copyTo.Bounds = copyFrom.Bounds;
		copyTo.SurfaceArea = copyFrom.SurfaceArea;
		copyTo.SurfaceAreaScale = copyFrom.SurfaceAreaScale;
		copyTo.BoundsOffset = copyFrom.BoundsOffset;
		copyTo.IgnoreSave = copyFrom.IgnoreSave;
		copyTo.generateTerrain = copyFrom.generateTerrain;
		copyTo.WillSave = copyFrom.WillSave;
		copyTo.Rotation = copyFrom.Rotation;
		copyTo.CanFrustumCull = copyFrom.CanFrustumCull;
		copyTo.ThermodynamicsScale = copyFrom.ThermodynamicsScale;
		copyTo.SolarHeatingScale = copyFrom.SolarHeatingScale;
		copyTo.WeatherDamageScale = copyFrom.WeatherDamageScale;
		cartridgeTraverse.Field<float>("shatterTemperature").Value = copyFrom.ShatterTemperature.ToFloat();
		copyTo.flashpointTemperature = copyFrom.flashpointTemperature;
		cartridgeTraverse.Field<float>("autoignitionTemperature").Value = copyFrom.AutoignitionTemperature.ToFloat();
		copyTo.BurnTime = copyFrom.BurnTime;
		copyTo.ThingHealth = copyFrom.ThingHealth;
		copyTo.EnergyReleasedWhenBurned = copyFrom.EnergyReleasedWhenBurned;
		copyTo.PaintableMaterial = copyFrom.PaintableMaterial;
		copyTo.CustomColor ??= new ColorSwatch();
		copyTo.CustomColor.Name = copyFrom.CustomColor.Name;
		copyTo.CustomColor.Bit = copyFrom.CustomColor.Bit;
		copyTo.CustomColor.StringKey = copyFrom.CustomColor.StringKey;
		copyTo.CustomColor.Normal = copyFrom.CustomColor.Normal;
		copyTo.CustomColor.Emissive = copyFrom.CustomColor.Emissive;
		copyTo.CustomColor.Cutable = copyFrom.CustomColor.Cutable;
		copyTo.CustomColor.Light = copyFrom.CustomColor.Light;
		copyTo.CustomColor.Color = copyFrom.CustomColor.Color;
		copyTo.IsCursor = copyFrom.IsCursor;
		copyTo.ThumbnailOffset = copyFrom.ThumbnailOffset;
		copyTo.ThumbnailRotation = copyFrom.ThumbnailRotation;
		copyTo.Thumbnail = copyFrom.Thumbnail;
		copyTo.Thumbnails = new Sprite[copyFrom.Thumbnails.Length];
		for (var i = 0; i < copyFrom.Thumbnails.Length; i++)
			copyTo.Thumbnails[i] = copyFrom.Thumbnails[i];
		copyTo.IsEntity = copyFrom.IsEntity;
		copyTo.IsOccluded = copyFrom.IsOccluded;
		copyTo.HasRunOnAtmospherics = copyFrom.HasRunOnAtmospherics;
		copyTo.HasErrorState = copyFrom.HasErrorState;
		copyTo.HasPowerState = copyFrom.HasPowerState;
		copyTo.HasActivateState = copyFrom.HasActivateState;
		copyTo.HasLockState = copyFrom.HasLockState;
		copyTo.HasOnOffState = copyFrom.HasOnOffState;
		copyTo.HasModeState = copyFrom.HasModeState;
		copyTo.HasOpenState = copyFrom.HasOpenState;
		copyTo.HasImportState = copyFrom.HasImportState;
		copyTo.HasExportState = copyFrom.HasExportState;
		copyTo.HasExport2State = copyFrom.HasExport2State;
		copyTo.HasButton1State = copyFrom.HasButton1State;
		copyTo.HasButton2State = copyFrom.HasButton2State;
		copyTo.HasButton3State = copyFrom.HasButton3State;
		copyTo.HasColorState = copyFrom.HasColorState;
		copyTo.HasAccessState = copyFrom.HasAccessState;
		copyTo.CanPickup = copyFrom.CanPickup;
		copyTo.DestroyChildrenOnDead = copyFrom.DestroyChildrenOnDead;
		copyTo.EmissionColor = copyFrom.EmissionColor;
		copyTo.DiffuseIndex = copyFrom.DiffuseIndex;
		copyTo.SmoothnessIndex = copyFrom.SmoothnessIndex;
		copyTo.SortingClass = copyFrom.SortingClass;
		copyTo.RecipeTier = copyFrom.RecipeTier;
		cartridgeTraverse.Field<CollisionClass>("CollisionSound").Value = copyFromTraverse.Field<CollisionClass>("CollisionSound").Value;
		copyTo.SlotWearAction = copyFrom.SlotWearAction;
		copyTo.SlotType = copyFrom.SlotType;
		copyTo.DeleteOnDestroyed = copyFrom.DeleteOnDestroyed;
		copyTo.UsingSound = copyFrom.UsingSound;
		copyTo.UseCompleteSound = copyFrom.UseCompleteSound;
		copyTo.HasLight = copyFrom.HasLight;
		copyTo.IsSmall = copyFrom.IsSmall;
		copyTo.FastSpeed = copyFrom.FastSpeed;
		copyTo.WorldCenterOfMass = copyFrom.WorldCenterOfMass;
		copyTo.GripType = copyFrom.GripType;
		copyTo.CastAnimation = copyFrom.CastAnimation;
		copyTo.AtmosphereDampeningScale = copyFrom.AtmosphereDampeningScale;
		copyTo.AttackWithEvent = copyFrom.AttackWithEvent;
		copyTo.CenterOfMassOffset = copyFrom.CenterOfMassOffset;
		copyTo.AtmosControl = copyFrom.AtmosControl;
		copyTo.OnConveyor = copyFrom.OnConveyor;
		copyTo.TradeValue = copyFrom.TradeValue;
		copyTo.PlayerSellScale = copyFrom.PlayerSellScale;
		copyTo.ImpactForceThreshhold = copyFrom.ImpactForceThreshhold;
		copyTo.SizeMultiplier = copyFrom.SizeMultiplier;
		copyTo.ImpactAudioCoolDown = copyFrom.ImpactAudioCoolDown;
		copyTo.YHeight = copyFrom.YHeight;
		copyTo.IsOutOfBounds = copyFrom.IsOutOfBounds;
		copyTo.RelativeVelocity = copyFrom.RelativeVelocity;
		copyTo.Orientation = copyFrom.Orientation;
		copyTo.ForceKinematicDistance = copyFrom.ForceKinematicDistance;
		copyTo.IsForceKinematic = copyFrom.IsForceKinematic;
		copyTo.AddExtraGravity = copyFrom.AddExtraGravity;
		copyTo.IsKinematic = copyFrom.IsKinematic;
		copyTo.ChildSlotOffset = copyFrom.ChildSlotOffset;
		copyTo.ChildSlotOffsetPosition = copyFrom.ChildSlotOffsetPosition;
		copyTo.ReplacementTool = copyFrom.ReplacementTool;
		copyTo.InventoryScale = copyFrom.InventoryScale;
		copyTo.LocalOffSetInHand = copyFrom.LocalOffSetInHand;
		copyTo.LocalRotationInHand = copyFrom.LocalRotationInHand;
		copyTo.PrecisionIk = copyFrom.PrecisionIk;
		copyTo.AllowSelfUse = copyFrom.AllowSelfUse;
		copyTo.AllowForwardCursor = copyFrom.AllowForwardCursor;
		copyTo.TraderUniqueIdentifierRatio = copyFrom.TraderUniqueIdentifierRatio;
		copyTo.CanDecay = copyFrom.CanDecay;
		copyTo.DecayRate = copyFrom.DecayRate;
		copyTo.IsDecayed = copyFrom.IsDecayed;
		copyTo.TemperatureDecayRateMultiplier = copyFrom.TemperatureDecayRateMultiplier;
		copyTo.PressureDecayRateMultiplier = copyFrom.PressureDecayRateMultiplier;
		copyTo.TemperatureDecayRateMultiplierCurve = copyFrom.TemperatureDecayRateMultiplierCurve;
		copyTo.NutritionDecayRateCurve = copyFrom.NutritionDecayRateCurve;
		copyTo.PressureDecayRateMultiplierCurve = copyFrom.PressureDecayRateMultiplierCurve;
		copyTo.GasesDecayMultiplier = new Item.DecayMixture[copyFrom.GasesDecayMultiplier.Length];
		for (var i = 0; i < copyFrom.GasesDecayMultiplier.Length; i++)
			copyTo.GasesDecayMultiplier[i] = copyFrom.GasesDecayMultiplier[i];
		copyTo.DecayedFoodPrefab = copyFrom.DecayedFoodPrefab;
	}
	private static void CopyRectTransform(GameObject copyToObject, RectTransform copyFrom)
	{
		var copyTo = copyToObject.AddComponent<RectTransform>();
		copyTo.localPosition = copyFrom.localPosition;
		copyTo.localRotation = copyFrom.localRotation;
		copyTo.localScale = copyFrom.localScale;
		copyTo.anchoredPosition = copyFrom.anchoredPosition;
		copyTo.anchoredPosition3D = copyFrom.anchoredPosition3D;
		copyTo.sizeDelta = copyFrom.sizeDelta;
		copyTo.anchorMin = copyFrom.anchorMin;
		copyTo.anchorMax = copyFrom.anchorMax;
		copyTo.pivot = copyFrom.pivot;
	}
	private static void CopyMeshRenderer(GameObject copyToObject, MeshRenderer copyFrom)
	{
		var copyTo = copyToObject.AddComponent<MeshRenderer>();
		copyTo.materials = copyFrom.materials;
		copyTo.shadowCastingMode = copyFrom.shadowCastingMode;
		copyTo.staticShadowCaster = copyFrom.staticShadowCaster;
		copyTo.receiveShadows = copyFrom.receiveShadows;
		//copyTo.receiveGI = copyFrom.receiveGI;
		copyTo.lightProbeUsage = copyFrom.lightProbeUsage;
		copyTo.reflectionProbeUsage = copyFrom.reflectionProbeUsage;
		copyTo.motionVectorGenerationMode = copyFrom.motionVectorGenerationMode;
		copyTo.allowOcclusionWhenDynamic = copyFrom.allowOcclusionWhenDynamic;
	}
	private static void CopyRigidbody(GameObject copyToObject, Rigidbody copyFrom)
	{
		var copyTo = copyToObject.AddComponent<Rigidbody>();
		copyTo.mass = copyFrom.mass;
		copyTo.drag = copyFrom.drag;
		copyTo.angularDrag = copyFrom.angularDrag;
		copyTo.useGravity = copyFrom.useGravity;
		copyTo.isKinematic = copyFrom.isKinematic;
		copyTo.interpolation = copyFrom.interpolation;
		copyTo.collisionDetectionMode = copyFrom.collisionDetectionMode;
		copyTo.constraints = copyFrom.constraints;
		copyTo.centerOfMass = copyFrom.centerOfMass;
		copyTo.inertiaTensor = copyFrom.inertiaTensor;
		copyTo.inertiaTensorRotation = copyFrom.inertiaTensorRotation;
		copyTo.detectCollisions = copyFrom.detectCollisions;
		copyTo.position = copyFrom.position;
		copyTo.rotation = copyFrom.rotation;
		copyTo.velocity = copyFrom.velocity;
		copyTo.angularVelocity = copyFrom.angularVelocity;
		copyTo.maxAngularVelocity = copyFrom.maxAngularVelocity;
		copyTo.solverIterations = copyFrom.solverIterations;
		copyTo.solverVelocityIterations = copyFrom.solverVelocityIterations;
		copyTo.sleepThreshold = copyFrom.sleepThreshold;
		copyTo.maxDepenetrationVelocity = copyFrom.maxDepenetrationVelocity;
	}
	private static void CopyBoxCollider(GameObject copyToObject, BoxCollider copyFrom)
	{
		var copyTo = copyToObject.AddComponent<BoxCollider>();
		copyTo.isTrigger = copyFrom.isTrigger;
		copyTo.material = copyFrom.material;
		copyTo.center = copyFrom.center;
		copyTo.size = copyFrom.size;

	}
	private static void CopyImage(GameObject copyToObject, Image copyFrom)
	{
		var copyTo = copyToObject.AddComponent<Image>();
		copyTo.color = copyFrom.color;
		copyTo.material = copyFrom.material;
		copyTo.raycastTarget = copyFrom.raycastTarget;
		copyTo.raycastPadding = copyFrom.raycastPadding;
		copyTo.maskable = copyFrom.maskable;
		copyTo.useSpriteMesh = copyFrom.useSpriteMesh;
		copyTo.preserveAspect = copyFrom.preserveAspect;
		copyTo.fillCenter = copyFrom.fillCenter;
		copyTo.fillMethod = copyFrom.fillMethod;
		copyTo.fillOrigin = copyFrom.fillOrigin;
		copyTo.fillAmount = copyFrom.fillAmount;
		copyTo.fillClockwise = copyFrom.fillClockwise;
		copyTo.type = copyFrom.type;
		copyTo.pixelsPerUnitMultiplier = copyFrom.pixelsPerUnitMultiplier;
		copyTo.overrideSprite = copyFrom.overrideSprite;
		copyTo.sprite = copyFrom.sprite;
	}
	private static void CopyText(GameObject copyToObject, Text copyFrom)
	{
		var copyTo = copyToObject.AddComponent<Text>();
		copyTo.font = copyFrom.font;
		copyTo.fontSize = copyFrom.fontSize;
		copyTo.fontStyle = copyFrom.fontStyle;
		copyTo.lineSpacing = copyFrom.lineSpacing;
		copyTo.supportRichText = copyFrom.supportRichText;
		copyTo.alignment = copyFrom.alignment;
		copyTo.alignByGeometry = copyFrom.alignByGeometry;
		copyTo.horizontalOverflow = copyFrom.horizontalOverflow;
		copyTo.verticalOverflow = copyFrom.verticalOverflow;
		copyTo.resizeTextForBestFit = copyFrom.resizeTextForBestFit;
		copyTo.color = copyFrom.color;
		copyTo.material = copyFrom.material;
		copyTo.raycastTarget = copyFrom.raycastTarget;
		copyTo.raycastPadding = copyFrom.raycastPadding;
		copyTo.maskable = copyFrom.maskable;
		copyTo.resizeTextMinSize = copyFrom.resizeTextMinSize;
		copyTo.resizeTextMaxSize = copyFrom.resizeTextMaxSize;
	}
	private static void CopyTextMeshPro(GameObject copyToObject, TextMeshProUGUI copyFrom)
	{
		var copyTo = copyToObject.AddComponent<TextMeshProUGUI>();
		copyTo.textStyle = copyFrom.textStyle;
		copyTo.font = copyFrom.font;
		copyTo.fontSize = copyFrom.fontSize;
		copyTo.fontStyle = copyFrom.fontStyle;
		copyTo.enableAutoSizing = copyFrom.enableAutoSizing;
		copyTo.colorGradient = copyFrom.colorGradient;
		copyTo.color = copyFrom.color;
		copyTo.enableVertexGradient = copyFrom.enableVertexGradient;
		copyTo.colorGradientPreset = copyFrom.colorGradientPreset;
		copyTo.overrideColorTags = copyFrom.overrideColorTags;
		copyTo.characterSpacing = copyFrom.characterSpacing;
		copyTo.wordSpacing = copyFrom.wordSpacing;
		copyTo.lineSpacing = copyFrom.lineSpacing;
		copyTo.paragraphSpacing = copyFrom.paragraphSpacing;
		copyTo.alignment = copyFrom.alignment;
		copyTo.enableWordWrapping = copyFrom.enableWordWrapping;
		copyTo.overflowMode = copyFrom.overflowMode;
		copyTo.horizontalMapping = copyFrom.horizontalMapping;
		copyTo.verticalMapping = copyFrom.verticalMapping;
		copyTo.maskable = copyFrom.maskable;
		copyTo.material = copyFrom.material;
		copyTo.raycastTarget = copyFrom.raycastTarget;
		copyTo.raycastPadding = copyFrom.raycastPadding;
	}
	private static void CopyScrollPanel(GameObject copyToObject, ScrollPanel copyFrom)
	{
		var copyTo = copyToObject.AddComponent<ScrollPanel>();
		var copyToTraverse = Traverse.Create(copyTo);
		var copyFromTraverse = Traverse.Create(copyFrom);
		copyToTraverse.Field<float>("_scrollBarWidth").Value = copyFromTraverse.Field<float>("_scrollBarWidth").Value;
		copyToTraverse.Field<float>("_scrollPosition").Value = copyFromTraverse.Field<float>("_scrollPosition").Value;
		copyToTraverse.Field<float>("_sensitivity").Value = copyFromTraverse.Field<float>("_sensitivity").Value;
		copyToTraverse.Field<float>("_scrollSpeed").Value = copyFromTraverse.Field<float>("_scrollSpeed").Value;
	}

	private static void CopyWireframe(GameObject copyToObject, Wireframe copyFrom)
	{
		var copyTo = copyToObject.AddComponent<Wireframe>();
		copyTo.WireframeEdges = new List<Edge>(copyFrom.WireframeEdges.Count);
		var copiedTriangles = new Dictionary<Triangle, Triangle>();
		var copiedEdges = new Dictionary<Edge, Edge>(copyFrom.WireframeEdges.Count);
		var edges = new List<Edge>(copyFrom.WireframeEdges.Count);
		DeepCopyEdges(edges, copyFrom.WireframeEdges, copiedEdges, copiedTriangles, copyTo.transform);
		copyTo.WireframeEdges.AddRange(edges);
	}

	private static void CopyCanvasRenderer(GameObject copyToObject, CanvasRenderer copyFrom)
	{
		var copyTo = copyToObject.AddComponent<CanvasRenderer>();
		copyTo.cullTransparentMesh = copyFrom.GetComponent<CanvasRenderer>().cullTransparentMesh;
	}
	private static void CopyMeshFilter(GameObject copyToObject, MeshFilter copyFrom)
	{
		var copyTo = copyToObject.AddComponent<MeshFilter>();
		copyTo.mesh = copyFrom.GetComponent<MeshFilter>().mesh;
	}
	private static void CopyMask(GameObject copyToObject, UnityEngine.UI.Mask copyFrom)
	{
		var copyTo = copyToObject.AddComponent<UnityEngine.UI.Mask>();
		copyTo.showMaskGraphic = copyFrom.GetComponent<UnityEngine.UI.Mask>().showMaskGraphic;
	}

	private static void DeepCopyEdges(List<Edge> copyTo, List<Edge> copyFrom, Dictionary<Edge, Edge> copiedEdges, Dictionary<Triangle, Triangle> copiedTriangles, Transform parent)
	{
		foreach (var edge in copyFrom)
			DeepCopyEdge(edge, copyTo, copiedEdges, copiedTriangles, parent);
	}

	private static Edge DeepCopyEdge(Edge edge, List<Edge> copyTo, Dictionary<Edge, Edge> copiedEdges, Dictionary<Triangle, Triangle> copiedTriangles, Transform parent)
	{
		if (copiedEdges.TryGetValue(edge, out var result))
			return result;
		result = new Edge();
		copiedEdges.Add(edge, result);
		if (!copiedTriangles.TryGetValue(edge.Triangle, out var triangle))
		{
			triangle = new Triangle();
			copiedTriangles.Add(edge.Triangle, triangle);
			triangle.Parent = parent;
			triangle.Point1 = edge.Triangle.Point1;
			triangle.Point2 = edge.Triangle.Point2;
			triangle.Point3 = edge.Triangle.Point3;
			triangle.Edge1 = DeepCopyEdge(edge.Triangle.Edge1, copyTo, copiedEdges, copiedTriangles, parent);
			triangle.Edge2 = DeepCopyEdge(edge.Triangle.Edge2, copyTo, copiedEdges, copiedTriangles, parent);
			triangle.Edge3 = DeepCopyEdge(edge.Triangle.Edge3, copyTo, copiedEdges, copiedTriangles, parent);
			triangle.Normal = edge.Triangle.Normal;
			triangle.Center = edge.Triangle.Center;
		}
		result.Triangle = triangle;
		result.Point1 = edge.Point1;
		result.Point2 = edge.Point2;
		result.CachedPoint1 = edge.CachedPoint1;
		result.CachedPoint2 = edge.CachedPoint2;
		copyTo.Add(result);
		return result;
	}
}