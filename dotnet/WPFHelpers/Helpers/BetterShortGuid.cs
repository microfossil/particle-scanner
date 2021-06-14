using System;

namespace WPFHelpers.Helpers
{
    /// <summary>
    /// Represents a globally unique identifier (GUID) with a 
    /// shorter string value. Sguid
    /// </summary>
    public struct BetterShortGuid
    {
        #region Static

        /// <summary>
        /// A read-only instance of the BetterShortGuid class whose value 
        /// is guaranteed to be all zeroes. 
        /// </summary>
        public static readonly BetterShortGuid Empty = new BetterShortGuid(Guid.Empty);

        #endregion

        #region Fields

        Guid _guid;
        string _value;

        #endregion

        #region Contructors

        /// <summary>
        /// Creates a BetterShortGuid from a base64 encoded string
        /// </summary>
        /// <param name="value">The encoded guid as a 
        /// base64 string</param>
        public BetterShortGuid(string value)
        {
            _value = value;
            _guid = Decode(value);
        }

        /// <summary>
        /// Creates a BetterShortGuid from a Guid
        /// </summary>
        /// <param name="guid">The Guid to encode</param>
        public BetterShortGuid(Guid guid)
        {
            _value = Encode(guid);
            _guid = guid;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the underlying Guid
        /// </summary>
        public Guid Guid
        {
            get { return _guid; }
            set
            {
                if (value != _guid)
                {
                    _guid = value;
                    _value = Encode(value);
                }
            }
        }

        /// <summary>
        /// Gets/sets the underlying base64 encoded string
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    _guid = Decode(value);
                }
            }
        }

        #endregion

        #region ToString

        /// <summary>
        /// Returns the base64 encoded guid as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _value;
        }

        #endregion

        #region Equals

        /// <summary>
        /// Returns a value indicating whether this instance and a 
        /// specified Object represent the same type and value.
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is BetterShortGuid)
                return _guid.Equals(((BetterShortGuid)obj)._guid);
            if (obj is Guid)
                return _guid.Equals((Guid)obj);
            if (obj is string)
                return _guid.Equals(((BetterShortGuid)obj)._guid);
            return false;
        }

        #endregion

        #region GetHashCode

        /// <summary>
        /// Returns the HashCode for underlying Guid.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _guid.GetHashCode();
        }

        #endregion

        #region NewGuid

        /// <summary>
        /// Initialises a new instance of the BetterShortGuid class
        /// </summary>
        /// <returns></returns>
        public static BetterShortGuid NewGuid()
        {
            return new BetterShortGuid(Guid.NewGuid());
        }

        #endregion

        #region Encode

        /// <summary>
        /// Creates a new instance of a Guid using the string value, 
        /// then returns the base64 encoded version of the Guid.
        /// </summary>
        /// <param name="value">An actual Guid string (i.e. not a BetterShortGuid)</param>
        /// <returns></returns>
        public static string Encode(string value)
        {
            Guid guid = new Guid(value);
            return Encode(guid);
        }

        /// <summary>
        /// Encodes the given Guid as a base64 string that is 22 
        /// characters long.
        /// </summary>
        /// <param name="guid">The Guid to encode</param>
        /// <returns></returns>
        public static string Encode(Guid guid)
        {
            //string encoded = guid.ToByteArray().ToBase62(); 
            string encoded = Convert.ToBase64String(guid.ToByteArray());
            encoded = encoded
                .Replace("z", "z0")
                .Replace("/", "z1")
                .Replace("+", "z2");
            encoded = encoded.Substring(0, encoded.Length-2);
            return encoded;
        }

        #endregion

        #region Decode

        /// <summary>
        /// Decodes the given base64 string
        /// </summary>
        /// <param name="value">The base64 encoded string of a Guid</param>
        /// <returns>A new Guid</returns>
        public static Guid Decode(string value)
        {
            //byte[] b = value.FromBase62();
            //return new Guid(b);
            value = value
                .Replace("z1", "/")
                .Replace("z2", "+")
                .Replace("z0", "z");
            if (!value.EndsWith("==")) value += "==";
            byte[] buffer = Convert.FromBase64String(value);
            return new Guid(buffer);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if both BetterShortGuids have the same underlying 
        /// Guid value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(BetterShortGuid x, BetterShortGuid y)
        {
            if ((object)x == null) return (object)y == null;
            return x._guid == y._guid;
        }

        /// <summary>
        /// Determines if both BetterShortGuids do not have the 
        /// same underlying Guid value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(BetterShortGuid x, BetterShortGuid y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Implicitly converts the BetterShortGuid to it's string equivilent
        /// </summary>
        /// <param name="BetterShortGuid"></param>
        /// <returns></returns>
        public static implicit operator string(BetterShortGuid BetterShortGuid)
        {
            return BetterShortGuid._value;
        }

        /// <summary>
        /// Implicitly converts the BetterShortGuid to it's Guid equivilent
        /// </summary>
        /// <param name="BetterShortGuid"></param>
        /// <returns></returns>
        public static implicit operator Guid(BetterShortGuid BetterShortGuid)
        {
            return BetterShortGuid._guid;
        }

        /// <summary>
        /// Implicitly converts the string to a BetterShortGuid
        /// </summary>
        /// <param name="BetterShortGuid"></param>
        /// <returns></returns>
        public static implicit operator BetterShortGuid(string BetterShortGuid)
        {
            return new BetterShortGuid(BetterShortGuid);
        }

        /// <summary>
        /// Implicitly converts the Guid to a BetterShortGuid 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static implicit operator BetterShortGuid(Guid guid)
        {
            return new BetterShortGuid(guid);
        }

        #endregion
    }

}
