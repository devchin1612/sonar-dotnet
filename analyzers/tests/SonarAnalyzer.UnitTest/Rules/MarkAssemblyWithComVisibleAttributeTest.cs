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

using CS = SonarAnalyzer.Rules.CSharp;
using VB = SonarAnalyzer.Rules.VisualBasic;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public class MarkAssemblyWithComVisibleAttributeTest
    {
        private readonly VerifierBuilder builderCS = new VerifierBuilder<CS.MarkAssemblyWithComVisibleAttribute>().WithConcurrentAnalysis(false);
        private readonly VerifierBuilder builderVB = new VerifierBuilder<VB.MarkAssemblyWithComVisibleAttribute>().WithConcurrentAnalysis(false);

        [TestMethod]
        public void MarkAssemblyWithComVisibleAttribute_CS() =>
            builderCS.AddPaths(@"MarkAssemblyWithComVisibleAttribute.cs").Verify();

        [TestMethod]
        public void MarkAssemblyWithComVisibleAttribute_VB() =>
            builderVB.AddPaths(@"MarkAssemblyWithComVisibleAttribute.vb").Verify();

        [TestMethod]
        public void MarkAssemblyWithComVisibleAttributeNoncompliant_CS()
        {
            var action = () => builderCS.AddPaths(@"MarkAssemblyWithComVisibleAttributeNoncompliant.cs").Verify();
            action.Should().Throw<UnexpectedDiagnosticException>().WithMessage("*Provide a 'ComVisible' attribute for assembly 'project0'.*");
        }

        [TestMethod]
        public void MarkAssemblyWithComVisibleAttributeNoncompliant_VB()
        {
            var action = () => builderVB.AddPaths(@"MarkAssemblyWithComVisibleAttributeNoncompliant.vb").Verify();
            action.Should().Throw<UnexpectedDiagnosticException>().WithMessage("*Provide a 'ComVisible' attribute for assembly 'project0'.*");
        }
    }
}
