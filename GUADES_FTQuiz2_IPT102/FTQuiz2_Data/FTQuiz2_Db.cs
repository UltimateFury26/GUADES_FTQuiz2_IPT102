using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace GUADES_FTQuiz2_IPT102.FTQuiz2_Data
{
    /// <summary>
    /// The single gateway to MoguMogu_DB (replaces source Data/Db.cs).
    /// Every call is a parameterized stored-procedure call through EF
    /// (Database.SqlQuery / ExecuteSqlCommand). No SQL is string-concatenated
    /// with values; only the procedure name and @parameter names are in the text.
    /// </summary>
    public class FTQuiz2_Db : IDisposable
    {
        // Same value as source Data/Db.cs CommandTimeoutSeconds.
        private const int CommandTimeoutSeconds = 10;

        private readonly FTQuiz2_PortfolioDbContext _context;

        public FTQuiz2_Db() : this(new FTQuiz2_PortfolioDbContext()) { }

        public FTQuiz2_Db(FTQuiz2_PortfolioDbContext context)
        {
            if (context == null) { throw new ArgumentNullException("context"); }
            _context = context;
            _context.Database.CommandTimeout = CommandTimeoutSeconds;
        }

        public FTQuiz2_PortfolioDbContext Context { get { return _context; } }

        // ---- stored-procedure helpers -------------------------------------

        /// <summary>List semantics (source ExecuteTable, many rows).</summary>
        public List<T> Query<T>(string procedure, params SqlParameter[] parameters)
        {
            parameters = parameters ?? new SqlParameter[0];
            return _context.Database
                .SqlQuery<T>(BuildCall(procedure, parameters), parameters)
                .ToList();
        }

        /// <summary>Exactly one row expected (result-code / NewId / Affected procedures).</summary>
        public T QuerySingle<T>(string procedure, params SqlParameter[] parameters)
        {
            parameters = parameters ?? new SqlParameter[0];
            return _context.Database
                .SqlQuery<T>(BuildCall(procedure, parameters), parameters)
                .Single();
        }

        /// <summary>First row or null/default (source "Rows.Count == 0 ? null" pattern).</summary>
        public T QueryFirstOrDefault<T>(string procedure, params SqlParameter[] parameters)
        {
            parameters = parameters ?? new SqlParameter[0];
            return _context.Database
                .SqlQuery<T>(BuildCall(procedure, parameters), parameters)
                .FirstOrDefault();
        }

        /// <summary>No result set needed (source ExecuteNonQuery, e.g. usp_Visit_Record).</summary>
        public int Execute(string procedure, params SqlParameter[] parameters)
        {
            parameters = parameters ?? new SqlParameter[0];
            return _context.Database.ExecuteSqlCommand(BuildCall(procedure, parameters), parameters);
        }

        /// <summary>
        /// Builds "EXEC dbo.usp_X @A = @A, @B = @B". Parameters are bound BY NAME,
        /// exactly like the source CommandType.StoredProcedure call, so the
        /// procedure's declared parameter order never matters.
        /// </summary>
        private static string BuildCall(string procedure, SqlParameter[] parameters)
        {
            if (string.IsNullOrWhiteSpace(procedure))
            {
                throw new ArgumentException("Procedure name is required.", "procedure");
            }

            // Procedure names are developer constants; reject anything that is not a plain identifier.
            if (!procedure.All(c => char.IsLetterOrDigit(c) || c == '_'))
            {
                throw new ArgumentException("Invalid procedure name.", "procedure");
            }

            if (parameters.Length == 0)
            {
                return "EXEC dbo." + procedure;
            }

            string list = string.Join(", ", parameters.Select(p => p.ParameterName + " = " + p.ParameterName));
            return "EXEC dbo." + procedure + " " + list;
        }

        /// <summary>Null-safe parameter (source Db.Param): null becomes DBNull.Value.</summary>
        public static SqlParameter P(string name, object value)
        {
            if (string.IsNullOrEmpty(name) || name[0] != '@')
            {
                throw new ArgumentException("Parameter name must start with '@'.", "name");
            }
            return new SqlParameter(name, value ?? DBNull.Value);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
