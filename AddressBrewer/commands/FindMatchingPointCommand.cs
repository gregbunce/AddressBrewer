using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddressBrewer.commands
{
    public class FindMatchingPointCommand
    {
        private readonly SqlDataReader _sqlDataReader;

        public FindMatchingPointCommand(SqlDataReader sqlDataReader)
        {
            _sqlDataReader = sqlDataReader;
        }

        public Dictionary<string, object> Execute()
        {

            return Enumerable.Range(0, _sqlDataReader.FieldCount)
                .ToDictionary(_sqlDataReader.GetName, _sqlDataReader.GetValue);
        }

    }
}
