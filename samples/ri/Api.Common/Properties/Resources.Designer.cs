﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Api.Common.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Api.Common.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to Failed to register method: &apos;{0}.{1}()&apos; in DomainFactory. AggregateRoot: {0} has more than one declared definition of &apos;AggregateRootFactory&lt;{0}&gt;&apos;.
        /// </summary>
        internal static string DomainFactory_AggregateRootFactoryMethodExists {
            get {
                return ResourceManager.GetString("DomainFactory_AggregateRootFactoryMethodExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to register AggregateRoot: &apos;{0}&apos; in DomainFactory. This class must declare a factory method with signature &apos;public static AggregateRootFactory&lt;{0}&gt; {1}()&apos;&apos;.
        /// </summary>
        internal static string DomainFactory_AggregateRootFactoryMethodNotFound {
            get {
                return ResourceManager.GetString("DomainFactory_AggregateRootFactoryMethodNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to register method: &apos;{0}.{1}()&apos; in DomainFactory. Entity: {0} has more than one declared definition of &apos;EntityFactory&lt;{0}&gt;&apos;.
        /// </summary>
        internal static string DomainFactory_EntityFactoryMethodExists {
            get {
                return ResourceManager.GetString("DomainFactory_EntityFactoryMethodExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to register Entity: &apos;{0}&apos; in DomainFactory. This class must declare a factory method with signature &apos;public static EntityFactory&lt;{0}&gt; {1}()&apos;&apos;.
        /// </summary>
        internal static string DomainFactory_EntityFactoryMethodNotFound {
            get {
                return ResourceManager.GetString("DomainFactory_EntityFactoryMethodNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No Entity factory is registered in the DomainFactory for type &apos;{0}&apos;.
        /// </summary>
        internal static string DomainFactory_EntityTypeNotFound {
            get {
                return ResourceManager.GetString("DomainFactory_EntityTypeNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to register method: &apos;{0}.{1}()&apos; in DomainFactory. This method must be named &apos;{2}&apos; and declared with no parameters.
        /// </summary>
        internal static string DomainFactory_FactoryMethodHasParameters {
            get {
                return ResourceManager.GetString("DomainFactory_FactoryMethodHasParameters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to register method: &apos;{0}.{1}()&apos; in DomainFactory. ValueObject: {0} has more than one declared definition of &apos;ValueObjectFactory&lt;{0}&gt;&apos;.
        /// </summary>
        internal static string DomainFactory_ValueObjectFactoryMethodExists {
            get {
                return ResourceManager.GetString("DomainFactory_ValueObjectFactoryMethodExists", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to register ValueObject: &apos;{0}&apos; in DomainFactory. This class must declare a factory method with signature &apos;public static ValueObjectFactory&lt;{0}&gt; {1}()&apos;&apos;.
        /// </summary>
        internal static string DomainFactory_ValueObjectFactoryMethodNotFound {
            get {
                return ResourceManager.GetString("DomainFactory_ValueObjectFactoryMethodNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No ValueObject factory is registered in the DomainFactory for type &apos;{0}&apos;.
        /// </summary>
        internal static string DomainFactory_ValueObjectTypeNotFound {
            get {
                return ResourceManager.GetString("DomainFactory_ValueObjectTypeNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format of the Embed expression is invalid.
        /// </summary>
        internal static string HasGetOptionsValidator_InvalidEmbed {
            get {
                return ResourceManager.GetString("HasGetOptionsValidator_InvalidEmbed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Embed expression contains too many resources. Maximum of {0} allowed.
        /// </summary>
        internal static string HasGetOptionsValidator_TooManyResourceReferences {
            get {
                return ResourceManager.GetString("HasGetOptionsValidator_TooManyResourceReferences", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format of the Filter expression is invalid.
        /// </summary>
        internal static string HasSearchOptionsValidator_InvalidFilter {
            get {
                return ResourceManager.GetString("HasSearchOptionsValidator_InvalidFilter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Limit for this operation must be between {0} and {1}.
        /// </summary>
        internal static string HasSearchOptionsValidator_InvalidLimit {
            get {
                return ResourceManager.GetString("HasSearchOptionsValidator_InvalidLimit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The offset must be between {0} and {1}.
        /// </summary>
        internal static string HasSearchOptionsValidator_InvalidOffset {
            get {
                return ResourceManager.GetString("HasSearchOptionsValidator_InvalidOffset", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format of the Sort expression is invalid.
        /// </summary>
        internal static string HasSearchOptionsValidator_InvalidSort {
            get {
                return ResourceManager.GetString("HasSearchOptionsValidator_InvalidSort", resourceCulture);
            }
        }
    }
}
