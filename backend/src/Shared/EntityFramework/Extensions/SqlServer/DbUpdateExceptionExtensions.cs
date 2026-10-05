using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace dZENcode.Forumish.Shared.EntityFramework.SqlServer.Extensions;

internal static class DbUpdateExceptionExtensions
{
    extension(DbUpdateException exception)
    {
        internal bool IsUniqueConstraintViolation
        {
            get
            {
                return exception.InnerException is SqlException { Number: 2601 or 2627 };
            }
        }
    }
}
