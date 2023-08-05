
using ApiGoldenstarServices.Data.DataAccess.Goldenstar;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiGoldenstarServices.Data.DataAccess 
{
    public class DABase
    {
        private SqlConfiguration _SqlConfiguration;
        protected EjecutorSql _EjecutorSql;

        #region " Constructores " 

        public DABase(SqlConfiguration sqlConfiguration) 
        {
            if (sqlConfiguration == null)
            {
                // Se instancia con el único propósito de poder usar
                // la función .GetType() y la propiedad .ConnectionString
                sqlConfiguration = new SqlConfiguration("");

                string _Message = 
                    "NO se puede obtener " + nameof(sqlConfiguration.ConnectionString) 
                    + " de un objeto Nulo de tipo '" + sqlConfiguration.GetType().Name + "'."
                    ;

                throw
                    new ArgumentNullException(nameof(sqlConfiguration), _Message);
            }

            this._EjecutorSql = new EjecutorSql(sqlConfiguration.ConnectionString);
            this._SqlConfiguration = sqlConfiguration;
        }
        public DABase(SqlConnection sqlConnection) 
        {
            //Type? xx = this.GetType(); 
            if (sqlConnection == null)
            {
                throw
                    new ArgumentNullException(
                        nameof(sqlConnection),
                        "Se requiere una conexión de Base de Datos para instanciar un objeto de Acceso a Datos de tipo " 
                        + this.GetType().Name + "." 
                        );
            }

            this._EjecutorSql = new EjecutorSql(sqlConnection);
            this._SqlConfiguration = new SqlConfiguration(sqlConnection.ConnectionString);
        }

        #endregion 

        protected SqlConnection DbConnectionBase()
        // Ésta Función es necesaria únicamente para poder invisibilizar (private) el objeto '_SqlConfiguration' 
        {
            return 
                new SqlConnection(this._SqlConfiguration.ConnectionString);
        }

        //protected void xx() {}

        protected DbType ToDbType(SqlDbType? sqlDbType) 
        {
            if (sqlDbType == null)
            {
                throw 
                    new ArgumentNullException(
                        nameof(sqlDbType), 
                        "NO especificó ningún tipo de columna para tabla de BD."
                    );
            }

            DbType? _DbType = null;

            switch (sqlDbType) 
            {
                //case SqlDbType.VarChar: _DbType = DbType.AnsiString; break;
                case SqlDbType.VarChar: _DbType = DbType.AnsiStringFixedLength; break;

                //
                case SqlDbType.TinyInt: _DbType = DbType.Byte; break;
                case SqlDbType.SmallInt: _DbType = DbType.Int16; break;
                case SqlDbType.Int: _DbType = DbType.Int32; break;
                case SqlDbType.BigInt: _DbType = DbType.Int64; break;

                //
                case SqlDbType.Real: _DbType = DbType.Single; break;
                case SqlDbType.Float: _DbType = DbType.Double; break;

                //
                case SqlDbType.Decimal: _DbType = DbType.Decimal; break;

                //
                case SqlDbType.SmallMoney: 
                case SqlDbType.Money: _DbType = DbType.Currency; break;

                //
                case SqlDbType.SmallDateTime: 
                case SqlDbType.DateTime: _DbType = DbType.DateTime; break;

            }

            if (_DbType == null)
            {
                throw 
                    new Exception(
                        "Falta asignar la conversión .ToDbType() correspondiente al tipo "
                        + sqlDbType.GetType().Name + "."
                        + sqlDbType.ToString() 
                    );
            }


            return
                //((DbType) sqlDbType); 
                //((DbType) _DbType); 
                ((DbType) _DbType); 
        }

        //
    }

    //
}
