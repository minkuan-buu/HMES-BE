using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HMES.Business.Utilities.Authentication;

namespace HMES.Business.Tests.Utilities
{
    public static class AuthenticationTestUtility
    {
        private static readonly Dictionary<string, MethodInfo> _originalMethods = new Dictionary<string, MethodInfo>();
        private static readonly Dictionary<string, Func<string, string, string>> _replacementMethods = new Dictionary<string, Func<string, string, string>>();

        public static void MockDecodeToken(Func<string, string, string> mockImplementation)
        {
            // Save the replacement method
            _replacementMethods["DecodeToken"] = mockImplementation;
        }

        public static void ResetMocks()
        {
            _replacementMethods.Clear();
        }

        // This method will be used instead of the original DecodeToken when mocked
        public static string MockedDecodeToken(string jwtToken, string nameClaim)
        {
            if (_replacementMethods.TryGetValue("DecodeToken", out var replacement))
            {
                return replacement(jwtToken, nameClaim);
            }
            
            // If no mock is set up, provide a default implementation for tests
            if (nameClaim == "userid")
            {
                return Guid.NewGuid().ToString();
            }
            
            return "mocked_value";
        }
    }

    // This class provides a runtime wrapper for Authentication.DecodeToken
    public static class AuthenticationTestWrapper
    {
        // Expose the MockedDecodeToken method as a replacement for Authentication.DecodeToken
        public static string DecodeToken(string jwtToken, string nameClaim)
        {
            return AuthenticationTestUtility.MockedDecodeToken(jwtToken, nameClaim);
        }
    }
} 