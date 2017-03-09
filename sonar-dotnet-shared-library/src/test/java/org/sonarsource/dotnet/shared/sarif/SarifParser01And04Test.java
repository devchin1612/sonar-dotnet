/*
 * SonarSource :: .NET :: Shared library
 * Copyright (C) 2016-2017 SonarSource SA
 * mailto:info AT sonarsource DOT com
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
package org.sonarsource.dotnet.shared.sarif;

import com.google.gson.JsonObject;
import com.google.gson.JsonParser;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.util.ArrayList;
import java.util.Collection;
import java.util.Collections;

import org.junit.Rule;
import org.junit.Test;
import org.junit.rules.ExpectedException;
import org.mockito.InOrder;
import org.mockito.Mockito;
import org.sonar.api.utils.log.LogTester;

import static org.assertj.core.api.Assertions.assertThat;
import static org.mockito.Matchers.eq;
import static org.mockito.Mockito.*;

public class SarifParser01And04Test {

  @Rule
  public LogTester logTester = new LogTester();

  @Rule
  public ExpectedException thrown = ExpectedException.none();

  private JsonObject getRoot(String fileName) throws IOException {
    return new JsonParser().parse(new String(Files.readAllBytes(Paths.get("src/test/resources/SarifParserTest/" + fileName)), StandardCharsets.UTF_8)).getAsJsonObject();
  }

  @Test
  public void should_not_fail_ony_empty_report() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser01And04(getRoot("v0_1_empty_issues.json")).parse(callback);
    new SarifParser01And04(getRoot("v0_1_empty_no_issues.json")).parse(callback);
    new SarifParser01And04(getRoot("v0_4_empty_no_results.json")).parse(callback);
    new SarifParser01And04(getRoot("v0_4_empty_no_runLogs.json")).parse(callback);
    new SarifParser01And04(getRoot("v0_4_empty_results.json")).parse(callback);
    new SarifParser01And04(getRoot("v0_4_empty_runLogs.json")).parse(callback);
    verify(callback, Mockito.never()).onIssue(Mockito.anyString(), Mockito.any(Location.class), Mockito.anyCollectionOf(Location.class));
  }

  // VS 2015 Update 1
  @Test
  public void sarif_version_0_1() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);

    new SarifParser01And04(getRoot("v0_1.json")).parse(callback);

    InOrder inOrder = inOrder(callback);


    Location location = new Location( "C:\\Foo.cs", "Remove this unused method parameter \"args\".", 43, 55, 44, 57);
    inOrder.verify(callback).onIssue("S1172", location, Collections.emptyList());
    location = new Location("C:\\Bar.cs", "There is just a full message.", 2, 2, 4, 4);
    inOrder.verify(callback).onIssue("CA1000", location, Collections.emptyList());
    verify(callback, Mockito.times(2)).onIssue(Mockito.anyString(), Mockito.any(Location.class), Mockito.anyCollectionOf(Location.class));

    inOrder.verify(callback).onProjectIssue("AssemblyLevelRule", "This is an assembly level Roslyn issue with no location.");
    inOrder.verify(callback).onProjectIssue("NoAnalysisTargetsLocation", "No analysis targets, report at assembly level.");
    verify(callback, Mockito.times(2)).onProjectIssue(Mockito.anyString(), Mockito.anyString());
  }

  // VS 2015 Update 2
  @Test
  public void sarif_version_0_4() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser01And04(getRoot("v0_4.json")).parse(callback);

    InOrder inOrder = inOrder(callback);

    Location location = new Location("C:\\Foo`1.cs", "Remove this commented out code.", 58, 12, 58, 50);
    inOrder.verify(callback).onIssue("S125", location, Collections.emptyList());
    verify(callback, Mockito.only()).onIssue(Mockito.anyString(), Mockito.any(Location.class), Mockito.anyCollectionOf(Location.class));

    verify(callback, Mockito.never()).onProjectIssue(Mockito.anyString(), Mockito.anyString());
  }

  @Test
  public void sarif_version_0_4_file_level() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser01And04(getRoot("v0_4_file_level_issue.json")).parse(callback);

    InOrder inOrder = inOrder(callback);
    inOrder.verify(callback).onFileIssue(eq("S104"), Mockito.anyString(), eq("Dummy"));
    inOrder.verify(callback).onFileIssue(eq("S105"), Mockito.anyString(), eq("Dummy"));
    Location location = new Location("C:\\Program.cs", "Dummy", 1, 0, 1, 1);
    inOrder.verify(callback).onIssue("S105", location, Collections.emptyList());

    inOrder.verify(callback).onFileIssue(eq("S106"), Mockito.anyString(), eq("Dummy"));

    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_0_4_secondary_locations() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser01And04(getRoot("v0_4_secondary_locations.json")).parse(callback);

    InOrder inOrder = inOrder(callback);
    Location primaryLocation = new Location("c:\\primary.cs", "Refactor this method to reduce its Cognitive Complexity from 30 to the 15 allowed",
            54, 21, 54, 24);
    Collection<Location> secondaryLocations = new ArrayList<>();
    secondaryLocations.add(new Location("c:\\secondary1.cs", "+1", 56, 12, 56, 14));
    secondaryLocations.add(new Location("c:\\secondary2.cs", "+2 (incl 1 for nesting)", 65, 16, 65, 18));
    inOrder.verify(callback).onIssue("S3776", primaryLocation, secondaryLocations);
    verifyNoMoreInteractions(callback);
  }

  @Test
  public void sarif_version_0_4_secondary_locations_no_messages() throws IOException {
    SarifParserCallback callback = mock(SarifParserCallback.class);
    new SarifParser01And04(getRoot("v0_4_secondary_locations_no_messages.json")).parse(callback);

    InOrder inOrder = inOrder(callback);
    Location primaryLocation = new Location("c:\\primary.cs", "Refactor this method to reduce its Cognitive Complexity from 30 to the 15 allowed",
            54, 21, 54, 24);
    Collection<Location> secondaryLocations = new ArrayList<>();
    secondaryLocations.add(new Location("c:\\secondary1.cs", null, 56, 12, 56, 14));
    secondaryLocations.add(new Location("c:\\secondary2.cs", null, 65, 16, 65, 18));
    inOrder.verify(callback).onIssue("S3776", primaryLocation, secondaryLocations);
    verifyNoMoreInteractions(callback);
  }
}
