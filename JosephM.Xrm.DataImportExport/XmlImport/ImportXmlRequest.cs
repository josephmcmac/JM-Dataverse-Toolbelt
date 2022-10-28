﻿using JosephM.Core.Attributes;
using JosephM.Core.FieldType;
using JosephM.Core.Log;
using JosephM.Core.Service;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;

namespace JosephM.Xrm.DataImportExport.XmlExport
{
    [DisplayName("Import XML")]
    [Instruction("All XML Files In The Folder Will Be Imported Into The Target Instance. Matches To Update Records In The Target Will By Done By Either Primary Key, Then Name, Else If No Match Is Found A New Record Will Be Created")]
    [AllowSaveAndLoad]
    [Group(Sections.Main, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 10)]
    [Group(Sections.Misc, Group.DisplayLayoutEnum.HorizontalLabelAbove, order: 40)]
    public class ImportXmlRequest : ServiceRequestBase, IImportXmlRequest
    {
        public ImportXmlRequest()
        {
            MatchByName = true;
            ExecuteMultipleSetSize = 50;
            TargetCacheLimit = 1000;
        }

        [GridWidth(350)]
        [DisplayOrder(20)]
        [Group(Sections.Main)]
        [RequiredProperty]
        [DisplayName("Select The Folder Containing The XML Files")]
        public Folder Folder { get; set; }

        [GridWidth(110)]
        [Group(Sections.Misc)]
        [DisplayOrder(399)]
        [RequiredProperty]
        public bool IncludeOwner { get; set; }

        [GridWidth(115)]
        [DisplayOrder(215)]
        [Group(Sections.Misc)]
        [RequiredProperty]
        public bool MatchByName { get; set; }

        [GridWidth(120)]
        [Group(Sections.Misc)]
        [DisplayOrder(400)]
        [RequiredProperty]
        public bool MaskEmails { get; set; }

        [Group(Sections.Misc)]
        [DisplayOrder(410)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(1000)]
        public int? ExecuteMultipleSetSize { get; set; }

        [Group(Sections.Misc)]
        [DisplayOrder(420)]
        [RequiredProperty]
        [MinimumIntValue(1)]
        [MaximumIntValue(5000)]
        public int? TargetCacheLimit { get; set; }

        public void ClearLoadedEntities()
        {
            _loadedEntities = null;
        }

        private IDictionary<string, Entity> _loadedEntities;
        public IDictionary<string, Entity> GetOrLoadEntitiesForImport(LogController logController)
        {
            if (Folder == null)
                throw new NullReferenceException($"Cannot load files {nameof(Folder)} property is null");
            if(_loadedEntities == null)
            {
                _loadedEntities = ImportXmlService.LoadEntitiesFromXmlFiles(Folder.FolderPath, controller: logController);
            }
            return _loadedEntities;
        }

        private static class Sections
        {
            public const string Main = "Main";
            public const string Misc = "Misc";
         }
    }
}