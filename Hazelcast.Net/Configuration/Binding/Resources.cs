namespace Hazelcast.Configuration.Binding
{
    // no idea how building Microsoft's runtime generates these,
    // so adding them by hand here
    // make sure to mark the class 'partial' in Resources.Designer.cs

    internal partial class Resources
    {
        internal static string FormatError_CannotActivateAbstractOrInterface(params object[] args)
            => string.Format(Error_CannotActivateAbstractOrInterface, args);

        internal static string FormatError_FailedBinding(params object[] args)
            => string.Format(Error_FailedBinding, args);

        internal static string FormatError_FailedToActivate(params object[] args)
            => string.Format(Error_FailedToActivate, args);

        internal static string FormatError_MissingParameterlessConstructor(params object[] args)
            => string.Format(Error_MissingParameterlessConstructor, args);

        internal static string FormatError_UnsupportedMultidimensionalArray(params object[] args)
            => string.Format(Error_UnsupportedMultidimensionalArray, args);
    }
}
