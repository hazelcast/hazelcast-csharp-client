namespace Hazelcast.Core
{
    /// <summary>
    ///     Concurrent, distributed implementation of
    ///     <p />
    ///     <b>
    ///         This class is <i>not</i> a general-purpose <tt>Set</tt> implementation! While this class implements
    ///         the <tt>Set</tt> interface, it intentionally violates <tt>Set's</tt> general contract, which mandates the
    ///         use of the <tt>Equals</tt> method when comparing objects. Instead of the equals method this implementation
    ///         compares the serialized byte version of the objects.
    ///     </b>
    /// </summary>
    public interface IHSet<E> : /*ISet<E>,*/ IHCollection<E>
    {
    }
}