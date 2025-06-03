// Comment out to enable debugging.
//#undef DEBUG

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;
#if DEBUG
using System.Diagnostics;
#endif

namespace ImGuiRoslynGenerator;

/// <summary>
/// Generates wrappers that allow using pointers to structs in a more C#-like way.
/// </summary>
[Generator]
public class PtrWrapperGenerator : ISourceGenerator
{
	/// <summary>
	/// Initializes the generator and registers for syntax notifications.
	/// </summary>
	/// <param name="context"> The context for the generator initialization.</param>
	public void Initialize(GeneratorInitializationContext context) =>
		context.RegisterForSyntaxNotifications(() => new StructReceiver());

	/// <summary>
	/// Executes the generator, generating source code for each struct that has a type parameter.
	/// </summary>
	/// <param name="context"> The context for the generator execution.</param>
	public void Execute(GeneratorExecutionContext context)
	{
		if(context.SyntaxReceiver is not StructReceiver receiver)
			return;

		var groupedByFile = receiver.Candidates.GroupBy(c => c.SyntaxTree);

		foreach(var group in groupedByFile)
		{
			if(group is null)
				continue;
			var model = context.Compilation.GetSemanticModel(group.Key);
			var usings = group.Key.GetRoot().DescendantNodes()
				.OfType<UsingDirectiveSyntax>()
				.Select(u => u.ToFullString().Trim())
				.ToArray();

			var fileSource = new StringBuilder();

			fileSource.AppendLine("#pragma warning disable CS1591");
			fileSource.AppendLine("#pragma warning disable CS8500");
			fileSource.AppendLine("#nullable enable");

			foreach(var u in usings)
				fileSource.AppendLine(u);

			fileSource.AppendLine();
			string? ns = null;
			var fileScopedNamespace = group.Key.GetRoot().DescendantNodes().OfType<FileScopedNamespaceDeclarationSyntax>().FirstOrDefault();
			if(fileScopedNamespace != null)
				ns = fileScopedNamespace.Name.ToFullString();
			else
			{
				var firstSymbol = model.GetDeclaredSymbol(group.First());
				INamespaceSymbol? firstNamespace = firstSymbol?.ContainingNamespace;
				if(group.Count() == 1 || group.All(x => model.GetDeclaredSymbol(x)?.ContainingNamespace == firstNamespace))
					ns = firstNamespace?.ToDisplayString();
			}
			if(ns is not null)
				fileSource.AppendLine($"namespace {ns};");

			foreach(var candidate in group)
			{
				var symbol = model.GetDeclaredSymbol(candidate);

				if(symbol is null || !symbol.IsValueType)
					continue;

				fileSource.AppendLine(GeneratePtrWrapper(model, receiver, candidate, symbol, ns is null));
			}

			var hintName = Path.GetFileNameWithoutExtension(group.Key.FilePath) + "Ptr.g.cs";
			context.AddSource(hintName, SourceText.From(fileSource.ToString(), Encoding.UTF8));
		}
	}

