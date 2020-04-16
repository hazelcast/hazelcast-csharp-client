namespace Hazelcast.Data
{
    /// <summary>
    /// Represents the version of a cluster member.
    /// </summary>
    public class MemberVersion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberVersion"/> class.
        /// </summary>
        /// <param name="major">The major version number.</param>
        /// <param name="minor">The minor version number.</param>
        /// <param name="patch">The patch version number.</param>
        public MemberVersion(byte major, byte minor, byte patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        /// <summary>
        /// Gets the major version number.
        /// </summary>
        public byte Major { get; }

        /// <summary>
        /// Gets the minor version number.
        /// </summary>
        public byte Minor { get; }

        /// <summary>
        /// Gets the patch version number.
        /// </summary>
        public byte Patch { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Major}.{Minor}.{Patch}";
        }
    }
}
