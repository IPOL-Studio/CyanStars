// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// This code is a "polyfill" to enable newer C# features on older .NET runtimes.
// See: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis

#if !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>Specifies that when a method returns, a member will not be null.</summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
    internal sealed class MemberNotNullAttribute : Attribute
    {
        /// <summary>Initializes the attribute with a field or property member.</summary>
        /// <param name="member">The field or property member that is promised to be not-null.</param>
        public MemberNotNullAttribute(string member) => Members = new[] { member };

        /// <summary>Initializes the attribute with the names of the credited members.</summary>
        /// <param name="members">The list of field and property members that are promised to be not-null.</param>
        public MemberNotNullAttribute(params string[] members) => Members = members;

        /// <summary>Gets the list of field and property members that are promised to be not-null.</summary>
        public string[] Members { get; }
    }
}

#endif
