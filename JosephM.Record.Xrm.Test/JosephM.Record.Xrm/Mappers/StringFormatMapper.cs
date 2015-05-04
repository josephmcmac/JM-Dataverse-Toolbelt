#region

using Microsoft.Xrm.Sdk.Metadata;
using JosephM.ObjectMapping;
using JosephM.Record.Metadata;

#endregion

namespace JosephM.Record.Xrm.Mappers
{
    public class StringFormatMapper : EnumMapper<StringFormat, TextFormat>
    {
        protected override TextFormat DefaultEnum2Option
        {
            get { return TextFormat.Text; }
        }
    }
}