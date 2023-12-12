using System;
using JetBrains.Annotations;

namespace FrameworkUnity.OOP.Custom_DI.DI
{
    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class InjectAttribute : Attribute { }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class Service : Attribute { }

    [MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class Listener : Attribute { }
}
