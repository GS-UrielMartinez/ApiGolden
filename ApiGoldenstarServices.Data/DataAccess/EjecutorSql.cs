
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ApiGoldenstarServices.Models.Goldenstar;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ApiGoldenstarServices.Data.DataAccess
{
    public class EjecutorSql
        //para que mande llamar métodos de  _DbConnection  sin visibilizar este objeto  
    {
        private string _ConnectionString;

        private SqlConnection _DbConnection;
        private SqlTransaction? _DbTransaction;


        #region " Constructores "

        public EjecutorSql(string connectionString)
        {
            if (connectionString == null)
            {
                throw
                    new ArgumentNullException(
                        nameof(connectionString),
                        "NO se puede instanciar un objeto de tipo 'EjecutorSql', con un ConnectionString Nulo."
                        );
            }

            this._ConnectionString = connectionString;
            this._DbConnection = new SqlConnection(this._ConnectionString);
        }
        public EjecutorSql(SqlConnection sqlConnection)
        {
            if (sqlConnection == null)
            {
                throw
                    new ArgumentNullException(
                        nameof(sqlConnection),
                        "Se requiere una conexión de Base de Datos para instanciar un objeto de tipo 'EjecutorSql'."
                        );
            }

            this._DbConnection = sqlConnection;
            this._ConnectionString = this._DbConnection.ConnectionString;
        }

        #endregion 


        #region " Apertura y Acceso a Conexión y Transacción "

        public async Task<SqlConnection?> OpenDbConnectionOrNullIfAlreadyAsync()
        // Si hay una Conexión abierta, retorna NULL, 
        // y únicamente retorna una Conexión al abrirla, 
        // esto con el propósito de que pueda cerrar la Conexión únicamente el objeto que la haya abierto, 
        // y como consecuencia también se vuelve responsabilidad del mismo objeto
        // cachar cualquier Excepción para igualmente cerrar la Conexión. 
        {
            SqlConnection? sqlConnection =
                (this._DbConnection.State == ConnectionState.Closed)
                ? this._DbConnection
                : null
                ;
            if (sqlConnection != null) await sqlConnection.OpenAsync();

            return
                sqlConnection;
        }

        public SqlTransaction? BeginTransactionOrNullIfCurrent()
        // Si hay una Transacción abierta, retorna NULL, 
        // y únicamente retorna una Transacción cuando se inicia una nueva, 
        // esto con el propósito de que pueda hacer Commit() únicamente el objeto que haya iniciado la Transacción, 
        // y como consecuencia también se vuelve responsabilidad del mismo objeto
        // cachar cualquier Excepción para ejecutar el RollBack(). 
        {
            if (this._DbConnection.State == ConnectionState.Closed)
            {
                throw new Exception("Para que exista una Transacción, primero debe abrirse la conexión a la Base de Datos.");
            }
            if (this._DbTransaction != null)
            {
                SqlTransaction? sqlTransaction =
                    (this._DbTransaction.Connection != null)
                    ? null
                    : this._DbConnection.BeginTransaction()
                    ;
                if (sqlTransaction != null)
                    this._DbTransaction = sqlTransaction;

                return
                    sqlTransaction;
            }

            this._DbTransaction =
                this._DbConnection.BeginTransaction();

            return
                this._DbTransaction;
        }

        #endregion 


        #region " Re-Direccionamiento a Métodos de  _DbConnection (SqlConnection) "

        public IDataReader ExecuteReader(
            string sql,
            object? param = null,
            //IDbTransaction? transaction = null, 
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            return
                //this._DbConnection.ExecuteReader(sql, param, transaction, commandTimeout, commandType); 
                this._DbConnection.ExecuteReader(sql, param, this._DbTransaction, commandTimeout, commandType);
        }
        public async Task<IDataReader> ExecuteReaderAsync(
            string sql,
            object? param = null,
            //IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            return
                //await this._DbConnection.ExecuteReaderAsync(sql, param, transaction, commandTimeout, commandType); 
                await this._DbConnection.ExecuteReaderAsync(sql, param, this._DbTransaction, commandTimeout, commandType);
        }


        public T QueryFirstOrDefault<T>(
            string sql,
            object? param = null,
            //IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            return
                //this._DbConnection.QueryFirstOrDefault<T>(sql, param, transaction, commandTimeout, commandType); 
                this._DbConnection.QueryFirstOrDefault<T>(sql, param, this._DbTransaction, commandTimeout, commandType);
        }
        public async Task<T> QueryFirstOrDefaultAsync<T>(
            string sql,
            object? param = null,
            //IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            return
                //await this._DbConnection.QueryFirstOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType); 
                await this._DbConnection.QueryFirstOrDefaultAsync<T>(sql, param, this._DbTransaction, commandTimeout, commandType);
        }


        public T QuerySingleOrDefault<T>(
            string sql,
            object? param = null,
            //IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            return
                //this._DbConnection.QuerySingleOrDefault<T>(sql, param, transaction, commandTimeout, commandType); 
                this._DbConnection.QuerySingleOrDefault<T>(sql, param, this._DbTransaction, commandTimeout, commandType);
        }
        public async Task<T> QuerySingleOrDefaultAsync<T>(
            string sql,
            object? param = null,
            //IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            return
                //await this._DbConnection.QuerySingleOrDefaultAsync<T>(sql, param, transaction, commandTimeout, commandType); 
                await this._DbConnection.QuerySingleOrDefaultAsync<T>(sql, param, this._DbTransaction, commandTimeout, commandType);
        }


        public int Execute(
            string sql,
            object? param = null,
            //IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            return
                //this._DbConnection.Execute(sql, param, transaction, commandTimeout, commandType); 
                this._DbConnection.Execute(sql, param, this._DbTransaction, commandTimeout, commandType);
        }
        public async Task<int> ExecuteAsync(
            string sql,
            object? param = null,
            //IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            return
                //await this._DbConnection.ExecuteAsync(sql, param, transaction, commandTimeout, commandType); 
                await this._DbConnection.ExecuteAsync(sql, param, this._DbTransaction, commandTimeout, commandType);
        }


        public IEnumerable<T> Query<T>(
            string sql,
            object? param = null,
            //IDbTransaction? transaction = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            return
                //this._DbConnection.Query<T>(sql, param, transaction, buffered, commandTimeout, commandType); 
                this._DbConnection.Query<T>(sql, param, this._DbTransaction, buffered, commandTimeout, commandType);
        }
        public async Task<IEnumerable<T>> QueryAsync<T>(
            string sql,
            object? param = null,
            //IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null
            )
        {
            return
                //await this._DbConnection.QueryAsync<T>(sql, param, transaction, commandTimeout, commandType); 
                await this._DbConnection.QueryAsync<T>(sql, param, this._DbTransaction, commandTimeout, commandType);
        }

        //
        //

        #endregion 

        //
    }

    //
}
