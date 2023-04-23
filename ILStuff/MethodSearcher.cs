using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NPCUnlimiter.ILStuff
{
    public class MethodSearcher
    {
        const BindingFlags AnyBind = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        private static Assembly TMLAssembly => Assembly.GetEntryAssembly(); // Could be calling assembly too? (in correct context maybe)


        private static Dictionary<string, Type> ArgsTypeMapping = null;
        public static Dictionary<string, Type> GetArgsTypeMapping()
        {
            if (ArgsTypeMapping is not null)
                return ArgsTypeMapping;

            // Generic types are a mess to handle so hardcode the type list
            ArgsTypeMapping = new()
            {
                { "NPC", typeof(Terraria.NPC) },
                { "Vector2", typeof(Microsoft.Xna.Framework.Vector2) },
                { "TargetSearchFlag", typeof(Terraria.Utilities.NPCUtils.TargetSearchFlag) },
                { "SearchFilter<Player>", typeof(Terraria.Utilities.NPCUtils.SearchFilter<Terraria.Player>) },
                { "SearchFilter<NPC>", typeof(Terraria.Utilities.NPCUtils.SearchFilter<Terraria.NPC>) }
            };

            /*
            // Dynamic, doesn't work and doesn't account for generics
            ArgsTypeMapping = new(TMLAssembly.DefinedTypes.Select(
                t => KeyValuePair.Create(t.Name, t.AsType())
            ));
            */

            return ArgsTypeMapping;
        }

        public static MethodBase GetFromString(string repr)
        {
            var parts = repr.Split(".");
            var typeName = string.Join(".", parts.SkipLast(1));
            var methodName = parts.Last();
            Type[] argsInfo = null;
            var infoMarker = methodName.IndexOf('(');
            if (infoMarker >= 0)
            {
                var argsInfoStr = methodName.Substring(infoMarker + 1, methodName.Length - infoMarker - 2);
                methodName = methodName.Substring(0, infoMarker);

                // Do the mapping
                var mappings = GetArgsTypeMapping();
                argsInfo = argsInfoStr.Split(',').Select(n => mappings[n]).ToArray();
            }

            var t = TMLAssembly.GetType(typeName);
            if (t is null)
            {
                throw new ArgumentException($"Not found TYPE: {repr}");
            }

            BindingFlags bflags = AnyBind;
            bool isCtor = false;
            if (methodName == "ctor")
            {
                bflags &= ~BindingFlags.Static;
                isCtor = true;
            }
            if (methodName == "cctor")
            {
                bflags &= ~BindingFlags.Instance;
                isCtor = true;
            }


            MethodBase m = null;
            if (isCtor)
            {
                if (argsInfo is null)
                {
                    m = t.GetConstructors(bflags).Single();
                }
                else
                {
                    m = t.GetConstructor(bflags, argsInfo);
                }
            }
            else
            {
                if (argsInfo is null)
                {
                    m = t.GetMethod(methodName, bflags);
                }
                else
                {
                    m = t.GetMethod(methodName, bflags, argsInfo);
                }
            }

            if (m is null)
            {
                throw new ArgumentException($"Not found METHOD: {repr}");
            }

            return m;
        }
    }
}
