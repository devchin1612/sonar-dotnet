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

using Microsoft.CodeAnalysis.CSharp;
using Moq;
using SonarAnalyzer.AnalysisContext;

namespace SonarAnalyzer.UnitTest.AnalysisContext;

[TestClass]
public class SonarCodeBlockStartAnalysisContextTest
{
    [TestMethod]
    public void Properties_ArePropagated()
    {
        var cancel = new CancellationToken(true);
        var codeBlock = SyntaxFactory.Block();
        var owningSymbol = Mock.Of<ISymbol>();
        var model = Mock.Of<SemanticModel>();
        var options = AnalysisScaffolding.CreateOptions();
        var context = new Mock<CodeBlockStartAnalysisContext<SyntaxKind>>(codeBlock, owningSymbol, model, options, cancel).Object;
        var sut = new SonarCodeBlockStartAnalysisContext<SyntaxKind>(AnalysisScaffolding.CreateSonarAnalysisContext(), context);

        sut.Tree.Should().Be(codeBlock.SyntaxTree);
        sut.Compilation.Should().Be(model.Compilation);
        sut.Options.Should().Be(options);
        sut.Cancel.Should().Be(cancel);
        sut.CodeBlock.Should().Be(codeBlock);
        sut.OwningSymbol.Should().Be(owningSymbol);
        sut.SemanticModel.Should().Be(model);
    }
}