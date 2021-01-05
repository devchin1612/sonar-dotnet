﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ExecutingSqlQueries : ExecutingSqlQueriesBase<SyntaxKind, ExpressionSyntax>
    {
        public ExecutingSqlQueries()
            : this(AnalyzerConfiguration.Hotspot)
        {
        }

        internal /*for testing*/ ExecutingSqlQueries(IAnalyzerConfiguration analyzerConfiguration) : base(RspecStrings.ResourceManager)
        {
            InvocationTracker = new CSharpInvocationTracker(analyzerConfiguration, Rule);
            PropertyAccessTracker = new CSharpPropertyAccessTracker(analyzerConfiguration, Rule);
            ObjectCreationTracker = new CSharpObjectCreationTracker(analyzerConfiguration, Rule);
        }

        protected override ExpressionSyntax GetInvocationExpression(SyntaxNode expression) =>
            expression is InvocationExpressionSyntax invocation
                ? invocation.Expression
                : null;

        protected override ExpressionSyntax GetArgumentAtIndex(InvocationContext context, int index) =>
            context.Invocation is InvocationExpressionSyntax invocation
                ? invocation.ArgumentList.Get(index)
                : null;

        protected override ExpressionSyntax GetSetValue(PropertyAccessContext context) =>
            context.Expression is MemberAccessExpressionSyntax setter && setter.IsLeftSideOfAssignment()
                ? ((AssignmentExpressionSyntax)setter.GetSelfOrTopParenthesizedExpression().Parent).Right.RemoveParentheses()
                : null;

        protected override ExpressionSyntax GetFirstArgument(ObjectCreationContext context) =>
            context.Expression is ObjectCreationExpressionSyntax objectCreation
                ? objectCreation.ArgumentList.Get(0)
                : null;

        protected override bool IsTracked(ExpressionSyntax argument, SemanticModel semanticModel) =>
            argument != null
            && (IsConcatenation(argument, semanticModel)
                || argument.IsKind(SyntaxKind.InterpolatedStringExpression)
                || (argument is InvocationExpressionSyntax invocation && IsInvocationOfInterest(invocation, semanticModel))
                || IsTrackedVariableDeclaration(argument, semanticModel));

        private static bool IsInvocationOfInterest(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            (invocation.IsMethodInvocation(KnownType.System_String, "Format", semanticModel) || invocation.IsMethodInvocation(KnownType.System_String, "Concat", semanticModel))
            && !AllConstants(invocation.ArgumentList.Arguments.ToList(), semanticModel);

        private static bool IsConcatenation(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression.IsKind(SyntaxKind.AddExpression)
            && expression is BinaryExpressionSyntax concatenation
            && !IsConcatenationOfConstants(concatenation, semanticModel);

        private bool IsTrackedVariableDeclaration(ExpressionSyntax argument, SemanticModel semanticModel) =>
            (argument is IdentifierNameSyntax identifierNameSyntax
             && semanticModel.GetDeclaringSyntaxNode(identifierNameSyntax) is VariableDeclaratorSyntax variableDeclaratorSyntax
             && IsTracked(variableDeclaratorSyntax.Initializer?.Value, semanticModel));

        private static bool AllConstants(IEnumerable<ArgumentSyntax> arguments, SemanticModel semanticModel) =>
            arguments.All(a => a.Expression.IsConstant(semanticModel));

        private static bool IsConcatenationOfConstants(BinaryExpressionSyntax binaryExpression, SemanticModel semanticModel)
        {
            System.Diagnostics.Debug.Assert(binaryExpression.IsKind(SyntaxKind.AddExpression), "Binary expression should be of syntax kind add expression.");
            if ((semanticModel.GetTypeInfo(binaryExpression).Type is ITypeSymbol) && binaryExpression.Right.IsConstant(semanticModel))
            {
                var nestedLeft = binaryExpression.Left;
                var nestedBinary = nestedLeft as BinaryExpressionSyntax;
                while (nestedBinary != null)
                {
                    if (nestedBinary.Right.IsConstant(semanticModel)
                        && (nestedBinary.IsKind(SyntaxKind.AddExpression) || nestedBinary.IsConstant(semanticModel)))
                    {
                        nestedLeft = nestedBinary.Left;
                        nestedBinary = nestedLeft as BinaryExpressionSyntax;
                    }
                    else
                    {
                        return false;
                    }
                }
                return nestedLeft.IsConstant(semanticModel);
            }
            return false;
        }
    }
}