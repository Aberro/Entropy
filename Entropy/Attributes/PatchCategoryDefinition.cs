namespace Entropy.Attributes
{
	/// <summary>
	/// Attribute that is used to define a patch category display name and description.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
	public class PatchCategoryDefinitionAttribute : Attribute
    {
		/// <summary>
		/// The name of the patch category, the name that is used in <see cref="HarmonyPatchCategoryAttribute"/>.
		/// </summary>
		public string Name { get; }
		/// <summary>
		/// The display name used to display the category name.
		/// </summary>
		public string DisplayName { get; }
		/// <summary>
		/// The description of the patch category used to display in hints.
		/// </summary>
		public string Description { get; }

		/// <summary>
		/// Creates a new instance of the <see cref="PatchCategoryDefinitionAttribute"/> attribute.
		/// </summary>
		/// <param name="name">Patch category name, the name that is used in <see cref="HarmonyPatchCategoryAttribute"/>.</param>
		/// <param name="displayName">Display name used to display the category name.</param>
		/// <param name="description">Description of the patch category used to display in hints.</param>
		public PatchCategoryDefinitionAttribute(string name, string displayName, string description)
		{
			Name = name;
			DisplayName = displayName;
			Description = description;
		}
	}
}
