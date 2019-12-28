using System.ComponentModel;

namespace UrbanSisters.Model
{
    public class ConflictErrorType
    {
        public static readonly ApiError EmailAlreadyUsed = new ApiError{ErrorType = "EMAIL_ALREADY_USED"};
        public static readonly ApiError UserNewlyModified = new ApiError{ErrorType = "USER_NEWLY_MODIFIED"};
    }
}