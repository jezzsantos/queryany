﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Services.Interfaces.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Services.Interfaces.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to The entity already has an Identifier, and cannot be changed.
        /// </summary>
        public static string EntityBase_IdentifierExists {
            get {
                return ResourceManager.GetString("EntityBase_IdentifierExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Access to the resource is forbidden to the current user.
        /// </summary>
        public static string ForbiddenException_Message {
            get {
                return ResourceManager.GetString("ForbiddenException_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The requested action cannot be performed on the resource at this time.
        /// </summary>
        public static string MethodNotAllowedException_Message {
            get {
                return ResourceManager.GetString("MethodNotAllowedException_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The resource already exists, or is in a conflicted state.
        /// </summary>
        public static string ResourceConflictException_Message {
            get {
                return ResourceManager.GetString("ResourceConflictException_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The resource was not found.
        /// </summary>
        public static string ResourceNotFoundException_Message {
            get {
                return ResourceManager.GetString("ResourceNotFoundException_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The identity of the caller for the current request for the resource is invalid.
        /// </summary>
        public static string RoleViolationException_Message {
            get {
                return ResourceManager.GetString("RoleViolationException_Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A pre-condition of the current request for the resource has not been met.
        /// </summary>
        public static string RuleViolationException_Message {
            get {
                return ResourceManager.GetString("RuleViolationException_Message", resourceCulture);
            }
        }
    }
}