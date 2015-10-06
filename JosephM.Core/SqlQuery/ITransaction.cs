#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using JosephM.Core.FieldType;

#endregion

namespace JosephM.Core.Sql
{
    public interface ITransaction : IDisposable
    {
        void Rollback();

        void Commit();
    }
}