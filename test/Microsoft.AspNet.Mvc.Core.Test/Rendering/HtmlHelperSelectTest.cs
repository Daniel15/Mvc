// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.AspNet.HtmlContent;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Testing;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;
using Microsoft.Framework.WebEncoders.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Rendering
{
    public class HtmlHelperSelectTest
    {
        private static readonly SelectListGroup GroupOne = new SelectListGroup { Name = "Group One", };
        private static readonly SelectListGroup GroupTwo = new SelectListGroup { Name = "Group Two", };
        private static readonly SelectListGroup DisabledGroup = new SelectListGroup
        {
            Disabled = true,
            Name = "Disabled Group",
        };

        private static readonly List<SelectListItem> BasicSelectList = new List<SelectListItem>
        {
            new SelectListItem { Text = "Zero",  Value = "0"},
            new SelectListItem { Text = "One",   Value = "1"},
            new SelectListItem { Text = "Two",   Value = "2"},
            new SelectListItem { Text = "Three", Value = "3"},
        };
        private static readonly List<SelectListItem> SomeDisabledOneSelectedSelectList = new List<SelectListItem>
        {
            new SelectListItem { Disabled = false, Selected = false, Text = "Zero",  Value = "0"},
            new SelectListItem { Disabled = true,  Selected = true,  Text = "One",   Value = "1"},
            new SelectListItem { Disabled = false, Selected = false, Text = "Two",  Value = "2"},
            new SelectListItem { Disabled = true,  Selected = false, Text = "Three", Value = "3"},
        };
        private static readonly List<SelectListItem> SomeGroupedSomeSelectedSelectList = new List<SelectListItem>
        {
            new SelectListItem { Group = GroupOne, Selected = true,  Text = "Zero",  Value = "0"},
            new SelectListItem { Group = GroupTwo, Selected = false, Text = "One",   Value = "1"},
            new SelectListItem { Group = GroupOne, Selected = true,  Text = "Two",   Value = "2"},
            new SelectListItem { Group = null,     Selected = false, Text = "Three", Value = "3"},
        };
        private static readonly List<SelectListItem> OneGroupSomeSelectedSelectList = new List<SelectListItem>
        {
            new SelectListItem { Group = GroupOne, Selected = true,  Text = "Zero",  Value = "0"},
            new SelectListItem { Group = GroupOne, Selected = true,  Text = "One",   Value = "1"},
            new SelectListItem { Group = GroupOne, Selected = false, Text = "Two",   Value = "2"},
            new SelectListItem { Group = GroupOne, Selected = false, Text = "Three", Value = "3"},
        };
        private static readonly List<SelectListItem> OneDisabledGroupAllSelectedSelectList = new List<SelectListItem>
        {
            new SelectListItem { Group = DisabledGroup, Selected = true, Text = "Zero",  Value = "0"},
            new SelectListItem { Group = DisabledGroup, Selected = true, Text = "One",   Value = "1"},
            new SelectListItem { Group = DisabledGroup, Selected = true, Text = "Two",   Value = "2"},
            new SelectListItem { Group = DisabledGroup, Selected = true, Text = "Three", Value = "3"},
        };

        // Select list -> expected HTML with null model, expected HTML with model containing "2".
        public static TheoryData<IEnumerable<SelectListItem>, string, string> DropDownListDataSet
        {
            get
            {
                return new TheoryData<IEnumerable<SelectListItem>, string, string>
                {
                    {
                        BasicSelectList,
                        "<select id=\"HtmlEncode[[Property1]]\" name=\"HtmlEncode[[Property1]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                        Environment.NewLine +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" name=\"HtmlEncode[[Property1]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                        Environment.NewLine +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>"
                    },
                    {
                        SomeDisabledOneSelectedSelectList,
                        "<select id=\"HtmlEncode[[Property1]]\" name=\"HtmlEncode[[Property1]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                        Environment.NewLine +
                        "<option disabled=\"HtmlEncode[[disabled]]\" selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" +
                        Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" name=\"HtmlEncode[[Property1]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                        Environment.NewLine +
                        "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>"
                    },
                    {
                        SomeGroupedSomeSelectedSelectList,
                        "<select id=\"HtmlEncode[[Property1]]\" name=\"HtmlEncode[[Property1]]\"><optgroup label=\"HtmlEncode[[Group One]]\">" +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "<optgroup label=\"HtmlEncode[[Group Two]]\">" +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" name=\"HtmlEncode[[Property1]]\"><optgroup label=\"HtmlEncode[[Group One]]\">" +
                        "<option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "<optgroup label=\"HtmlEncode[[Group Two]]\">" +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>"
                    },
                    {
                        OneGroupSomeSelectedSelectList,
                        "<select id=\"HtmlEncode[[Property1]]\" name=\"HtmlEncode[[Property1]]\"><optgroup label=\"HtmlEncode[[Group One]]\">" +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" name=\"HtmlEncode[[Property1]]\"><optgroup label=\"HtmlEncode[[Group One]]\">" +
                        "<option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "</select>"
                    },
                    {
                        OneDisabledGroupAllSelectedSelectList,
                        "<select id=\"HtmlEncode[[Property1]]\" name=\"HtmlEncode[[Property1]]\"><optgroup disabled=\"HtmlEncode[[disabled]]\" label=\"HtmlEncode[[Disabled Group]]\">" +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" name=\"HtmlEncode[[Property1]]\"><optgroup disabled=\"HtmlEncode[[disabled]]\" label=\"HtmlEncode[[Disabled Group]]\">" +
                        "<option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "</select>"
                    },
                };
            }
        }

        // Select list -> expected HTML with null model, with model containing "2", and with model containing "1", "3".
        public static TheoryData<IEnumerable<SelectListItem>, string, string, string> ListBoxDataSet
        {
            get
            {
                return new TheoryData<IEnumerable<SelectListItem>, string, string, string>
                {
                    {
                        BasicSelectList,
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                        Environment.NewLine +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                        Environment.NewLine +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                        Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>"
                    },
                    {
                        SomeDisabledOneSelectedSelectList,
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                        Environment.NewLine +
                        "<option disabled=\"HtmlEncode[[disabled]]\" selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" +
                        Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                        Environment.NewLine +
                        "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                        Environment.NewLine +
                        "<option disabled=\"HtmlEncode[[disabled]]\" selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option disabled=\"HtmlEncode[[disabled]]\" selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>"
                    },
                    {
                        SomeGroupedSomeSelectedSelectList,
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><optgroup label=\"HtmlEncode[[Group One]]\">" +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "<optgroup label=\"HtmlEncode[[Group Two]]\">" +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><optgroup label=\"HtmlEncode[[Group One]]\">" +
                        "<option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "<optgroup label=\"HtmlEncode[[Group Two]]\">" +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><optgroup label=\"HtmlEncode[[Group One]]\">" +
                        "<option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "<optgroup label=\"HtmlEncode[[Group Two]]\">" +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</select>"
                    },
                    {
                        OneGroupSomeSelectedSelectList,
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><optgroup label=\"HtmlEncode[[Group One]]\">" +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><optgroup label=\"HtmlEncode[[Group One]]\">" +
                        "<option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\"><optgroup label=\"HtmlEncode[[Group One]]\">" +
                        "<option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "</select>"
                    },
                    {
                        OneDisabledGroupAllSelectedSelectList,
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\">" +
                        "<optgroup disabled=\"HtmlEncode[[disabled]]\" label=\"HtmlEncode[[Disabled Group]]\">" +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\">" +
                        "<optgroup disabled=\"HtmlEncode[[disabled]]\" label=\"HtmlEncode[[Disabled Group]]\">" +
                        "<option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "</select>",
                        "<select id=\"HtmlEncode[[Property1]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[Property1]]\">" +
                        "<optgroup disabled=\"HtmlEncode[[disabled]]\" label=\"HtmlEncode[[Disabled Group]]\">" +
                        "<option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                        "<option value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                        "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                        "</optgroup>" + Environment.NewLine +
                        "</select>"
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownList_WithNullModel_GeneratesExpectedValue_DoesNotChangeSelectList(
            IEnumerable<SelectListItem> selectList,
            string expectedHtml,
            string ignoredHtml)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            var savedDisabled = selectList.Select(item => item.Disabled).ToList();
            var savedGroup = selectList.Select(item => item.Group).ToList();
            var savedSelected = selectList.Select(item => item.Selected).ToList();
            var savedText = selectList.Select(item => item.Text).ToList();
            var savedValue = selectList.Select(item => item.Value).ToList();

            // Act
            var html = helper.DropDownList("Property1", selectList, optionLabel: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedDisabled, selectList.Select(item => item.Disabled));
            Assert.Equal(savedGroup, selectList.Select(item => item.Group));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
            Assert.Equal(savedText, selectList.Select(item => item.Text));
            Assert.Equal(savedValue, selectList.Select(item => item.Value));
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownList_WithNullSelectList_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string expectedHtml,
            string ignoredHtml)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            helper.ViewData["Property1"] = selectList;
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.DropDownList("Property1", selectList: null, optionLabel: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownList_WithModelValue_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml,
            string expectedHtml)
        {
            // Arrange
            var model = new DefaultTemplatesUtilities.ObjectTemplateModel { Property1 = "2" };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.DropDownList("Property1", selectList, optionLabel: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownListFor_WithNullModel_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string expectedHtml,
            string ignoredHtml)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.DropDownListFor(
                value => value.Property1,
                selectList,
                optionLabel: null,
                htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownListFor_WithNullModelAndNullSelectList_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string expectedHtml,
            string ignoredHtml)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            helper.ViewData["Property1"] = selectList;
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.DropDownListFor(
                value => value.Property1,
                selectList: null,
                optionLabel: null,
                htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownListFor_WithModelValue_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml,
            string expectedHtml)
        {
            // Arrange
            var model = new DefaultTemplatesUtilities.ObjectTemplateModel { Property1 = "2" };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.DropDownListFor(
                value => value.Property1,
                selectList,
                optionLabel: null,
                htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownListFor_WithModelValueAndNullSelectList_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml,
            string expectedHtml)
        {
            // Arrange
            var model = new DefaultTemplatesUtilities.ObjectTemplateModel { Property1 = "2" };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);
            helper.ViewData["Property1"] = selectList;
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.DropDownListFor(
                value => value.Property1,
                selectList: null,
                optionLabel: null,
                htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Fact]
        public void DropDownListFor_WithIndexerExpression_GeneratesExpectedValue()
        {
            // Arrange
            var model = new ModelContainingList { Property1 = { "0", "1", "2" } };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);
            var selectList = SomeDisabledOneSelectedSelectList;
            var savedSelected = selectList.Select(item => item.Selected).ToList();
            var expectedHtml =
                "<select id=\"HtmlEncode[[Property1_2_]]\" name=\"HtmlEncode[[Property1[2]]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                Environment.NewLine +
                "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                "</select>";

            // Act
            var html = helper.DropDownListFor(
                value => value.Property1[2],
                selectList,
                optionLabel: null,
                htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Fact]
        public void DropDownListFor_WithUnrelatedExpression_GeneratesExpectedValue()
        {
            // Arrange
            var unrelated = "2";
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            var selectList = SomeDisabledOneSelectedSelectList;
            var savedSelected = selectList.Select(item => item.Selected).ToList();
            var expectedHtml =
                "<select id=\"HtmlEncode[[unrelated]]\" name=\"HtmlEncode[[unrelated]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" + Environment.NewLine +
                "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                "</select>";

            // Act
            var html = helper.DropDownListFor(value => unrelated, selectList, optionLabel: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(ListBoxDataSet))]
        public void ListBox_WithNullModel_GeneratesExpectedValue_DoesNotChangeSelectList(
            IEnumerable<SelectListItem> selectList,
            string expectedHtml,
            string ignoredHtml1,
            string ignoredHtml2)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper<ModelContainingList>(model: null);
            var savedDisabled = selectList.Select(item => item.Disabled).ToList();
            var savedGroup = selectList.Select(item => item.Group).ToList();
            var savedSelected = selectList.Select(item => item.Selected).ToList();
            var savedText = selectList.Select(item => item.Text).ToList();
            var savedValue = selectList.Select(item => item.Value).ToList();

            // Act
            var html = helper.ListBox("Property1", selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedDisabled, selectList.Select(item => item.Disabled));
            Assert.Equal(savedGroup, selectList.Select(item => item.Group));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
            Assert.Equal(savedText, selectList.Select(item => item.Text));
            Assert.Equal(savedValue, selectList.Select(item => item.Value));
        }

        [Theory]
        [MemberData(nameof(ListBoxDataSet))]
        public void ListBox_WithModelValue_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml1,
            string expectedHtml,
            string ignoredHtml2)
        {
            // Arrange
            var model = new ModelContainingList { Property1 = { "2" } };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.ListBox("Property1", selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(ListBoxDataSet))]
        public void ListBox_WithMultipleModelValues_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml1,
            string ignoredHtml2,
            string expectedHtml)
        {
            // Arrange
            var model = new ModelContainingList { Property1 = { "1", "3" } };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.ListBox("Property1", selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(ListBoxDataSet))]
        public void ListBoxFor_WithNullModel_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string expectedHtml,
            string ignoredHtml1,
            string ignoredHtml2)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper<ModelContainingList>(model: null);
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.ListBoxFor(value => value.Property1, selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(ListBoxDataSet))]
        public void ListBoxFor_WithModelValue_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml1,
            string expectedHtml,
            string ignoredHtml2)
        {
            // Arrange
            var model = new ModelContainingList { Property1 = { "2" } };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.ListBoxFor(value => value.Property1, selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Fact]
        public void ListBoxFor_WithUnreleatedExpression_GeneratesExpectedValue()
        {
            // Arrange
            var unrelated = new[] { "2" };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper();
            var selectList = SomeDisabledOneSelectedSelectList;
            var savedSelected = selectList.Select(item => item.Selected).ToList();
            var expectedHtml =
                "<select id=\"HtmlEncode[[unrelated]]\" multiple=\"HtmlEncode[[multiple]]\" name=\"HtmlEncode[[unrelated]]\"><option value=\"HtmlEncode[[0]]\">HtmlEncode[[Zero]]</option>" +
                Environment.NewLine +
                "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[1]]\">HtmlEncode[[One]]</option>" + Environment.NewLine +
                "<option selected=\"HtmlEncode[[selected]]\" value=\"HtmlEncode[[2]]\">HtmlEncode[[Two]]</option>" + Environment.NewLine +
                "<option disabled=\"HtmlEncode[[disabled]]\" value=\"HtmlEncode[[3]]\">HtmlEncode[[Three]]</option>" + Environment.NewLine +
                "</select>";

            // Act
            var html = helper.ListBoxFor(value => unrelated, selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(ListBoxDataSet))]
        public void ListBoxFor_WithMultipleModelValues_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml1,
            string ignoredHtml2,
            string expectedHtml)
        {
            // Arrange
            var model = new ModelContainingList { Property1 = { "1", "3" } };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.ListBoxFor(value => value.Property1, selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownListInTemplate_WithNullModel_GeneratesExpectedValue_DoesNotChangeSelectList(
            IEnumerable<SelectListItem> selectList,
            string expectedHtml,
            string ignoredHtml)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper<string>(model: null);
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Property1";
            var savedDisabled = selectList.Select(item => item.Disabled).ToList();
            var savedGroup = selectList.Select(item => item.Group).ToList();
            var savedSelected = selectList.Select(item => item.Selected).ToList();
            var savedText = selectList.Select(item => item.Text).ToList();
            var savedValue = selectList.Select(item => item.Value).ToList();

            // Act
            var html = helper.DropDownList(
                expression: string.Empty,
                selectList: selectList,
                optionLabel: null,
                htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedDisabled, selectList.Select(item => item.Disabled));
            Assert.Equal(savedGroup, selectList.Select(item => item.Group));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
            Assert.Equal(savedText, selectList.Select(item => item.Text));
            Assert.Equal(savedValue, selectList.Select(item => item.Value));
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownListInTemplate_WithModelValue_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml,
            string expectedHtml)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper("2");
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Property1";
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.DropDownList(
                expression: string.Empty,
                selectList: selectList,
                optionLabel: null,
                htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownListForInTemplate_WithNullModel_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string expectedHtml,
            string ignoredHtml)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper<string>(model: null);
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Property1";
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.DropDownListFor(value => value, selectList, optionLabel: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(DropDownListDataSet))]
        public void DropDownListForInTemplate_WithModelValue_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml,
            string expectedHtml)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper("2");
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Property1";
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.DropDownListFor(value => value, selectList, optionLabel: null, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(ListBoxDataSet))]
        public void ListBoxInTemplate_WithNullModel_GeneratesExpectedValue_DoesNotChangeSelectList(
            IEnumerable<SelectListItem> selectList,
            string expectedHtml,
            string ignoredHtml1,
            string ignoredHtml2)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper<List<string>>(model: null);
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Property1";
            var savedDisabled = selectList.Select(item => item.Disabled).ToList();
            var savedGroup = selectList.Select(item => item.Group).ToList();
            var savedSelected = selectList.Select(item => item.Selected).ToList();
            var savedText = selectList.Select(item => item.Text).ToList();
            var savedValue = selectList.Select(item => item.Value).ToList();

            // Act
            var html = helper.ListBox(expression: string.Empty, selectList: selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedDisabled, selectList.Select(item => item.Disabled));
            Assert.Equal(savedGroup, selectList.Select(item => item.Group));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
            Assert.Equal(savedText, selectList.Select(item => item.Text));
            Assert.Equal(savedValue, selectList.Select(item => item.Value));
        }

        [Theory]
        [MemberData(nameof(ListBoxDataSet))]
        public void ListBoxInTemplate_WithModelValue_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml1,
            string expectedHtml,
            string ignoredHtml2)
        {
            // Arrange
            var model = new List<string> { "2" };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Property1";
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.ListBox(expression: string.Empty, selectList: selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(ListBoxDataSet))]
        public void ListBoxForInTemplate_WithNullModel_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string expectedHtml,
            string ignoredHtml1,
            string ignoredHtml2)
        {
            // Arrange
            var helper = DefaultTemplatesUtilities.GetHtmlHelper<List<string>>(model: null);
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Property1";
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.ListBoxFor(value => value, selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Theory]
        [MemberData(nameof(ListBoxDataSet))]
        public void ListBoxForInTemplate_WithModelValue_GeneratesExpectedValue(
            IEnumerable<SelectListItem> selectList,
            string ignoredHtml1,
            string expectedHtml,
            string ignoredHtml2)
        {
            // Arrange
            var model = new List<string> { "2" };
            var helper = DefaultTemplatesUtilities.GetHtmlHelper(model);
            helper.ViewData.TemplateInfo.HtmlFieldPrefix = "Property1";
            var savedSelected = selectList.Select(item => item.Selected).ToList();

            // Act
            var html = helper.ListBoxFor(value => value, selectList, htmlAttributes: null);

            // Assert
            Assert.Equal(expectedHtml, DefaultTemplatesUtilities.HtmlContentToString(html));
            Assert.Equal(savedSelected, selectList.Select(item => item.Selected));
        }

        [Fact]
        public void GetEnumSelectListTEnum_ThrowsWithFlagsEnum()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var htmlHelper = new TestHtmlHelper(metadataProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => htmlHelper.GetEnumSelectList<EnumWithFlags>(),
                "TEnum",
                $"The type '{ typeof(EnumWithFlags).FullName }' is not supported.");
        }

        [Fact]
        public void GetEnumSelectListTEnum_ThrowsWithNonEnum()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var htmlHelper = new TestHtmlHelper(metadataProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => htmlHelper.GetEnumSelectList<StructWithFields>(),
                "TEnum",
                $"The type '{ typeof(StructWithFields).FullName }' is not supported.");
        }

        [Fact]
        public void GetEnumSelectListTEnum_WrapsGetEnumSelectListModelMetadata()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForType(typeof(EnumWithFields));
            var htmlHelper = new TestHtmlHelper(metadataProvider);

            // Act
            var result = htmlHelper.GetEnumSelectList<EnumWithFields>();

            // Assert
            Assert.Equal(metadata.ModelType, htmlHelper.Metadata.ModelType);

            Assert.Same(htmlHelper.SelectListItems, result);            // No replacement of the underlying List
            VerifySelectList(htmlHelper.CopiedSelectListItems, result); // No change to the (mutable) items
        }

        [Fact]
        public void GetEnumSelectListType_ThrowsWithFlagsEnum()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var htmlHelper = new TestHtmlHelper(metadataProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => htmlHelper.GetEnumSelectList(typeof(EnumWithFlags)),
                "enumType",
                $"The type '{ typeof(EnumWithFlags).FullName }' is not supported.");
        }

        [Fact]
        public void GetEnumSelectListType_ThrowsWithNonEnum()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var htmlHelper = new TestHtmlHelper(metadataProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => htmlHelper.GetEnumSelectList(typeof(StructWithFields)),
                "enumType",
                $"The type '{ typeof(StructWithFields).FullName }' is not supported.");
        }

        [Fact]
        public void GetEnumSelectListType_ThrowsWithNonStruct()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var htmlHelper = new TestHtmlHelper(metadataProvider);

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => htmlHelper.GetEnumSelectList(typeof(ClassWithFields)),
                "enumType",
                $"The type '{ typeof(ClassWithFields).FullName }' is not supported.");
        }

        [Fact]
        public void GetEnumSelectListType_WrapsGetEnumSelectListModelMetadata()
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForType(typeof(EnumWithFields));
            var htmlHelper = new TestHtmlHelper(metadataProvider);

            // Act
            var result = htmlHelper.GetEnumSelectList(typeof(EnumWithFields));

            // Assert
            Assert.Equal(metadata.ModelType, htmlHelper.Metadata.ModelType);

            Assert.Same(htmlHelper.SelectListItems, result);            // No replacement of the underlying List
            VerifySelectList(htmlHelper.CopiedSelectListItems, result); // No change to the (mutable) items
        }

        public static TheoryData<Type, IEnumerable<SelectListItem>> GetEnumSelectListData
        {
            get
            {
                return new TheoryData<Type, IEnumerable<SelectListItem>>
                {
                    { typeof(EmptyEnum), Enumerable.Empty<SelectListItem>() },
                    { typeof(EmptyEnum?), Enumerable.Empty<SelectListItem>() },
                    {
                        typeof(EnumWithDisplayNames),
                        new List<SelectListItem>
                        {
                            new SelectListItem { Text = "cero", Value = "0" },
                            new SelectListItem { Text = nameof(EnumWithDisplayNames.One), Value = "1" },
                            new SelectListItem { Text = "dos", Value = "2" },
                            new SelectListItem { Text = "tres", Value = "3" },
                            new SelectListItem { Text = "name from resources", Value = "-2" },
                            new SelectListItem { Text = "menos uno", Value = "-1" },
                        }
                    },
                    {
                        typeof(EnumWithDisplayNames?),
                        new List<SelectListItem>
                        {
                            new SelectListItem { Text = "cero", Value = "0" },
                            new SelectListItem { Text = nameof(EnumWithDisplayNames.One), Value = "1" },
                            new SelectListItem { Text = "dos", Value = "2" },
                            new SelectListItem { Text = "tres", Value = "3" },
                            new SelectListItem { Text = "name from resources", Value = "-2" },
                            new SelectListItem { Text = "menos uno", Value = "-1" },
                        }
                    },
                    {
                        typeof(EnumWithDuplicates),
                        new List<SelectListItem>
                        {
                            new SelectListItem { Text = nameof(EnumWithDuplicates.Zero), Value = "0" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.None), Value = "0" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.One), Value = "1" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.Duece), Value = "2" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.Two), Value = "2" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.MoreThanTwo), Value = "3" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.Three), Value = "3" },
                        }
                    },
                    {
                        typeof(EnumWithDuplicates?),
                        new List<SelectListItem>
                        {
                            new SelectListItem { Text = nameof(EnumWithDuplicates.Zero), Value = "0" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.None), Value = "0" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.One), Value = "1" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.Duece), Value = "2" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.Two), Value = "2" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.MoreThanTwo), Value = "3" },
                            new SelectListItem { Text = nameof(EnumWithDuplicates.Three), Value = "3" },
                        }
                    },
                    {
                        typeof(EnumWithFields),
                        new List<SelectListItem>
                        {
                            new SelectListItem { Text = nameof(EnumWithFields.Zero), Value = "0" },
                            new SelectListItem { Text = nameof(EnumWithFields.One), Value = "1" },
                            new SelectListItem { Text = nameof(EnumWithFields.Two), Value = "2" },
                            new SelectListItem { Text = nameof(EnumWithFields.Three), Value = "3" },
                            new SelectListItem { Text = nameof(EnumWithFields.MinusTwo), Value = "-2" },
                            new SelectListItem { Text = nameof(EnumWithFields.MinusOne), Value = "-1" },
                        }
                    },
                    {
                        typeof(EnumWithFields?),
                        new List<SelectListItem>
                        {
                            new SelectListItem { Text = nameof(EnumWithFields.Zero), Value = "0" },
                            new SelectListItem { Text = nameof(EnumWithFields.One), Value = "1" },
                            new SelectListItem { Text = nameof(EnumWithFields.Two), Value = "2" },
                            new SelectListItem { Text = nameof(EnumWithFields.Three), Value = "3" },
                            new SelectListItem { Text = nameof(EnumWithFields.MinusTwo), Value = "-2" },
                            new SelectListItem { Text = nameof(EnumWithFields.MinusOne), Value = "-1" },
                        }
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(GetEnumSelectListData))]
        public void GetEnumSelectList_ReturnsExpectedItems(Type type, IEnumerable<SelectListItem> expected)
        {
            // Arrange
            var metadataProvider = TestModelMetadataProvider.CreateDefaultProvider();
            var metadata = metadataProvider.GetMetadataForType(type);
            var htmlHelper = new TestHtmlHelper(metadataProvider);

            // Act
            var result = htmlHelper.GetEnumSelectList(type);

            // Assert
            VerifySelectList(expected, result);
        }

        // Confirm methods that wrap GetEnumSelectList(ModelMetadata) are not changing anything in returned collection.
        private void VerifySelectList(IEnumerable<SelectListItem> expected, IEnumerable<SelectListItem> actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Count(), actual.Count());
            for (var i = 0; i < actual.Count(); i++)
            {
                var expectedItem = expected.ElementAt(i);
                var actualItem = actual.ElementAt(i);

                Assert.False(actualItem.Disabled);
                Assert.Null(actualItem.Group);
                Assert.False(actualItem.Selected);
                Assert.Equal(expectedItem.Text, actualItem.Text);
                Assert.Equal(expectedItem.Value, actualItem.Value);
            }
        }

        private class TestHtmlHelper : HtmlHelper
        {
            public TestHtmlHelper([NotNull] IModelMetadataProvider metadataProvider)
                : base(
                      new Mock<IHtmlGenerator>(MockBehavior.Strict).Object,
                      new Mock<ICompositeViewEngine>(MockBehavior.Strict).Object,
                      metadataProvider,
                      new Mock<IHtmlEncoder>(MockBehavior.Strict).Object,
                      new Mock<IUrlEncoder>(MockBehavior.Strict).Object,
                      new Mock<IJavaScriptStringEncoder>(MockBehavior.Strict).Object)
            {
            }

            public ModelMetadata Metadata { get; private set; }

            public IEnumerable<SelectListItem> SelectListItems { get; private set; }

            public IEnumerable<SelectListItem> CopiedSelectListItems { get; private set; }

            protected override IEnumerable<SelectListItem> GetEnumSelectList([NotNull] ModelMetadata metadata)
            {
                Metadata = metadata;
                SelectListItems = base.GetEnumSelectList(metadata);
                if (SelectListItems != null)
                {
                    // Perform a deep copy to help confirm the mutable items are not changed.
                    var copiedSelectListItems = new List<SelectListItem>();
                    CopiedSelectListItems = copiedSelectListItems;
                    foreach (var item in SelectListItems)
                    {
                        var copy = new SelectListItem
                        {
                            Disabled = item.Disabled,
                            Group = item.Group,
                            Selected = item.Selected,
                            Text = item.Text,
                            Value = item.Value,
                        };

                        copiedSelectListItems.Add(copy);
                    }
                }

                return SelectListItems;
            }
        }

        private class ClassWithFields
        {
            public const int Zero = 0;

            public const int One = 1;
        }

        private enum EmptyEnum
        {
        }

        private enum EnumWithDisplayNames
        {
            [Display(Name = "tres")]
            Three = 3,

            [Display(Name = "dos")]
            Two = 2,

            // Display attribute exists but does not set Name.
            [Display(ShortName = "uno")]
            One = 1,

            [Display(Name = "cero")]
            Zero = 0,

            [Display(Name = "menos uno")]
            MinusOne = -1,

#if USE_REAL_RESOURCES
            [Display(Name = nameof(Test.Resources.DisplayAttribute_Name), ResourceType = typeof(Test.Resources))]
#else
            [Display(Name = nameof(TestResources.DisplayAttribute_Name), ResourceType = typeof(TestResources))]
#endif
            MinusTwo = -2,
        }

        private enum EnumWithDuplicates
        {
            Zero = 0,
            One = 1,
            Three = 3,
            MoreThanTwo = 3,
            Two = 2,
            None = 0,
            Duece = 2,
        }

        [Flags]
        private enum EnumWithFlags
        {
            Four = 4,
            Two = 2,
            One = 1,
            Zero = 0,
            All = -1,
        }

        private enum EnumWithFields
        {
            MinusTwo = -2,
            MinusOne = -1,
            Three = 3,
            Two = 2,
            One = 1,
            Zero = 0,
        }

        private struct StructWithFields
        {
            public const int Zero = 0;

            public const int One = 1;
        }

        private class ModelContainingList
        {
            public List<string> Property1 { get; } = new List<string>();
        }
    }
}