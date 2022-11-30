using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Markup;

namespace Flithor_ReusableCodes
{
    public class AssemblyVersion : MarkupExtension
    {
        public string Formatter { get; set; }
        public Type AssemblyType { get; set; }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            Assembly assembly = AssemblyType?.GetType().Assembly ?? Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            if (!string.IsNullOrEmpty(Formatter))
            {
                try { return string.Format(Formatter, fvi.FileVersion); }
                catch { }
            }
            return fvi.FileVersion;
        }
    }
}
