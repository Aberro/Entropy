using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;

if (args.Length < 3)
{
	Console.Error.WriteLine("Usage: AttrDump <assemblyPath> <referencedAssemblies> <outFile>");
	return 2;
}

var assemblyPath = Path.GetFullPath(args[0]);
var referencedAssemblies = Path.GetFullPath(args[1]);
var outputPath = Path.GetFullPath(args[2]);

var runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
// Include runtime assemblies + target assembly directory so resolver can find referenced types if needed.
var paths = Directory.EnumerateFiles(runtimeDir, "*.dll")
	.Concat(referencedAssemblies.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
	.Append(assemblyPath)
	.Distinct(StringComparer.OrdinalIgnoreCase)
	.ToArray();
var resolver = new PathAssemblyResolver(paths);
using var loadContext = new MetadataLoadContext(resolver);

var asm = loadContext.LoadFromAssemblyPath(assemblyPath);
var id = asm.GetName().Name;
var customAttributes = CustomAttributeData.GetCustomAttributes(asm);
var title = customAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(AssemblyTitleAttribute))?
	.ConstructorArguments[0].Value?.ToString() ?? id;
var description = customAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(AssemblyDescriptionAttribute))?
	.ConstructorArguments[0].Value?.ToString() ?? "";
var version = customAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(AssemblyInformationalVersionAttribute))?.ConstructorArguments[0].Value?.ToString()
	?? customAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(AssemblyVersionAttribute))?.ConstructorArguments[0].Value?.ToString() ?? "1.0";
var author = customAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(AssemblyCompanyAttribute))?.ConstructorArguments[0].Value?.ToString() ?? "";
var inGameDescription = customAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(AssemblyMetadataAttribute)
	&& x.ConstructorArguments[0].Value?.ToString() == "InGameDescription")?.ConstructorArguments[1].Value?.ToString();
var workshopHandle = customAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(AssemblyMetadataAttribute)
	&& x.ConstructorArguments[0].Value?.ToString() == "WorkshopHandle")?.ConstructorArguments[1].Value?.ToString();
var changelog = customAttributes.FirstOrDefault(x => x.AttributeType.Name == nameof(AssemblyMetadataAttribute)
	&& x.ConstructorArguments[0].Value?.ToString() == "ChangeLog")?.ConstructorArguments[1].Value?.ToString();
var tags = customAttributes.Where(x => x.AttributeType.Name == nameof(AssemblyMetadataAttribute)
	&& x.ConstructorArguments[0].Value?.ToString() == "Tag").Select(x => x.ConstructorArguments[1].Value?.ToString()).ToArray();
var dependencies = customAttributes.Where(x => x.AttributeType.Name == nameof(AssemblyMetadataAttribute)
	&& x.ConstructorArguments[0].Value?.ToString() == "DependsOn").Select(x => x.ConstructorArguments[1].Value?.ToString());
var orderBefore = customAttributes.Where(x => x.AttributeType.Name == nameof(AssemblyMetadataAttribute)
	&& x.ConstructorArguments[0].Value?.ToString() == "OrderBefore").Select(x => x.ConstructorArguments[1].Value?.ToString()).ToArray();
var orderAfter = customAttributes.Where(x => x.AttributeType.Name == nameof(AssemblyMetadataAttribute)
	&& x.ConstructorArguments[0].Value?.ToString() == "OrderAfter").Select(x => x.ConstructorArguments[1].Value?.ToString()).ToArray();
