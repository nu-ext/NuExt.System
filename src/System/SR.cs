namespace System;

internal static class SR
{
    public const string Arg_BasePathNotFullyQualified = "Basepath argument is not fully qualified.";
    public const string Arg_PathNotFullyQualified = "Path argument is not fully qualified.";
    public const string Arg_PathEmpty = "The path is empty.";
    public const string Arg_WrongType = """The value "{0}" is not of type "{1}" and cannot be used in this generic collection.""";

    public const string Argument_AddingDuplicateWithKey = "An item with the same key has already been added. Key: {0}";
    public const string Argument_DestinationTooShort = "Destination is too short.";
    public const string Argument_EmptyString = "The value cannot be an empty string.";
    public const string Argument_EmptyOrWhiteSpaceString = "The value cannot be an empty string or composed entirely of whitespace.";
    public const string Argument_IncompatibleArrayType = "Target array type is not compatible with the type of items in the collection.";
    public const string Argument_MinMaxValue = "'{0}' cannot be greater than {1}.";
    public const string Argument_NullCharInPath = "Null character in path.";
    public const string Argument_WhiteSpaceString = "The value cannot be a white-space string.";

    public const string ArgumentOutOfRange_Generic_MustBeNonZero = "{0} ('{1}') must be a non-zero value.";
    public const string ArgumentOutOfRange_Generic_MustBeNonNegative = "{0} ('{1}') must be a non-negative value.";
    public const string ArgumentOutOfRange_Generic_MustBeNonNegativeNonZero = "{0} ('{1}') must be a non-negative and non-zero value.";
    public const string ArgumentOutOfRange_Generic_MustBeLessOrEqual = "{0} ('{1}') must be less than or equal to '{2}'.";
    public const string ArgumentOutOfRange_Generic_MustBeLess = "{0} ('{1}') must be less than '{2}'.";
    public const string ArgumentOutOfRange_Generic_MustBeGreaterOrEqual = "{0} ('{1}') must be greater than or equal to '{2}'.";
    public const string ArgumentOutOfRange_Generic_MustBeGreater = "{0} ('{1}') must be greater than '{2}'.";
    public const string ArgumentOutOfRange_Generic_MustBeEqual = "{0} ('{1}') must be equal to '{2}'.";
    public const string ArgumentOutOfRange_Generic_MustBeNotEqual = "{0} ('{1}') must not be equal to '{2}'.";
    public const string ArgumentOutOfRange_NeedNonNegNum = "Non-negative number required.";

    public const string Format_IndexOutOfRange = "Index (zero based) must be greater than or equal to zero and less than the size of the argument list.";
    public const string Format_InvalidStringWithOffsetAndReason = "Input string was not in a correct format. Failure to parse near offset {0}. {1}";

    public const string InvalidOperation_ThreadAccessError =
        "Cannot access this object from the current thread (Thread ID: {0}). " +
        "If this object has a SynchronizationContext, use the SynchronizationContext.Invoke or SynchronizationContext.InvokeAsync methods for thread-safe access. ";
    public const string InvalidOperation_SynchronizationContextAccessDenied =
        "Thread access violation. The calling thread (ID: {0}) attempted to access " +
        "a synchronization context bound to thread (ID: {1}). Use the Send or Post methods " +
        "for cross-thread operations.";

    public const string ObservableObservableDictionaryReentrancyNotAllowed = "Cannot change ObservableDictionary during a CollectionChanged event.";
    public const string OutOfMemory_StringTooLong = "String length exceeded supported range.";

    public const string VerifyAccess = "The calling thread cannot access this object because a different thread owns it.";

    internal static string Format(string resourceFormat, object? p1)
    {
        return string.Format(resourceFormat, p1);
    }
    internal static string Format(string resourceFormat, object? p1, object? p2)
    {
        return string.Format(resourceFormat, p1, p2);
    }

    internal static string Format(string resourceFormat, object? p1, object? p2, object? p3)
    {
        return string.Format(resourceFormat, p1, p2, p3);
    }
}
