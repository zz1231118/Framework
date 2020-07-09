using System;
using System.Collections.Generic;

namespace SimpleProtoGenerater.Emit
{
    class AppDomainBuilder
    {
        private static readonly AppDomainBuilder _currentDomain = new AppDomainBuilder();
        private readonly List<AssemblyBuilder> _assemblys = new List<AssemblyBuilder>();

        public static AppDomainBuilder CurrentDomain => _currentDomain;

        public IReadOnlyList<AssemblyBuilder> Assemblys => _assemblys;

        internal void AddAssembly(AssemblyBuilder assembly)
        {
            _assemblys.Add(assembly);
        }
    }
}
