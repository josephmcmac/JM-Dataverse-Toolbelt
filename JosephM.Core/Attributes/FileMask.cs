using JosephM.Core.FieldType;
using System;
using System.Linq;

namespace JosephM.Core.Attributes
{
    /// <summary>
    ///     Defines The Property With The Attribute In Context If The Property With A Given Name Has One Of The Given Values
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Property,
        AllowMultiple = false)]
    public class FileMask : PropertyValidator
    {
        public FileMask(string mask)
        {
            Mask = mask;
        }

        public string Mask { get; set; }

        public override string GetErrorMessage(string propertyLabel)
        {
            return $"{propertyLabel} Must Have The File Extention {GetValidExtention()}";
        }

        public override bool IsValid(object value)
        {
            var fileReference = value as FileReference;
            if(fileReference != null && fileReference.FileName != null && Mask != null)
            {
                var extention = GetValidExtention();
                if (!fileReference.FileName.EndsWith(extention))
                    return false;
            }
            return true;
        }

        private string GetValidExtention()
        {
            //e.g "Excel Files|*.xlsx";
            var split = Mask.Split('|');
            if (split.Count() != 2)
                throw new Exception($"Error validating the file mask. Expected mask with form 'Type|*.[extention]'. The actual mask is {Mask}");
            var splitAgain = split[1].Split('.');
            if (splitAgain.Count() != 2)
                throw new Exception($"Error validating the file mask. Expected mask with form 'Type|*.[extention]'. The actual mask is {Mask}");
            var extention = splitAgain[1];
            return extention;
        }
    }
}