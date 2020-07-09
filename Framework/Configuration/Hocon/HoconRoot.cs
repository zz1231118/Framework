﻿using System.Collections.Generic;
using System.Linq;

namespace Framework.Configuration.Hocon
{
    public class HoconRoot
    {
        public HoconRoot(HoconValue value, IEnumerable<HoconSubstitution> substitutions)
        {
            Value = value;
            Substitutions = substitutions;
        }

        public HoconRoot(HoconValue value)
        {
            Value = value;
            Substitutions = Enumerable.Empty<HoconSubstitution>();
        }

        public HoconValue Value { get; private set; }

        public IEnumerable<HoconSubstitution> Substitutions { get; private set; }
    }
}
