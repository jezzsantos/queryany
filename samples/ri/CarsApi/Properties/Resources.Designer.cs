﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CarsApi.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CarsApi.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Id of the resource is not a valid identifier.
        /// </summary>
        internal static string AnyValidator_InvalidId {
            get {
                return ResourceManager.GetString("AnyValidator_InvalidId", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Make of the car can only be {0}.
        /// </summary>
        internal static string CreateCarRequestValidator_InvalidMake {
            get {
                return ResourceManager.GetString("CreateCarRequestValidator_InvalidMake", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Manufacturer of the car must be {0}.
        /// </summary>
        internal static string CreateCarRequestValidator_InvalidModel {
            get {
                return ResourceManager.GetString("CreateCarRequestValidator_InvalidModel", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Year for the car must be between {0} and {1}.
        /// </summary>
        internal static string CreateCarRequestValidator_InvalidYear {
            get {
                return ResourceManager.GetString("CreateCarRequestValidator_InvalidYear", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The UntilUtc must be a valid date and time.
        /// </summary>
        internal static string OccupyCarRequestValidator_InvalidUntilUtc {
            get {
                return ResourceManager.GetString("OccupyCarRequestValidator_InvalidUntilUtc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The UntilUtc cannot be a date and time in the past.
        /// </summary>
        internal static string OccupyCarRequestValidator_PastUntilUtc {
            get {
                return ResourceManager.GetString("OccupyCarRequestValidator_PastUntilUtc", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The license plate jurisdiction is not valid.
        /// </summary>
        internal static string RegisterCarRequestValidator_InvalidJurisdiction {
            get {
                return ResourceManager.GetString("RegisterCarRequestValidator_InvalidJurisdiction", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The license plate number is invalid for this jurisdiction..
        /// </summary>
        internal static string RegisterCarRequestValidator_InvalidNumber {
            get {
                return ResourceManager.GetString("RegisterCarRequestValidator_InvalidNumber", resourceCulture);
            }
        }
    }
}
