using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace NLua
{
    public static class LuaRegistrationHelper
    { 
        #region Tagged methods
        /// <summary>
        /// Registers all methods matching <paramref name="flags"/> in an object tagged with <see cref="LuaGlobalAttribute"/> as Lua functions
        /// </summary>
        /// <param name="lua">The Lua VM to add the methods to</param>
        /// <param name="o">The object to get the methods from</param>
        /// <param name="flags">The <see cref="BindingFlags" to use when getting the methods></param>
        /// <param name="path">The path to which to register the methods. Empty string by default, which results in a global function.</param>
        public static void TaggedMethods(Lua lua, object o, BindingFlags flags, string path = "")
        {
            #region Sanity checks
            if (lua == null)
                throw new ArgumentNullException(nameof(lua));

            if (o == null)
                throw new ArgumentNullException(nameof(o));

            if (path == null)
                throw new ArgumentNullException(nameof(o));
            #endregion

            if(path.Length != 0) path += '.';
            Type type = o as Type;
            object target = null;
            if(!(o is Type))
            {
                type = o.GetType();
                target = o;
            }

            foreach (var method in type.GetMethods(flags))
            {
                foreach (LuaGlobalAttribute attribute in method.GetCustomAttributes(typeof(LuaGlobalAttribute), true))
                {
                    if (string.IsNullOrEmpty(attribute.Name))
                        lua.RegisterFunction(path + method.Name, target, method); // CLR name
                    else
                        lua.RegisterFunction(path + attribute.Name, target, method); // Custom name
                }
            }
        }
        #endregion
        #region Tagged instance methods
        /// <summary>
        /// Registers all public instance methods in an object tagged with <see cref="LuaGlobalAttribute"/> as Lua functions
        /// </summary>
        /// <param name="lua">The Lua VM to add the methods to</param>
        /// <param name="o">The object to get the methods from</param>
        /// <param name="path">The path to which to register the methods. Empty string by default, which results in a global function.</param>
        public static void TaggedInstanceMethods(Lua lua, object o, string path = "") =>
            TaggedMethods(lua, o, BindingFlags.Instance | BindingFlags.Public, path);
        #endregion

        #region Tagged static methods
        /// <summary>
        /// Registers all public static methods in a class tagged with <see cref="LuaGlobalAttribute"/> as Lua functions
        /// </summary>
        /// <param name="lua">The Lua VM to add the methods to</param>
        /// <param name="type">The class type to get the methods from</param>
        /// <param name="path">The path to which to register the methods. Empty string by default, which results in a global function.</param>
        public static void TaggedStaticMethods(Lua lua, Type type, string path = "") =>
            TaggedMethods(lua, type, BindingFlags.Static | BindingFlags.Public, path);
        #endregion

        /// <summary>
        /// Registers an enumeration's values for usage as a Lua variable table
        /// </summary>
        /// <typeparam name="T">The enum type to register</typeparam>
        /// <param name="lua">The Lua VM to add the enum to</param>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "The type parameter is used to select an enum type")]
        public static void Enumeration<T>(Lua lua) where T : Enum
        {
            if (lua == null)
                throw new ArgumentNullException(nameof(lua));

            Type type = typeof(T);

            string[] names = Enum.GetNames(type);
            var values = (T[])Enum.GetValues(type);
            lua.NewTable(type.Name);

            for (int i = 0; i < names.Length; i++)
            {
                string path = type.Name + "." + names[i];
                lua.SetObjectToPath(path, values[i]);
            }
        }
    }
}
