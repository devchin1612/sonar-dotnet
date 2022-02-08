﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class RedundantConditionalAroundAssignmentCodeFix : SonarCodeFix
    {
        private const string Title = "Remove redundant conditional";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(RedundantConditionalAroundAssignment.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, CodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var condition = root.FindNode(diagnosticSpan) as ExpressionSyntax;
            var ifStatement = condition?.FirstAncestorOrSelf<IfStatementSyntax>();

            if (ifStatement != null)
            {
                return HandleIfStatement(root, context, ifStatement);
            }

            var switchExpression = root.FindNode(diagnosticSpan).FirstAncestorOrSelf<SyntaxNode>(x => x.IsKind(SyntaxKindEx.SwitchExpression));
            if (switchExpression != null)
            {
                return HandleSwitchExpression(root, context, switchExpression);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        private static Task HandleIfStatement(SyntaxNode root, CodeFixContext context, IfStatementSyntax ifStatement)
        {
            var statement = ifStatement.Statement;
            if (statement is BlockSyntax block)
            {
                statement = block.Statements.FirstOrDefault();
            }

            if (statement == null)
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var newRoot = root.ReplaceNode(
                            ifStatement,
                            statement.WithTriviaFrom(ifStatement));
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);

            return Task.CompletedTask;
        }

        private static Task HandleSwitchExpression(SyntaxNode root, CodeFixContext context, SyntaxNode switchExpression)
        {
            var switchArm = ((SwitchExpressionSyntaxWrapper)switchExpression).Arms.FirstOrDefault();
            if (switchArm.SyntaxNode == null || switchArm.SyntaxNode.Parent.ChildNodes().Count(x => x.IsKind(SyntaxKindEx.SwitchExpressionArm)) != 1)
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    Title,
                    c =>
                    {
                        var newRoot = root.ReplaceNode(
                            switchExpression,
                            switchArm.Expression.WithTriviaFrom(switchExpression));
                        return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                    }),
                context.Diagnostics);

            return Task.CompletedTask;
        }
    }
}