// Remove line breaks:
description = Regex.Replace(description, @"\r?\n", "");
inGameDescription = inGameDescription != null ? Regex.Replace(inGameDescription, @"\r?\n", "") : null;
// Insert line breaks instead of [br]
description = Regex.Replace(description, @"\[br\]", "\r\n", RegexOptions.IgnoreCase);
inGameDescription = inGameDescription != null ? Regex.Replace(inGameDescription, @"<br>", "\r\n", RegexOptions.IgnoreCase) : null;
// Convert BBCode to Unity TMP text formatting when no InGameDescription metadata is provided
if (inGameDescription is null)
{
	inGameDescription = description;
	// Replace [b], [i], [u], [s], [noparse] tags and closing tags with their <...> counterpart
	inGameDescription = Regex.Replace(inGameDescription, @"\[(\/?)(b|i|u|s|noparse)\]", @"<$1$2>", RegexOptions.IgnoreCase);
	// Replace [color=#...]...[/color] with <color=#...>...</color>
	inGameDescription = Regex.Replace(inGameDescription, @"\[color=(.+?)\](.+?)\[/color\]", @"<color=$1>$2</color>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
	// Replace [url]: [url=link]text[/url] to <link="link">text</link>
	inGameDescription = Regex.Replace(inGameDescription, @"\[url=(.+?)\](.+?)\[/url\]", @"<color=#90D5FF><u><link=""$1"">$2</link></u></color>", RegexOptions.IgnoreCase);
	// Additionally, find any http[s] links and convert them into proper <link>
	inGameDescription = Regex.Replace(inGameDescription, @"(?<!<link="")(https?://[^\s<\]]+)", @"<color=#90D5FF><u><link=""$1"">$1</link></u></color>", RegexOptions.IgnoreCase);
	// Replace [h1/h2/h3]...[/h1/h2/h3] with <size="30/24/18">...</size>
	inGameDescription = Regex.Replace(inGameDescription, @"\[h([1-3])\](.+?)\[/h\1\]", m =>
	{
		var size = m.Groups[1].Value switch
		{
			"1" => "30",
			"2" => "24",
			"3" => "18",
			_ => throw new NotSupportedException("Only headers 1 to 3 are supported")
		};
		return $@"<size=""{size}"">{m.Groups[2].Value}</size>";
	}, RegexOptions.IgnoreCase | RegexOptions.Singleline);
	// Replace [spoiler]...[/spoiler] with <mark=#00000000><color=#00000000>...</color></mark> (black background and transparent text)
	inGameDescription = Regex.Replace(inGameDescription, @"\[spoiler\](.+?)\[/spoiler\]", @"<mark=#00000000><color=#00000000>$1</color></mark>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
	// We can't really support [hr] in TMP, but at least add a page break <page>.
	inGameDescription = Regex.Replace(inGameDescription, @"\[hr\]", @"<page>", RegexOptions.IgnoreCase);
	// Also, remove optional [/hr] - that shouldn't be replaced, just removed.
	inGameDescription = Regex.Replace(inGameDescription, @"\[/hr\]", @"", RegexOptions.IgnoreCase);
	// Replace [code] with <mspace><nobr><noparse>
	inGameDescription = Regex.Replace(inGameDescription, @"\[code\](.+?)\[/code\]", @"<mspace><nobr><noparse>$1</noparse></nobr></mspace>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
	// Replace [center/right/left]...[...] with <align="center/right/left>...<...>
	inGameDescription = Regex.Replace(inGameDescription, @"\[(center|right|left)\](.+?)\[/\1\]", @"<align=""$1"">$2</align>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
	// Replace [list] [*] ... [/list]. This one is more complicated as there's no TMP counterpart.
	// For bullets we use "\u2022<indent=3em>"
	inGameDescription = Regex.Replace(inGameDescription, @"\[list\](.+?)\[/list\]", m =>
	{
		var items = Regex.Split(m.Groups[1].Value, @"\[\*\]").Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => $" \u2022<indent=3em>{x.Trim()}</indent>");
		return string.Join("\r\n", items);
	}, RegexOptions.IgnoreCase | RegexOptions.Singleline);
	// Now even more complicated, [olist], same as list, but we need to numerate bullets on our side:
	inGameDescription = Regex.Replace(inGameDescription, @"\[olist\](.+?)\[/olist\]", m =>
	{
		var items = Regex.Split(m.Groups[1].Value, @"\[\*\]").Where(x => !string.IsNullOrWhiteSpace(x)).Select((x, i) => $" {i + 1}.<indent=3em>{x.Trim()}</indent>");
		return string.Join("\r\n", items);
	}, RegexOptions.IgnoreCase | RegexOptions.Singleline);
	// Finally, the pinnacle, tables, that aren't supported by TMP either and we can't even remotely simulate them.
	// So, instead, we just try to keep things aligned;
	// Table example:
	// [table]
	//	[tr]
	//		[th]Name[/th]
	//		[th]Age[/th]
	//	[/tr]
	//	[tr]
	//		[td]John[/td]
	//		[td]65[/td]
	//	[/tr]
	// [/table]
	// And this is what it should be converted to:
	// <mspace><b>Name  </b>  <b>Age </b>
	// John    65  </mspace>
	inGameDescription = Regex.Replace(inGameDescription, @"\[table\](.+?)\[/table\]", m =>
	{
		var rows = Regex.Matches(m.Groups[1].Value, @"\[tr\](.+?)\[/tr\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
		var parsed = rows.Select(r =>
		{
			var cells = Regex.Matches(r.Groups[1].Value, @"\[t([hd])\](.+?)\[/t\1\]", RegexOptions.IgnoreCase | RegexOptions.Singleline);
			return cells.Select(c => (IsHeader: c.Groups[1].Value.Equals("h", StringComparison.OrdinalIgnoreCase), Text: c.Groups[2].Value.Trim())).ToArray();
		}).ToArray();

		var colCount = parsed.Max(r => r.Length);
		var colWidths = new int[colCount];
		foreach (var row in parsed)
			for (var i = 0; i < row.Length; i++)
				colWidths[i] = Math.Max(colWidths[i], row[i].Text.Length);

		var lines = parsed.Select(row =>
		{
			var cells = row.Select((c, i) =>
			{
				var padded = c.Text.PadRight(colWidths[i]);
				return c.IsHeader ? $"<b>{padded}</b>" : padded;
			});
			return string.Join("  ", cells);
		});

		return $"<mspace>{string.Join("\r\n", lines)}</mspace>";
	}, RegexOptions.IgnoreCase | RegexOptions.Singleline);
	// Finally, just replace remaining square brackets with angle brackets and hope for the best, or at least that TMP would ignore unknown tags.
	// But do not change brackets within <noparse>...</noparse>
	inGameDescription = Regex.Replace(inGameDescription, @"(<noparse>.*?</noparse>)|\[(\/?[^\]]+)\]", m =>
	{
		// If we matched a <noparse>...</noparse> block, return it unchanged
		if (m.Groups[1].Success)
			return m.Groups[1].Value;
		// Otherwise, replace square brackets with angle brackets
		return $"<{m.Groups[2].Value}>";
	}, RegexOptions.IgnoreCase | RegexOptions.Singleline);
	// Replace \u... unicode characters with actual characters
	inGameDescription = Regex.Replace(inGameDescription, @"\\u([0-9A-Fa-f]{4})", m =>
	{
		if (int.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out var code))
			return char.ConvertFromUtf32(code);
		return m.Value;
	});
	// Wrap it all into CDATA:
	inGameDescription = $"<![CDATA[{inGameDescription}]]>";
}
// Remove [color]...[/color], [center/right/left] tags from description as those aren't supported by steam, except within [noparse] region
description = Regex.Replace(description, @"(<noparse>.*?</noparse>)|\[(color)=.+?\](.+?)\[/\2\]|\[(center|right|left)\](.+?)\[/\4\]", m =>
{
	// If we matched a <noparse>...</noparse> block, return it unchanged
	if (m.Groups[1].Success)
		return m.Groups[1].Value;
	// Otherwise, remove color/alignment tags but keep inner content
	return m.Groups[3].Success ? m.Groups[3].Value : m.Groups[5].Value;
}, RegexOptions.IgnoreCase | RegexOptions.Singleline);
// Replace \u... unicode characters with actual characters
description = Regex.Replace(description, @"\\u([0-9A-Fa-f]{4})", m =>
{
	if (int.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out var code))
		return char.ConvertFromUtf32(code);
	return m.Value;
});
// Replace <>&'" with &lt;&gt;&amp;&apos;&quot;
description = description.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("'", "&apos;").Replace("\"", "&quot;");

if (!Directory.Exists(outputPath))
	Directory.CreateDirectory(outputPath);
using var writer = new StreamWriter(Path.Combine(outputPath, "About.xml"), new FileStreamOptions {
	Mode = FileMode.Create,
	Access = FileAccess.ReadWrite,
});
writer.Write(@$"<?xml version=""1.0"" encoding=""utf-8""?>
<!-- This file is auto-generated by AboutGenerator tool. Do not edit manually - it will be overwritten on next build. -->
<!-- Edit the assembly attributes in AssemblyInfo.cs file of the mod instead. -->
<ModMetadata xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	<ModID>{id}</ModID>
	<Name>{title}</Name>
	<Author>{author}</Author>
	<Version>{version}</Version>
	<Description>{description}</Description>
	<InGameDescription>{inGameDescription}</InGameDescription>
	{(changelog != null ? $"<ChangeLog>\r\n{changelog}\r\n\t</ChangeLog>" : "")}
	{(workshopHandle != null ? $"<WorkshopHandle>{workshopHandle}</WorkshopHandle>" : "")}
	{(tags.Any() ? $"<Tags>{string.Concat(tags.Select(tag => $"\r\n\t\t<Tag>{tag}</Tag>"))}\r\n\t</Tags>" : "<Tags />")}
{string.Concat(orderBefore.Select(x => $"\r\n\t<OrderBefore {x} />"))}
{string.Concat(orderAfter.Select(x => $"\r\n\t<OrderAfter {x} />"))}
{string.Concat(dependencies.Select(x => $"\r\n\t<DependsOn {x} />"))}
</ModMetadata>");
writer.Flush();

return 0;
