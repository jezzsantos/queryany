﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace QueryAny.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("QueryAny.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to You cannot use an &apos;WhereAll&apos; after a &apos;WhereAll&apos;, or after a &apos;Where&apos;, or after a &apos;AndWhere/OrWhere&apos;, or after a &apos;WhereNoOp&apos;.
        /// </summary>
        internal static string FromClause_WhereAllAndNotEmpty {
            get {
                return ResourceManager.GetString("FromClause_WhereAllAndNotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot use an &apos;Where&apos; after a &apos;Where&apos;, or after a &apos;AndWhere/OrWhere&apos;, or after a &apos;WhereNoOp&apos;, or after a &apos;WhereAll&apos;.
        /// </summary>
        internal static string FromClause_WhereAndNotEmpty {
            get {
                return ResourceManager.GetString("FromClause_WhereAndNotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot use an &apos;WhereNoOp&apos; after a &apos;WhereNoOp&apos;, or  after a &apos;Where&apos;, or after a &apos;AndWhere/OrWhere&apos;, or after a &apos;WhereAll&apos;.
        /// </summary>
        internal static string FromClause_WhereNoOpAndNotEmpty {
            get {
                return ResourceManager.GetString("FromClause_WhereNoOpAndNotEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot &apos;Join&apos; on the same Entity (&apos;{0}&apos;) twice.
        /// </summary>
        internal static string QueriedEntities_JoinSameEntity {
            get {
                return ResourceManager.GetString("QueriedEntities_JoinSameEntity", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot &apos;Take&apos; more than once.
        /// </summary>
        internal static string QueriedEntities_LimitAlreadySet {
            get {
                return ResourceManager.GetString("QueriedEntities_LimitAlreadySet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot &apos;Skip&apos; more than once.
        /// </summary>
        internal static string QueriedEntities_OffsetAlreadySet {
            get {
                return ResourceManager.GetString("QueriedEntities_OffsetAlreadySet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot &apos;OrderBy&apos; more than once.
        /// </summary>
        internal static string QueriedEntities_OrderByAlreadySet {
            get {
                return ResourceManager.GetString("QueriedEntities_OrderByAlreadySet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot use an &apos;AndWhere&apos; before a &apos;Where&apos;, or before a &apos;WhereNoOp&apos;, or after a &apos;WhereAll&apos;.
        /// </summary>
        internal static string QueryClause_AndWhereBeforeWheres {
            get {
                return ResourceManager.GetString("QueryClause_AndWhereBeforeWheres", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot use an &apos;OrWhere&apos; before a &apos;Where&apos;, or before a &apos;WhereNoOp&apos;, or after a &apos;WhereAll&apos;.
        /// </summary>
        internal static string QueryClause_OrWhereBeforeWheres {
            get {
                return ResourceManager.GetString("QueryClause_OrWhereBeforeWheres", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot &apos;SelectFromJoin&apos; on entity {0}, because no joins are yet defined.
        /// </summary>
        internal static string QueryClause_SelectFromJoin_NoJoins {
            get {
                return ResourceManager.GetString("QueryClause_SelectFromJoin_NoJoins", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot &apos;SelectFromJoin&apos; on entity {0}, because no join for that entity is yet defined.
        /// </summary>
        internal static string QueryClause_SelectFromJoin_UnknownJoin {
            get {
                return ResourceManager.GetString("QueryClause_SelectFromJoin_UnknownJoin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The expression is not a property expression or is not convertable.
        /// </summary>
        internal static string Reflector_ErrorNotMemberAccessOrConvertible {
            get {
                return ResourceManager.GetString("Reflector_ErrorNotMemberAccessOrConvertible", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Member is not a property.
        /// </summary>
        internal static string Reflector_ErrorNotProperty {
            get {
                return ResourceManager.GetString("Reflector_ErrorNotProperty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot &apos;Take&apos; less than 0 results.
        /// </summary>
        internal static string ResultOptions_InvalidLimit {
            get {
                return ResourceManager.GetString("ResultOptions_InvalidLimit", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot &apos;Skip&apos; less than 0 results.
        /// </summary>
        internal static string ResultOptions_InvalidOffset {
            get {
                return ResourceManager.GetString("ResultOptions_InvalidOffset", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to You cannot &apos;OrderBy&apos; null.
        /// </summary>
        internal static string ResultOptions_InvalidOrderBy {
            get {
                return ResourceManager.GetString("ResultOptions_InvalidOrderBy", resourceCulture);
            }
        }
    }
}
