/*
Copyright 2019 NetFoundry, Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace OpenZiti
{
    /// <summary>
    /// A class which provides an extension method to get a Description from an enum. The enum must be
    /// decorated with a <see cref="DescriptionAttribute"/>
    /// </summary>
    public static class EnumHelper
    {
        public const string NO_DESC = "__NO DESCRIPTOIN AVAILABLE__";

        /// <summary>
        /// Extension method to return the <see cref="DescriptionAttribute"/> of the enum.
        /// If no <see cref="DescriptionAttribute"/> exists, returns <see cref="NO_DESC"/>
        /// </summary>
        /// <param name="enumVal">The enum in question to get the description from</param>
        /// <returns></returns>
        public static string GetDescription(this Enum enumVal)
        {
            var type = enumVal.GetType();
            var memberInfo = type.GetMember(enumVal.ToString());
            var atts = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (atts.Length < 1)
            {
                return NO_DESC;
            }
            var descAttr = (DescriptionAttribute)(atts[0]);
            return descAttr.Description;
        }
    }

    /// <summary>
    /// A class which provides an extension method to free a <see cref="GCHandle"/> which checks
    /// to make sure the handle being free'ed is not <see cref="Util.NO_CONTEXT"/>
    /// </summary>
    public static class GCHandleExtensions
    {
        /// <summary>
        /// An extension method which checks to make sure the handle is not <see cref="Util.NO_CONTEXT"/>
        /// before Free'ing
        /// </summary>
        /// <param name="handle"></param>
        public static void SafeFreeGCHandle(this GCHandle handle)
        {
            try
            {
                if (ZitiUtil.NO_CONTEXT == handle)
                {
                    return; //never call free on the NO_CONTEXT handle...
                }
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.WriteLine("an unexpected exception occrred in SafeFreeGCHandle: " + e.Message);
            }
        }
    }
}
