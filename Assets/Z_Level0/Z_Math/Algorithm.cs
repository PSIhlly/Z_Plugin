//#define DEBUG_GRAPH
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Windows;

namespace Z_Math
{
    public static class Algorithm
    {
       public static int GetHash(string str,int hashRange=10000000)
        {
            if (string.IsNullOrEmpty(str))
                return 0;

            long hash = 0;
            const long prime = 31;

            foreach (char c in str)
            {
                hash = (hash * prime + c) % hashRange;
            }

            return (int)hash;
        }

    }
}
