using System;
using System.ComponentModel;

namespace NetFoundry
{
    /// <summary>
    /// An enum representing the outcome of the given Ziti operation.
    /// </summary>
    public enum ZitiStatus
    {
        /// <summary>
        /// Indicates a successful outcome
        /// </summary>
        [Description("OK")]
        OK = 0,

        /// <summary>
        /// A user error indicating the provided configuration file was not found.
        /// </summary>
        [Description("Configuration not found")]
        CONFIG_NOT_FOUND = -1,

        /// <summary>
        /// A user error indicating the identity from the provided configuration 
        /// file is not authorized to perform the action.
        /// </summary>
        [Description("Not Authorized")]
        NOT_AUTHORIZED = -2,

        /// <summary>
        /// A network error indicating the Ziti Network Controller was not able to be contacted.
        /// </summary>
        [Description("Ziti Controller is not available")]
        CONTROLLER_UNAVAILABLE = -3,

        /// <summary>
        /// A network error indicating the Ziti Network Gateway was not able to be contacted.
        /// </summary>
        [Description("Ziti Gateway is not available")]
        GATEWAY_UNAVAILABLE = -4,

        /// <summary>
        /// A user error indicating the service name provided was not available. 
        /// Either it does not exist or the provided identity does not have sufficient 
        /// rights to the service.
        /// </summary>
        [Description("Service not available")]
        SERVICE_UNAVALABLE = -5,

        /// <summary>
        /// A normal status indicating the connection is closed.
        /// </summary>
        [Description("Connection closed")]
        EOF = -6,

        /// <summary>
        /// An exceptional status indicating the operation did not complete 
        /// within the specified timeout
        /// </summary>
        [Description("Operation did not complete in time")]
        TIMEOUT = -7,

        /// <summary>
        /// An exceptional situation indicating the connection between the 
        /// client and the Ziti Network Gateway was interrupted
        /// </summary>
        [Description("Connection to gateway terminated")]
        CONNABORT = -8
    }

    internal static class EnumHelper
    {
        /// <summary>
        /// Extension method to enum to return the DescriptionAttribute
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
                return "__NO DESCRIPTOIN AVAILABLE__";
            }
            var descAttr = (DescriptionAttribute)(atts[0]);
            return descAttr.Description;
        }
    }
}
