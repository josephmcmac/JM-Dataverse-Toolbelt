﻿using JosephM.Application.ViewModel.RecordEntry.Form;
using System.Collections.Generic;

namespace JosephM.Application.ViewModel.RecordEntry.Metadata
{
    public class FormSection
    {
        public FormSection(string sectionLabel, int order, IEnumerable<CustomFormFunction> customFunctions = null, bool displayLabel = true, bool isHiddenSection = false)
        {
            SectionLabel = sectionLabel;
            Order = order;
            CustomFunctions = customFunctions ?? new CustomFormFunction[0];
            DisplayLabel = displayLabel;
            IsHiddenSection = isHiddenSection;
        }

        public string SectionLabel { get; private set; }

        public int Order { get; set; }

        public bool IsHiddenSection { get; set; }

        public IEnumerable<CustomFormFunction> CustomFunctions { get; private set; }
        public bool DisplayLabel { get; set; }
    }
}