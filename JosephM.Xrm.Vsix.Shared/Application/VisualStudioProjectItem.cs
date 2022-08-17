﻿using EnvDTE;
using System;
using System.IO;

namespace JosephM.Xrm.Vsix.Application
{
    public class VisualStudioProjectItem : IProjectItem
    {
        public ProjectItem ProjectItem { get; set; }

        public VisualStudioProjectItem(ProjectItem projectItem)
        {
            ProjectItem = projectItem;
        }

        public void SetProperty(string propertyName, object value)
        {
            foreach (Property prop in ProjectItem.Properties)
            {
                if (prop.Name == propertyName)
                {
                    prop.Value = value;
                    return;
                }
            }
            throw new NullReferenceException(string.Format("Could not find property {0}", propertyName));
        }

        public bool HasFile
        {
            get
            {
                return ProjectItem?.FileCount > 0;
            }
        }

        public string FileName
        {
            get
            {
                string fileName = null;
                if (HasFile)
                {
                    try
                    {
                        fileName = ProjectItem.FileNames[1];
                    }
                    catch (Exception) { }
                    if (fileName == null)
                    {
                        try
                        {
                            fileName = ProjectItem.FileNames[0];
                        }
                        catch (Exception) { }
                    }
                    if (fileName == null)
                    {
                        throw new Exception($"Could not extract file name for ProjectItem {Name}");
                    }
                }
                return fileName;
            }
        }

        public string FileFolder
        {
            get
            {
                return FileName == null ? null : new FileInfo(FileName).DirectoryName;
            }
        }

        public string Name
        {
            get
            {
                return ProjectItem.Name;
            }
        }

        public string NameOfContainingProject
        {
            get
            {
                return ProjectItem?.ContainingProject?.Name;
            }
        }
    }
}
