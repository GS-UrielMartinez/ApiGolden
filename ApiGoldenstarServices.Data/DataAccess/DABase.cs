
using ApiGoldenstarServices.Data.DataAccess.Goldenstar;
using Microsoft.Data.SqlClient;
using System;
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

        //Type? xx = this.GetType(); 

        #region " Constructores " 

        public DABase(SqlConfiguration sqlConfiguration) 
        {
            if (sqlConfiguration == null)
            {
                throw
                    new ArgumentNullException(
                        nameof(sqlConfiguration),
                        "NO se puede obtener ConnectionString de un objeto Nulo de tipo 'SqlConfiguration'."
                        );
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
                        "Se requiere una conexión de Base de Datos para instanciar un objeto de Acceso a Datos."
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

        //
    }

    //
}
