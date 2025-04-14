﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel
{
    public class DisplayNameAttribute : Attribute
    {
        public string DisplayName { get; }

        public DisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}
