using System.Collections.Generic;
using UnityEngine;
namespace Ulbe
{
    public class Cvars : UnitySingleton<Cvars>
    {
        public Dictionary<string, Cvar> cvarList;

        protected override void Awake()
        {
            base.Awake();
            if (_Instance != this)
                return;
            InitCvars();
        }

        protected void InitCvars()
        {
            cvarList = new Dictionary<string, Cvar>(System.StringComparer.InvariantCultureIgnoreCase);
        }

        public Cvar Get(string var_name, string var_value, Cvar.CvarFlags flags = 0)
        {
            Cvar var = Cvar_Find(var_name);

            if (var != null)
            {
                if (flags.HasFlag(Cvar.CvarFlags.CHEAT) && !flags.HasFlag(Cvar.CvarFlags.USER_CREATED) && var_value != "")
                {
                    var.flags &= ~Cvar.CvarFlags.USER_CREATED;
                    var.resetString = var_value;
                }

                var.flags |= flags;

                if (var.resetString == "")
                    var.resetString = var_value;

                return var;
            }

            // Create
            var = new Cvar
            {
                name = var_name,
                stringValue = var_value,
                floatValue = Atof(var_value),
                intValue = Atoi(var_value),
                resetString = var_value,
                flags = flags,
            };

            cvarList.Add(var_name, var);
            return var;
        }

        public Cvar ForceSet(string var_name, string value)
        {
            return Set(var_name, value, true);
        }

        public Cvar Set(string var_name, string value, bool force)
        {
            Cvar var;

            if (!ValidateString(var_name))
                var_name = "BADNAME";

            var = Cvar_Find(var_name);

            // Not found ?
            if (var == null)
            {
                if (value == "")
                    return null;
                if (!force)
                    return Get(var_name, value, Cvar.CvarFlags.USER_CREATED);
                else
                    return Get(var_name, value, 0);
            }

            if (value == "" || value == var.stringValue)
                return var;

            // check if cvar is protected
            if (!force && (var.flags.HasFlag(Cvar.CvarFlags.ROM) || var.flags.HasFlag(Cvar.CvarFlags.INIT) || var.flags.HasFlag(Cvar.CvarFlags.CHEAT)))
                if (!Get("devmode", "0").BoolValue)
                    return var;

            // Set values
            var.name = var_name.ToLower();
            var.stringValue = value;
            var.floatValue = Atof(var.stringValue);
            var.intValue = Atoi(var.stringValue);

            return var;
        }

        public static bool ValidateString(string s)
        {
            if (s == "" || s.Contains("\\") || s.Contains("\"") || s.Contains(";"))
                return false;
            return true;
        }

        public Cvar Cvar_Find(string var_name)
        {
            return cvarList.ContainsKey(var_name) ? cvarList[var_name] : null;
        }

        public int Cvar_VariableIntegerValue(string var_name)
        {
            Cvar var = Cvar_Find(var_name);
            if (var == null)
                return 0;
            return var.intValue;
        }

        public bool Cvar_VariableBooleanValue(string var_name)
        {
            return Cvar_VariableIntegerValue(var_name) == 1;
        }

        public float Cvar_VariableValue(string var_name)
        {
            Cvar var = Cvar_Find(var_name);
            if (var == null)
                return 0f;
            return var.floatValue;
        }

        public string Cvar_VariableString(string var_name)
        {
            Cvar var = Cvar_Find(var_name);
            if (var == null)
                return "";
            return var.stringValue;
        }

        public static int Atoi(string value)
        {
            return int.TryParse(value, out int intvalue) ? intvalue : 0;
        }

        public static float Atof(string value)
        {
            return float.TryParse(value, out float floatValue) ? floatValue : 0f;
        }
    }
}