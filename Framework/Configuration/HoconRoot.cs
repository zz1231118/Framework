using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Configuration
{
    /// <inheritdoc />
    public class HoconRoot
    {
        /// <inheritdoc />
        public HoconRoot(HoconValue value, IEnumerable<HoconSubstitution> substitutions)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            if (substitutions == null)
                throw new ArgumentNullException(nameof(substitutions));

            Value = value;
            Substitutions = substitutions;
        }

        /// <inheritdoc />
        public HoconRoot(HoconValue value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Value = value;
            Substitutions = Enumerable.Empty<HoconSubstitution>();
        }

        /// <inheritdoc />
        public HoconValue Value { get; private set; }

        /// <inheritdoc />
        public IEnumerable<HoconSubstitution> Substitutions { get; private set; }
    }
}
