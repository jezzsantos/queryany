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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format of the Embed expression in invalid.
        /// </summary>
        public static string HasGetOptionsValidator_InvalidEmbed {
            get {
                return ResourceManager.GetString("HasGetOptionsValidator_InvalidEmbed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Embed contains invalid resource references.
        /// </summary>
        public static string HasGetOptionsValidator_InvalidResourceReference {
            get {
                return ResourceManager.GetString("HasGetOptionsValidator_InvalidResourceReference", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Embed expression contains too many resources. Maximum of {0} allowed.
        /// </summary>
        public static string HasGetOptionsValidator_TooManyResourceReferences {
            get {
                return ResourceManager.GetString("HasGetOptionsValidator_TooManyResourceReferences", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format of the Distinct expression is invalid.
        /// </summary>
        public static string HasSearchOptionsValidator_InvalidDistinct {
            get {
                return ResourceManager.GetString("HasSearchOptionsValidator_InvalidDistinct", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format of the Filter expression is invalid.
        /// </summary>
        public static string HasSearchOptionsValidator_InvalidFilter {
            get {
                return ResourceManager.GetString("HasSearchOptionsValidator_InvalidFilter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Limit for this operation must be between {0} and {1}.
        /// </summary>
        public static string HasSearchOptionsValidator_InvalidLimit {
            get {
                return ResourceManager.GetString("HasSearchOptionsValidator_InvalidLimit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The offset must be between {0} and {1}.
        /// </summary>
        public static string HasSearchOptionsValidator_InvalidOffset {
            get {
                return ResourceManager.GetString("HasSearchOptionsValidator_InvalidOffset", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format of the Sort expression is invalid.
        /// </summary>
        public static string HasSearchOptionsValidator_InvalidSort {
            get {
                return ResourceManager.GetString("HasSearchOptionsValidator_InvalidSort", resourceCulture);
            }
        }
    }
}
