﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class UseUnixEpochCodeFix : UseUnixEpochCodeFixBase<SyntaxKind>
    {
        protected override SyntaxNode ReplaceConstructorWithField(SyntaxNode root, SyntaxNode node, SonarCodeFixContext context)
        {
            var leadingTrivia = node.GetLeadingTrivia();
            var trailingTrivia = node.GetTrailingTrivia();
            return root.ReplaceNode(node,
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ((ObjectCreationExpressionSyntax)node).Type,
                    SyntaxFactory.Token(SyntaxKind.DotToken),
                    SyntaxFactory.IdentifierName("UnixEpoch")).WithLeadingTrivia(leadingTrivia).WithTrailingTrivia(trailingTrivia));
        }
    }
}