	private string GeneratePtrWrapper(SemanticModel model, StructReceiver receiver, StructDeclarationSyntax syntax, INamedTypeSymbol structSymbol, bool declareInNamespace)
	{
		var name = structSymbol.Name;
#if DEBUG
		if(receiver.IsDebug(syntax))
			if(!Debugger.IsAttached)
			{
				Debugger.Launch();
				while(!Debugger.IsAttached)
					Thread.Sleep(100);
				Debugger.Break();
			}
#endif
		var namePtr = name + "Ptr";
		var typeParams = structSymbol.TypeParameters.Select(tp => tp.Name).ToArray();
		var typeParamList = typeParams.Length > 0 ? "<" + string.Join(", ", typeParams) + ">" : string.Empty;
		var qualifiedName = name + typeParamList;
		var qualifiedNamePtr = namePtr + typeParamList;
		var constraints = GenerateConstraints(structSymbol.TypeArguments);


		var result = new StringBuilder();
		if(declareInNamespace)
			result.AppendLine($"namespace {structSymbol.ContainingNamespace.ToDisplayString()};");
		if(syntax.HasLeadingTrivia)
		{
			foreach(var trivia in syntax.GetLeadingTrivia())
				result.AppendLine(trivia.ToFullString());
		}
		result.AppendLine($"public unsafe readonly partial struct {qualifiedNamePtr}({qualifiedName}* nativePtr){constraints}");
		result.AppendLine("{");
		result.AppendLine($"\tpublic readonly {qualifiedName}* NativePtr {{ get; }} = nativePtr;");
		result.AppendLine();
		result.AppendLine($"\tpublic {namePtr}(IntPtr nativePtr) : this(({qualifiedName}*)(void*)nativePtr) {{ }}");
		result.AppendLine($"\tpublic static implicit operator {qualifiedNamePtr}({qualifiedName}* nativePtr) => new(nativePtr);");
		result.AppendLine($"\tpublic static implicit operator {qualifiedName}*({qualifiedNamePtr} wrappedPtr) => wrappedPtr.NativePtr;");
		result.AppendLine($"\tpublic static implicit operator {qualifiedNamePtr}(IntPtr nativePtr) => new(nativePtr);");
		result.AppendLine($"\tpublic static implicit operator IntPtr({qualifiedNamePtr} wrappedPtr) => new(wrappedPtr.NativePtr);");
		var triviaMap = new Dictionary<ISymbol, SyntaxTriviaList>();
		foreach(var node in syntax.ChildNodes())
		{
			ISymbol? symbol;
			switch(node)
			{
			case FieldDeclarationSyntax field:
				var fieldSymbol = model.GetDeclaredSymbol(field.Declaration.Variables.First()) as IFieldSymbol;
				if(fieldSymbol == null || fieldSymbol.DeclaredAccessibility != Accessibility.Public || fieldSymbol.IsConst)
					continue;
				symbol = fieldSymbol;
				break;
			case PropertyDeclarationSyntax property:
				var propertySymbol = model.GetDeclaredSymbol(property);
				if(propertySymbol == null || propertySymbol.DeclaredAccessibility != Accessibility.Public 
					|| ((propertySymbol.GetMethod == null || propertySymbol.GetMethod.DeclaredAccessibility != Accessibility.Public)
						&& (propertySymbol.SetMethod == null || propertySymbol.SetMethod.DeclaredAccessibility != Accessibility.Public)))
					continue;
				symbol = propertySymbol;
				break;
			case MethodDeclarationSyntax method:
				var methodSymbol = model.GetDeclaredSymbol(method);
				if(methodSymbol == null || methodSymbol.MethodKind != MethodKind.Ordinary || methodSymbol.DeclaredAccessibility != Accessibility.Public)
					continue;
				symbol = methodSymbol;
				break;
			default:
				continue;
			}
			if(symbol is null)
				continue;
			var trivia = node.GetLeadingTrivia();
			if(trivia.Count > 0)
				triviaMap[symbol] = trivia;
		}

		foreach(var member in structSymbol.GetMembers())
		{
			// We only add entries for symbols that passed filtering, so we could add trivia immediately.
			if(triviaMap.TryGetValue(member, out var trivia))
			{
				foreach(var t in trivia)
				{
					result.Append(t.ToFullString());
				}
			}
			if(member is IMethodSymbol method)
			{
				if(method.MethodKind != MethodKind.Ordinary || method.DeclaredAccessibility != Accessibility.Public)
					continue;
				var parameters = string.Join(", ", method.Parameters.Select(p => $"{p.Type.ToDisplayString()} {p.Name}"));
				var methodTypeParamList = method.TypeArguments.Length > 0 ? "<" + string.Join(", ", method.TypeArguments.Select(t => t.ToDisplayString())) + ">" : string.Empty;
				var methodTypeParamConstraint = GenerateConstraints(method.TypeArguments);
				var args = string.Join(", ", method.Parameters.Select(p => p.Name));

				result.AppendLine($"public {method.ReturnType.ToDisplayString()} {method.Name}{methodTypeParamList}({parameters}){methodTypeParamConstraint} => NativePtr->{method.Name}{methodTypeParamList}({args});");
			}
			if(member is IFieldSymbol field)
			{
				if(field.DeclaredAccessibility != Accessibility.Public || field.IsConst || field.IsStatic)
					continue;
				result.AppendLine($"public {field.Type.ToDisplayString()} {field.Name}");
				result.AppendLine("\t{");
				result.AppendLine($"\t\tget => NativePtr->{field.Name};");
				if(!field.IsReadOnly && !field.IsFixedSizeBuffer)
					result.AppendLine($"\t\tset => NativePtr->{field.Name} = value;");
				result.AppendLine("\t}");
			}
			if(member is IPropertySymbol prop)
			{
				if(prop.DeclaredAccessibility != Accessibility.Public || prop.IsIndexer || prop.IsStatic)
					continue;
				if(prop.SetMethod is not null && prop.SetMethod.DeclaredAccessibility == Accessibility.Public)
				{
					result.AppendLine($"public {prop.Type.ToDisplayString()} {prop.Name}");
					result.AppendLine("\t{");
					if(prop.GetMethod is not null && prop.SetMethod.DeclaredAccessibility == Accessibility.Public)
						result.AppendLine($"\t\tget => NativePtr->{prop.Name};");
					result.AppendLine($"\t\tset => NativePtr->{prop.Name} = value;");
					result.AppendLine("\t}");
				}
				else if(prop.GetMethod is not null && prop.DeclaredAccessibility == Accessibility.Public)
				{
					result.AppendLine($"public {prop.Type.ToDisplayString()} {prop.Name} => NativePtr->{prop.Name};");
				}
			}
		}
		result.Append("}");
		return result.ToString();
	}

