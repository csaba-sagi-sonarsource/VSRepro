using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Razor.Analyzers.PoC;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ReproAnalyzer : DiagnosticAnalyzer
{
    private const string DiagnosticId = "AB1234";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, "XXX", "{0}", "Razor", DiagnosticSeverity.Warning, true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }

    public ReproAnalyzer()
    {
        SupportedDiagnostics = ImmutableArray.Create(Rule);
    }

    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                               GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.RegisterSyntaxNodeAction(context =>
        {
            if (context.Node is InvocationExpressionSyntax invocation
                && invocation.Expression is { } expression
                && expression is IdentifierNameSyntax identifierName
                && identifierName.Identifier.ValueText == "RaiseHere")
            {
                var location = identifierName.Identifier.GetLocation();
                var lineSpan = location.GetMappedLineSpan();
                location = Location.Create(Path.GetFullPath(lineSpan.Path), location.SourceSpan, lineSpan.Span);

                context.ReportDiagnostic(Diagnostic.Create(Rule, location, $"I raised here!"));
            }
        }, SyntaxKind.InvocationExpression);
    }
}