	private string GenerateConstraints(ImmutableArray<ITypeSymbol> typeArguments)
	{
		var methodTypeParamConstraint = new StringBuilder();
		foreach(var typeParam in typeArguments.OfType<ITypeParameterSymbol>())
		{
			bool hasConstraint = typeParam.HasReferenceTypeConstraint
			                     || typeParam.HasValueTypeConstraint
			                     || typeParam.HasConstructorConstraint
			                     || typeParam.HasUnmanagedTypeConstraint
			                     || typeParam.HasNotNullConstraint
			                     || typeParam.ConstraintTypes.Any();
			if(!hasConstraint)
				continue;
			bool isFirst = true;
			methodTypeParamConstraint.Append(" where ");
			methodTypeParamConstraint.Append(typeParam.Name);
			methodTypeParamConstraint.Append(" : ");
			if(typeParam.HasReferenceTypeConstraint)
			{
				methodTypeParamConstraint.Append(isFirst ? "class" : ", class");
				isFirst = false;
			}

			if(typeParam.HasUnmanagedTypeConstraint)
			{
				methodTypeParamConstraint.Append(isFirst ? "unmanaged" : ", unmanaged");
				isFirst = false;
			}
			else if(typeParam.HasValueTypeConstraint)
			{
				methodTypeParamConstraint.Append(isFirst ? "struct" : ", struct");
				isFirst = false;
			}

			if(typeParam.HasConstructorConstraint)
			{
				methodTypeParamConstraint.Append(isFirst ? "new()" : ", new()");
				isFirst = false;
			}

			if(typeParam.HasNotNullConstraint)
			{
				methodTypeParamConstraint.Append(isFirst ? "notnull" : ", notnull");
				isFirst = false;
			}

			if(typeParam.ConstraintTypes.Any())
			{
				foreach(var constraintType in typeParam.ConstraintTypes)
				{
					if(!isFirst)
						methodTypeParamConstraint.Append(", ");
					methodTypeParamConstraint.Append(constraintType.ToDisplayString());
					isFirst = false;
				}
			}
		}
		return methodTypeParamConstraint.ToString();
	}

	private class StructReceiver : ISyntaxReceiver
	{
		private List<StructDeclarationSyntax> _candidates = new();
		private List<string> _excludedDeclarations = new();
#if DEBUG
		private List<StructDeclarationSyntax> _debug = new();
		public bool IsDebug(StructDeclarationSyntax syntax) => this._debug.Contains(syntax);
#endif
		public IEnumerable<StructDeclarationSyntax> Candidates => this._candidates.Where(c => !this._excludedDeclarations.Contains(c.Identifier.Text + "Ptr"));

		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if(syntaxNode is StructDeclarationSyntax structDecl && structDecl.Identifier.Text.StartsWith("Im"))
			{
				if(structDecl.Identifier.Text.EndsWith("Ptr") && structDecl.Modifiers.All(x => x.ValueText != "partial"))
					this._excludedDeclarations.Add(structDecl.Identifier.Text);
				else
				{
#if DEBUG
					var leadingTrivia = structDecl.GetLeadingTrivia();
					foreach(var trivia in leadingTrivia)
						if(trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) && trivia.ToString().Contains("ROSLYN_DEBUG"))
							this._debug.Add(structDecl);
#endif
					this._candidates.Add(structDecl);
				}
			}
		}
	}
}
